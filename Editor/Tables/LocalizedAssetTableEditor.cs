using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Localization;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Localization.UI;

namespace UnityEditor.Localization
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Texture2DAssetTable), true)]
    public class Texture2DLocalizedAssetTableEditor : LocalizedAssetTableEditor<Texture2D> { }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(AudioClipAssetTable), true)]
    public class AudioClipLocalizedAssetTableEditor : LocalizedAssetTableEditor<AudioClip> { }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(SpriteAssetTable), true)]
    public class SpriteAssetTableEditor : LocalizedAssetTableEditor<Sprite> { }

    public class LocalizedAssetTableEditor<TObject> : LocalizedTableEditor where TObject : Object
    {
        LocalizedAssetTableListView<TObject> m_TreeView;
        VisualElement m_Root;
        IMGUIContainer m_IMGUIContainer;

        public override VisualElement CreateInspectorGUI()
        {
            m_Root = UI.Resources.GetTemplate("LocalizedAssetTableEditor");
            m_Root.Bind(serializedObject);

            m_Root.Q<PropertyField>("m_TableName").Q<TextField>().OnValueChanged(TableNameChanged);
            m_Root.Q<PropertyField>("m_TableName").Q<TextField>().isDelayed = true; // Prevent an undo per char change.
            var tableContainer = m_Root.Q("tableContainer");
            m_IMGUIContainer = new IMGUIContainer(OnIMGUI);
            tableContainer.Add(m_IMGUIContainer);
            m_IMGUIContainer.StretchToParentSize();
            return m_Root;
        }

        void TableNameChanged(ChangeEvent<string> evt)
        {
            var atf = m_Root.panel.visualTree.Q<AssetTablesField>();
            if (atf != null)
            {
                // Force the label to update itself.
                atf.value = atf.value;
            }
        }

        public override List<LocalizedTable> Tables
        {
            set
            {
                base.Tables = value;
                m_TreeView = new LocalizedAssetTableListView<TObject>() { Tables = value.Cast<AddressableAssetTableT<TObject>>().ToList() };
                m_TreeView.Initialize();
                m_TreeView.Reload();
            }
        }

        protected override void UndoRedoPerformed()
        {
            base.UndoRedoPerformed();
            if (m_TreeView != null)
                m_TreeView.Reload();
        }

        void OnIMGUI()
        {
            if (m_TreeView == null)
            {
                Tables = targets.Cast<LocalizedTable>().ToList();
            }

            m_TreeView.OnGUI(m_IMGUIContainer.layout);
        }
    }
}