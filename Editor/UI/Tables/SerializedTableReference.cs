using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.UI
{
    class SerializedTableReference
    {
        readonly SerializedProperty m_TableName;
        TableReference m_Reference;

        public TableReference Reference
        {
            get => m_Reference;
            set
            {
                m_Reference = value;
                m_TableName.stringValue = m_Reference.GetSerializedString();
            }
        }

        public SerializedTableReference(SerializedProperty property)
        {
            m_TableName = property.FindPropertyRelative("m_TableCollectionName");
            Reference = TableReference.TableReferenceFromString(m_TableName.stringValue);
        }
    }
}
