using System;
using System.Collections.Generic;
using UnityEngine.Localization.Pseudo;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization.Settings
{
    /// <summary>
    /// Responsible for providing the list of locales that are currently available to this application.
    /// </summary>
    [Serializable]
    public class LocalesProvider : ILocalesProvider, IPreloadRequired, IReset
    {
        List<Locale> m_Locales = new List<Locale>();
        AsyncOperationHandle? m_LoadOperation;

        /// <summary>
        /// The list of all supported locales.
        /// </summary>
        public List<Locale> Locales
        {
            get
            {
                if (LocalizationSettings.Instance.IsPlayingOrWillChangePlaymode && m_LoadOperation == null)
                    Debug.LogError("Locales PreloadOperation has not been initialized, can not return the available locales.");
                return m_Locales;
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
                    m_Locales.Clear();
                    m_LoadOperation = AddressablesInterface.LoadAssetsWithLabel<Locale>(LocalizationSettings.LocaleLabel, AddLocale);
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
                if (locale != null && locale.Identifier.Equals(id))
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
                if (locale == null)
                    continue;

                // Ignore PseudoLocale's
                if (locale is PseudoLocale)
                    continue;

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
            if (locale == null)
                return;

            var isPseudoLocale = locale is PseudoLocale;
            if (!isPseudoLocale)
            {
                var foundLocale = GetLocale(locale.Identifier);
                if (foundLocale != null && !(foundLocale is PseudoLocale))
                {
                    Debug.LogWarning("Ignoring locale. A locale with the same Id has already been added: " + locale.Identifier);
                    return;
                }
            }

            var index = m_Locales.BinarySearch(locale);
            m_Locales.Insert(~index, locale);
        }

        /// <summary>
        /// Removes support for a specific Locale.
        /// </summary>
        /// <param name="locale">The locale that should be removed if possible.</param>
        /// <returns>true if the locale was removed or false if the locale did not exist.</returns>
        public bool RemoveLocale(Locale locale)
        {
            if (locale == null)
                return false;

            var ret = Locales.Remove(locale);
            var settings = LocalizationSettings.GetInstanceDontCreateDefault();
            settings?.OnLocaleRemoved(locale);
            return ret;
        }

        /// <summary>
        /// Resets the state of the provider by removing all the Locales and clearing the preload operation.
        /// </summary>
        public void ResetState()
        {
            m_Locales.Clear();
            m_LoadOperation = null;
        }
    }
}
