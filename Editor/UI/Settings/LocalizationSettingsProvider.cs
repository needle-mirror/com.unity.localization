using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

namespace UnityEditor.Localization.UI
{
    class LocalizationSettingsProvider : AssetSettingsProvider
    {
        class Texts
        {
            public GUIContent noSettingsMsg = new GUIContent("You have no active Localization Settings. Would you like to create one?");
            public GUIContent activeSettings = new GUIContent("Active Settings", "The Localization Settings that will be used by this project and included into any builds.");
        }
        static Texts s_Texts;

        string m_SearchContext;
        VisualElement m_RootElement;

        public LocalizationSettingsProvider()
            : base("Project/Localization", () => LocalizationEditorSettings.ActiveLocalizationSettings)
        {
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            m_SearchContext = searchContext;
            m_RootElement = rootElement;
            if (s_Texts == null)
                s_Texts = new Texts();
            base.OnActivate(searchContext, rootElement);
        }

        public override void OnGUI(string searchContext)
        {
            if (LocalizationEditorSettings.ActiveLocalizationSettings == null)
            {
                EditorGUI.BeginChangeCheck();
                var obj = EditorGUILayout.ObjectField(s_Texts.activeSettings, LocalizationEditorSettings.ActiveLocalizationSettings, typeof(LocalizationSettings), false) as LocalizationSettings;
                if (EditorGUI.EndChangeCheck())
                {
                    LocalizationEditorSettings.ActiveLocalizationSettings = obj;

                    // Trigger the activate so it generates a new Editor
                    base.OnActivate(m_SearchContext, m_RootElement);
                }

                EditorGUILayout.HelpBox(s_Texts.noSettingsMsg.text, MessageType.Info, true);
                if (GUILayout.Button("Create", GUILayout.Width(100)))
                {
                    var created = LocalizationSettingsMenuItems.CreateLocalizationAsset();
                    if (created != null)
                    {
                        LocalizationEditorSettings.ActiveLocalizationSettings = created;

                        // Trigger the activate so it generates a new Editor
                        base.OnActivate(m_SearchContext, m_RootElement);
                    }
                }
            }
            else
            {
                base.OnGUI(searchContext);
            }
        }

        [SettingsProvider]
        static SettingsProvider CreateProjectSettingsProvider() => new LocalizationSettingsProvider();
    }
}
