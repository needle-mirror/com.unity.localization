namespace UnityEngine.Localization.Samples
{
    /// <summary>
    /// This example shows how we can create a Locale selector that uses the Player Prefs to keep track of the last used locale.
    /// Whenever the locale is changed, we record the new Locale and update the player pref
    /// </summary>
    public class PlayerPrefLocaleSelector : StartupLocaleSelector
    {
        public string playerPreferenceKey = "selected-locale";

        void OnEnable()
        {
            if(Application.isPlaying)
                LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
        }

        void OnDisable()
        {
            if (Application.isPlaying)
                LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
        }

        void OnSelectedLocaleChanged(Locale selectedLocale)
        {
            // Record the new selected locale so it can persist between runs
            PlayerPrefs.SetString(playerPreferenceKey, selectedLocale.Identifier.Code);
        }

        /// <summary>
        /// Returns the last locale set or null if no value has been recorded yet.
        /// </summary>
        /// <param name="availableLocales"></param>
        /// <returns></returns>
        public override Locale GetStartupLocale(LocalesProvider availableLocales)
        {
            if (PlayerPrefs.HasKey(playerPreferenceKey))
            {
                var code = PlayerPrefs.GetString(playerPreferenceKey);
                if (!string.IsNullOrEmpty(code))
                {
                    return availableLocales.GetLocale(code);
                }
            }

            // No locale could be found.
            return null;
        }
    }
}