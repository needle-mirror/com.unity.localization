using UnityEngine;
using UnityEngine.Localization.Components;

namespace UnityEditor.Localization.UI
{
    [CustomEditor(typeof(LocalizeStringEvent))]
    class LocalizeStringEditor : UnityEditor.Editor
    {
        SerializedProperty m_StringReference;
        SerializedProperty m_FormatArguments;
        SerializedProperty m_UpdateString;

        LocalizedStringPropertyDrawer m_StringReferenceDrawer;

        static readonly GUIContent s_StringReference = new GUIContent("String Reference");

        void OnEnable()
        {
            m_StringReference = serializedObject.FindProperty("m_StringReference");
            m_FormatArguments = serializedObject.FindProperty("m_FormatArguments");
            m_UpdateString = serializedObject.FindProperty("m_UpdateString");

            m_StringReferenceDrawer = new LocalizedStringPropertyDrawer() { ShowPreview = false, Arguments = new UnityEngine.Localization.SmartFormat.SmartObjects() };
            UpdateArgumentsPreview();
        }

        void UpdateArgumentsPreview()
        {
            m_StringReferenceDrawer.Arguments.Clear();
            for (int i = 0; i < m_FormatArguments.arraySize; ++i)
            {
                var item = m_FormatArguments.GetArrayElementAtIndex(i);
                if (item.objectReferenceValue != null)
                    m_StringReferenceDrawer.Arguments.Add(item.objectReferenceValue);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var height = m_StringReferenceDrawer.GetPropertyHeight(m_StringReference, s_StringReference);
            var rect = EditorGUILayout.GetControlRect(true, height);
            m_StringReferenceDrawer.OnGUI(rect, m_StringReference, s_StringReference);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_FormatArguments);
            if (EditorGUI.EndChangeCheck())
            {
                UpdateArgumentsPreview();
            }

            EditorGUILayout.PropertyField(m_UpdateString);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
