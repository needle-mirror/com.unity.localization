using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.Plugins.Google.Columns
{
    [Serializable]
    public class LocaleColumn : SheetColumn
    {
        [SerializeField]
        LocaleIdentifier m_LocaleIdentifier;

        [SerializeField]
        bool m_IncludeComments = false;

        int m_CollectionTableIndex = -1;

        StringTable m_PullTable;

        public override PushFields PushFields => m_IncludeComments ? PushFields.ValueAndNote : PushFields.Value;

        /// <summary>
        /// The Id of the Locale.
        /// </summary>
        public LocaleIdentifier LocaleIdentifier
        {
            get => m_LocaleIdentifier;
            set => m_LocaleIdentifier = value;
        }

        /// <summary>
        /// Should any <see cref="Comment"/> metadata be included as cell notes?
        /// </summary>
        public bool IncludeComments
        {
            get => m_IncludeComments;
            set => m_IncludeComments = value;
        }

        public override void PushBegin(StringTableCollection collection)
        {
            var tables = collection.StringTables;
            for (int i = 0; i < tables.Count; ++i)
            {
                if (tables[i].LocaleIdentifier == LocaleIdentifier)
                {
                    m_CollectionTableIndex = i;
                    return;
                }
            }
            m_CollectionTableIndex = -1;
        }

        public override void PushHeader(StringTableCollection collection, out string header, out string headerNote)
        {
            header = LocaleIdentifier.ToString();
            headerNote = null;
        }

        public override void PushCellData(SharedTableData.SharedTableEntry keyEntry, IList<StringTableEntry> tableEntries, out string value, out string note)
        {
            if (m_CollectionTableIndex != -1 && tableEntries[m_CollectionTableIndex] != null)
            {
                var entry = tableEntries[m_CollectionTableIndex];
                value = entry?.LocalizedValue;

                if (m_IncludeComments)
                {
                    var comment = entry.GetMetadata<Comment>();
                    note = comment?.CommentText;
                }
                else
                {
                    note = null;
                }
            }
            else
            {
                value = null;
                note = null;
            }
        }

        public override void PullBegin(StringTableCollection collection)
        {
            var tables = collection.StringTables;
            for (int i = 0; i < tables.Count; ++i)
            {
                if (tables[i].LocaleIdentifier == LocaleIdentifier)
                {
                    m_PullTable = collection.StringTables[i];
                    return;
                }
            }
            m_PullTable = null;
        }

        public override void PullCellData(SharedTableData.SharedTableEntry keyEntry, string cellValue, string cellNote)
        {
            if (m_PullTable == null)
                return;

            var entry = m_PullTable.AddEntry(keyEntry.Id, cellValue);

            var comment = entry.GetMetadata<Comment>();
            if (string.IsNullOrEmpty(cellNote))
            {
                if (comment != null)
                    entry.RemoveMetadata(comment);
            }
            else
            {
                if (comment != null)
                    comment.CommentText = cellNote;
                else
                    entry.AddMetadata(new Comment() { CommentText = cellNote });
            }
        }
    }
}
