using UnityEngine;
using UnityEngine.Localization.Components;
using static UnityEditor.Localization.UI.LocalizedStringPropertyDrawer;

namespace UnityEditor.Localization.UI
{
    [CustomEditor(typeof(LocalizeStringEvent))]
    class LocalizeStringEventEditor : UnityEditor.Editor
    {
        SerializedProperty m_StringReference;
        SerializedProperty m_FormatArguments;
        SerializedProperty m_UpdateString;
        LocalizedStringPropertyDrawer m_StringReferenceDrawer;
        bool m_Init;

        static readonly GUIContent s_StringReference = new GUIContent("String Reference");

        void OnEnable()
        {
            m_StringReference = serializedObject.FindProperty("m_StringReference");
            m_FormatArguments = serializedObject.FindProperty("m_FormatArguments");
            m_UpdateString = serializedObject.FindProperty("m_UpdateString");

            m_StringReferenceDrawer = new LocalizedStringPropertyDrawer() { ShowPreview = false };
        }

        void UpdateArgumentsPreview()
        {
            var arguments = new object[m_FormatArguments.arraySize];
            for (int i = 0; i < m_FormatArguments.arraySize; ++i)
            {
                var item = m_FormatArguments.GetArrayElementAtIndex(i);
                arguments[i] = item.objectReferenceValue;
            }
            var data = m_StringReferenceDrawer.GetDataForProperty(m_StringReference) as StringPropertyData;
            data.PreviewArguments = arguments;
        }

        public override void OnInspectorGUI()
        {
            if (!m_Init)
            {
                m_Init = true;

                // We need to update in here as editor styles may not be available in OnEnable.
                UpdateArgumentsPreview();
            }

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
