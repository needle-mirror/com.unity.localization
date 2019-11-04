using System;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization
{
    /// <summary>
    /// A Localized String contains a reference to a <see cref="StringTableEntry"/> inside of a specific <see cref="StringTable"/>.
    /// This provides a centralized way to work with localized strings.
    /// </summary>
    [Serializable]
    public class LocalizedString : LocalizedReference
    {
        ChangeHandler m_ChangeHandler;

        AsyncOperationHandle<LocalizedStringDatabase.TableEntryResult>? m_CurrentLoadingOperation;
        LocalizedTable m_Table;
        StringTableEntry m_Entry;

        /// <summary>
        /// Arguments that will be passed through to Smart Format. These arguments are not serialized and will need to be set during play mode.
        /// </summary>
        public SmartObjects Arguments { get; set; } = new SmartObjects();

        /// <summary>
        /// <inheritdoc cref="RegisterChangeHandler"/>
        /// </summary>
        /// <param name="value"></param>
        public delegate void ChangeHandler(string value);

        /// <summary>
        /// The current loading operation for the string. A string may not be immediately available,
        /// such as when loading the <see cref="StringTable"/>, so all string operations are wrapped
        /// with an <see cref="AsyncOperationHandle"/>.
        /// See also <seealso cref="RefreshString"/>
        /// </summary>
        public AsyncOperationHandle<LocalizedStringDatabase.TableEntryResult>? CurrentLoadingOperation
        {
            get => m_CurrentLoadingOperation;
            private set => m_CurrentLoadingOperation = value;
        }

        /// <summary>
        /// Register a handler that will be called whenever a localized string is available.
        /// When a handler is registered, the string will then be automatically loaded whenever the
        /// <see cref="LocalizationSettings.SelectedLocaleChanged"/> is changed, during initialization and if
        /// <see cref="RefreshString"/> is called.
        /// <seealso cref="LoadAssetAsync"/> when not using a change handler.
        /// </summary>
        /// <param name="handler"></param>
        public void RegisterChangeHandler(ChangeHandler handler)
        {
            LocalizationSettings.ValidateSettingsExist();
            m_ChangeHandler = handler ?? throw new ArgumentNullException(nameof(handler), "Handler must not be null");
            LocalizationSettings.SelectedLocaleChanged += HandleLocaleChange;

            ForceUpdate();
        }

        /// <summary>
        /// Removes the handler and stops listening to changes to <see cref="LocalizationSettings.SelectedLocaleChanged"/>.
        /// </summary>
        public void ClearChangeHandler()
        {
            LocalizationSettings.ValidateSettingsExist();
            LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChange;
            m_ChangeHandler = null;
        }
        
        /// <summary>
        /// Forces a refresh of the string when using a <see cref="ChangeHandler"/>.
        /// Note, this will only only force the refresh if there is currently no loading operation, if one is still being executed then it will be ignored and false will be returned.
        /// If a string is not static and will change during game play, such as when using format arguments, then this can be used to force the string update itself.
        /// </summary>
        /// <returns>True if a refresh was requested or false if it could not.</returns>
        public bool RefreshString()
        {
            if (m_ChangeHandler == null)
                throw new Exception($"{nameof(RefreshString)} should be used with {nameof(RegisterChangeHandler)} however no change handler has been registered.");

            if (m_CurrentLoadingOperation == null || !m_CurrentLoadingOperation.Value.IsDone)
                return false;

            string translatedText;
            if (m_Entry != null)
            {
                translatedText = Arguments.Count == 0 ? m_Entry.GetLocalizedString() : m_Entry.GetLocalizedString(Arguments);
            }
            else
            {
                translatedText = LocalizationSettings.StringDatabase?.ProcessUntranslatedText(TableEntryReference.ResolveKeyName(m_Table?.Keys));
            }
            m_ChangeHandler(translatedText);
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
        public AsyncOperationHandle<string> GetLocalizedString()
        {
            LocalizationSettings.ValidateSettingsExist();
            return LocalizationSettings.StringDatabase.GetLocalizedStringAsync(TableReference, TableEntryReference);
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
        
        void ForceUpdate()
        {
            HandleLocaleChange(null);
        }

        void HandleLocaleChange(Locale _)
        {
            // Cancel any previous loading operations.
            if (CurrentLoadingOperation != null && !CurrentLoadingOperation.Value.IsDone)
            {
                CurrentLoadingOperation.Value.Completed -= AutomaticLoadingCompleted;
            }
            m_Entry = null;
            m_Table = null;

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

            m_Entry = loadOperation.Result.Entry;
            m_Table = loadOperation.Result.Table;
            RefreshString();
        }
    }
}