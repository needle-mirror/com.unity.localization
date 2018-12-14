using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Localization
{
    class AddressableLocalesProviderListView : TreeView
    {
        protected enum Column
        {
            Name,
            Code,
            Id,
            Fallback
        }

        public AddressableLocalesProviderListView() :
            base(new TreeViewState())
        {
            showBorder = true;
            showAlternatingRowBackgrounds = true;

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

                new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent("Id"),
                    minWidth = 50,
                    headerTextAlignment = TextAlignment.Center,
                    canSort = true
                },

                new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent("Fallback"),
                    minWidth = 50,
                    headerTextAlignment = TextAlignment.Center,
                    canSort = false
                },
            };

            var multiColState = new MultiColumnHeaderState(columns);
            multiColState.visibleColumns = new[] { (int)Column.Name, (int)Column.Fallback };
            multiColumnHeader = new MultiColumnHeader(multiColState);
            multiColumnHeader.ResizeToFit();
            multiColumnHeader.sortingChanged += mch => Reload();
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            TreeViewItem root = new TreeViewItem(-1, -1, "root");

            var items = new List<TreeViewItem>();
            foreach(var l in LocalizationAddressableSettings.GetLocales())
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
                bool acend = multiColumnHeader.IsSortedAscending(multiColumnHeader.sortedColumnIndex);
                switch ((Column)multiColumnHeader.sortedColumnIndex)
                {
                    case Column.Name:
                        items.Sort((x, y) =>
                        {
                            var a = (SerializedLocaleItem)x;
                            var b = (SerializedLocaleItem)y;
                            return acend ? string.Compare(b.Name, a.Name) : string.Compare(a.Name, b.Name);
                        });
                        break;
                    case Column.Code:
                        items.Sort((x, y) =>
                        {
                            var a = (SerializedLocaleItem)x;
                            var b = (SerializedLocaleItem)y;
                            return acend ? string.Compare(b.IdentifierCode, a.IdentifierCode) : string.Compare(a.IdentifierCode, b.IdentifierCode);
                        });
                        break;
                    case Column.Id:
                        items.Sort((x, y) =>
                        {
                            var a = (SerializedLocaleItem)x;
                            var b = (SerializedLocaleItem)y;
                            return acend ? b.IdentifierId - a.IdentifierId : a.IdentifierId - b.IdentifierId;
                        });
                        break;
                }
            }
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var sli = args.item as SerializedLocaleItem;
            if (sli.SerializedObject != null)
                sli.SerializedObject.Update();

            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), sli, (Column)args.GetColumn(i));
            }

            if (sli.SerializedObject != null)
                sli.SerializedObject.ApplyModifiedProperties();
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

                case Column.Id:
                    if (item.IdentifierIdProp != null)
                        EditorGUI.PropertyField(cellRect, item.IdentifierIdProp, GUIContent.none);
                    break;

                case Column.Fallback:
                    if (item.FallbackProp != null)
                        EditorGUI.PropertyField(cellRect, item.FallbackProp, GUIContent.none);
                    break;
            }
        }

    }
}

