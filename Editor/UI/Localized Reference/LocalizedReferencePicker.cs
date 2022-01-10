#if ENABLE_SEARCH

using UnityEditor.Localization.Search;
using UnityEditor.Search;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.UI
{
    class LocalizedReferencePicker<TCollection> where TCollection : LocalizationTableCollection
    {
        readonly SearchContext m_SearchContext;
        readonly SerializedObject m_SerializedObject;
        readonly LocalizedReferencePropertyDrawer<TCollection> m_PropertyDrawer;
        readonly string m_Title;
        readonly string m_PropertyPath;
        readonly int m_UndoGroup;

        ISearchView m_View;
        bool m_Revert = true;

        public LocalizedReferencePicker(SearchContext context, string title, LocalizedReferencePropertyDrawer<TCollection>.Data data, LocalizedReferencePropertyDrawer<TCollection> drawer)
        {
            m_SerializedObject = data.serializedObject;
            m_PropertyPath = data.rootProperty.propertyPath;
            m_PropertyDrawer = drawer;
            m_SearchContext = context;
            m_Title = title;

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
            var state = new SearchViewState(m_SearchContext) { title = m_Title };
            m_View = UnityEditor.Search.SearchService.ShowPicker(m_SearchContext, Select, Track, null, null, m_Title);
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
                SetItem(selection?.Collection, selection?.Entry, true);
            }
            m_View = null;
            Selection.selectionChanged -= CloseNoRevert;
        }

        void Track(SearchItem item)
        {
            var selection = (TableEntrySearchData)item.data;
            SetItem(selection?.Collection, selection?.Entry, false);
        }

        void SetItem(LocalizationTableCollection collection, SharedTableData.SharedTableEntry entry, bool collapse)
        {
            var property = m_SerializedObject.FindProperty(m_PropertyPath);
            if (property == null)
                return;

            var data = m_PropertyDrawer.GetDataForProperty(property);
            data.SelectedTableCollection = collection as TCollection;
            data.SelectedTableEntry = entry;
            m_SerializedObject.ApplyModifiedProperties();

            if (collapse)
                Undo.CollapseUndoOperations(m_UndoGroup);
        }
    }
}

#endif
