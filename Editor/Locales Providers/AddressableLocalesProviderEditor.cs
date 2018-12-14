using UnityEngine.AddressableAssets;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Localization;

namespace UnityEditor.Localization
{
    [CustomEditor(typeof(AddressableLocalesProvider))]
    class AddressableLocalesProviderEditor : Editor
    {
        AddressableLocalesProviderListView m_ListView;
        SearchField m_SearchField;

        Editor m_SelectedLocaleEditor;
        int m_SelectedLocaleIndex = -1;

        enum ToolBarChoices
        {
            LocaleGeneratorWindow,
            RemoveSelected,
            AddAsset,
            AddAllAssets
        }

        class Texts
        {
            public GUIContent localeDetails = new GUIContent("Locale Details");
            public GUIContent[] toolbarButtons = new[]
            {
                new GUIContent("Locale Generator", "Opens the Locale Generator window."),
                new GUIContent("Remove Selected"), 
                new GUIContent("Add", "Add a new Locale asset."),
                new GUIContent("Add All", "Add all Locale assets from the project.")
            };
        }

        static Texts s_Texts;

        const float k_MinListHeight = 200;

        void OnEnable()
        {
            // Can go null when we have an embedded editor.
            if (target == null) 
                return;

            if (s_Texts == null)
                s_Texts = new Texts();

            m_ListView = new AddressableLocalesProviderListView();
            m_SearchField = new SearchField();
            m_SearchField.downOrUpArrowKeyPressed += m_ListView.SetFocusAndEnsureSelectedItem;
            Undo.undoRedoPerformed += UndoPerformed;
        }

        void UndoPerformed()
        {
            m_ListView.Reload();
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoPerformed;
        }

        public void Reset()
        {
            if (m_ListView != null)
                m_ListView.Reload();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.Space();
            DrawLocaleList();
            DrawToolbar();
            serializedObject.ApplyModifiedProperties();
        }

        void DrawLocaleList()
        {
            m_ListView.searchString = m_SearchField.OnToolbarGUI(m_ListView.searchString);
            m_ListView.OnGUI(EditorGUILayout.GetControlRect(false, Mathf.Max(k_MinListHeight, m_ListView.totalHeight)));
        }

        void DrawToolbar()
        {
            string commandName = Event.current.commandName;
            int controlId = GUIUtility.GetControlID(FocusType.Passive);

            var selection = (ToolBarChoices)GUILayout.Toolbar(-1, s_Texts.toolbarButtons, EditorStyles.toolbarButton);
            switch (selection)
            {
                case ToolBarChoices.LocaleGeneratorWindow:
                    LocaleGeneratorWindow.ShowWindow((locale) =>
                    {
                        LocalizationAddressableSettings.AddLocale(locale);
                        Reset();
                    });
                    break;
                case ToolBarChoices.RemoveSelected:
                    {
                        var selectedLocales = m_ListView.GetSelection();
                        for (int i = selectedLocales.Count - 1; i >= 0; --i)
                        {
                            var item = m_ListView.GetRows()[selectedLocales[i]] as SerializedLocaleItem;
                            LocalizationAddressableSettings.RemoveLocale(item.SerializedObject.targetObject as Locale);
                        }
                        m_ListView.SetSelection(new int[0]);
                        m_ListView.Reload();
                    }
                    break;
                case ToolBarChoices.AddAsset:
                    EditorGUIUtility.ShowObjectPicker<Locale>(null, false, string.Empty, controlId);
                    break;
                case ToolBarChoices.AddAllAssets:
                    {
                        var assets = AssetDatabase.FindAssets("t:Locale");
                        for (int i = 0; i < assets.Length; ++i)
                        {
                            LocalizationAddressableSettings.AddLocale(AssetDatabase.LoadAssetAtPath<Locale>(AssetDatabase.GUIDToAssetPath(assets[i])));
                        }
                        m_ListView.Reload();
                    }
                    break;
            }

            // Selected Locale
            if (m_ListView.GetSelection().Count == 1)
            {
                if (m_SelectedLocaleEditor == null || m_SelectedLocaleIndex != m_ListView.GetSelection()[0])
                {
                    m_SelectedLocaleIndex = m_ListView.GetSelection()[0];
                    var item = m_ListView.GetRows()[m_SelectedLocaleIndex] as SerializedLocaleItem;
                    CreateCachedEditor(item.SerializedObject.targetObjects, null, ref m_SelectedLocaleEditor);
                }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(s_Texts.localeDetails);
                EditorGUI.indentLevel++;
                m_SelectedLocaleEditor.OnInspectorGUI();
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }

            if (commandName == "ObjectSelectorClosed" && EditorGUIUtility.GetObjectPickerControlID() == controlId && EditorGUIUtility.GetObjectPickerObject() != null)
            {
                LocalizationAddressableSettings.AddLocale(EditorGUIUtility.GetObjectPickerObject() as Locale);
                m_ListView.Reload();
            }
        }
    }
}
