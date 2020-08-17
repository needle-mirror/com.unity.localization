using UnityEngine.Localization;

namespace UnityEditor.Localization.UI
{
    class SerializedLocaleItem
    {
        public SerializedObject SerializedObject { get; }

        public SerializedProperty NameProp => GetProperty("m_Name");
        public SerializedProperty IdentifierIdProp => GetProperty("m_Identifier.m_Id");
        public SerializedProperty IdentifierCodeProp => GetProperty("m_Identifier.m_Code");
        public SerializedProperty SortOrderProp => GetProperty("m_SortOrder");

        SerializedProperty GetProperty(string propName) => SerializedObject?.FindProperty(propName);

        public Locale Reference => SerializedObject.targetObject as Locale;

        public SerializedLocaleItem(Locale locale)
        {
            SerializedObject = new SerializedObject(locale);
        }
    }
}
