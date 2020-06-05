using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.UI
{
    class SerializedTableReference
    {
        SerializedProperty tableName;
        TableReference m_Reference;

        public TableReference Reference
        {
            get => m_Reference;
            set
            {
                m_Reference = value;
                tableName.stringValue = m_Reference.GetSerializedString();
            }
        }

        public SerializedTableReference(SerializedProperty property)
        {
            tableName = property.FindPropertyRelative("m_TableCollectionName");
            Reference = TableReference.TableReferenceFromString(tableName.stringValue);
        }
    }
}
