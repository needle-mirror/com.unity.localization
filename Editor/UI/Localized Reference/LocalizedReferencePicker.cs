#if ENABLE_SEARCH

using UnityEditor.Localization.Search;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Localization.Tables;
using UnityEngine.Search;

namespace UnityEditor.Localization.UI
{
    class LocalizedReferencePicker<TCollection> where TCollection : LocalizationTableCollection
    {
        readonly SearchContext m_SearchContext;
        readonly string m_Title;
        readonly int m_UndoGroup;

        ISearchView m_View;
        bool m_Revert = true;

        Object[] m_Targets;
        string m_TablePath;
        string m_EntryPath;

        public LocalizedReferencePicker(SearchContext context, string title, SerializedProperty tableProperty, SerializedProperty entryProperty)
        {
            m_SearchContext = context;
            m_Title = title;

            m_Targets = tableProperty.serializedObject.targetObjects;
            m_TablePath = tableProperty.propertyPath;
            m_EntryPath = entryProperty.propertyPath;

            Undo.IncrementCurrentGroup();
            m_UndoGroup = Undo.GetCurrentGroup();
            Selection.selectionChanged += CloseNoRevert; // Close if Undo changes the selection.
        }

        void CloseNoRevert()
        {
            m_Revert = false;
            m_View?.Close();
        }

        public void Show()
        {
            #if UNITY_2022_2_OR_NEWER
            var state = new SearchViewState(m_SearchContext)
            {
                title = m_Title,
                ignoreSaveSearches = true,
                hideAllGroup = false,
                queryBuilderEnabled = true,
                hideTabs = true,
                flags = SearchViewFlags.DisableInspectorPreview | SearchViewFlags.OpenInBuilderMode,
                selectHandler = Select,
                trackingHandler = Track
            };
            m_View = UnityEditor.Search.SearchService.ShowPicker(state);
            #else
            var state = new SearchViewState(m_SearchContext) { title = m_Title };
            m_View = UnityEditor.Search.SearchService.ShowPicker(m_SearchContext, Select, Track, null, null, m_Title);
            #endif
        }

        void Select(SearchItem item, bool cancelled)
        {
            if (cancelled)
            {
                if (m_Revert)
                    Undo.RevertAllDownToGroup(m_UndoGroup);
            }
            else
            {
                // Apply selection
                var selection = (TableEntrySearchData)item.data;
                SetItem(selection?.Collection, selection?.Entry);
                Undo.CollapseUndoOperations(m_UndoGroup);
            }
            m_View = null;
            Selection.selectionChanged -= CloseNoRevert;
        }

        void Track(SearchItem item)
        {
            var selection = (TableEntrySearchData)item.data;
            SetItem(selection?.Collection, selection?.Entry);
        }

        void SetItem(LocalizationTableCollection collection, SharedTableData.SharedTableEntry entry)
        {
            var so = new SerializedObject(m_Targets);
            var tableProp = so.FindProperty(m_TablePath);
            var entryProp = so.FindProperty(m_EntryPath);

            var tableRef = new SerializedTableReference(tableProp);
            var entryRef = new SerializedTableEntryReference(entryProp);
            entryRef.SetReference(entry);
            tableRef.SetReference(collection);
            so.ApplyModifiedProperties();
        }
    }
}

#endif
