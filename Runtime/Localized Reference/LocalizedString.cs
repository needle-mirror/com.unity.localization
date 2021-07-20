using System;
using System.Collections.Generic;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.Extensions;
using UnityEngine.Localization.SmartFormat.GlobalVariables;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization
{
    /// <summary>
    /// Provides a way to reference a <see cref="StringTableEntry"/> inside of a specific <see cref="StringTable"/> and request the localized string.
    /// </summary>
    [Serializable]
    public partial class LocalizedString : LocalizedReference
    {
        ChangeHandler m_ChangeHandler;
        AsyncOperationHandle<LocalizedStringDatabase.TableEntryResult>? m_CurrentLoadingOperation;
        string m_CurrentStringChangedValue;
        List<IGlobalVariableValueChanged> m_LastUsedGlobalVariables = new List<IGlobalVariableValueChanged>();
        Action<IGlobalVariable> m_OnGlobalVariableChanged;
        IList<object> m_SmartArguments;
        bool m_WaitingForGlobalVariablesEndUpdate;

        /// <summary>
        /// Arguments that will be passed through to Smart Format. These arguments are not serialized and will need to be set at runtime.
        /// </summary>
        public IList<object> Arguments
        {
            get => m_SmartArguments;
            set
            {
                if (m_SmartArguments == value)
                    return;

                m_SmartArguments = value;
                RefreshString();
            }
        }

        /// <summary>
        /// The current loading operation for the string when using <see cref="StringChanged"/> or null if one is not available.
        /// A string may not be immediately available, such as when loading the <see cref="StringTable"/> asset, so all string operations are wrapped
        /// with an <see cref="AsyncOperationHandle"/>.
        /// See also <seealso cref="RefreshString"/>
        /// </summary>
        public AsyncOperationHandle<LocalizedStringDatabase.TableEntryResult>? CurrentLoadingOperation
        {
            get => m_CurrentLoadingOperation;
            internal set => m_CurrentLoadingOperation = value;
        }

        /// <summary>
        /// Delegate used by <see cref="StringChanged"/>.
        /// </summary>
        /// <param name="value">The localized string.</param>
        public delegate void ChangeHandler(string value);

        /// <summary>
        /// Provides a callback that will be invoked when the translated string has changed.
        /// </summary>
        /// <remarks>
        /// The following events will trigger an update:
        /// - The first time the action is added to the event.
        /// - The <seealso cref="LocalizationSettings.SelectedLocale"/> changing.
        /// - The <see cref="Arguments"/> changing.
        /// - If the string is currently using a <see cref="IGlobalVariable"/> which supports <see cref="IGlobalVariableValueChanged"/> and it's value has changed.
        /// - When <see cref="RefreshString"/> is called.
        /// - The <see cref="TableReference"/> or <see cref="TableEntryReference"/> changing.
        ///
        /// When the first <see cref="ChangeHandler"/> is added, a loading operation (see <see cref="CurrentLoadingOperation"/>) automatically starts.
        /// When the loading operation is completed, the localized string value is sent to the subscriber.
        /// If you add additional subscribers after loading has completed, they are also sent the latest localized string value.
        /// This ensures that a subscriber will always have the correct localized value regardless of when it was added.
        /// </remarks>
        /// <example>
        /// This example shows how the <see cref="StringChanged"/> event can be used to trigger updates to a string.
        /// <code source="../../DocCodeSamples.Tests/LocalizedStringSamples.cs" region="localized-string-events"/>
        /// </example>
        public event ChangeHandler StringChanged
        {
            add
            {
                if (value == null)
                    throw new ArgumentNullException();

                bool wasEmpty = m_ChangeHandler == null;
                m_ChangeHandler += value;

                if (wasEmpty)
                {
                    LocalizationSettings.ValidateSettingsExist();
                    ForceUpdate();

                    // We subscribe after the first update as its possible that a SelectedLocaleChanged may be fired
                    // during ForceUpdate when using WaitForCompletion and we want to avoid this.
                    LocalizationSettings.SelectedLocaleChanged += HandleLocaleChange;
                }
                else if (CurrentLoadingOperation.HasValue && CurrentLoadingOperation.Value.IsDone)
                {
                    // Call the event with the latest value.
                    value(m_CurrentStringChangedValue);
                }
            }
            remove
            {
                if (value == null)
                    throw new ArgumentNullException();

                m_ChangeHandler -= value;

                if (m_ChangeHandler == null)
                {
                    LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChange;
                    ClearLoadingOperation();
                }
            }
        }

        /// <summary>
        /// Returns <c>true</c> if <seealso cref="StringChanged"/> has any subscribers.
        /// </summary>
        public bool HasChangeHandler => m_ChangeHandler != null;

        /// <summary>
        /// Initializes and returns an empty instance of a <see cref="LocalizedString"/>.
        /// </summary>
        public LocalizedString() {}

        /// <summary>
        /// Initializes and returns an instance of a <see cref="LocalizedString"/>.
        /// </summary>
        /// <param name="tableReference">Reference to the String Table Collection.
        /// This can either be the name of the collection as a <c>string</c> or the Collection Guid as a [System.Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid).</param>
        /// <param name="entryReference">Reference to the String Table Collection entry.
        /// This can either be the name of the Key as a <c>string</c> or the <c>long</c> Key Id.</param>
        /// <example>
        /// This example shows the different ways to construct a LocalizedString.
        /// <code source="../../DocCodeSamples.Tests/LocalizedStringSamples.cs" region="localized-string-constructor"/>
        /// </example>
        /// <example>
        /// This example shows how a LocalizedString could be set up in the Editor.
        /// By using the Guid and Id the table and entry references will not be lost if the table collection name or entry name was to be changed.
        /// Note: The performance benefits to using a Guid and Id are negligible.
        /// <code source="../../DocCodeSamples.Tests/LocalizedStringSamples.cs" region="localized-string-constructor-editor"/>
        /// </example>
        public LocalizedString(TableReference tableReference, TableEntryReference entryReference)
        {
            TableReference = tableReference;
            TableEntryReference = entryReference;
        }

        /// <summary>
        /// Provides a way to force a refresh of the string when using <see cref="StringChanged"/>.
        /// </summary>
        /// <remarks>
        /// <para>This will only force the refresh if there is currently no active <see cref="CurrentLoadingOperation"/>, if one is still being executed then it will be ignored and <c>false</c> will be returned.
        /// If a string is not static and will change during game play, such as when using format arguments, then this can be used to force the string to update itself.</para>
        /// You may wish to call this if the values <b>inside</b> of the <see cref="Arguments"/> list have changed or you wish to force all <see cref="StringChanged"/> subscribers to update.
        /// Note if setting the <see cref="Arguments"/> with a new list value then you do not need to call this as it will be called by the <see cref="Arguments"/> set method automatically.
        /// </remarks>
        /// <returns>Returns <c>true</c> if a new refresh could be requested or <c>false</c> if it could not, such as when <see cref="CurrentLoadingOperation"/> is still loading.</returns>
        /// <example>
        /// This example shows how the string can be refreshed, such as when showing dynamic values like the current time.
        /// <code source="../../DocCodeSamples.Tests/LocalizedStringSamples.cs" region="localized-string-smart"/>
        /// </example>
        public bool RefreshString()
        {
            if (m_ChangeHandler == null || m_CurrentLoadingOperation == null)
                return false;

            if (!m_CurrentLoadingOperation.Value.IsDone)
            {
                if (!WaitForCompletion)
                    return false;
                m_CurrentLoadingOperation.Value.WaitForCompletion();
            }

            // Clear any previous global variables.
            var entry = m_CurrentLoadingOperation.Value.Result.Entry;
            entry?.FormatCache?.GlobalVariableTriggers.Clear();

            var translatedText = LocalizationSettings.StringDatabase.GenerateLocalizedString(m_CurrentLoadingOperation.Value.Result.Table, entry, TableReference, TableEntryReference, LocaleOverride ?? LocalizationSettings.SelectedLocale, Arguments);
            UpdateGlobalVariableListeners(entry?.FormatCache?.GlobalVariableTriggers);

            m_CurrentStringChangedValue = translatedText;
            m_ChangeHandler(m_CurrentStringChangedValue);
            return true;
        }

        /// <summary>
        /// Provides a translated string from a <see cref="StringTable"/> with the <see cref="TableReference"/> and the
        /// the translated string that matches <see cref="TableEntryReference"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="AsyncOperationHandle.Completed"/> event provides a notification once the operation has finished and the string has been found or an error has occurred.
        /// A string table may have already been loaded during a previous operation or when using Preload mode.
        /// Check the <see cref="AsyncOperationHandle.IsDone"/> property to see if the string table has already been loaded and the translated string is immediately available.
        /// See [Async operation handling](https://docs.unity3d.com/Packages/com.unity.addressables@latest/index.html?subfolder=/manual/AddressableAssetsAsyncOperationHandle.html) for further details.
        /// To force the operation to complete immediately, call <see cref="AsyncOperationHandle.WaitForCompletion"/>.
        /// </remarks>
        /// <returns>Returns the loading operation for the request.</returns>
        /// <example>
        /// This example shows how <see cref="GetLocalizedStringAsync"/> can be used to request an updated string when the <see cref="LocalizationSettings.SelectedLocale"/> changes.
        /// <code source="../../DocCodeSamples.Tests/LocalizedStringSamples.cs" region="get-localized-string"/>
        /// </example>
        /// <example>
        /// This example shows how <see cref="GetLocalizedStringAsync"/> can be forced to complete immediately using <see cref="AsyncOperationHandle.WaitForCompletion"/>.
        /// <code source="../../DocCodeSamples.Tests/LocalizedStringSamples.cs" region="get-localized-string-synchronous"/>
        /// </example>
        public AsyncOperationHandle<string> GetLocalizedStringAsync()
        {
            LocalizationSettings.ValidateSettingsExist();
            return LocalizationSettings.StringDatabase.GetLocalizedStringAsync(TableReference, TableEntryReference, Arguments, LocaleOverride, FallbackState);
        }

        /// <summary>
        /// Provides a translated string from a <see cref="StringTable"/> with the <see cref="TableReference"/> and the
        /// the translated string that matches <see cref="TableEntryReference"/>.
        /// </summary>
        public string GetLocalizedString() => GetLocalizedStringAsync().WaitForCompletion();

        /// <summary>
        /// Provides a translated string from a <see cref="StringTable"/> with the <see cref="TableReference"/> and the
        /// the translated string that matches <see cref="TableEntryReference"/>.
        /// </summary>
        /// <param name="arguments">The arguments to pass into the Smart String formatter or <c>String.Format</c>.</param>
        /// <returns>Returns the loading operation for the request.</returns>
        public AsyncOperationHandle<string> GetLocalizedStringAsync(params object[] arguments)
        {
            LocalizationSettings.ValidateSettingsExist();
            return LocalizationSettings.StringDatabase.GetLocalizedStringAsync(TableReference, TableEntryReference, arguments, LocaleOverride, FallbackState);
        }

        /// <summary>
        /// Provides a translated string from a <see cref="StringTable"/> with the <see cref="TableReference"/> and the
        /// the translated string that matches <see cref="TableEntryReference"/>.
        /// </summary>
        /// <param name="arguments">The arguments to pass into the Smart String formatter or <c>String.Format</c>.</param>
        /// <returns></returns>
        public string GetLocalizedString(params object[] arguments) => GetLocalizedStringAsync(arguments).WaitForCompletion();

        /// <summary>
        /// Provides a translated string from a <see cref="StringTable"/> with the <see cref="TableReference"/> and the
        /// the translated string that matches <see cref="TableEntryReference"/>.
        /// </summary>
        /// <param name="arguments">The arguments to pass into the Smart String formatter or <c>String.Format</c>.</param>
        /// <returns>Returns the loading operation for the request.</returns>
        public AsyncOperationHandle<string> GetLocalizedStringAsync(IList<object> arguments)
        {
            LocalizationSettings.ValidateSettingsExist();
            return LocalizationSettings.StringDatabase.GetLocalizedStringAsync(TableReference, TableEntryReference, arguments, LocaleOverride, FallbackState);
        }

        /// <summary>
        /// Provides a translated string from a <see cref="StringTable"/> with the <see cref="TableReference"/> and
        /// the translated string that matches <see cref="TableEntryReference"/>.
        /// </summary>
        /// <param name="arguments">The arguments to pass into the Smart String formatter or <c>String.Format</c>.</param>
        /// <returns></returns>
        public string GetLocalizedString(IList<object> arguments) => GetLocalizedStringAsync(arguments).WaitForCompletion();

        protected internal override void ForceUpdate()
        {
            if (m_ChangeHandler != null)
            {
                HandleLocaleChange(null);
            }
        }

        void UpdateGlobalVariableListeners(List<IGlobalVariableValueChanged> variables)
        {
            if (m_OnGlobalVariableChanged == null)
                m_OnGlobalVariableChanged = OnGlobalVariableChanged;

            // Unsubscribe from any old ones
            foreach (var gv in m_LastUsedGlobalVariables)
            {
                gv.ValueChanged -= m_OnGlobalVariableChanged;
            }

            m_LastUsedGlobalVariables.Clear();
            if (variables == null)
                return;

            foreach (var gv in variables)
            {
                m_LastUsedGlobalVariables.Add(gv);
                gv.ValueChanged += m_OnGlobalVariableChanged;
            }
        }

        void OnGlobalVariableChanged(IGlobalVariable globalVariable)
        {
            if (m_WaitingForGlobalVariablesEndUpdate)
                return;

            if (GlobalVariablesSource.IsUpdating)
            {
                // Its possible that multiple global variables will be changed, we don't want to force the
                // string to be updated for each change so we defer and do a single update during EndUpdate.
                m_WaitingForGlobalVariablesEndUpdate = true;
                GlobalVariablesSource.EndUpdate += OnGlobalVariablesSourceUpdateCompleted;
            }
            else
            {
                RefreshString();
            }
        }

        void OnGlobalVariablesSourceUpdateCompleted()
        {
            GlobalVariablesSource.EndUpdate -= OnGlobalVariablesSourceUpdateCompleted;
            m_WaitingForGlobalVariablesEndUpdate = false;
            RefreshString();
        }

        void HandleLocaleChange(Locale _)
        {
            // Cancel any previous loading operations.
            ClearLoadingOperation();
            m_CurrentStringChangedValue = null;

            #if UNITY_EDITOR
            m_CurrentTable = TableReference;
            m_CurrentTableEntry = TableEntryReference;

            // Dont update if we have no selected Locale
            if (!LocalizationSettings.Instance.IsPlayingOrWillChangePlaymode && LocaleOverride == null && LocalizationSettings.SelectedLocale == null)
                return;
            #endif

            if (IsEmpty)
            {
                #if UNITY_EDITOR
                // If we are empty and playing or previewing then we should force an update.
                if (!LocalizationSettings.Instance.IsPlayingOrWillChangePlaymode)
                    m_ChangeHandler(null);
                #endif
                return;
            }

            CurrentLoadingOperation = LocalizationSettings.StringDatabase.GetTableEntryAsync(TableReference, TableEntryReference, LocaleOverride, FallbackState);

            AddressablesInterface.Acquire(CurrentLoadingOperation.Value);

            if (!CurrentLoadingOperation.Value.IsDone)
            {
                if (!WaitForCompletion)
                {
                    CurrentLoadingOperation.Value.Completed += AutomaticLoadingCompleted;
                    return;
                }

                CurrentLoadingOperation.Value.WaitForCompletion();
            }

            AutomaticLoadingCompleted(CurrentLoadingOperation.Value);
        }

        void AutomaticLoadingCompleted(AsyncOperationHandle<LocalizedStringDatabase.TableEntryResult> loadOperation)
        {
            if (loadOperation.Status != AsyncOperationStatus.Succeeded)
            {
                CurrentLoadingOperation = null;
                return;
            }
            RefreshString();
        }

        void ClearLoadingOperation()
        {
            if (CurrentLoadingOperation.HasValue)
            {
                if (CurrentLoadingOperation.Value.IsValid())
                {
                    // We should only call this if we are not done as its possible that the internal list is null if its not been used.
                    if (!CurrentLoadingOperation.Value.IsDone)
                        CurrentLoadingOperation.Value.Completed -= AutomaticLoadingCompleted;
                    AddressablesInterface.Release(CurrentLoadingOperation.Value);
                }

                CurrentLoadingOperation = null;
            }
        }

        protected override void Reset()
        {
            ClearLoadingOperation();
        }
    }
}
