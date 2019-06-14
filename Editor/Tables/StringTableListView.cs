using System.Collections.Generic;
using System.Text;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Localization;

namespace UnityEditor.Localization
{
    class StringTableListViewItem : GenericAssetTableTreeViewItem
    {
        public StringTableEntry GetEntry(StringTable table) => table.GetEntry(KeyEntry.Id) ?? table.AddEntry(KeyEntry.Id);

        public void UpdateSearchString(List<StringTable> tables)
        {
            var sb = new StringBuilder();
            sb.AppendLine(KeyEntry.Key);
            foreach (var table in tables)
            {
                var entry = GetEntry(table);
                if (entry?.TranslatedPlurals != null)
                {
                    foreach (var tp in entry.TranslatedPlurals)
                    {
                        sb.AppendLine(tp);
                    }
                }
            }
            SearchString = sb.ToString();
        }
    }

    class StringTableListView : GenericAssetTableListView<StringTable, StringTableListViewItem>
    {
        class Styles
        {
            public GUIContent multiLine = EditorGUIUtility.IconContent(@"GUISystem/align_horizontally_center", "Center Align");
            public GUIContent edit = EditorGUIUtility.IconContent(@"GUISystem/align_vertically_center", "Middle Align");

            public Styles()
            {
                multiLine.tooltip = "Toggle multiline view";
                edit.tooltip = "Use inline editing or detailed editing";
            }
        }
        static Styles s_Styles = new Styles();

        public delegate void EditTable(StringTableListViewItem row, StringTable col);
        public event EditTable editTargetTable;

        float m_CurrentWidth;
        bool m_Init;
        bool m_Multiline;
        bool m_InlineEditing;
        StringTable m_SelectedTable;

        StringTableListViewItem m_SelectedItem;

        public StringTable SelectedTable
        {
            get => m_SelectedTable;
            set
            {
                if (m_SelectedTable != value)
                {
                    m_SelectedTable = value;
                    SelectionChanged(GetSelection());
                }
            }
        }

        /// <summary>
        /// Rows will allow for directly editing keys and translated text.
        /// </summary>
        public bool InlineEditing
        {
            get => m_InlineEditing;
            set
            {
                if (m_InlineEditing == value)
                    return;

                m_InlineEditing = value;

                Reload();

                // Deselect
                if (m_InlineEditing)
                    editTargetTable?.Invoke(null, null);
            }
        }

        /// <summary>
        /// Rows will expand to show the full contents.
        /// </summary>
        public bool Multiline
        {
            get => m_Multiline;
            set
            {
                if (value != m_Multiline)
                {
                    m_Multiline = value;
                    if(m_Init)
                        RefreshCustomRowHeights();
                }
            }
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            var rows = GetRows();
            m_SelectedItem = null;
            if (!InlineEditing && selectedIds.Count > 0 && rows.Count > 1 && selectedIds[0] < rows.Count)
            {
                m_SelectedItem = rows[selectedIds[0]] as StringTableListViewItem;
            }

            editTargetTable?.Invoke(m_SelectedItem, SelectedTable);

            base.SelectionChanged(selectedIds);
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override StringTableListViewItem CreateTreeViewItem(int id, KeyDatabase.KeyDatabaseEntry itemKey)
        {
            var item = base.CreateTreeViewItem(id, itemKey);
            item.UpdateSearchString(Tables);
            return item;
        }

        protected override Rect DrawSearchField(Rect rect)
        {
            var searchRect = new Rect(rect.x, rect.y, rect.width - 50, rect.height);
            var multiLineButtonRect = new Rect(searchRect.xMax, rect.y, 24, rect.height);
            var editButtonRect = new Rect(multiLineButtonRect.xMax + 2, rect.y, 24, rect.height);

            base.DrawSearchField(searchRect);
            Multiline = GUI.Toggle(multiLineButtonRect, Multiline, s_Styles.multiLine, EditorStyles.toolbarButton);
            InlineEditing = GUI.Toggle(editButtonRect, InlineEditing, s_Styles.edit, EditorStyles.toolbarButton);
            rect.yMin += EditorGUIUtility.singleLineHeight;
            return rect;
        }

        public override void OnGUI(Rect rect)
        {
            if (!m_Init)
            {
                // We can not use EditorStyles until OnGUI has been called at least once, it will throw null ref exceptions before then.
                m_Init = true;
                if(Multiline)
                    RefreshCustomRowHeights();
                SelectionChanged(GetSelection());
            }

            m_CurrentWidth = rect.width;
            base.OnGUI(rect);
        }

        protected override void DrawKeyField(Rect cellRect, StringTableListViewItem keyItem)
        {
            if(InlineEditing)
                base.DrawKeyField(cellRect, keyItem);
            else
                EditorGUI.LabelField(cellRect, keyItem.Key);
        }

        protected override void DrawItemField(Rect cellRect, int colIdx, TableColumn col, StringTableListViewItem item)
        {
            var entry = item.GetEntry(col.Table);
            var text = entry != null ? entry.Translated : string.Empty;
            if (InlineEditing)
            {
                EditorGUI.BeginChangeCheck();
                var newText = EditorGUI.TextArea(cellRect, text);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(col.Table.Keys, "Add key");
                    Undo.RecordObject(col.Table, "Change translated text");
                    EditorUtility.SetDirty(col.Table);

                    if (entry == null)
                        entry = col.Table.AddEntry(item.KeyEntry.Id);

                    entry.Translated = newText;
                    item.UpdateSearchString(Tables);
                    RefreshCustomRowHeights();
                }
            }
            else
            {
                if (GUI.Button(cellRect, text, SelectedTable == col.Table ? EditorStyles.boldLabel : EditorStyles.label))
                {
                    SelectedTable = col.Table;
                    SetSelection(new List<int>(){item.id}, TreeViewSelectionOptions.FireSelectionChanged);
                }
            }
        }

        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
            if (!m_Init || !Multiline)
            {
                return base.GetCustomRowHeight(row, item);
            }

            var stringItem = item as StringTableListViewItem;
            var style = InlineEditing ? EditorStyles.textField : EditorStyles.label;
            var maxHeight = EditorGUIUtility.singleLineHeight;

            if (stringItem != null)
            {
                maxHeight = Mathf.Max(maxHeight, style.CalcHeight(new GUIContent(stringItem.Key), m_CurrentWidth));

                for (int i = 0; i < Tables.Count; ++i)
                {
                    if (multiColumnHeader.IsColumnVisible(i + k_InitialColumns))
                    {
                        var entry = stringItem.GetEntry(Tables[i]);
                        if (entry != null)
                        {
                            maxHeight = Mathf.Max(maxHeight, style.CalcHeight(new GUIContent(entry.Translated), m_CurrentWidth));
                        }
                    }
                }
            }
            else
            {
                // New item field
                maxHeight = Mathf.Max(maxHeight, style.CalcHeight(new GUIContent(NewKey), m_CurrentWidth));
            }
            
            return maxHeight;
        }
    }
}