using System.Collections.Generic;

namespace UnityEngine.Localization
{
    /// <summary>
    /// Responsible for providing the list of locales that are currently available to this application.
    /// </summary>
    public abstract class LocalesProvider : ScriptableObject
    {
        /// <summary>
        /// The list of all supported locales.
        /// </summary>
        public abstract List<Locale> Locales { get; set; }

        /// <summary>
        /// Attempt to retrieve a Locale using the identifier.
        /// </summary>
        /// <param name="id"><see cref="LocaleIdentifier"/> to find.</param>
        /// <returns>If no Locale can be found then null is returned.</returns>
        public virtual Locale GetLocale(LocaleIdentifier id)
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
        public virtual Locale GetLocale(string code)
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
        /// <returns>If no Locale can be found then null is returned.</returns>
        public Locale GetLocale(SystemLanguage systemLanguage)
        {
            return GetLocale(SystemLanguageConverter.GetSystemLanguageCultureCode(systemLanguage));
        }

        /// <summary>
        /// Add a Locale to allow support for a specific language.
        /// </summary>
        public virtual void AddLocale(Locale locale)
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
        public virtual bool RemoveLocale(Locale locale)
        {
            bool ret = Locales.Remove(locale);
            var settings = LocalizationSettings.GetInstanceDontCreateDefault();
            if(settings != null)
                settings.OnLocaleRemoved(locale);
            return ret;
        }
    }
}