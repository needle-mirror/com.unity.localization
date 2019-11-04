using System;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization.Settings
{
    /// <summary>
    /// Responsible for providing the list of locales that are currently available to this application.
    /// </summary>
    [Serializable]
    public class LocalesProvider : ILocalesProvider, IPreloadRequired
    {
        // There is a bug with SerializeField that causes empty instances to not deserialize. This is a workaround while we wait for the fix (case 1183543)
        [SerializeField, HideInInspector]
        int dummyObject;
        
        List<Locale> m_Locales;
        AsyncOperationHandle? m_LoadOperation;

        /// <summary>
        /// The list of all supported locales.
        /// </summary>
        public List<Locale> Locales
        {
            get
            {
                if (m_LoadOperation == null)
                    Debug.LogError("Locales PreloadOperation has not been initialized, can not return the available locales.");
                return m_Locales;
            }
            set
            {
                m_Locales = value;
            }
        }

        /// <summary>
        /// The Locales loading operation. When set to isDone then all locales have been loaded. Can be Null if the operation has not started yet.
        /// </summary>
        public AsyncOperationHandle PreloadOperation
        {
            get
            {
                if (m_LoadOperation == null)
                {
                    Locales = new List<Locale>();
                    m_LoadOperation = AddressableAssets.Addressables.LoadAssetsAsync<Locale>(LocalizationSettings.LocaleLabel, AddLocale);
                }

                return m_LoadOperation.Value;
            }
        }

        /// <summary>
        /// Attempt to retrieve a Locale using the identifier.
        /// </summary>
        /// <param name="id"><see cref="LocaleIdentifier"/> to find.</param>
        /// <returns>If no Locale can be found then null is returned.</returns>
        public Locale GetLocale(LocaleIdentifier id)
        {
            foreach (var locale in Locales)
            {
                if (locale.Identifier.Equals(id))
                    return locale;
            }
            return null;
        }

        /// <summary>
        /// Attempt to retrieve a Locale using a Code.
        /// </summary>
        /// <param name="code">If no Locale can be found then null is returned.</param>
        /// <returns></returns>
        public Locale GetLocale(string code)
        {
            foreach (var locale in Locales)
            {
                if (locale.Identifier.Code == code)
                    return locale;
            }
            return null;
        }

        /// <summary>
        /// Attempt to retrieve a Locale using a <see cref="UnityEngine.SystemLanguage"/>.
        /// </summary>
        /// <param name="systemLanguage"></param>
        /// <returns>If no Locale can be found then null is returned.</returns>
        public Locale GetLocale(SystemLanguage systemLanguage)
        {
            return GetLocale(SystemLanguageConverter.GetSystemLanguageCultureCode(systemLanguage));
        }

        /// <summary>
        /// Add a Locale to allow support for a specific language.
        /// </summary>
        /// <param name="locale"></param>
        public void AddLocale(Locale locale)
        {
            if (GetLocale(locale.Identifier) != null)
            {
                Debug.LogWarning("Ignoring locale. A locale with the same Id has already been added: " + locale.Identifier);
                return;
            }
            Locales.Add(locale);
        }

        /// <summary>
        /// Removes support for a specific Locale.
        /// </summary>
        /// <param name="locale">The locale that should be removed if possible.</param>
        /// <returns>true if the locale was removed or false if the locale did not exist.</returns>
        public bool RemoveLocale(Locale locale)
        {
            bool ret = Locales.Remove(locale);
            var settings = LocalizationSettings.GetInstanceDontCreateDefault();
            settings?.OnLocaleRemoved(locale);
            return ret;
        }
    }
}