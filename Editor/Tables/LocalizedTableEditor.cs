using System.Collections.Generic;
using UnityEditor.Experimental;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Localization
{
    [CanEditMultipleObjects()]
    [CustomEditor(typeof(LocalizedTable), true)]
    public class LocalizedTableEditor : UIElementsEditor
    {
        GUIContent m_TableEditorButton;
        GUIContent m_Label;

        SerializedProperty m_LocaleId;
        SerializedProperty m_TableName;

        /// <summary>
        /// Tables being edited when in TableEditor mode.
        /// </summary>
        public virtual List<LocalizedTable> Tables { get; set; }

        public override VisualElement CreateInspectorGUI()
        {
            return new Label(){text = "No editor was found for this asset table type. Did you create one that inherits from LocalizedAssetTableEditor?" };
        }

        public virtual void OnEnable()
        {
            m_TableEditorButton = new GUIContent("Open Table Editor", EditorGUIUtility.ObjectContent(target, typeof(LocalizedTable)).image);
            m_LocaleId = serializedObject.FindProperty("m_LocaleId");
            m_TableName = serializedObject.FindProperty("m_TableName");
            Undo.undoRedoPerformed += UndoRedoPerformed;
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoRedoPerformed;
        }

        protected virtual void UndoRedoPerformed()
        {
            UpdateAddressable();
        }

        protected void UpdateAddressable()
        {
            // Updates the addressables version so it has the correct label and address, such as if the locale or name was changed.
            if (targets != null && targets.Length > 0)
            {
                foreach (var t in targets)
                {
                    LocalizationAddressableSettings.AddOrUpdateAssetTable((LocalizedTable)t);
                }
            }
            else
            {
                LocalizationAddressableSettings.AddOrUpdateAssetTable((LocalizedTable)target);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_LocaleId);
            EditorGUILayout.PropertyField(m_TableName);

            EditorGUILayout.Space();
            if (GUILayout.Button(m_TableEditorButton, EditorStyles.miniButton, GUILayout.Width(150), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
            {
                AssetTablesWindow.ShowWindow(target as LocalizedTable);
            }
            EditorGUILayout.Space();
            if (serializedObject.ApplyModifiedProperties())
            {
                UpdateAddressable();
            }
        }

        //public void DrawTableEditorGUI(Rect rect)
        //{
        //    serializedObject.Update();
        //    OnTableEditorGUI(rect);
        //    if (serializedObject.ApplyModifiedProperties())
        //        UpdateAddressable();
        //}

        //public virtual Rect OnTableEditorGUI(Rect rect)
        //{
        //    //EditorGUI.BeginChangeCheck();
        //    var propRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
        //    EditorGUI.PropertyField(propRect, m_TableName);
        //    //if (EditorGUI.EndChangeCheck())
        //    //    SetLabelText(m_TableName.stringValue);

        //    return new Rect(rect.x, propRect.yMax + EditorGUIUtility.standardVerticalSpacing, rect.width, rect.height - propRect.height - EditorGUIUtility.standardVerticalSpacing);
        //}
    }
}