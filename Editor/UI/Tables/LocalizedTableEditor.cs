using UnityEngine;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.UI
{
    [CustomEditor(typeof(LocalizedTable), true)]
    class LocalizedTableEditor : UnityEditor.Editor
    {
        GUIContent m_TableEditorButton;
        SerializedProperty m_LocaleId;
        SerializedObject m_SharedTableDataSerializedObject;
        SerializedProperty m_TableName;

        public virtual void OnEnable()
        {
            if (target == null)
                return;

            m_TableEditorButton = new GUIContent("Open Table Editor", EditorGUIUtility.ObjectContent(target, target.GetType()).image);

            var table = target as LocalizedTable;
            m_SharedTableDataSerializedObject = new SerializedObject(table.SharedData);
            m_LocaleId = serializedObject.FindProperty("m_LocaleId");
            m_TableName = m_SharedTableDataSerializedObject.FindProperty("m_TableName");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            m_SharedTableDataSerializedObject.Update();

            EditorGUILayout.LabelField("Table Name", m_TableName.stringValue);
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
