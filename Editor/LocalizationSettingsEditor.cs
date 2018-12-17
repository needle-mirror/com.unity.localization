using UnityEngine;
using UnityEngine.Localization;

namespace UnityEditor.Localization
{
    [CustomEditor(typeof(LocalizationSettings))]
    class LocalizationSettingsEditor : Editor
    {
        SerializedProperty m_PreloadBehavior;
        SerializedProperty m_LocaleSelector;
        SerializedProperty m_AvailableLocales;
        SerializedProperty m_AssetDatabase;
        SerializedProperty m_StringDatabase;

        Editor m_LocaleSelectorEditor;
        Editor m_AvailableLocalesEditor;
        Editor m_AssetDatabaseEditor;
        Editor m_StringDatabaseEditor;

        class Texts
        {
            public GUIContent activeSettings = new GUIContent("Active Settings", "The Localization Settings that will be used by this project and included into any builds.");
            public GUIContent assetDatabase = new GUIContent("Asset Database");
            public GUIContent availableLocales = new GUIContent("Available Locales", "Controls what locales are supported by this application.");
            public GUIContent helpTextNotActive = new GUIContent("This is not the active localization settings and will not be used when localizing the application. Would you like to make it active?");
            public GUIContent helpTextActive = new GUIContent("This is the active localization settings and will be automatically included and loaded in any builds.");
            public GUIContent localeSelector = new GUIContent("Locale Selector", "Determines what locale should be used when the application starts or does not currently have an active locale and needs one.");
            public GUIContent makeActive = new GUIContent("Set Active");
            public GUIContent preloadBehavior = new GUIContent("Preload Behavior", "Should all localization data be preloaded before use or only when requested?");
            public GUIContent stringDatabase = new GUIContent("String Database", "Handles loading string tables for selected language.");
        }
        static Texts s_Texts;

        #if !UNITY_2018_3_OR_NEWER
        [MenuItem("Edit/Project Settings/Localization")]
        static void ShowActive()
        {
            if (LocalizationPlayerSettings.ActiveLocalizationSettings == null)
            {
                if (EditorUtility.DisplayDialog("Create Localization Settings", "You have no active Localization Settings. Would you like to create one?", "Create", "Cancel"))
                    LocalizationPlayerSettings.ActiveLocalizationSettings = LocalizationSettingsMenuItems.CreateLocalizationAsset();
            }
            else
            {
                Selection.activeObject = LocalizationPlayerSettings.ActiveLocalizationSettings;
            }
        }
        #endif

        void OnEnable()
        {
            if (s_Texts == null)
                s_Texts = new Texts();

            m_PreloadBehavior = serializedObject.FindProperty("m_PreloadBehavior");
            m_LocaleSelector = serializedObject.FindProperty("m_LocaleSelector");
            m_AvailableLocales = serializedObject.FindProperty("m_AvailableLocales");
            m_AssetDatabase = serializedObject.FindProperty("m_AssetDatabase");
            m_StringDatabase = serializedObject.FindProperty("m_StringDatabase");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawActiveSettings();
            EditorGUILayout.PropertyField(m_PreloadBehavior, s_Texts.preloadBehavior);
            ObjectReferenceWithEditorView(m_AvailableLocales, s_Texts.availableLocales, ref m_AvailableLocalesEditor);
            ObjectReferenceWithEditorView(m_LocaleSelector, s_Texts.localeSelector, ref m_LocaleSelectorEditor);
            ObjectReferenceWithEditorView(m_AssetDatabase, s_Texts.assetDatabase, ref m_AssetDatabaseEditor);
            ObjectReferenceWithEditorView(m_StringDatabase, s_Texts.stringDatabase, ref m_StringDatabaseEditor);
            serializedObject.ApplyModifiedProperties();
        }

        void DrawActiveSettings()
        {
            EditorGUI.BeginChangeCheck();
            var obj = EditorGUILayout.ObjectField(s_Texts.activeSettings, LocalizationPlayerSettings.ActiveLocalizationSettings, typeof(LocalizationSettings), false) as LocalizationSettings; 
            if (EditorGUI.EndChangeCheck())
                LocalizationPlayerSettings.ActiveLocalizationSettings = obj;

            if (LocalizationPlayerSettings.ActiveLocalizationSettings != target)
            {
                EditorGUILayout.HelpBox(s_Texts.helpTextNotActive.text, MessageType.Warning, true);
                if (GUILayout.Button(s_Texts.makeActive, GUILayout.Width(150)))
                    LocalizationPlayerSettings.ActiveLocalizationSettings = (LocalizationSettings)target;
            }
            else
            {
                EditorGUILayout.HelpBox(s_Texts.helpTextActive.text, MessageType.Info, true);
            }
            EditorGUILayout.Space();
        }

        static void ObjectReferenceWithEditorView(SerializedProperty property, GUIContent label, ref Editor editor)
        {
            EditorGUILayout.BeginHorizontal();
            if(property.objectReferenceValue != null)
                property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label);
            else
                GUILayout.Label(label);
            EditorGUILayout.PropertyField(property, GUIContent.none);
            EditorGUILayout.EndHorizontal();

            if (property.isExpanded)
            {
                if (editor == null || editor.target != property.objectReferenceValue)
                {
                    CreateCachedEditor(property.objectReferenceValue, null, ref editor);
                }
                if(editor != null)
                    editor.OnInspectorGUI();
            }
            EditorGUILayout.Space();
        }
    }
}
