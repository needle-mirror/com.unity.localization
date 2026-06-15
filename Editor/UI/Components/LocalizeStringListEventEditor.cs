using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

namespace UnityEditor.Localization.UI
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(LocalizeStringListEvent))]
    class LocalizeStringListEventEditor : UnityEditor.Editor
    {
        static readonly GUIContent k_SourceLabel = EditorGUIUtility.TrTextContent("Source Type", "Choose between a single delimited entry (List) or a collection of individual entries (Group).");
        static readonly string[] k_SourceOptions = { L10n.Tr("Localized String List"), L10n.Tr("Localized String Group") };

        SerializedProperty m_ListReference;
        SerializedProperty m_UpdateList;

        void OnEnable()
        {
            m_ListReference = serializedObject.FindProperty("m_ListReference");
            m_UpdateList = serializedObject.FindProperty("m_UpdateList");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            int currentIndex = GetCurrentIndex();
            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUILayout.Popup(k_SourceLabel, currentIndex, k_SourceOptions);
            if (EditorGUI.EndChangeCheck() && newIndex != currentIndex)
            {
                m_ListReference.managedReferenceValue = newIndex == 0
                    ? new LocalizedStringList()
                    : (ILocalizedStringList)new LocalizedStringGroup();
            }

            EditorGUILayout.PropertyField(m_ListReference, true);
            EditorGUILayout.PropertyField(m_UpdateList);

            serializedObject.ApplyModifiedProperties();
        }

        int GetCurrentIndex()
        {
            var typeName = m_ListReference.managedReferenceFullTypename;
            if (!string.IsNullOrEmpty(typeName) && typeName.EndsWith(nameof(LocalizedStringGroup)))
                return 1;
            return 0;
        }
    }
}
