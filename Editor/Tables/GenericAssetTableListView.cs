using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Localization;
using System.Linq;

namespace UnityEditor.Localization
{
    public class GenericAssetTableTreeViewItem : TreeViewItem
    {
        public virtual string SearchString { get { return displayName; } set { displayName = value; } }
        public virtual string Key { get; set; }
    }

    public abstract class GenericAssetTableListView<T1, T2> : TreeView 
        where T1 : LocalizedTable
        where T2 : GenericAssetTableTreeViewItem, new()
    {
        SearchField m_SearchField;

        protected string NewKey { get; set; }

        public List<T1> Tables { get; set; }

        const int k_AddItemId = int.MaxValue;

        TreeViewItem m_AddKeyItem;

        public new virtual float totalHeight
        {
            get
            {
                return base.totalHeight + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
        }

        protected GenericAssetTableListView() :
            base(new TreeViewState())
        {
            m_SearchField = new SearchField();
            m_SearchField.downOrUpArrowKeyPressed += SetFocusAndEnsureSelectedItem;
        }

        public virtual void Initialize()
        {
            InitializeColumns();
            Reload();
            multiColumnHeader.sortingChanged += mch => Reload();
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            // Disable multi select
            return false;
        }

        protected virtual void InitializeColumns()
        {
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            var columns = new MultiColumnHeaderState.Column[Tables.Count + 1];

            columns[0] = new MultiColumnHeaderState.Column()
            {
                headerContent = new GUIContent("Key"),
                headerTextAlignment = TextAlignment.Center,
                canSort = true,
                allowToggleVisibility = false,
                sortedAscending = true
            };

            // Update column labels if possible
            var locales = LocalizationAddressableSettings.GetLocales();
            for (int i = 0; i < Tables.Count; ++i)
            {
                var foundLocale = locales.FirstOrDefault(o => o.Identifier.Code == Tables[i].LocaleIdentifier.Code);
                columns[i+1] = new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent(foundLocale != null ? foundLocale.name : Tables[i].LocaleIdentifier.Code),
                    headerTextAlignment = TextAlignment.Center,
                    canSort = false, // TODO: Support sorting
                    allowToggleVisibility = true,
                };
            }
            
            var multiColState = new MultiColumnHeaderState(columns);
            multiColumnHeader = new MultiColumnHeader(multiColState);
            multiColumnHeader.ResizeToFit();
        }

        protected virtual T2 CreateTreeViewItem(int index, string itemKey)
        {
            return new T2() { id = index, Key = itemKey };
        }

        protected override TreeViewItem BuildRoot()
        {
            TreeViewItem root = new TreeViewItem(-1, -1, "root");
            var items = new List<TreeViewItem>();

            // Collect all keys
            HashSet<string> keys = new HashSet<string>();
            if(Tables != null)
                Tables.ForEach(tbl => tbl.GetKeys(keys));

            var keysList = keys.ToList();
            if (multiColumnHeader.sortedColumnIndex == 0)
            {
                bool acend = multiColumnHeader.IsSortedAscending(0);
                keysList.Sort((a, b) => acend ? string.Compare(b, a) : string.Compare(a, b));
            }

            for (int i = 0; i < keysList.Count; ++i)
            {
                var tvi = CreateTreeViewItem(i, keysList[i]);
                items.Add(tvi);
            }

            // At the end we add an extra node which will be used to add new keys.
            m_AddKeyItem = new GenericAssetTableTreeViewItem() { id = k_AddItemId };
            items.Add(m_AddKeyItem);
            SetupParentsAndChildrenFromDepths(root, items);
            return root;
        }

        protected virtual Rect DrawSearchField(Rect rect)
        {
            var searchRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
            searchString = m_SearchField.OnToolbarGUI(searchRect, searchString);
            rect.yMin += EditorGUIUtility.singleLineHeight;
            return rect;
        }

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(DrawSearchField(rect));
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                var cellRect = args.GetCellRect(i);
                int colId = args.GetColumn(i);
                if (colId == 0)
                {
                    if (args.item.id == k_AddItemId)
                    {
                        DrawNewKeyField(cellRect);
                        return;
                    }
                    DrawKeyField(cellRect, args.item as T2);
                }
                else
                {
                    DrawItemField(cellRect, colId, args.item as T2, Tables[colId - 1]);
                }
            }
        }

        protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
        {
            // Ignore add button
            if (item.id == k_AddItemId)
                return false;
            return base.DoesItemMatchSearch(item, search);
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            var rows = base.BuildRows(root);

            if (hasSearch)
                rows.Add(m_AddKeyItem);
            return rows;
        }

        protected virtual void DrawKeyField(Rect cellRect, T2 keyItem)
        {
            var keyFieldRect = new Rect(cellRect.x, cellRect.y, cellRect.width - 20, cellRect.height);
            var removeKeyButtonRect = new Rect(keyFieldRect.xMax, cellRect.y, 20, cellRect.height);

            EditorGUI.BeginChangeCheck();
            var newKey = EditorGUI.TextArea(keyFieldRect, keyItem.Key);
            if(EditorGUI.EndChangeCheck())
            {
                if (IsKeyUsed(newKey))
                {
                    Debug.LogWarningFormat("Cannot rename key {0} to {1}. Key must be unique and this one has already been used.", keyItem.Key, newKey);
                }
                else
                {
                    foreach (var addressableAssetTable in Tables)
                    {
                        Undo.RecordObject(addressableAssetTable, "Rename table key");
                        addressableAssetTable.ReplaceKey(keyItem.Key, newKey);
                        EditorUtility.SetDirty(addressableAssetTable);
                    }
                    keyItem.Key = newKey;
                    RefreshCustomRowHeights();
                }
            }

            if (GUI.Button(removeKeyButtonRect, "-"))
            {
                foreach (var addressableAssetTable in Tables)
                {
                    Undo.RecordObject(addressableAssetTable, "Remove table key");
                    addressableAssetTable.RemoveKey(keyItem.Key);
                    EditorUtility.SetDirty(addressableAssetTable);
                }
                Reload();
            }
        }

        /// <summary>
        /// Draws a field for adding a new key.
        /// </summary>
        protected virtual void DrawNewKeyField(Rect cellRect)
        {
            var newKeyFieldRect = new Rect(cellRect.x, cellRect.y, cellRect.width - 20, cellRect.height);
            var addKeyButtonRect = new Rect(newKeyFieldRect.xMax, cellRect.y, 20, cellRect.height);

            NewKey = EditorGUI.TextArea(newKeyFieldRect, NewKey);

            bool isKeyUsed = IsKeyUsed(NewKey);
            using (new EditorGUI.DisabledGroupScope(string.IsNullOrEmpty(NewKey) || isKeyUsed))
            {
                if (GUI.Button(addKeyButtonRect, new GUIContent("+", isKeyUsed ? "Can not add a duplicate key" : string.Empty)))
                {
                    AddNewKey(NewKey);
                    Reload();
                    NewKey = string.Empty;
                }
            }
        }

        protected virtual void AddNewKey(string key)
        {
            // Add the key to all tables
            foreach (var addressableAssetTable in Tables)
            {
                Undo.RecordObject(addressableAssetTable, "Add table key");
                addressableAssetTable.AddKey(key);
                EditorUtility.SetDirty(addressableAssetTable);
            }
        }

        /// <summary>
        /// Is the key already used in one or more of the tables?
        /// </summary>
        public bool IsKeyUsed(string key)
        {
            foreach (var item in rootItem.children)
            {
                if (item.id == k_AddItemId)
                    continue;

                var genricItem = item as GenericAssetTableTreeViewItem;
                if(genricItem != null && genricItem.Key.Equals(key))
                    return true;
            }

            return false;
        }

        protected abstract void DrawItemField(Rect cellRect, int col, T2 item, T1 table);
    }
}