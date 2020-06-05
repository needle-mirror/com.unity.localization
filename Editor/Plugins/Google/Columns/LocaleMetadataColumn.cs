using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.Plugins.Google.Columns
{
    [Serializable]
    public abstract class LocaleMetadataColumn<TMetadata> : SheetColumn where TMetadata : IMetadata
    {
        [SerializeField]
        LocaleIdentifier m_LocaleIdentifier;

        int m_PushCollectionTableIndex = -1;
        StringTable m_PullTable;

        /// <summary>
        /// The Id of the Locale.
        /// </summary>
        public LocaleIdentifier LocaleIdentifier
        {
            get => m_LocaleIdentifier;
            set => m_LocaleIdentifier = value;
        }

        public override void PushBegin(StringTableCollection collection)
        {
            var tables = collection.StringTables;
            for (int i = 0; i < tables.Count; ++i)
            {
                if (tables[i].LocaleIdentifier == LocaleIdentifier)
                {
                    m_PushCollectionTableIndex = i;
                    return;
                }
            }
            m_PushCollectionTableIndex = -1;
        }

        public abstract void PushMetadata(TMetadata metadata, out string value, out string note);

        public override void PushCellData(SharedTableData.SharedTableEntry keyEntry, IList<StringTableEntry> tableEntries, out string value, out string note)
        {
            if (m_PushCollectionTableIndex != -1 && tableEntries[m_PushCollectionTableIndex] != null)
            {
                var entry = tableEntries[m_PushCollectionTableIndex];
                var metadata = entry.GetMetadata<TMetadata>();
                if (metadata != null)
                {
                    PushMetadata(metadata, out value, out note);
                    return;
                }
            }
            value = null;
            note = null;
        }

        public override void PushEnd()
        {
            m_PushCollectionTableIndex = -1;
        }

        public override void PullBegin(StringTableCollection collection)
        {
            var tables = collection.StringTables;
            for (int i = 0; i < tables.Count; ++i)
            {
                if (tables[i].LocaleIdentifier == LocaleIdentifier)
                {
                    m_PullTable = tables[i];
                    return;
                }
            }
            m_PullTable = null;
        }

        public abstract void PullMetadata(StringTableEntry entry, TMetadata metadata, string cellValue, string cellNote);

        public override void PullCellData(SharedTableData.SharedTableEntry keyEntry, string cellValue, string cellNote)
        {
            if (m_PullTable == null)
                return;

            var entry = m_PullTable.GetEntry(keyEntry.Id) ?? m_PullTable.AddEntry(keyEntry.Id, string.Empty);
            var metadata = entry.GetMetadata<TMetadata>();
            PullMetadata(entry, metadata, cellValue, cellNote);
        }

        public override void PullEnd()
        {
            m_PullTable = null;
        }
    }
}
