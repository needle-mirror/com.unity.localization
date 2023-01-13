using System;
using UnityEngine.UIElements;

namespace UnityEditor.Localization.UI
{
    [TableCollectionEditor(typeof(StringTableCollection))]
    class StringTableEditor : TableEditor
    {
        IMGUIContainer m_ImguiContainer;
        StringTableListView m_StringTableListView;

        public override void OnEnable()
        {
            base.OnEnable();
            m_ImguiContainer = new IMGUIContainer(OnIMGUI);
            m_TableContentsPanel.Add(m_ImguiContainer);
            m_ImguiContainer.StretchToParentSize();
            RegisterCallback<DetachFromPanelEvent>(evt =>
            {
                m_StringTableListView?.Dispose();
                m_StringTableListView = null;
            });
        }

        protected override void UndoRedoPerformed()
        {
            base.UndoRedoPerformed();

            // TODO: We only need to do this when an item is added or removed from the list. Can we be more efficient?
            m_StringTableListView?.Reload();
        }

        void OnIMGUI()
        {
            if (m_StringTableListView == null)
            {
                m_StringTableListView = new StringTableListView(TableCollection);
                m_StringTableListView.Initialize();

                m_StringTableListView.SelectedForEditing += TableListViewOnSelectedForEditing;
            }

            m_StringTableListView.OnGUI(m_ImguiContainer.layout);
        }
    }
}
