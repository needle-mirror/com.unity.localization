namespace UnityEngine.Localization.Settings
{
    /// <summary>
    /// Determines what locale should be used when the application starts or does not currently have an active locale.
    /// </summary>
    public interface IStartupLocaleSelector
    {
        /// <summary>
        /// Determine which <see cref="Locale"/> should be used.
        /// </summary>
        /// <param name="availableLocales">The available locales to select from.</param>
        /// <returns>The selected locale or null.</returns>
        Locale GetStartupLocale(ILocalesProvider availableLocales);
    }
}
