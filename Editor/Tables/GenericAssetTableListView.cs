using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Localization;
using System.Linq;
using System.IO;

namespace UnityEditor.Localization
{
    public class GenericAssetTableTreeViewItem : TreeViewItem
    {
        public virtual string SearchString
        {
            get => displayName;
            set => displayName = value;
        }

        public virtual KeyDatabase.KeyDatabaseEntry KeyEntry { get; set; }

        public string Key
        {
            get => KeyEntry.Key;
            set => KeyEntry.Key = value;
        }

        public uint KeyId => KeyEntry.Id;
    }

    public abstract class GenericAssetTableListView<T1, T2> : TreeView 
        where T1 : LocalizedTable
        where T2 : GenericAssetTableTreeViewItem, new()
    {
        public class GenericAssetTableListViewMultiColumnHeader : MultiColumnHeader
        {
            public GenericAssetTableListViewMultiColumnHeader(MultiColumnHeaderState state)
                : base(state)
            {
            }

            protected override void ColumnHeaderGUI(MultiColumnHeaderState.Column column, Rect headerRect, int columnIndex)
            {
                base.ColumnHeaderGUI(column, headerRect, columnIndex);

                if (column is MissingTableColumn mtc)
                {
                    var labelRect = new Rect(headerRect.x, headerRect.yMax - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing, headerRect.width, EditorGUIUtility.singleLineHeight);
                    if (GUI.Button(labelRect, mtc.Label))
                    {
                        CreateMissingTable(mtc);
                    }
                }
            }

            void CreateMissingTable(MissingTableColumn mtc)
            {
                var tableToCopy = ((TableColumn)state.columns.First(col => col is TableColumn)).Table;
                string tableToCopyDir = Path.GetDirectoryName(AssetDatabase.GetAssetPath(tableToCopy));
                LocalizationEditorSettings.CreateAssetTableFilePanel(mtc.TableLocale, tableToCopy.Keys, tableToCopy.TableName, tableToCopy.GetType(), tableToCopyDir);
            }
        }

        public class TableColumn : MultiColumnHeaderState.Column
        {
            public T1 Table { get; }

            public Locale TableLocale { get; }

            public TableColumn(T1 table, Locale locale)
            {
                Table = table;
                TableLocale = locale;
                headerContent = new GUIContent(TableLocale != null ? TableLocale.ToString() : table.LocaleIdentifier.Code);
                headerTextAlignment = TextAlignment.Center;
                canSort = false;
                allowToggleVisibility = true;
            }

            public override string ToString() => "Table: " + Table;
        }

        public class MissingTableColumn : MultiColumnHeaderState.Column
        {
            public Locale TableLocale { get; }

            public GUIContent Label { get; }

            public GenericAssetTableListView<T1, T2> TreeView { get; }

            public MissingTableColumn(Locale locale, GenericAssetTableListView<T1, T2> treeView)
            {
                TreeView = treeView;
                TableLocale = locale;
                Label = new GUIContent("Add " + TableLocale);
                contextMenuText = "Add Missing Tables";
                headerTextAlignment = TextAlignment.Center;
                canSort = false;
                allowToggleVisibility = true;
            }

            public override string ToString() => "Missing Table: " + TableLocale;
        }

        SearchField m_SearchField;

        protected string NewKey { get; set; }

        public List<T1> Tables
        {
            get => m_Tables;
            set
            {
                m_Tables = value;
                if (m_Tables != null && m_Tables.Count > 0)
                    Keys = m_Tables[0].Keys;
            }
        }

        protected string TableName => Tables.Count > 0 ? Tables[0].TableName : null;

        public KeyDatabase Keys { get; private set; }

        const int k_AddItemId = int.MaxValue;

        protected const int k_InitialColumns = 2; // Key Name and Id

        TreeViewItem m_AddKeyItem;

        List<T1> m_Tables;

        public new virtual float totalHeight => base.totalHeight + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        protected GenericAssetTableListView() :
            base(new TreeViewState())
        {
            m_SearchField = new SearchField();
            m_SearchField.downOrUpArrowKeyPressed += SetFocusAndEnsureSelectedItem;
            LocalizationEditorSettings.OnModification += LocalizationEditorSettingsModified;
        }

        ~GenericAssetTableListView()
        {
            LocalizationEditorSettings.OnModification -= LocalizationEditorSettingsModified;
        }

        void LocalizationEditorSettingsModified(LocalizationEditorSettings.ModificationEvent evt, object obj)
        {
            var table = obj as T1;
            if (table != null && table.TableName == TableName)
            {
                if (evt == LocalizationEditorSettings.ModificationEvent.TableAdded)
                {
                    Tables.Add(table);
                    Initialize();
                }
                else if (evt == LocalizationEditorSettings.ModificationEvent.TableRemoved)
                {
                    Tables.Remove(table);
                    Initialize();
                }
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

            var columns = new List<MultiColumnHeaderState.Column>
            {
                new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent("Key"),
                    headerTextAlignment = TextAlignment.Center,
                    canSort = true,
                    allowToggleVisibility = false,
                    sortedAscending = true
                },
                new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent("Key Id"),
                    headerTextAlignment = TextAlignment.Center,
                    canSort = true,
                    allowToggleVisibility = true,
                    sortedAscending = true
                }
            };

            // Update column labels if possible
            var locales = LocalizationEditorSettings.GetLocales();
            for (int i = 0; i < Tables.Count; ++i)
            {
                var foundLocale = locales.FirstOrDefault(o => o.Identifier.Code == Tables[i].LocaleIdentifier.Code);
                locales.Remove(foundLocale);
                columns.Add(new TableColumn(Tables[i], foundLocale));
            }

            // Add columns for the missing locales.
            locales.ForEach(l => columns.Add(new MissingTableColumn(l, this)));

            var multiColState = new MultiColumnHeaderState(columns.ToArray());

            // Hide the Key Id column by default.
            var visibleColumns = new int[columns.Count - 1];
            for (int i = 1; i < visibleColumns.Length; ++i)
                visibleColumns[i] = i + 1;

            multiColState.visibleColumns = visibleColumns;

            multiColumnHeader = new GenericAssetTableListViewMultiColumnHeader(multiColState);
            multiColumnHeader.ResizeToFit();
        }

        protected virtual T2 CreateTreeViewItem(int index, KeyDatabase.KeyDatabaseEntry keyEntry) => new T2() { id = index, KeyEntry = keyEntry };

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem(-1, -1, "root");
            var items = new List<TreeViewItem>();

            if (Tables == null || Tables.Count == 0)
            {
                SetupParentsAndChildrenFromDepths(root, items);
                return root;
            }

            Keys = Tables[0].Keys;
            if (!Tables.TrueForAll(tbl => tbl.Keys == Keys))
                Debug.LogError("Tables must use the same KeyDatabase.");

            if (Keys == null)
            {
                Debug.LogError("No KeyDatabase assigned to Table: " + TableName);
                SetupParentsAndChildrenFromDepths(root, items);
                return root;
            }

            var keys = Keys.Entries;

            // Apply Sorting?
            if (multiColumnHeader.sortedColumnIndex >= 0)
            {
                var ascend = multiColumnHeader.IsSortedAscending(multiColumnHeader.sortedColumnIndex);
                if (multiColumnHeader.sortedColumnIndex == 0)
                    keys.Sort((a, b) => ascend ? string.Compare(b.Key, a.Key) : string.Compare(a.Key, b.Key));
                else if (multiColumnHeader.sortedColumnIndex == 1)
                    keys.Sort((a, b) => ascend ? b.Id.CompareTo(a.Id) : a.Id.CompareTo(b.Id));
            }

            for (int i = 0; i < keys.Count; ++i)
            {
                var tvi = CreateTreeViewItem(i, keys[i]);
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

        public override void OnGUI(Rect rect) => base.OnGUI(DrawSearchField(rect));

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
                        return;
                    }

                }
                else
                {
                    switch (colId)
                    {
                        case 0:
                            DrawKeyField(cellRect, args.item as T2);
                            break;

                        case 1:
                            DrawKeyIdField(cellRect, args.item as T2);
                            break;

                        default:
                            if (col is TableColumn tc)
                            {
                                DrawItemField(cellRect, colId, tc, args.item as T2);
                            }
                            else
                            {
                                var mtc = col as MissingTableColumn;
                                Debug.Assert(mtc != null, "Unexpected column type.");
                                DrawMissingTableField(cellRect, colId, mtc);
                            }
                            break;
                    }
                }
            }
        }

        protected override bool DoesItemMatchSearch(TreeViewItem item, string search) => item.id != k_AddItemId && base.DoesItemMatchSearch(item, search); // Ignore add button

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            var rows = base.BuildRows(root);

            if (hasSearch)
                rows.Add(m_AddKeyItem);
            return rows;
        }

        protected virtual void DrawKeyIdField(Rect cellRect, T2 keyItem) => EditorGUI.LabelField(cellRect, keyItem.KeyId.ToString());

        protected virtual void DrawKeyField(Rect cellRect, T2 keyItem)
        {
            var keyFieldRect = new Rect(cellRect.x, cellRect.y, cellRect.width - 20, cellRect.height);
            var removeKeyButtonRect = new Rect(keyFieldRect.xMax, cellRect.y, 20, cellRect.height);

            EditorGUI.BeginChangeCheck();
            var newKey = EditorGUI.TextArea(keyFieldRect, keyItem.Key);
            if(EditorGUI.EndChangeCheck())
            {
                if (Keys.Contains(newKey))
                {
                    Debug.LogWarningFormat("Cannot rename key {0} to {1}. Key must be unique and this one has already been used.", keyItem.Key, newKey);
                }
                else
                {
                    Undo.RecordObject(Keys, "Rename key");
                    Keys.RenameKey(keyItem.Key, newKey);
                    EditorUtility.SetDirty(Keys);
                    RefreshCustomRowHeights();
                }
            }

            if (GUI.Button(removeKeyButtonRect, "-"))
            {
                Undo.RecordObject(Keys, "Remove key");
                Keys.RemoveKey(keyItem.KeyId);
                EditorUtility.SetDirty(Keys);
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

            var isKeyUsed = Keys.Contains(NewKey);
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

        protected virtual void DrawMissingTableField(Rect cellRect, int colIdx, MissingTableColumn col)
        {
            // Just a blank field by default.
        }

        protected virtual void AddNewKey(string key)
        {
            Undo.RecordObject(Keys, "Add new key");
            Keys.AddKey(key);
            EditorUtility.SetDirty(Keys);
        }

        protected abstract void DrawItemField(Rect cellRect, int colIdx, TableColumn col, T2 item);
    }
}