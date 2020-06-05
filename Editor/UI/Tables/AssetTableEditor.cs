using UnityEngine.UIElements;

namespace UnityEditor.Localization.UI
{
    [TableCollectionEditor(typeof(AssetTableCollection))]
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

            // TODO: We only need to do this when an item is added or removed from the list. Can we be more efficient?
            m_AssetTableListView?.Reload();
        }

        void OnIMGUI()
        {
            if (m_AssetTableListView == null)
            {
                m_AssetTableListView = new LocalizedAssetTableListView(TableCollection as AssetTableCollection);
                m_AssetTableListView.Initialize();
                m_AssetTableListView.Reload();

                m_AssetTableListView.SelectedForEditing += TableListViewOnSelectedForEditing;
            }

            m_AssetTableListView.OnGUI(m_ImguiContainer.layout);
        }
    }
}
