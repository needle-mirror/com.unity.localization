using UnityEngine.Localization.Tables;
using UnityEngine.UIElements;

namespace UnityEditor.Localization.UI
{
    [TableEditor(typeof(StringTable))]
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
        }

        protected override void UndoRedoPerformed()
        {
            base.UndoRedoPerformed();

            // Because we store the table name in the meta data we can not just use the Undo system.
            m_StringTableListView?.Reload();
        }

        void OnIMGUI()
        {
            if (m_StringTableListView == null)
            {
                m_StringTableListView = new StringTableListView(TableCollection);
                m_StringTableListView.Initialize();
                m_StringTableListView.Reload();

                m_StringTableListView.SelectedForEditing += TableListViewOnSelectedForEditing;
            }

            m_StringTableListView.OnGUI(m_ImguiContainer.layout);
        }
    }
}
