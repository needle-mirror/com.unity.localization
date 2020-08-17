using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.Plugins.Google.Columns
{
    [Serializable]
    public class KeyColumn : SheetColumn, IPullKeyColumn
    {
        SharedTableData m_SharedTableData;

        public const string ColumnHeader = "Key";

        public override PushFields PushFields => PushFields.ValueAndNote;

        public override void PushBegin(StringTableCollection collection) {}

        public override void PushHeader(StringTableCollection collection, out string header, out string headerNote)
        {
            header = ColumnHeader;
            headerNote = null;
        }

        public override void PushCellData(SharedTableData.SharedTableEntry keyEntry, IList<StringTableEntry> tableEntries, out string value, out string note)
        {
            value = keyEntry.Key;
            note = keyEntry.Id.ToString();
        }

        public override void PullBegin(StringTableCollection collection)
        {
            m_SharedTableData = collection.SharedData;
        }

        public override void PullCellData(SharedTableData.SharedTableEntry keyEntry, string cellValue, string cellNote) {}

        public SharedTableData.SharedTableEntry PullKey(string cellValue, string cellNote)
        {
            if (!string.IsNullOrEmpty(cellNote) && long.TryParse(cellNote, out var keyId))
            {
                var entry = m_SharedTableData.GetEntry(keyId);
                if (entry != null)
                {
                    if (entry.Key != cellValue)
                        m_SharedTableData.RenameKey(entry.Key, cellValue);
                    return entry;
                }
                else
                {
                    // Create a new entry with the id
                    return m_SharedTableData.AddKey(cellValue, keyId);
                }
            }

            return m_SharedTableData.GetEntry(cellValue) ?? m_SharedTableData.AddKey(cellValue);
        }
    }
}
