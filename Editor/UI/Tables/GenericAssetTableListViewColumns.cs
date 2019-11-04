﻿using System.IO;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Tables;
using UnityEngine.UIElements;

namespace UnityEditor.Localization.UI
{
    class GenericAssetTableListViewMultiColumnHeader<T1, T2> : MultiColumnHeader 
        where T1 : LocalizedTable
        where T2 : GenericAssetTableTreeViewItem<T1>, new()
    {
        const float k_MetadataLabelHeight = 20;

        static readonly GUIContent k_MissingTableMenuItem = new GUIContent("Missing Tables");

        static readonly GUIContent k_MetadataIcon = new GUIContent(AssetDatabase.LoadAssetAtPath<Texture>("Packages/com.unity.localization/Editor/Icons/Localization_AssetTable.png"), "Edit Table Metadata");

        AssetTableCollection TableCollection { get; set; }

        GenericAssetTableListView<T1, T2> m_Parent;

        public GenericAssetTableListViewMultiColumnHeader(MultiColumnHeaderState state, GenericAssetTableListView<T1, T2> parent, AssetTableCollection collection)
            : base(state)
        {
            height += k_MetadataLabelHeight;
            m_Parent = parent;
            TableCollection = collection;
        }

        protected override void AddColumnHeaderContextMenuItems(GenericMenu menu)
        {
            menu.AddItem(EditorGUIUtility.TrTextContent("Resize to Fit"), false, ResizeToFit);

            menu.AddSeparator("");

            for (int i = 0; i < state.columns.Length; ++i)
            {
                var column = state.columns[i];

                var menuText = column.headerContent.text;
                if (column.allowToggleVisibility)
                    menu.AddItem(new GUIContent(menuText), state.visibleColumns.Contains(i), ToggleVisibilityCallback, i);
                else if (column is MissingTableColumn)
                {
                    menu.AddSeparator("");
                    menu.AddItem(k_MissingTableMenuItem, IsColumnVisible(i), ToggleMissingTableColumnsVisibility);
                    break;
                }
            }
        }

        void ToggleVisibilityCallback(object userData)
        {
            int colIdx = (int)userData;
            var visibleCol = GetColumn(colIdx) as VisibleColumn;
            if (visibleCol != null)
                visibleCol.Visible = !visibleCol.Visible;
            ToggleVisibility(colIdx);
        }

        void ToggleMissingTableColumnsVisibility()
        {
            for (int i = 0; i < state.columns.Length; ++i)
            {
                var column = state.columns[i];
                if (column is MissingTableColumn)
                    ToggleVisibility(i);
            }
        }

        protected override void ColumnHeaderGUI(MultiColumnHeaderState.Column column, Rect headerRect, int columnIndex)
        {
            var btnRect = new Rect(headerRect.x, headerRect.y, headerRect.width, k_MetadataLabelHeight);
            headerRect.yMin += k_MetadataLabelHeight;
            switch (column)
            {
                case ISelectable selectable:
                    {
                        EditorGUI.BeginChangeCheck();
                        GUI.Toggle(btnRect, selectable.Selected, k_MetadataIcon, GUI.skin.button);
                        if (EditorGUI.EndChangeCheck())
                        {
                            m_Parent.Selected = selectable;
                        }
                        base.ColumnHeaderGUI(column, headerRect, columnIndex);
                        break;
                    }

                case VisibleColumn _:
                    base.ColumnHeaderGUI(column, headerRect, columnIndex);
                    break;

                case MissingTableColumn mtc:
                    {
                        var labelRect = new Rect(headerRect.x, headerRect.yMax - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing, headerRect.width, EditorGUIUtility.singleLineHeight);
                        if (GUI.Button(labelRect, mtc.headerContent))
                        {
                            CreateMissingTable(mtc);
                        }
                        break;
                    }
            }
        }

        void CreateMissingTable(MissingTableColumn mtc)
        {
            var tableToCopyPath = TableCollection.TableEntries[0].AssetPath;
            string tableToCopyDir = Path.GetDirectoryName(tableToCopyPath);
            LocalizationEditorSettings.CreateAssetTableFilePanel(mtc.TableLocale, TableCollection.Keys, TableCollection.TableName, TableCollection.TableType, tableToCopyDir);
        }
    }

    class VisibleColumn : MultiColumnHeaderState.Column
    {
        const string k_ColumnVisiblePref = "Localization-Table-Window-{0}";

        public bool defaultVisibility { get; set; } = true;

        public bool Visible
        {
            get => EditorPrefs.GetBool(string.Format(k_ColumnVisiblePref, headerContent.text), defaultVisibility);
            set => EditorPrefs.SetBool(string.Format(k_ColumnVisiblePref, headerContent.text), value);
        }
    }

    class TableColumn<T1> : VisibleColumn, ISelectable  where T1 : LocalizedTable
    {
        public T1 Table { get; set; }
        public SerializedObject SerializedObjectTable{ get; private set; }
        public SerializedObject SerializedObjectKeyDatabase{ get; private set; }

        public Locale TableLocale { get; set; }
        public bool Selected { get; set; }

        AssetTableCollection m_TableCollection;
        bool m_PreloadAllMixedValues;
        bool m_PreloadAllToggle;
        bool m_PreloadSelectedToggle;

        public TableColumn(AssetTableCollection collection, LocalizedTable table, Locale locale)
        {
            m_TableCollection = collection;
            minWidth = 100;
            Table = table as T1;
            SerializedObjectTable = new SerializedObject(Table);
            SerializedObjectKeyDatabase = new SerializedObject(Table.Keys);
            TableLocale = locale;
            headerContent = new GUIContent(TableLocale != null ? TableLocale.ToString() : table.LocaleIdentifier.Code);
            headerTextAlignment = TextAlignment.Center;
            canSort = false;
            allowToggleVisibility = true;
            Undo.undoRedoPerformed += RefreshPreloadToggle;
        }

        ~TableColumn()
        {
            Undo.undoRedoPerformed -= RefreshPreloadToggle;
        }

        void RefreshPreloadToggle()
        {
            int preloadCount = m_TableCollection.Tables.Count(LocalizationEditorSettings.GetPreloadTableFlag);
            if (preloadCount == 0)
            {
                m_PreloadAllToggle = false;
                m_PreloadAllMixedValues = false;
                m_PreloadSelectedToggle = false;
            }
            else if (preloadCount == m_TableCollection.Tables.Count)
            {
                m_PreloadAllToggle = true;
                m_PreloadAllMixedValues = false;
                m_PreloadSelectedToggle = true;
            }
            else
            {
                m_PreloadAllToggle = true;
                m_PreloadAllMixedValues = true;
                m_PreloadSelectedToggle = LocalizationEditorSettings.GetPreloadTableFlag(Table);
            }
        }

        public VisualElement CreateEditor()
        {
            var root = new VisualElement(){ style = { marginLeft = 5, marginRight = 5, marginTop = 5, marginBottom = 5 } };

            var preloadEditor = new IMGUIContainer(() =>
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = m_PreloadAllMixedValues;
                m_PreloadAllToggle = EditorGUILayout.Toggle("Preload All Tables", m_PreloadAllToggle);
                EditorGUI.showMixedValue = false;
                if (EditorGUI.EndChangeCheck())
                {
                    var group = Undo.GetCurrentGroup();
                    Undo.SetCurrentGroupName("Set Preload flag");
                    foreach (var table in m_TableCollection.Tables)
                    {
                        LocalizationEditorSettings.SetPreloadTableFlag(table, m_PreloadAllToggle, true);
                    }
                    Undo.CollapseUndoOperations(group);
                    RefreshPreloadToggle();
                }

                EditorGUI.BeginChangeCheck();
                m_PreloadSelectedToggle = EditorGUILayout.Toggle("Preload Table", m_PreloadSelectedToggle);
                if (EditorGUI.EndChangeCheck())
                {
                    LocalizationEditorSettings.SetPreloadTableFlag(Table, m_PreloadSelectedToggle, true);
                    RefreshPreloadToggle();
                }
            });
            preloadEditor.AddToClassList("unity-box");
            root.Add(preloadEditor);

            var keyDatabaseEditor = new IMGUIContainer(() =>
            {
                SerializedObjectKeyDatabase.Update();
                EditorGUILayout.LabelField("Shared", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(SerializedObjectKeyDatabase.FindProperty("m_Metadata"));
                SerializedObjectKeyDatabase.ApplyModifiedProperties();
            });
            keyDatabaseEditor.style.borderBottomWidth = 10;
            root.Add(keyDatabaseEditor);

            RefreshPreloadToggle();

            // TODO: Clean this up so we have editors for each type.
            var tablePropDrawer = new MetadataCollectionField();
            tablePropDrawer.Type = new MetadataTypeAttribute(Table is AssetTable ? MetadataType.AssetTable : MetadataType.StringTable);
            var label = new GUIContent("Metadata");
            var tableEditor = new IMGUIContainer(() =>
            {
                SerializedObjectTable.Update();
                EditorGUILayout.LabelField(headerContent, EditorStyles.boldLabel);

                var p = SerializedObjectTable.FindProperty("m_Metadata");
                var rect = EditorGUILayout.GetControlRect(true, tablePropDrawer.GetPropertyHeight(p, label));
                tablePropDrawer.OnGUI(rect, p, label);
                SerializedObjectTable.ApplyModifiedProperties();
            });
            root.Add(tableEditor);

            return root;
        }

        public override string ToString() => "Table: " + Table;
    }

    class KeyColumn : VisibleColumn
    {
        public KeyDatabase Keys { get; }

        public KeyColumn(KeyDatabase keys)
        {
            minWidth = 100;
            Keys = keys;
            headerContent = new GUIContent("Key");
            headerTextAlignment = TextAlignment.Center;
            canSort = true;
            allowToggleVisibility = true;
        }

        public override string ToString() => "Key";
    }

    class KeyIdColumn : VisibleColumn
    {
        public KeyDatabase Keys { get; }

        public KeyIdColumn(KeyDatabase keys)
        {
            minWidth = 50;
            maxWidth = 100;
            Keys = keys;
            headerContent = new GUIContent("Key Id");
            headerTextAlignment = TextAlignment.Center;
            canSort = true;
            defaultVisibility = false;
            allowToggleVisibility = true;
        }

        public override string ToString() => "Key Id";
    }

    class MissingTableColumn : MultiColumnHeaderState.Column
    {
        public Locale TableLocale { get; }

        public MissingTableColumn(Locale locale)
        {
            minWidth = 50;
            maxWidth = 150;
            
            TableLocale = locale;
            headerContent = new GUIContent("Add " + TableLocale);
            headerTextAlignment = TextAlignment.Center;
            canSort = false;
            allowToggleVisibility = false;
        }

        public override string ToString() => "Missing Table: " + TableLocale;
    }
}