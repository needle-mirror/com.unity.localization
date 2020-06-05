using UnityEditor.IMGUI.Controls;
using UnityEngine.Localization;

namespace UnityEditor.Localization.UI
{
    class SerializedLocaleItem : TreeViewItem
    {
        SerializedObject m_SerializedObject;

        public SerializedObject SerializedObject
        {
            get
            {
                if (m_SerializedObject == null && Reference != null)
                {
                    m_SerializedObject = new SerializedObject(Reference);
                }
                return m_SerializedObject;
            }
        }

        public SerializedProperty Property { get; set; }
        public SerializedProperty NameProp => GetProperty(k_NamePropertyName);
        public SerializedProperty IdentifierIdProp => GetProperty(k_IdPropertyName);
        public SerializedProperty IdentifierCodeProp => GetProperty(k_CodePropertyName);

        SerializedProperty GetProperty(string propName) => SerializedObject?.FindProperty(propName);

        public Locale Reference => m_SerializedObject.targetObject as Locale;

        const string k_NamePropertyName = "m_Name";
        const string k_IdPropertyName = "m_Identifier.m_Id";
        const string k_CodePropertyName = "m_Identifier.m_Code";

        public override string displayName => Name + " " + IdentifierCode + " " + id;

        public int IdentifierId
        {
            get => IdentifierIdProp?.intValue ?? 0;
            set
            {
                if (IdentifierIdProp != null)
                    IdentifierIdProp.intValue = value;
            }
        }

        public string Name
        {
            get => NameProp != null ? NameProp.stringValue : string.Empty;
            set
            {
                if (NameProp != null)
                    NameProp.stringValue = value;
            }
        }

        public string IdentifierCode
        {
            get => IdentifierCodeProp != null ? IdentifierCodeProp.stringValue : string.Empty;
            set
            {
                if (IdentifierCodeProp != null)
                    IdentifierCodeProp.stringValue = value;
            }
        }

        public SerializedLocaleItem(Locale locale)
        {
            m_SerializedObject = new SerializedObject(locale);
        }
    }
}
