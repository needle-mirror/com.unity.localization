using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.UI
{
    class SerializedTableReference
    {
        TableReference m_Reference;

        public SerializedProperty TableNameProperty { get; }

        public bool HasMultipleDifferentValues => TableNameProperty.hasMultipleDifferentValues;

        public TableReference Reference
        {
            get => m_Reference;
            set
            {
                m_Reference = value;
                TableNameProperty.stringValue = m_Reference.GetSerializedString();
            }
        }

        public SerializedTableReference(SerializedProperty property)
        {
            TableNameProperty = property.FindPropertyRelative("m_TableCollectionName");

            if (HasMultipleDifferentValues)
                return;

            Reference = TableReference.TableReferenceFromString(TableNameProperty.stringValue);
        }
    }
}
