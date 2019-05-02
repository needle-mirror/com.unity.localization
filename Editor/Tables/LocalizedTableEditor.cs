using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental;
#endif

namespace UnityEditor.Localization
{
    [CanEditMultipleObjects()]
    [CustomEditor(typeof(LocalizedTable), true)]
    public abstract class LocalizedTableEditor :
        #if UNITY_2019_1_OR_NEWER
        Editor
        #else
        UIElementsEditor
        #endif
    {
        GUIContent m_TableEditorButton;
        GUIContent m_Label;

        SerializedProperty m_LocaleId;

        /// <summary>
        /// Tables being edited when in TableEditor mode.
        /// </summary>
        public virtual List<LocalizedTable> Tables { get; set; }

        public virtual KeyDatabase Keys
        {
            get
            {
                KeyDatabase keys = null;
                if (Tables != null && Tables.Count > 0)
                {
                    keys = Tables[0].Keys;
                }
                return keys;
            }
        }

        public abstract VisualElement CreateTableEditorGUI();

        public virtual void OnEnable()
        {
            if (target == null)
                return;

            m_TableEditorButton = new GUIContent("Open Table Editor", EditorGUIUtility.ObjectContent(target, typeof(LocalizedTable)).image);
            m_LocaleId = serializedObject.FindProperty("m_LocaleId");
            Undo.undoRedoPerformed += UndoRedoPerformed;
        }

        void OnDisable() => Undo.undoRedoPerformed -= UndoRedoPerformed;

        protected virtual void UndoRedoPerformed()
        {
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_LocaleId);

            EditorGUILayout.Space();
            if (GUILayout.Button(m_TableEditorButton, EditorStyles.miniButton, GUILayout.Width(150), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                AssetTablesWindow.ShowWindow(target as LocalizedTable);
            }
            EditorGUILayout.Space();
            serializedObject.ApplyModifiedProperties();
        }
    }
}