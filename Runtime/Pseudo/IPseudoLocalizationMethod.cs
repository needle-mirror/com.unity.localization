namespace UnityEngine.Localization.Pseudo
{
    /// <summary>
    /// Provides a Pseudo-Localization transformation method as used by <see cref="PseudoLocale"/>.
    /// </summary>
    public interface IPseudoLocalizationMethod
    {
        /// <summary>
        /// Apply a Pseudo-Localization transformation to the string and return the Pseudo-Localized string.
        /// </summary>
        /// <param name="input">The string to be transformed.</param>
        /// <returns></returns>
        string Transform(string input);
    }
}
