using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UnityEditor.Localization
{
    class LocalizedReferencePopupWindow : PopupWindowContent
    {
        SearchField m_SearchField;
        TreeView m_TreeView;
        bool m_ShouldClose;

        public float Width { get; set; }

        public LocalizedReferencePopupWindow(TreeView contents)
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
                EditorGUIUtility.hotControl = 0;
                editorWindow.Close();
            }

           // if(m_TreeView.HasSelection())
           //     ForceClose();
        }

        public override Vector2 GetWindowSize()
        {
            Vector2 result = base.GetWindowSize();
            result.x = Width;
            return result;
        }

        public override void OnOpen()
        {
            m_SearchField.SetFocus();
            base.OnOpen();
        }

        public void ForceClose()
        {
            m_ShouldClose = true;
        }
    }
}
