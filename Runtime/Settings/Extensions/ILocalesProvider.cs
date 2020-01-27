using System.Collections.Generic;

namespace UnityEngine.Localization.Settings
{
    /// <summary>
    /// Holds the Locale's that the project supports.
    /// </summary>
    public interface ILocalesProvider
    {
        List<Locale> Locales { get; set; }

        /// <summary>
        /// Attempt to retrieve a Locale using the identifier.
        /// </summary>
        /// <param name="id"><see cref="LocaleIdentifier"/> to find.</param>
        /// <returns>If no Locale can be found then null is returned.</returns>
        Locale GetLocale(LocaleIdentifier id);

        /// <summary>
        /// Attempt to retrieve a Locale using a Code.
        /// </summary>
        /// <param name="code">If no Locale can be found then null is returned.</param>
        Locale GetLocale(string code);

        /// <summary>
        /// Attempt to retrieve a Locale using a <see cref="SystemLanguage"/>.
        /// </summary>
        /// <returns>If no Locale can be found then null is returned.</returns>
        Locale GetLocale(SystemLanguage systemLanguage);

        /// <summary>
        /// Add a Locale to allow support for a specific language.
        /// </summary>
        void AddLocale(Locale locale);

        /// <summary>
        /// Removes support for a specific Locale.
        /// </summary>
        /// <param name="locale">The locale that should be removed if possible.</param>
        /// <returns>true if the locale was removed or false if the locale did not exist.</returns>
        bool RemoveLocale(Locale locale);
    }
}
