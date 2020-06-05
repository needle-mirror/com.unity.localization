using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.UI
{
    /// <summary>
    /// Popup that wraps a TreeView element
    /// </summary>
    class TableEntryPopupWindow : PopupWindowContent
    {
        readonly SearchField m_SearchField;
        readonly TreeView m_TreeView;
        bool m_ShouldClose;

        public float Width { get; set; }

        public TableEntryPopupWindow(TreeView contents)
        {
            m_SearchField = new SearchField();
            m_TreeView = contents;
        }

        public override void OnGUI(Rect rect)
        {
            const int border = 4;
            const int topPadding = 12;
            const int searchHeight = 20;
            const int remainTop = topPadding + searchHeight + border;
            var searchRect = new Rect(border, topPadding, rect.width - border * 2, searchHeight);
            var remainingRect = new Rect(border, topPadding + searchHeight + border, rect.width - border * 2, rect.height - remainTop - border);

            m_TreeView.searchString = m_SearchField.OnGUI(searchRect, m_TreeView.searchString);
            m_TreeView.OnGUI(remainingRect);

            if (m_ShouldClose)
            {
                GUIUtility.hotControl = 0;
                editorWindow.Close();
            }

            if (m_TreeView.HasSelection())
                ForceClose();
        }

        public override Vector2 GetWindowSize()
        {
            var result = base.GetWindowSize();
            result.x = Width;
            return result;
        }

        public override void OnOpen()
        {
            m_SearchField.SetFocus();
            base.OnOpen();
        }

        public void ForceClose() => m_ShouldClose = true;
    }

    /// <summary>
    /// Tree view item to represent an entry from a table.
    /// </summary>
    class TableEntryTreeViewItem : TreeViewItem
    {
        public LocalizedTableCollection TableCollection { get; set; }
        public SharedTableData.SharedTableEntry SharedEntry { get; set; }

        public TableEntryTreeViewItem(LocalizedTableCollection collection, SharedTableData.SharedTableEntry sharedEntry, int id, int depth) :
            base(id, depth)
        {
            TableCollection = collection;
            if (sharedEntry != null)
            {
                SharedEntry = sharedEntry;
                displayName = SharedEntry.Key;
            }
        }
    }

    /// <summary>
    /// Allows for selecting a table entry from the projects tables.
    /// </summary>
    class TableEntryReferenceTreeView : TreeView
    {
        readonly Type m_AssetType;
        readonly Action<LocalizedTableCollection, SharedTableData.SharedTableEntry> m_SelectionHandler;

        public TableEntryReferenceTreeView(Type assetType, Action<LocalizedTableCollection, SharedTableData.SharedTableEntry> selectionHandler)
            : base(new TreeViewState())
        {
            m_AssetType = assetType;
            m_SelectionHandler = selectionHandler;

            showAlternatingRowBackgrounds = true;
            showBorder = true;
            Reload();
        }

        protected override bool CanMultiSelect(TreeViewItem item) => false;

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem(-1, -1);
            var id = 1;

            root.AddChild(new TableEntryTreeViewItem(null, null, id++, 0) { displayName = $"None ({m_AssetType.Name})" });

            if (m_AssetType == typeof(string))
            {
                var stringTableCollections = LocalizationEditorSettings.GetStringTableCollections();
                foreach (var collection in stringTableCollections)
                {
                    var tableNode = new TreeViewItem(id++, 0, collection.TableCollectionName)
                    {
                        icon = AssetDatabase.GetCachedIcon(AssetDatabase.GetAssetPath(collection)) as Texture2D
                    };
                    root.AddChild(tableNode);

                    var sharedData = collection.SharedData;
                    foreach (var entry in sharedData.Entries)
                    {
                        tableNode.AddChild(new TableEntryTreeViewItem(collection, entry, id++, 1));
                    }
                }
            }
            else
            {
                var assetTableCollections = LocalizationEditorSettings.GetAssetTableCollections();
                foreach (var collection in assetTableCollections)
                {
                    var tableNode = new TreeViewItem(id++, 0, collection.TableCollectionName)
                    {
                        icon = AssetDatabase.GetCachedIcon(AssetDatabase.GetAssetPath(collection)) as Texture2D
                    };
                    root.AddChild(tableNode);

                    // Only show keys that have a compatible type.
                    var sharedData = collection.SharedData;
                    foreach (var entry in sharedData.Entries)
                    {
                        var typeMetadata = entry.Metadata.GetMetadata<AssetTypeMetadata>();
                        if (typeMetadata == null || m_AssetType.IsAssignableFrom(typeMetadata.Type))
                        {
                            tableNode.AddChild(new TableEntryTreeViewItem(collection, entry, id++, 1));
                        }
                    }
                }
            }

            if (!root.hasChildren)
            {
                root.AddChild(new TreeViewItem(1, 0, "No Tables Found."));
            }

            return root;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (FindItem(selectedIds[0], rootItem) is TableEntryTreeViewItem keyNode)
            {
                m_SelectionHandler(keyNode.TableCollection, keyNode.SharedEntry);
            }
            else
            {
                // Ignore Table selections. We just care about table entries.
                SetSelection(new int[] {});
            }
        }
    }
}
