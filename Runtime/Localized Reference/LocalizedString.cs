using System;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization
{
    /// <summary>
    /// A Localized String contains a reference to a <see cref="StringTableEntry"/> inside of a specific <see cref="StringTable"/>.
    /// This provides a centralized way to work with localized strings.
    /// </summary>
    [Serializable]
    public partial class LocalizedString : LocalizedReference
    {
        ChangeHandler m_ChangeHandler;
        AsyncOperationHandle<LocalizedStringDatabase.TableEntryResult>? m_CurrentLoadingOperation;
        string m_CurrentStringChangedValue;

        /// <summary>
        /// Arguments that will be passed through to Smart Format. These arguments are not serialized and will need to be set during play mode.
        /// </summary>
        public object[] Arguments { get; set; }

        /// <summary>
        /// <inheritdoc cref="RegisterChangeHandler"/>
        /// </summary>
        /// <param name="value"></param>
        public delegate void ChangeHandler(string value);

        /// <summary>
        /// The current loading operation for the string when using <see cref="StringChanged"/>.
        /// A string may not be immediately available, such as when loading the <see cref="StringTable"/>, so all string operations are wrapped
        /// with an <see cref="AsyncOperationHandle"/>.
        /// See also <seealso cref="RefreshString"/>
        /// </summary>
        public AsyncOperationHandle<LocalizedStringDatabase.TableEntryResult>? CurrentLoadingOperation
        {
            get => m_CurrentLoadingOperation;
            internal set => m_CurrentLoadingOperation = value;
        }

        /// <summary>
        /// Called whenever a localized string is available.
        /// When the first <see cref="ChangeHandler"/> is added, a loading operation will automatically start and the localized string value will be sent to the event when completed.
        /// Any adding additional subscribers added after loading has completed will also be sent the latest localized string value when they are added.
        /// This ensures that a subscriber will always have the correct localized value regardless of when it was added.
        /// The string will be refreshed whenever <see cref="LocalizationSettings.SelectedLocaleChanged"/> is changed and when <see cref="RefreshString"/> is called.
        /// <seealso cref="GetLocalizedString"/> when not using the event.
        /// </summary>
        /// <example>
        /// This example shows how we can fetch and update a single string value.
        /// <code>
        /// public class LocalizedStringWithChangeHandlerExample : MonoBehaviour
        /// {
        ///     // A LocalizedString provides an interface to retrieving translated strings.
        ///     // This example assumes a String Table Collection with the name "My String Table" and an entry with the Key "Hello World" exists.
        ///     // You can change the Table Collection and Entry target in the inspector.
        ///     public LocalizedString stringRef = new LocalizedString() { TableReference = "My String Table", TableEntryReference = "Hello World" };
        ///     string m_TranslatedString;
        ///
        ///     void OnEnable()
        ///     {
        ///         stringRef.StringChanged += UpdateString;
        ///     }
        ///
        ///     void OnDisable()
        ///     {
        ///         stringRef.StringChanged -= UpdateString;
        ///     }
        ///
        ///     void UpdateString(string translatedValue)
        ///     {
        ///         m_TranslatedString = translatedValue;
        ///         Debug.Log("Translated Value Updated: " + translatedValue);
        ///     }
        ///
        ///     void OnGUI()
        ///     {
        ///         GUILayout.Label(m_TranslatedString);
        ///     }
        /// }
        /// </code>
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
                    LocalizationSettings.SelectedLocaleChanged += HandleLocaleChange;
                    ForceUpdate();
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
        /// Forces a refresh of the string when using <see cref="StringChanged"/>.
        /// Note, this will only only force the refresh if there is currently no loading operation, if one is still being executed then it will be ignored and false will be returned.
        /// If a string is not static and will change during game play, such as when using format arguments, then this can be used to force the string to update itself.
        /// </summary>
        /// <returns>True if a refresh was requested or false if it could not.</returns>
        public bool RefreshString()
        {
            if (m_ChangeHandler == null)
                throw new Exception($"{nameof(RefreshString)} should be used with {nameof(StringChanged)} however no change handler has been registered.");

            if (m_CurrentLoadingOperation == null || !m_CurrentLoadingOperation.Value.IsDone)
                return false;

            string translatedText;
            if (m_CurrentLoadingOperation.Value.Result.Entry != null)
            {
                var entryResult = LocalizationSettings.StringDatabase.GetLocalizedStringProcessTableEntry(m_CurrentLoadingOperation.Value, TableEntryReference, LocalizationSettings.SelectedLocale, Arguments);
                translatedText = entryResult.Result;
            }
            else
            {
                var table = m_CurrentLoadingOperation.Value.Result.Table;
                translatedText = LocalizationSettings.StringDatabase?.ProcessUntranslatedText(TableEntryReference.ResolveKeyName(table?.SharedData));
            }

            m_CurrentStringChangedValue = translatedText;
            m_ChangeHandler(m_CurrentStringChangedValue);
            return true;
        }

        /// <summary>
        /// This function will load the requested string table and return the translated string.
        /// The Completed event will provide notification once the operation has finished and the string has been
        /// found or an error has occurred, this will be called during LateUpdate.
        /// It is possible that a string table may have already been loaded, such as during a previous operation
        /// or when using Preload mode, the IsDone property can be checked as it is possible the translated
        /// string is immediately available.
        /// </summary>
        /// <returns></returns>
        /// <example>
        /// <code>
        /// // A LocalizedString provides an interface to retrieving translated strings.
        /// // This example assumes a String Table Collection with the name "My String Table" and an entry with the Key "Hello World" exists.
        /// // You can change the Table Collection and Entry target in the inspector.
        /// public LocalizedString stringRef = new LocalizedString() { TableReference = "My String Table", TableEntryReference = "Hello World" };
        ///
        /// void OnGUI()
        /// {
        ///     // This will make a request to the StringDatabase each time using the LocalizedString properties.
        ///     var stringOperation = stringRef.GetLocalizedString();
        ///     if (stringOperation.IsDone && stringOperation.Status == AsyncOperationStatus.Succeeded)
        ///         GUILayout.Label(stringOperation.Result);
        /// }
        /// </code>
        /// </example>
        public AsyncOperationHandle<string> GetLocalizedString()
        {
            LocalizationSettings.ValidateSettingsExist();
            return LocalizationSettings.StringDatabase.GetLocalizedStringAsync(TableReference, TableEntryReference, Arguments);
        }

        /// <summary>
        /// Loads the requested string table and return the translated string after being formatted using the provided arguments.
        /// The Completed event will provide notification once the operation has finished and the string has been
        /// found or an error has occurred, this will be called during LateUpdate.
        /// It is possible that a string table may have already been loaded, such as during a previous operation
        /// or when using Preload mode, the IsDone property can be checked as it is possible the translated
        /// string is immediately available.
        /// </summary>
        /// <param name="arguments">Arguments that will be passed to Smart Format or String.Format.</param>
        /// <returns></returns>
        public AsyncOperationHandle<string> GetLocalizedString(params object[] arguments)
        {
            LocalizationSettings.ValidateSettingsExist();
            return LocalizationSettings.StringDatabase.GetLocalizedStringAsync(TableReference, TableEntryReference, arguments);
        }

        protected override void ForceUpdate()
        {
            if (m_ChangeHandler != null)
            {
                HandleLocaleChange(null);
            }
        }

        void HandleLocaleChange(Locale _)
        {
            m_CurrentStringChangedValue = null;

            // Cancel any previous loading operations.
            ClearLoadingOperation();

            // Don't try and load empty references.
            if (IsEmpty)
                return;

            CurrentLoadingOperation = LocalizationSettings.StringDatabase.GetTableEntryAsync(TableReference, TableEntryReference);
            if (CurrentLoadingOperation.Value.IsDone)
                AutomaticLoadingCompleted(CurrentLoadingOperation.Value);
            else
                CurrentLoadingOperation.Value.Completed += AutomaticLoadingCompleted;
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
                // We should only call this if we are not done as its possible that the internal list is null if its not been used.
                if (!CurrentLoadingOperation.Value.IsDone)
                    CurrentLoadingOperation.Value.Completed -= AutomaticLoadingCompleted;
                CurrentLoadingOperation = null;
            }
        }
    }
}
