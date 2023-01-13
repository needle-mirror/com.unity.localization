using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.UI
{
    class StringTableTreeViewItem : GenericAssetTableTreeViewItem<StringTable>
    {
        (SmartFormatField editor, ISelectable selected)[] m_TableProperties;
        List<LocalizationTable> m_SortedTables;
        int m_StartIndex;

        public override string displayName
        {
            get
            {
                if (base.displayName == null)
                    UpdateSearchString();
                return base.displayName;
            }
            set => base.displayName = value;
        }

        public override void Initialize(LocalizationTableCollection collection, int startIdx, List<LocalizationTable> sortedTables)
        {
            m_SortedTables = sortedTables;
            m_StartIndex = startIdx;
        }

        void DelayedInit()
        {
            if (m_TableProperties == null)
            {
                m_TableProperties = new(SmartFormatField, ISelectable)[m_StartIndex + m_SortedTables.Count];

                for (int i = m_StartIndex; i < m_TableProperties.Length; ++i)
                {
                    var table = m_SortedTables[i - m_StartIndex] as StringTable;
                    var smartEditor = new SmartFormatField();
                    smartEditor.KeyId = KeyId;
                    smartEditor.Table = table;
                    m_TableProperties[i].editor = smartEditor;
                }

                RefreshFields();
            }
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
            DelayedInit();

            // Some cols may be missing table cols.
            return colIdx < m_TableProperties.Length ? m_TableProperties[colIdx].editor : null;
        }

        public bool Draw(int colIdx, Rect rect, StringTableListView listView)
        {
            DelayedInit();

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
            DelayedInit();
            ISelectable s = m_TableProperties[colIdx].selected;
            return s?.Selected ?? false;
        }

        public override void OnDeleteKey()
        {
            DelayedInit();
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

                sb.Append(SharedEntry.Id.ToString());
                sb.Append(SharedEntry.Key);

                foreach (StringTable st in m_SortedTables)
                {
                    var entry = st.GetEntry(KeyId);
                    if (entry != null)
                        sb.Append(entry.LocalizedValue);
                }

                displayName = sb.ToString();
            }
        }
    }
}
