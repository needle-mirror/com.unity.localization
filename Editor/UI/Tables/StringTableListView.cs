using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.UI
{
    class StringTableListView : GenericAssetTableListView<StringTable, StringTableTreeViewItem>
    {
        protected static readonly Color k_SelectedColor = new Color(0.37f, 0.41f, 0.7f);

        public StringTableListView(LocalizationTableCollection tableCollection) :
            base(tableCollection)
        {
        }

        protected override void UndoRedoPerformed()
        {
            rootItem.children.ForEach(i =>
            {
                if (i is AssetTableTreeViewItem tvi)
                {
                    tvi.RefreshFields();
                }
            });
            base.UndoRedoPerformed();
        }

        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
            float maxHeight = base.GetCustomRowHeight(row, item);
            var visibleColumns = multiColumnHeader.state.visibleColumns;
            if (item is StringTableTreeViewItem stringTableItem)
            {
                foreach (var colIdx in visibleColumns)
                {
                    var editor = stringTableItem.GetSmartFormatEditor(colIdx);
                    if (editor != null)
                    {
                        maxHeight = Mathf.Max(maxHeight, editor.Height);
                    }
                }
            }
            return maxHeight;
        }

        protected override void DrawItemField(Rect cellRect, int colIdx, TableColumn<StringTable> col, StringTableTreeViewItem item)
        {
            if (item.Draw(colIdx, cellRect, this))
                RefreshCustomRowHeights();
        }
    }
}
