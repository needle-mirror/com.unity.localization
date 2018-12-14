using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Localization;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Localization.UI;
using Resources = UnityEditor.Localization.UI.Resources;

// TODO: Detect if a table is part of addressables. 
// TODO: Detect changes to assets/imports etc.

namespace UnityEditor.Localization
{
    public class AssetTablesWindow : EditorWindow
    {
        static readonly Vector2 k_MinWindowSize = new Vector2(850, 450);
        VisualElement m_Root;
        VisualElement m_PanelView;
        AssetTablesGenerator m_AssetTablesGeneratorPanel;
        VisualElement m_EditTablePanel;
        VisualElement m_EditTableContainer;
        VisualElement m_ActiveTableEditor;
        AssetTablesField m_AssetTablesField;

        [MenuItem("Window/Localization/Asset Tables")]
        static void ShowWindow()
        {
            var window = GetWindow<AssetTablesWindow>(false, "Asset Tables", true);
            window.minSize = k_MinWindowSize;
            window.Show();
        }

        public static void ShowWindow(LocalizedTable selectedTable)
        {
            var window = GetWindow<AssetTablesWindow>(false, "Asset Tables", true);
            window.minSize = k_MinWindowSize;
            window.Show();
            window.EditTable(selectedTable);
        }

        public void EditTable(LocalizedTable selectedTable)
        {
            var toolbar = m_Root.Q<AssetTablesWindowToolbar>();
            toolbar.EditButton.value = true;
            m_AssetTablesField.SetValueFromTable(selectedTable);

            // If EditTable is called during OnEnable then the change event will not be sent.
            if (m_AssetTablesField.panel == null)
            {
                ShowTableEditor(m_AssetTablesField.value);
            }
        }

        void OnEnable()
        {
            m_Root = this.GetRootVisualContainer();
            m_Root.AddStyleSheetPath(Resources.GetStyleSheetPath("AssetTablesWindow"));
            var template = Resources.GetTemplate("AssetTablesWindow");
            m_Root.Add(template);
            template.StretchToParentSize();
            SetupPanels();
        }

        void SetupPanels()
        {
            m_PanelView = m_Root.Q<VisualElement>("panelsView");
            m_AssetTablesGeneratorPanel = m_Root.Q<AssetTablesGenerator>();
            m_EditTablePanel = m_Root.Q("editTablePanel");
            m_EditTableContainer = m_Root.Q("editTableContainer");
            var toolbar = m_Root.Q<AssetTablesWindowToolbar>();
            toolbar.selectionChanged += panel => UpdatePanels();

            m_AssetTablesField = m_Root.Q<AssetTablesField>();
            m_AssetTablesField.OnValueChanged(TableSelected);
            if (m_AssetTablesField.value != null && !(m_AssetTablesField.value is AssetTablesField.NoTables))
            {
                m_ActiveTableEditor = m_AssetTablesField.value.TableEditor.CreateInspectorGUI();
                m_EditTableContainer.Add(m_ActiveTableEditor);
                m_ActiveTableEditor.StretchToParentSize();
            }

            UpdatePanels();
        }

        void TableSelected(ChangeEvent<AssetTableCollection> evt)
        {
            ShowTableEditor(evt.newValue);
        }

        void ShowTableEditor(AssetTableCollection tableCollection)
        {
            if (m_ActiveTableEditor != null)
                m_EditTableContainer.Remove(m_ActiveTableEditor);
            m_ActiveTableEditor = tableCollection.TableEditor.CreateInspectorGUI();
            m_EditTableContainer.Add(m_ActiveTableEditor);
            m_ActiveTableEditor.StretchToParentSize();
        }

        void UpdatePanels()
        {
            var toolbar = m_Root.Q<AssetTablesWindowToolbar>();
            VisualElement selected;

            if (toolbar.selectedPanel == AssetTablesWindowToolbar.SelectedPanel.NewTables)
            {
                m_EditTablePanel.RemoveFromHierarchy();
                selected = m_AssetTablesGeneratorPanel;
            }
            else
            {
                m_AssetTablesGeneratorPanel.RemoveFromHierarchy();
                selected = m_EditTablePanel;
            }

            m_PanelView.Add(selected);
        }
    }
}