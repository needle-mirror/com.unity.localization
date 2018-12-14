using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEditorInternal;

namespace UnityEditor.Localization
{
    [CustomEditor(typeof(StartupLocaleSelectorCollection))]
    class StartupLocaleSelectorCollectionEditor : Editor
    {
        SerializedProperty m_StartupSelectors;

        ReorderableList m_List;
        Editor m_SelectedSelectorEditor;

        class Texts
        {
            public GUIContent listTitle = new GUIContent("Locale Selection Order(top to bottom)");
        }
        static Texts s_Texts;

        void OnEnable()
        {
            // It can sometimes be null if we are embedded into the LocalizationSettings editor.
            if (target == null)
                return;

            if (s_Texts == null)
                s_Texts = new Texts();

            m_StartupSelectors = serializedObject.FindProperty("m_StartupSelectors");

            m_List = new ReorderableList(serializedObject, m_StartupSelectors);
            m_List.drawElementCallback = DrawListItem;
            m_List.drawHeaderCallback = DrawHeaderCallback;
            m_List.onSelectCallback = OnSelectCallback;
        }

        void OnSelectCallback(ReorderableList list)
        {
            if (list.index != -1 && list.index <= m_StartupSelectors.arraySize)
            {
                var item = m_StartupSelectors.GetArrayElementAtIndex(list.index);
                if (m_SelectedSelectorEditor == null || m_SelectedSelectorEditor.target != item.objectReferenceValue)
                {
                    CreateCachedEditor(item.objectReferenceValue, null, ref m_SelectedSelectorEditor);
                }
            }
            else
            {
                m_SelectedSelectorEditor = null;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            m_List.DoLayoutList();

            if (m_SelectedSelectorEditor != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                m_SelectedSelectorEditor.OnInspectorGUI();
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }

        static void DrawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, s_Texts.listTitle);
        }

        void DrawListItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            var item = m_StartupSelectors.GetArrayElementAtIndex(index);
            rect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(rect, item, new GUIContent("Selector " + index));
        }
    }
}
