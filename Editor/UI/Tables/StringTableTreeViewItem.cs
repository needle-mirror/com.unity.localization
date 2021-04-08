using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.UI
{
    class StringTableTreeViewItem : GenericAssetTableTreeViewItem<StringTable>
    {
        SharedTableData m_SharedTableData;

        (SmartFormatField editor, ISelectable selected)[] m_TableProperties;

        public override void Initialize(LocalizationTableCollection collection, int startIdx, List<LocalizationTable> sortedTables)
        {
            m_TableProperties = new(SmartFormatField, ISelectable)[startIdx + sortedTables.Count];

            // Get the shared data
            m_SharedTableData = collection.SharedData;

            Debug.Assert(m_SharedTableData != null);
            for (int i = startIdx; i < m_TableProperties.Length; ++i)
            {
                var table = sortedTables[i - startIdx] as StringTable;
                var smartEditor = new SmartFormatField();
                smartEditor.KeyId = KeyId;
                smartEditor.Table = table;
                smartEditor.RefreshData();
                m_TableProperties[i].editor = smartEditor;
            }
            RefreshFields();
        }

        public void RefreshFields()
        {
            for (int i = 0; i < m_TableProperties.Length; ++i)
            {
                var smartField = m_TableProperties[i].editor;
                smartField?.RefreshData();
            }
            UpdateSearchString();
        }

        public SmartFormatField GetSmartFormatEditor(int colIdx)
        {
            // Some cols may be missing table cols.
            return colIdx < m_TableProperties.Length ? m_TableProperties[colIdx].editor : null;
        }

        public bool Draw(int colIdx, Rect rect, StringTableListView listView)
        {
            var editor = m_TableProperties[colIdx].editor;

            if (m_TableProperties[colIdx].selected != null)
                editor.Selected = m_TableProperties[colIdx].selected.Selected;
            bool OldSelected = editor.Selected;
            if (editor.Draw(rect))
            {
                UpdateSearchString();
                return true;
            }

            if (editor.Selected != OldSelected)
            {
                var iselected = m_TableProperties[colIdx].selected;
                if (iselected == null)
                {
                    iselected = new TableEntrySelected(editor.Table, editor.KeyId, LocalizationEditorSettings.GetLocale(editor.Table.LocaleIdentifier.Code), MetadataType.StringTableEntry | MetadataType.SharedStringTableEntry);
                    m_TableProperties[colIdx].selected = iselected;
                }
                listView.Selected = iselected;
            }

            return false;
        }

        public bool IsTableEntrySelected(int colIdx)
        {
            ISelectable s = m_TableProperties[colIdx].selected;
            return s?.Selected ?? false;
        }

        public override void OnDeleteKey()
        {
            foreach (var tableField in m_TableProperties)
            {
                // If the column is selected then we need to disable it, so we are not trying to edit data that has been removed.
                if (tableField.selected != null)
                    tableField.selected.Selected = false;

                tableField.editor?.Table.RemoveEntry(KeyId);
            }
        }

        void UpdateSearchString()
        {
            using (StringBuilderPool.Get(out var sb))
            {
                sb.Clear();

                sb.AppendLine(SharedEntry.Id.ToString());
                sb.AppendLine(SharedEntry.Key);

                foreach (var tableField in m_TableProperties)
                {
                    if (tableField.editor != null)
                    {
                        sb.Append(tableField.editor.RawText);
                    }
                }

                displayName = sb.ToString();
            }
        }
    }
}
