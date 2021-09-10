using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.UI
{
    class LocalizedAssetTableListView : GenericAssetTableListView<AssetTable, AssetTableTreeViewItem>
    {
        protected static readonly Color k_SelectedColor = new Color(0.37f, 0.41f, 0.7f);
        static readonly GUIContent k_MetadataIcon = new GUIContent(EditorIcons.Metadata, "Edit Table Entry Metadata");
        const int k_MetadataLabelWidth = 20;
        const float k_ThumbnailPreviewHeight = 80;

        public LocalizedAssetTableListView(AssetTableCollection tableCollection) :
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
            var height = base.GetCustomRowHeight(row, item);
            if (item is AssetTableTreeViewItem tvi && tvi.AssetType != typeof(Object))
            {
                if (EditorGUIUtility.HasObjectThumbnail(tvi.AssetType))
                    return Mathf.Max(k_ThumbnailPreviewHeight, height);
            }

            return height;
        }

        protected override void DrawItemField(Rect cellRect, int colIdx, TableColumn<AssetTable> col, AssetTableTreeViewItem item)
        {
            var buttonColor = GUI.backgroundColor;
            GUI.backgroundColor = item.IsTableEntrySelected(colIdx) ? k_SelectedColor : buttonColor;
            cellRect.xMax -= k_MetadataLabelWidth;
            var btnRect = new Rect(cellRect.xMax, cellRect.y, k_MetadataLabelWidth, cellRect.height);
            if (GUI.Button(btnRect, k_MetadataIcon))
            {
                Selected = item.Select(colIdx, col.TableLocale);
            }
            GUI.backgroundColor = buttonColor;

            EditorGUI.BeginChangeCheck();
            var newAsset = EditorGUI.ObjectField(cellRect, item.GetTableAsset(colIdx), item.AssetType, false);
            if (EditorGUI.EndChangeCheck())
            {
                var oldType = item.AssetType;
                item.SetAsset(newAsset, colIdx);

                if (oldType != item.AssetType)
                    RefreshCustomRowHeights();
            }
        }
    }
}
