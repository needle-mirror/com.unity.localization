using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.UI
{
    class StringTableTreeViewItem : GenericAssetTableTreeViewItem<StringTable>
    {
        KeyDatabase m_Keys;

        (SmartFormatField editor, ISelectable selected)[] m_TableProperties;

        public override void Initialize(List<LocalizedTable> tables, int startIdx)
        {
            m_TableProperties = new (SmartFormatField, ISelectable)[startIdx + tables.Count];

            m_Keys = tables[startIdx].Keys;
            Debug.Assert(m_Keys != null);
            for (int i = startIdx; i < m_TableProperties.Length; ++i)
            {
                var table = tables[i - startIdx] as StringTable;
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
            return m_TableProperties[colIdx].editor;
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
                    iselected = new TableEntrySelected(editor.Table, editor.KeyId, LocalizationEditorSettings.GetLocale(editor.Table.LocaleIdentifier.Code), MetadataType.StringTableEntry);
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
                tableField.editor?.Table.RemoveEntry(KeyId);
            }
        }

        void UpdateSearchString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(KeyEntry.Key);
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