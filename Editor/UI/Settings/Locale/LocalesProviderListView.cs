using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UnityEditor.Localization.UI
{
    class LocalesProviderListView : TreeView
    {
        protected enum Column
        {
            Name,
            Code
        }

        public LocalesProviderListView() :
            base(new TreeViewState())
        {
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            rowHeight = 18;

            var columns = new[]
            {
                new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent("Name"),
                    minWidth = 100,
                    headerTextAlignment = TextAlignment.Center,
                    canSort = true
                },

                new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent("Code"),
                    minWidth = 50,
                    headerTextAlignment = TextAlignment.Center,
                    canSort = true
                },
            };

            var multiColState = new MultiColumnHeaderState(columns);
            multiColumnHeader = new MultiColumnHeader(multiColState);
            multiColumnHeader.ResizeToFit();
            multiColumnHeader.sortingChanged += mch => Reload();
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem(-1, -1, "root");

            var items = new List<TreeViewItem>();
            foreach (var l in LocalizationEditorSettings.GetLocales())
            {
                items.Add(new SerializedLocaleItem(l));
            }

            ApplySorting(items);

            for (int i = 0; i < items.Count; ++i)
            {
                items[i].id = i;
            }

            SetupParentsAndChildrenFromDepths(root, items);
            return root;
        }

        protected void ApplySorting(List<TreeViewItem> items)
        {
            if (multiColumnHeader.sortedColumnIndex >= 0)
            {
                var ascend = multiColumnHeader.IsSortedAscending(multiColumnHeader.sortedColumnIndex);
                switch ((Column)multiColumnHeader.sortedColumnIndex)
                {
                    case Column.Name:
                        items.Sort((x, y) =>
                        {
                            var a = (SerializedLocaleItem)x;
                            var b = (SerializedLocaleItem)y;
                            return ascend ? string.Compare(b.Name, a.Name) : string.Compare(a.Name, b.Name);
                        });
                        break;
                    case Column.Code:
                        items.Sort((x, y) =>
                        {
                            var a = (SerializedLocaleItem)x;
                            var b = (SerializedLocaleItem)y;
                            return ascend ? string.Compare(b.IdentifierCode, a.IdentifierCode) : string.Compare(a.IdentifierCode, b.IdentifierCode);
                        });
                        break;
                }
            }
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var sli = args.item as SerializedLocaleItem;
            sli.SerializedObject?.Update();

            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), sli, (Column)args.GetColumn(i));
            }

            sli.SerializedObject?.ApplyModifiedProperties();
        }

        protected void CellGUI(Rect cellRect, SerializedLocaleItem item, Column col)
        {
            CenterRectUsingSingleLineHeight(ref cellRect);

            switch (col)
            {
                case Column.Name:
                    if (item.NameProp != null)
                        EditorGUI.PropertyField(cellRect, item.NameProp, GUIContent.none);
                    break;

                case Column.Code:
                    if (item.IdentifierCodeProp != null)
                        EditorGUI.PropertyField(cellRect, item.IdentifierCodeProp, GUIContent.none);
                    break;
            }
        }
    }
}
