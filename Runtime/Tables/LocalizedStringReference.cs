using System;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization
{
    [Serializable]
    public class LocalizedStringReference : LocalizedReference
    {
        [Serializable]
        public class LocalizationUnityEvent : UnityEvent<string> { };

        /// <summary>
        /// TODO: DOC
        /// </summary>
        public bool IsPlural
        {
            get => m_IsPlural;
            set
            {
                if (m_IsPlural == value)
                    return;

                m_IsPlural = value;
                AutomaticUpdate();
            }
        }

        /// <summary>
        /// TODO: DOC
        /// </summary>
        public int PluralValue
        {
            get => m_PluralValue;
            set
            {
                if (m_PluralValue == value)
                    return;

                m_PluralValue = value;
                AutomaticUpdate();
            }
        }

        public override string TableName
        {
            get => base.TableName;
            set
            {
                if (base.TableName == value)
                    return;

                base.TableName = value;
                AutomaticUpdate();
            }
        }

        public override uint KeyId
        {
            get => base.KeyId;
            set
            {
                if (base.KeyId == value)
                    return;

                base.KeyId = value;
                AutomaticUpdate();
            }
        }

        public override string Key
        {
            get => base.Key;
            set
            {
                if (base.Key == value)
                    return;

                base.Key = value;
                AutomaticUpdate();
            }
        }

        /// <summary>
        /// TODO: DOC
        /// </summary>
        public LocalizationUnityEvent UpdateString
        {
            get => m_UpdateString;
            set => m_UpdateString = value;
        }

        /// <summary>
        /// Whenever a change is detected(locale, table name or key) the localized value will automatically be loaded. 
        /// This should be enabled in the OnEnabled/Awake or Start method.
        /// </summary>
        public bool AutoUpdate
        {
            get => m_AutoUpdate;
            set
            {
                if (m_AutoUpdate == value)
                    return;

                Debug.Assert(Application.isPlaying, $"Can not set {nameof(AutoUpdate)} while application is not playing");

                m_AutoUpdate = value;

                if (!LocalizationSettings.HasSettings)
                {
                    Debug.LogWarning("Can not use Automatic Updates, no Localization Settings exist.");
                    return;
                }

                if (m_AutoUpdate)
                {
                    LocalizationSettings.SelectedLocaleChanged += AutomaticUpdate;
                    if (LocalizationSettings.InitializationOperation.Value.IsDone)
                        AutomaticUpdate();
                }
                else
                {
                    LocalizationSettings.SelectedLocaleChanged -= AutomaticUpdate;
                }
            }
        }

        [SerializeField]
        LocalizationUnityEvent m_UpdateString = new LocalizationUnityEvent();

        [SerializeField]
        bool m_IsPlural;

        [SerializeField]
        int m_PluralValue;

        AsyncOperationHandle<string>? m_CurrentLoadingOperation;
        bool m_AutoUpdate;

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
            return KeyId == KeyDatabase.EmptyId ? LocalizationSettings.StringDatabase.GetLocalizedString(TableName, Key) : LocalizationSettings.StringDatabase.GetLocalizedString(TableName, KeyId);
        }

        /// <summary>
        /// This function will load the requested string table and return the translated string formatted using the Locale PluralForm.
        /// The Completed event will provide notification once the operation has finished and the string has been
        /// found or an error has occurred, this will be called during LateUpdate.
        /// It is possible that a string table may have already been loaded, such as during a previous operation
        /// or when using Preload mode, the IsDone property can be checked as it is possible the translated
        /// string is immediately available.
        /// </summary>
        /// <returns></returns>
        /// <param name="plural">A plural value to be used when translating the string.</param>
        public AsyncOperationHandle<string> GetLocalizedString(int plural)
        {
            return KeyId == KeyDatabase.EmptyId ? LocalizationSettings.StringDatabase.GetLocalizedString(TableName, Key, plural) : LocalizationSettings.StringDatabase.GetLocalizedString(TableName, KeyId, plural);
        }

        /// <summary>
        /// This function will load the requested string table. This is useful when multiple strings are required.
        /// The Completed event will provide notification once the operation has finished and the string table has been
        /// found or an error has occurred, this will be called during LateUpdate.
        /// It is possible that a string table may have already been loaded, such as during a previous operation
        /// or when using Preload mode, the IsDone property can be checked as it is possible the translated
        /// string is immediately available.
        /// </summary>
        /// <returns></returns>
        public AsyncOperationHandle<StringTableBase> GetLocalizedStringTable() => LocalizationSettings.StringDatabase.GetTable(TableName);

        void AutomaticUpdate(Locale obj = null)
        {
            if (!AutoUpdate)
                return;

            // Cancel any previous loading operations.
            if (m_CurrentLoadingOperation != null)
            {
                m_CurrentLoadingOperation.Value.Completed -= AutomaticLoadingCompleted;
            }

            m_CurrentLoadingOperation = IsPlural ? GetLocalizedString(PluralValue) : GetLocalizedString();
            if (m_CurrentLoadingOperation.Value.IsDone)
                AutomaticLoadingCompleted(m_CurrentLoadingOperation.Value);
            else
                m_CurrentLoadingOperation.Value.Completed += AutomaticLoadingCompleted;
        }

        void AutomaticLoadingCompleted(AsyncOperationHandle<string> stringOperation)
        {
            if (stringOperation.Status != AsyncOperationStatus.Succeeded)
            {
                var error = "Failed to load string: " + ToString();
                if (stringOperation.OperationException != null)
                    error += "\n" + stringOperation.OperationException;

                Debug.LogError(error);
                m_CurrentLoadingOperation = null;
                return;
            }

            m_CurrentLoadingOperation = null;
            m_UpdateString.Invoke(stringOperation.Result);            
        }
    }
}