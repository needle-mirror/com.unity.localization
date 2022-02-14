using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using UnityEngine.Pool;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using TreeView = UnityEditor.IMGUI.Controls.TreeView;

namespace UnityEditor.Localization.UI
{
    interface ISelectable
    {
        bool Selected { get; set; }

        VisualElement CreateEditor();
    }

    abstract class GenericAssetTableListView<T1, T2> : TreeView, IDisposable
        where T1 : LocalizationTable
        where T2 : GenericAssetTableTreeViewItem<T1>, new()
    {
        const string k_DragId = "GenericAssetTableListViewDragging";
        const string k_CurrentPagePref = "Localization-CurrentPage";
        const string k_PageSizePref = "Localization-PageSize";
        const string k_Search = "Localization-TablesView-Search";
        const int k_DefaultPageSize = 50;
        const int k_TableStartIndex = 2; // Key, Key Id and then tables

        static readonly GUIContent k_NewEntry = EditorGUIUtility.TrTextContent("Add New Entry");
        static readonly GUIContent k_PrevPage = new GUIContent(EditorGUIUtility.IconContent("Animation.PrevKey").image as Texture2D);
        static readonly GUIContent k_NextPage = new GUIContent(EditorGUIUtility.IconContent("Animation.NextKey").image as Texture2D);
        static readonly GUIContent k_FirstPage = new GUIContent(EditorGUIUtility.IconContent("Animation.FirstKey").image as Texture2D);
        static readonly GUIContent k_LastPage = new GUIContent(EditorGUIUtility.IconContent("Animation.LastKey").image as Texture2D);
        static readonly GUIContent k_PageSize = new GUIContent(EditorGUIUtility.TrTextContent("Page Size"));

        protected string TableCollectionName => TableCollection.TableCollectionName;

        public LocalizationTableCollection TableCollection { get; private set; }

        public ISelectable Selected
        {
            get => m_Selected;
            set
            {
                if (m_Selected != null)
                    m_Selected.Selected = false;

                // Toggle?
                if (m_Selected == value)
                    value = null;

                m_Selected = value;

                if (m_Selected != null)
                {
                    m_Selected.Selected = true;
                }

                SelectedForEditing?.Invoke(m_Selected);
            }
        }

        int PageSize
        {
            get => m_PageSize;
            set
            {
                var clampedValue = Mathf.Clamp(value, 1, 1000);
                if (clampedValue != m_PageSize)
                {
                    m_PageSize = clampedValue;
                    TotalPages = Mathf.CeilToInt(TableCollection.SharedData.Entries.Count / (float)m_PageSize);
                    m_CurrentPage = Mathf.Clamp(value, 1, TotalPages);
                    EditorPrefs.SetInt(k_PageSizePref, m_PageSize);
                    ReloadPages();
                }
            }
        }

        int CurrentPage
        {
            get => m_CurrentPage;
            set
            {
                var clampedValue = Mathf.Clamp(value, 1, TotalPages);
                if (clampedValue != m_CurrentPage)
                {
                    m_CurrentPage = clampedValue;
                    EditorPrefs.SetInt(k_CurrentPagePref, m_CurrentPage);
                    ReloadPages();
                }
            }
        }

        int TotalPages
        {
            get => m_TotalPages;
            set
            {
                m_TotalPages = value;
                m_TotalPagesLabel = new GUIContent($"/{value}");
            }
        }

        public delegate void SelectedDelegate(ISelectable selected);
        public event SelectedDelegate SelectedForEditing;

        const int k_AddItemId = int.MaxValue;

        ISelectable m_Selected;
        TreeViewItem m_AddKeyItem;
        SearchField m_SearchField;

        int m_CurrentPage = -1;
        int m_PageSize = -1;
        int m_TotalPages = -1;
        GUIContent m_TotalPagesLabel;

        List<TreeViewItem> m_AllItems = new List<TreeViewItem>();
        List<TreeViewItem> m_VisibleRows = new List<TreeViewItem>();
        TreeViewItem m_Root;

        List<LocalizationTable> m_SortedTables = new List<LocalizationTable>();

        protected GenericAssetTableListView(LocalizationTableCollection tableCollection) :
            base(new TreeViewState())
        {
            TableCollection = tableCollection;
            m_SearchField = new SearchField();
            m_SearchField.downOrUpArrowKeyPressed += SetFocusAndEnsureSelectedItem;
            searchString = SessionState.GetString(k_Search, string.Empty);
            Undo.undoRedoPerformed += UndoRedoPerformed;
            LocalizationEditorSettings.EditorEvents.TableEntryAdded += EditorEvents_TableEntryModified;
            LocalizationEditorSettings.EditorEvents.TableEntryRemoved += EditorEvents_TableEntryModified;
            LocalizationEditorSettings.EditorEvents.AssetTableEntryAdded += EditorEvents_AssetTableEntryAdded;
            LocalizationEditorSettings.EditorEvents.AssetTableEntryRemoved += EditorEvents_AssetTableEntryRemoved;
            LocalizationEditorSettings.EditorEvents.TableAddedToCollection += EditorEvents_TableAddedOrRemoveFromCollection;
            LocalizationEditorSettings.EditorEvents.TableRemovedFromCollection += EditorEvents_TableAddedOrRemoveFromCollection;
            LocalizationEditorSettings.EditorEvents.CollectionModified += EditorEvents_CollectionModified;
            LocalizationEditorSettings.EditorEvents.LocaleSortOrderChanged += EditorEvents_LocaleSortOrderChanged;

            m_PageSize = EditorPrefs.GetInt(k_PageSizePref, k_DefaultPageSize);
            m_CurrentPage = EditorPrefs.GetInt(k_CurrentPagePref, 1);

            rowHeight = EditorStyles.textArea.lineHeight;
        }

        ~GenericAssetTableListView()
        {
            Dispose();
        }

        public void Dispose()
        {
            Undo.undoRedoPerformed -= UndoRedoPerformed;
            LocalizationEditorSettings.EditorEvents.TableEntryAdded -= EditorEvents_TableEntryModified;
            LocalizationEditorSettings.EditorEvents.TableEntryRemoved -= EditorEvents_TableEntryModified;
            LocalizationEditorSettings.EditorEvents.AssetTableEntryAdded -= EditorEvents_AssetTableEntryAdded;
            LocalizationEditorSettings.EditorEvents.AssetTableEntryRemoved -= EditorEvents_AssetTableEntryRemoved;
            LocalizationEditorSettings.EditorEvents.TableAddedToCollection -= EditorEvents_TableAddedOrRemoveFromCollection;
            LocalizationEditorSettings.EditorEvents.TableRemovedFromCollection -= EditorEvents_TableAddedOrRemoveFromCollection;
            LocalizationEditorSettings.EditorEvents.CollectionModified -= EditorEvents_CollectionModified;
            LocalizationEditorSettings.EditorEvents.LocaleSortOrderChanged -= EditorEvents_LocaleSortOrderChanged;
        }

        void EditorEvents_CollectionModified(object sender, LocalizationTableCollection collection)
        {
            if (sender != this && collection == TableCollection)
            {
                // Tables may have been modified.
                InitializeColumns();
                Reload();
            }
        }

        void EditorEvents_TableEntryModified(LocalizationTableCollection collection, SharedTableData.SharedTableEntry entry)
        {
            if (collection == TableCollection)
                Reload();
        }

        void EditorEvents_AssetTableEntryAdded(AssetTableCollection collection, AssetTable table, AssetTableEntry entry)
        {
            if (collection == TableCollection)
            {
                // If the changed item is being displayed then force a refresh.
                var item = rootItem.children.FirstOrDefault(tbl => tbl is AssetTableTreeViewItem at && at.KeyId == entry.KeyId) as AssetTableTreeViewItem;
                item?.RefreshFields();
            }
        }

        void EditorEvents_AssetTableEntryRemoved(AssetTableCollection collection, AssetTable table, AssetTableEntry entry, string removedAssetGuid)
        {
            if (collection == TableCollection)
            {
                // If the changed item is being displayed then force a refresh.
                var item = rootItem.children.FirstOrDefault(tbl => tbl is AssetTableTreeViewItem at && at.KeyId == entry.KeyId) as AssetTableTreeViewItem;
                item?.RefreshFields();
            }
        }

        void EditorEvents_TableAddedOrRemoveFromCollection(LocalizationTableCollection collection, LocalizationTable table)
        {
            if (collection == TableCollection)
            {
                Initialize();
                Reload();
            }
        }

        void EditorEvents_LocaleSortOrderChanged(object sender, Locale locale)
        {
            InitializeColumns();
            Reload();
        }

        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
            if (item is T2 i)
            {
                // Height of key field
                return EditorStyles.textArea.CalcSize(new GUIContent(i.Key)).y;
            }
            return base.GetCustomRowHeight(row, item);
        }

        protected virtual void UndoRedoPerformed()
        {
            RefreshCustomRowHeights();

            if (TableCollection != null && Selected is TableEntrySelected entrySelected)
            {
                // If the selected entry was removed by an undo operation then deselect it.
                if (!TableCollection.SharedData.Contains(entrySelected.KeyId))
                    Selected = null;
            }
        }

        public virtual void Initialize()
        {
            InitializeColumns();
            Reload();
            multiColumnHeader.sortingChanged += mch => Reload();
        }

        protected override bool CanMultiSelect(TreeViewItem item) => false; // Disable multi select

        protected virtual void InitializeColumns()
        {
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            var keys = TableCollection.SharedData;

            var columns = new List<MultiColumnHeaderState.Column>
            {
                new KeyColumn(keys),
                new KeyIdColumn(keys)
            };

            var localesWithNoMatchingTable = ListPool<Locale>.Get();
            var projectTables = ListPool<LazyLoadReference<LocalizationTable>>.Get();
            projectTables.AddRange(TableCollection.Tables);
            m_SortedTables.Clear();

            // Add the tables in the order of Locales so Locale sorting order is respected.
            foreach (var locale in LocalizationEditorSettings.GetLocales())
            {
                var matchingTableIdx = projectTables.FindIndex(tbl => tbl.asset?.LocaleIdentifier.Code == locale.Identifier.Code);
                if (matchingTableIdx != -1)
                {
                    var table = projectTables[matchingTableIdx];
                    m_SortedTables.Add(table.asset);
                    projectTables.RemoveAt(matchingTableIdx);
                    columns.Add(new TableColumn<T1>(TableCollection, table.asset, locale));
                }
                else
                {
                    localesWithNoMatchingTable.Add(locale);
                }
            }

            // We need to add the remaining tables that do not have any Locales.
            for (int i = 0; i < projectTables.Count; ++i)
                m_SortedTables.Add(projectTables[i].asset);

            // Tables with missing Locales
            projectTables.ForEach(tbl => columns.Add(new TableColumn<T1>(TableCollection, tbl.asset, null)));

            // Locales with no matching tables
            localesWithNoMatchingTable.ForEach(l => columns.Add(new MissingTableColumn(l)));

            var multiColState = new MultiColumnHeaderState(columns.ToArray());
            multiColumnHeader = new GenericAssetTableListViewMultiColumnHeader<T1, T2>(multiColState, this, TableCollection);
            multiColumnHeader.visibleColumnsChanged += (header) => RefreshCustomRowHeights();
            multiColumnHeader.ResizeToFit();

            ListPool<Locale>.Release(localesWithNoMatchingTable);
            ListPool<LazyLoadReference<LocalizationTable>>.Release(projectTables);
        }

        protected virtual T2 CreateTreeViewItem(int index, SharedTableData.SharedTableEntry entry)
        {
            var item = new T2() { id = index, SharedEntry = entry };
            item.Initialize(TableCollection, k_TableStartIndex, m_SortedTables);
            return item;
        }

        void ReloadPages()
        {
            m_Root = rootItem;
            Reload();
            m_Root = null;
        }

        protected override TreeViewItem BuildRoot()
        {
            if (m_Root != null)
                return m_Root;

            var root = new TreeViewItem(-1, -1, "root");
            m_AllItems.Clear();

            if (TableCollection.SharedData == null)
            {
                Debug.LogError($"No {nameof(SharedTableData)} assigned to Table: " + TableCollectionName);
                SetupParentsAndChildrenFromDepths(root, m_AllItems);
                return root;
            }

            var sharedEntries = TableCollection.SharedData.Entries;

            // Apply Sorting?
            if (multiColumnHeader.sortedColumnIndex >= 0)
            {
                var ascend = multiColumnHeader.IsSortedAscending(multiColumnHeader.sortedColumnIndex);
                if (multiColumnHeader.sortedColumnIndex == 0)
                    sharedEntries.Sort((a, b) => ascend ? string.Compare(b.Key, a.Key) : string.Compare(a.Key, b.Key));
                else if (multiColumnHeader.sortedColumnIndex == 1)
                    sharedEntries.Sort((a, b) => ascend ? b.Id.CompareTo(a.Id) : a.Id.CompareTo(b.Id));
            }

            for (int i = 0; i < sharedEntries.Count; ++i)
            {
                var tvi = CreateTreeViewItem(i, sharedEntries[i]);
                m_AllItems.Add(tvi);
            }

            // At the end we add an extra node which will be used to add new keys.
            m_AddKeyItem = new GenericAssetTableTreeViewItem<T1>() { id = k_AddItemId, displayName = "Add Key" };
            m_AllItems.Add(m_AddKeyItem);
            SetupParentsAndChildrenFromDepths(root, m_AllItems);
            return root;
        }

        protected virtual Rect DrawSearchField(Rect rect)
        {
            // Apply a small border around the search field
            const float borderWidth = 2;
            const float borderHeight = 1;
            var searchRect = new Rect(rect.x + borderWidth, rect.y + borderHeight, rect.width - (2 * borderWidth), EditorGUIUtility.singleLineHeight);
            EditorGUI.BeginChangeCheck();
            searchString = m_SearchField.OnToolbarGUI(searchRect, searchString);
            if (EditorGUI.EndChangeCheck())
            {
                SessionState.SetString(k_Search, searchString);
            }
            rect.yMin += EditorGUIUtility.singleLineHeight + (2 * borderHeight);
            return rect;
        }

        void DrawPageControls(Rect rect)
        {
            var buttonWidth = GUILayout.Width(30);

            GUILayout.BeginArea(rect);
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button(new GUIContent(k_FirstPage), EditorStyles.toolbarButton, buttonWidth))
            {
                CurrentPage = 0;
            }
            if (GUILayout.Button(new GUIContent(k_PrevPage), EditorStyles.toolbarButton, buttonWidth))
            {
                CurrentPage--;
            }

            var size = EditorStyles.textField.CalcSize(new GUIContent(CurrentPage.ToString()));
            EditorGUI.BeginChangeCheck();
            var page = EditorGUILayout.DelayedIntField(CurrentPage, EditorStyles.toolbarTextField, GUILayout.ExpandWidth(false), GUILayout.Width(size.x));
            if (EditorGUI.EndChangeCheck())
            {
                CurrentPage = page;
            }

            var totalePagesWidth = EditorStyles.textField.CalcSize(m_TotalPagesLabel).x;
            GUILayout.Label(m_TotalPagesLabel, GUILayout.Width(totalePagesWidth));
            if (GUILayout.Button(new GUIContent(k_NextPage), EditorStyles.toolbarButton, buttonWidth))
            {
                CurrentPage++;
            }
            if (GUILayout.Button(new GUIContent(k_LastPage), EditorStyles.toolbarButton, buttonWidth))
            {
                CurrentPage = TotalPages;
            }

            GUILayout.FlexibleSpace();
            EditorGUI.BeginChangeCheck();

            EditorGUI.BeginChangeCheck();
            PageSize = EditorGUILayout.DelayedIntField(k_PageSize, PageSize, EditorStyles.toolbarTextField);

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        public override void OnGUI(Rect rect)
        {
            rect = DrawSearchField(rect);

            rect.yMax -= 20;
            base.OnGUI(rect);

            rect.y = rect.yMax;
            rect.height = 20;
            DrawPageControls(rect);
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                var cellRect = args.GetCellRect(i);
                var colId = args.GetColumn(i);
                var col = multiColumnHeader.GetColumn(colId);

                if (args.item.id == k_AddItemId)
                {
                    if (colId == 0)
                    {
                        DrawNewKeyField(cellRect);
                    }
                }
                else
                {
                    switch (col)
                    {
                        case KeyColumn _:
                            DrawKeyField(cellRect, args.item as T2);
                            break;
                        case KeyIdColumn _:
                            DrawKeyIdField(cellRect, args.item as T2);
                            break;
                        case TableColumn<T1> tc:
                            DrawItemField(cellRect, colId, tc, args.item as T2);
                            break;
                        case MissingTableColumn mtc:
                            DrawMissingTableField(cellRect, colId, mtc);
                            break;
                        default:
                            Debug.LogError($"Unexpected column type \"{col.GetType().Name}\"");
                            break;
                    }
                }
            }
        }

        protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
        {
            return item.id != k_AddItemId && base.DoesItemMatchSearch(item, search); // Ignore add button
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            var rows = base.BuildRows(root);
            if (hasSearch)
                rows.Add(m_AddKeyItem);

            TotalPages = Mathf.CeilToInt(rows.Count / (float)m_PageSize);
            m_CurrentPage = Mathf.Clamp(m_CurrentPage, 1, TotalPages);

            m_VisibleRows.Clear();
            var startIndex = (m_CurrentPage - 1) * m_PageSize;
            var endIndex = Mathf.Min(startIndex + m_PageSize, rows.Count);
            for (int i = startIndex; i < endIndex; i++)
            {
                m_VisibleRows.Add(rows[i]);
            }

            return m_VisibleRows;
        }

        protected virtual void DrawKeyIdField(Rect cellRect, T2 keyItem) => EditorGUI.LabelField(cellRect, keyItem.KeyId.ToString());

        protected virtual void DrawKeyField(Rect cellRect, T2 keyItem)
        {
            var keyFieldRect = new Rect(cellRect.x, cellRect.y, cellRect.width - 20, cellRect.height);
            var removeKeyButtonRect = new Rect(keyFieldRect.xMax, cellRect.y, 20, cellRect.height);

            EditorGUI.BeginChangeCheck();
            var newKey = EditorGUI.TextArea(keyFieldRect, keyItem.Key);
            if (EditorGUI.EndChangeCheck())
            {
                if (TableCollection.SharedData.Contains(newKey))
                {
                    Debug.LogWarningFormat("Cannot rename key {0} to {1}. Key must be unique and this one has already been used.", keyItem.Key, newKey);
                }
                else
                {
                    Undo.RecordObject(TableCollection.SharedData, "Rename key");
                    TableCollection.SharedData.RenameKey(keyItem.Key, newKey);
                    EditorUtility.SetDirty(TableCollection.SharedData);
                    RefreshCustomRowHeights();
                }
            }

            if (GUI.Button(removeKeyButtonRect, "-"))
            {
                // Remove any selections (LOC-296)
                Selected = null;

                var objects = new Object[TableCollection.Tables.Count + 1];
                for (int i = 0; i < TableCollection.Tables.Count; ++i)
                {
                    objects[i] = TableCollection.Tables[i].asset;
                }
                objects[TableCollection.Tables.Count] = TableCollection.SharedData;
                Undo.RecordObjects(objects, "Remove key from collection");
                keyItem.OnDeleteKey();
                TableCollection.RemoveEntry(keyItem.KeyId);

                foreach (var o in objects)
                    EditorUtility.SetDirty(o);

                Reload();
            }
        }

        /// <summary>
        /// Draws a field for adding a new key.
        /// </summary>
        protected virtual void DrawNewKeyField(Rect cellRect)
        {
            if (GUI.Button(cellRect, k_NewEntry))
            {
                searchString = String.Empty;
                SessionState.SetString(k_Search, searchString);

                AddNewKey();
                var s = state;
                s.scrollPos += new Vector2(0, 100);
                Reload();

                CurrentPage = TotalPages;
                var index = TableCollection.SharedData.Entries.Count - 1;
                SetSelection(new int[] { index }, TreeViewSelectionOptions.RevealAndFrame);
            }
        }

        protected virtual void DrawMissingTableField(Rect cellRect, int colIdx, MissingTableColumn col)
        {
            // Just a blank field by default.
        }

        protected virtual void AddNewKey()
        {
            Undo.RecordObject(TableCollection.SharedData, "Add new key");
            TableCollection.SharedData.AddKey();
            EditorUtility.SetDirty(TableCollection.SharedData);
        }

        protected abstract void DrawItemField(Rect cellRect, int colIdx, TableColumn<T1> col, T2 item);

        protected override bool CanStartDrag(CanStartDragArgs args) => true;

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            var draggedRows = args.draggedItemIDs;

            if (draggedRows.Count == 1)
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.SetGenericData(k_DragId, draggedRows[0]);
                DragAndDrop.objectReferences = new UnityEngine.Object[] {};   // this is required for dragging to work
                DragAndDrop.StartDrag("Move Key");
            }
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            if (args.dragAndDropPosition == DragAndDropPosition.OutsideItems)
                return DragAndDropVisualMode.None;

            // Check if we can handle the current drag data (could be dragged in from other areas/windows in the editor)
            var dragDropData = DragAndDrop.GetGenericData(k_DragId);
            if (dragDropData == null)
                return DragAndDropVisualMode.None;

            var itemIndex = (int)dragDropData;
            if (args.dragAndDropPosition != DragAndDropPosition.BetweenItems)
                return DragAndDropVisualMode.None;

            // Don't allow insertion on either side of the selected item, it results in no change.
            if (itemIndex == args.insertAtIndex || args.insertAtIndex == itemIndex + 1)
                return DragAndDropVisualMode.Rejected;

            if (args.performDrop)
            {
                var sharedData = TableCollection.SharedData;
                Undo.RecordObject(sharedData, "Move Key");

                var newIndex = Mathf.Clamp(args.insertAtIndex, 0, sharedData.Entries.Count);
                sharedData.Entries.Insert(newIndex, sharedData.Entries[itemIndex]);
                if (newIndex <= itemIndex)
                    ++itemIndex;
                sharedData.Entries.RemoveAt(itemIndex);
                SetSelection(new[] { newIndex });
                Reload();
            }
            return DragAndDropVisualMode.Move;
        }
    }
}
