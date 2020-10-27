using System;
using System.Collections.Generic;
using CsvHelper;
using UnityEditor.Localization.Plugins.CSV.Columns;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.Samples.CSV
{
    [Serializable]
    [DisplayName("Custom Data")]
    [Metadata(AllowedTypes = MetadataType.StringTableEntry)]
    public class MyCustomDataMetadata : IMetadata
    {
        public string someValue;
        public int someOtherValue;
    }

    /// <summary>
    /// This example shows reading and writing custom metadata to CSV
    /// </summary>
    public class CsvCustomColumnsExample : CsvColumns
    {
        [SerializeField] LocaleIdentifier m_LocaleIdentifier;

        int m_SomeValueIndex, m_SomeOtherValueIndex, m_CollectionTableIndex;
        StringTable m_ImportTable;

        string SomeValueFieldName => m_LocaleIdentifier + " Some Value";
        string SomeOtherValueFieldName => m_LocaleIdentifier + " Some Other Value";

        public override void ReadBegin(StringTableCollection collection, CsvReader reader)
        {
            m_ImportTable = collection.GetTable(m_LocaleIdentifier) as StringTable;
            if (m_ImportTable != null)
            {
                m_SomeValueIndex = reader.GetFieldIndex(SomeValueFieldName, isTryGet: true);
                m_SomeOtherValueIndex = reader.GetFieldIndex(SomeOtherValueFieldName, isTryGet: true);
            }
        }

        public override void ReadRow(SharedTableData.SharedTableEntry keyEntry, CsvReader reader)
        {
            if (m_ImportTable == null)
                return;

            // Get the entry or create one
            StringTableEntry entry = m_ImportTable.GetEntry(keyEntry.Id) ?? m_ImportTable.AddEntry(keyEntry.Id, string.Empty);

            // Get the metadata or add one
            var metadata = entry.GetMetadata<MyCustomDataMetadata>();
            if (metadata == null)
            {
                metadata = new MyCustomDataMetadata();
                entry.AddMetadata(metadata);
            }

            if (m_SomeValueIndex != -1)
            {
                metadata.someValue = reader.GetField(m_SomeValueIndex);
            }

            if (m_SomeOtherValueIndex != -1)
            {
                metadata.someOtherValue = reader.GetField<int>(m_SomeOtherValueIndex);
            }
        }

        public override void WriteBegin(StringTableCollection collection, CsvWriter writer)
        {
            // Does the collection contain a string table for our Locale Id?
            var tables = collection.StringTables;
            m_CollectionTableIndex = -1;
            for (int i = 0; i < tables.Count; ++i)
            {
                if (tables[i].LocaleIdentifier == m_LocaleIdentifier)
                {
                    m_CollectionTableIndex = i;
                }
            }

            if (m_CollectionTableIndex != -1)
            {
                writer.WriteField(SomeValueFieldName);
                writer.WriteField(SomeOtherValueFieldName);
            }
        }

        public override void WriteRow(SharedTableData.SharedTableEntry keyEntry, IList<StringTableEntry> tableEntries, CsvWriter writer)
        {
            if (m_CollectionTableIndex != -1 && tableEntries[m_CollectionTableIndex] != null)
            {
                var entry = tableEntries[m_CollectionTableIndex];
                var metadata = entry.GetMetadata<MyCustomDataMetadata>();
                if (metadata != null)
                {
                    writer.WriteField(metadata.someValue, true);
                    writer.WriteField(metadata.someOtherValue);
                    return;
                }
            }

            // Write empty entries
            writer.WriteField(string.Empty);
            writer.WriteField(0);
        }
    }
}
