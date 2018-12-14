using System.Collections.Generic;
using System.Globalization;
using UnityScript.Scripting;

namespace UnityEngine.Localization.Samples
{
    /// <summary>
    /// This example shows how a simple language selection menu can be implemented.
    /// </summary>
    public class LanguageSelectionMenuExample : MonoBehaviour
    {
        public Rect windowRect = new Rect(0, 0, 200, 300);
        public Color selectColor = Color.yellow;
        public Color defaultColor = Color.gray;
        Vector2 m_ScrollPos;

        [Tooltip("Use the current active settings if possible or create a new one for the example")]
        public bool useActiveLocalizationSettings = false;

        public class SimpleLocalesProvider : LocalesProvider
        {
            public override List<Locale> Locales { get; set; }

            public SimpleLocalesProvider()
            {
                Locales = new List<Locale>();
            }
        }

        void Start()
        {
            var localizationSettings = LocalizationSettings.GetInstanceDontCreateDefault();

            // Use included settings if one is available.
            if (useActiveLocalizationSettings && localizationSettings != null)
            {
                Debug.Log("Using included localization data");
                return;
            }

            Debug.Log("Creating default localization data");

            // Create our localization settings. If a LocalizationSettings asset has been created and configured in the editor then we can leave this step out.
            localizationSettings = LocalizationSettings.CreateDefault();

            // Replace the default Locales Provider with something we can manage locally for the example
            var simpleLocalesProvider = ScriptableObject.CreateInstance<SimpleLocalesProvider>();
            localizationSettings.SetAvailableLocales(simpleLocalesProvider);

            // Add the locales we support
            simpleLocalesProvider.AddLocale(Locale.CreateLocale(new LocaleIdentifier(SystemLanguage.Arabic)));
            simpleLocalesProvider.AddLocale(Locale.CreateLocale(new LocaleIdentifier(SystemLanguage.English)));
            simpleLocalesProvider.AddLocale(Locale.CreateLocale(new LocaleIdentifier(SystemLanguage.French)));
            simpleLocalesProvider.AddLocale(Locale.CreateLocale(new LocaleIdentifier(SystemLanguage.German)));
            simpleLocalesProvider.AddLocale(Locale.CreateLocale(new LocaleIdentifier(SystemLanguage.Japanese)));
            simpleLocalesProvider.AddLocale(Locale.CreateLocale(CultureInfo.InvariantCulture));

            // Set English to be our default
            localizationSettings.OnSelectedLocaleChanged += OnSelectedLocaleChanged;
            LocalizationSettings.Instance = localizationSettings;
        }

        static void OnSelectedLocaleChanged(Locale newLocale)
        {
            Debug.Log("OnSelectedLocaleChanged: The locale just changed to " + newLocale);
        }

        void OnGUI()
        {
            windowRect = GUI.Window(GetHashCode(), windowRect, DrawWindowContents, "Select Language");
        }

        void DrawWindowContents(int id)
        {
            // We need to wait for the localization system to initialize
            if (!LocalizationSettings.InitializationOperation.IsDone)
            {
                GUILayout.Label("Initializing Localization: " + LocalizationSettings.InitializationOperation.PercentComplete);
                GUI.DragWindow();
                return;
            }

            var supported = LocalizationSettings.AvailableLocales;
            if (supported.Locales.Count == 0)
            {
                GUILayout.Label("No Locales included in the active Localization Settings.");
            }
            else
            {
                var originalColor = GUI.contentColor;

                m_ScrollPos = GUILayout.BeginScrollView(m_ScrollPos);
                for (int i = 0; i < supported.Locales.Count; ++i)
                {
                    var locale = supported.Locales[i];

                    GUI.contentColor = LocalizationSettings.SelectedLocale == locale ? selectColor : defaultColor;

                    if (GUILayout.Button(locale.ToString()))
                    {
                        LocalizationSettings.SelectedLocale = locale;
                    }
                }
                GUILayout.EndScrollView();

                GUI.contentColor = originalColor;
            }

            GUI.DragWindow();
        }
    }
}