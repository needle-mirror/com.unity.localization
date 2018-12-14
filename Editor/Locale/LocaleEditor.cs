using UnityEngine.Localization;

namespace UnityEditor.Localization
{
    [CustomEditor(typeof(Locale))]
    class LocaleEditor : Editor
    {
        SerializedProperty m_Name;

        void OnEnable()
        {
            m_Name = serializedObject.FindProperty("m_Name");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_Name);
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();
        }
    }
}
