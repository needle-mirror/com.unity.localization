using UnityEngine;
using UnityEngine.Localization;
using UnityEditor.Localization.UI;
using Resources = UnityEditor.Localization.UI.Resources;

#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#endif

// TODO: Detect if a table is part of Addressables. 
// TODO: Detect changes to assets/imports etc.
// TODO: Add event system for rename, add, remove key etc. Then editors can refresh themselves.

namespace UnityEditor.Localization
{
    class AssetTablesWindow : EditorWindow
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
        public static void ShowWindow()
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

            // If the panel is not active yet then then the change event will not have been sent yet.
            if (m_AssetTablesField.panel == null)
            {
                ShowTableEditor(m_AssetTablesField.value);
            }
        }

        void OnEnable()
        {
            #if UNITY_2019_1_OR_NEWER
            m_Root = rootVisualElement;
            m_Root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(Resources.GetStyleSheetPath("AssetTablesWindow")));
            #else
            m_Root = this.GetRootVisualContainer();
            m_Root.AddStyleSheetPath(Resources.GetStyleSheetPath("AssetTablesWindow"));
            #endif

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
            m_AssetTablesField.RegisterCallback<ChangeEvent<AssetTableCollection>>(TableSelected);
            ShowTableEditor(m_AssetTablesField.value);

            UpdatePanels();
        }

        void TableSelected(ChangeEvent<AssetTableCollection> evt) => ShowTableEditor(evt.newValue);

        void ShowTableEditor(AssetTableCollection tableCollection)
        {
            if (m_ActiveTableEditor != null)
                m_EditTableContainer.Remove(m_ActiveTableEditor);

            if (tableCollection == null || tableCollection is AssetTablesField.NoTables)
                return;

            var tableEditor = tableCollection.TableEditor;
            if (tableEditor == null)
                return;

            m_ActiveTableEditor = tableEditor.CreateTableEditorGUI();
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