using UnityEngine.Localization.Tables;
using UnityEngine.UIElements;

namespace UnityEditor.Localization.UI
{
    [TableEditor(typeof(AssetTable))]
    class AssetTableEditor : TableEditor
    {
        IMGUIContainer m_ImguiContainer;
        LocalizedAssetTableListView m_AssetTableListView;

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
            m_AssetTableListView?.Reload();
        }

        void OnIMGUI()
        {
            if (m_AssetTableListView == null)
            {
                m_AssetTableListView = new LocalizedAssetTableListView(TableCollection);
                m_AssetTableListView.Initialize();
                m_AssetTableListView.Reload();

                m_AssetTableListView.SelectedForEditing += TableListViewOnSelectedForEditing;
            }

            m_AssetTableListView.OnGUI(m_ImguiContainer.layout);
        }
    }
}
