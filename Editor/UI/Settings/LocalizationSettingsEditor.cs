using UnityEngine;
using UnityEngine.Localization.Settings;

namespace UnityEditor.Localization.UI
{
    [CustomEditor(typeof(LocalizationSettings))]
    class LocalizationSettingsEditor : UnityEditor.Editor
    {
        // Player
        SerializedProperty m_StartupSelectors;
        SerializedProperty m_AvailableLocales;
        SerializedProperty m_AssetDatabase;
        SerializedProperty m_StringDatabase;

        // Editor Only
        SerializedProperty m_ShowLocaleMenuInGameView;

        ReorderableListExtended m_StartupSelectorsList;

        class Styles
        {
            public static readonly GUIContent activeSettings = new GUIContent("Active Settings", "The Localization Settings that will be used by this project and included into any builds.");
            public static readonly GUIContent editor = new GUIContent("Editor");
            public static readonly GUIContent helpTextNotActive = new GUIContent("This is not the active localization settings and will not be used when localizing the application. Would you like to make it active?");
            public static readonly GUIContent localeSelectors = new GUIContent("Locale Selectors", "Determines what locale should be used when the application starts or does not currently have an active locale and needs one.");
            public static readonly GUIContent makeActive = new GUIContent("Set Active");
            public static readonly GUIContent player = new GUIContent("Player");
            public static readonly GUIContent showGameViewToolbar = new GUIContent("Locale GameView Menu", "Show a menu in the GameView for changing the selected locale.");
        }

        void OnEnable()
        {
            m_StartupSelectors = serializedObject.FindProperty("m_StartupSelectors");
            m_AvailableLocales = serializedObject.FindProperty("m_AvailableLocales");
            m_AssetDatabase = serializedObject.FindProperty("m_AssetDatabase");
            m_StringDatabase = serializedObject.FindProperty("m_StringDatabase");
            m_ShowLocaleMenuInGameView = serializedObject.FindProperty("m_ShowLocaleMenuInGameView");

            m_StartupSelectorsList = new ReorderableListExtended(serializedObject, m_StartupSelectors);
            m_StartupSelectorsList.Header = Styles.localeSelectors;
            m_StartupSelectorsList.AddMenuType = typeof(IStartupLocaleSelector);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawActiveSettings();

            // Player
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField(Styles.player, EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_AvailableLocales, true);

            m_StartupSelectorsList.DoLayoutList();

            EditorGUILayout.PropertyField(m_AssetDatabase, true);
            EditorGUILayout.PropertyField(m_StringDatabase, true);

            EditorGUILayout.EndVertical();

            // Editor
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(Styles.editor, EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_ShowLocaleMenuInGameView, Styles.showGameViewToolbar, true);
            if (EditorGUI.EndChangeCheck() && Application.isPlaying)
            {
                if (m_ShowLocaleMenuInGameView.boolValue)
                    GameViewLanguageMenu.Show();
                else
                    GameViewLanguageMenu.Hide();
            }

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawActiveSettings()
        {
            if (LocalizationEditorSettings.ActiveLocalizationSettings != target)
            {
                EditorGUI.BeginChangeCheck();
                var obj = EditorGUILayout.ObjectField(Styles.activeSettings, LocalizationEditorSettings.ActiveLocalizationSettings, typeof(LocalizationSettings), false) as LocalizationSettings;
                if (EditorGUI.EndChangeCheck())
                    LocalizationEditorSettings.ActiveLocalizationSettings = obj;

                EditorGUILayout.HelpBox(Styles.helpTextNotActive.text, MessageType.Warning, true);
                if (GUILayout.Button(Styles.makeActive, GUILayout.Width(150)))
                    LocalizationEditorSettings.ActiveLocalizationSettings = (LocalizationSettings)target;
                EditorGUILayout.Space();
            }
        }
    }
}
