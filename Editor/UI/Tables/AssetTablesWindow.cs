using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Localization.Tables;
using UnityEngine.UIElements;

namespace UnityEditor.Localization.UI
{
    class AssetTablesWindow : EditorWindow
    {
        const string k_EditorPrefValueKey = "Localization-AssetTablesWindow-Selected-Tab";
        static readonly Vector2 k_MinSize = new Vector2(900, 600);
        List<ToolbarToggle> m_TabToggles;
        List<VisualElement> m_TabPanels;

        public int SelectedTab
        {
            get => EditorPrefs.GetInt(k_EditorPrefValueKey, 0);
            set => EditorPrefs.SetInt(k_EditorPrefValueKey, value);
        }

        [MenuItem("Window/Asset Management/Localization Tables")]
        public static void ShowWindow()
        {
            var window = GetWindow<AssetTablesWindow>(false, "Asset Tables", true);
            window.titleContent = new GUIContent("Asset Tables", EditorIcons.LocalizationSettings.image);
            window.Show();
        }

        public static void ShowWindow(LocalizedTable selectedTable)
        {
            var window = GetWindow<AssetTablesWindow>(false, "Asset Tables", true);
            window.titleContent = new GUIContent("Asset Tables", EditorIcons.LocalizationSettings.image);
            window.Show();
            window.EditTable(selectedTable);
        }

        public static void ShowTableCreator()
        {
            var window = GetWindow<AssetTablesWindow>(false, "Asset Tables", true);
            window.Show();
            if (LocalizationEditorSettings.ActiveLocalizationSettings != null)
            {
                int idx = window.m_TabPanels.FindIndex(p => p is TableCreator);
                window.m_TabToggles[idx].value = true;
            }
        }

        public void EditTable(LocalizedTable selectedTable)
        {
            int idx = m_TabPanels.FindIndex(p => p is EditAssetTables);
            TabSelected(idx);

            var panel = m_TabPanels[idx] as EditAssetTables;
            panel.Select(selectedTable);
        }

        void OnEnable()
        {
            minSize = k_MinSize;
            var root = rootVisualElement;
            if (LocalizationEditorSettings.ActiveLocalizationSettings == null)
            {
                var asset = Resources.GetTemplateAsset("CreateLocalizationSettings");
                asset.CloneTree(root);
                var button = root.Q<Button>("create-settings-button");
                button.clickable.clicked += () =>
                {
                    var created = LocalizationSettingsMenuItems.CreateLocalizationAsset();
                    if (created != null)
                    {
                        LocalizationEditorSettings.ActiveLocalizationSettings = created;
                        root.Clear();
                        OnEnable();
                    }
                };
            }
            else
            {
                var asset = Resources.GetTemplateAsset(nameof(AssetTablesWindow));
                asset.CloneTree(root);
                Init();
            }
        }

        void Init()
        {
            m_TabToggles = rootVisualElement.Query<ToolbarToggle>().ToList();
            m_TabPanels = new List<VisualElement>();
            for (int i = 0; i < m_TabToggles.Count; ++i)
            {
                var toggle = m_TabToggles[i];
                var panelName = $"{toggle.name}-panel";
                var panel = rootVisualElement.Q(panelName);
                Debug.Assert(panel != null, $"Could not find panel \"{panelName}\"");
                m_TabPanels.Add(panel);
                panel.style.display = SelectedTab == i ? DisplayStyle.Flex : DisplayStyle.None;
                toggle.value = SelectedTab == i;
                int idx = i;
                toggle.RegisterValueChangedCallback((chg) => TabSelected(idx));
            }

            Debug.Assert(m_TabPanels.Count == m_TabToggles.Count, "Expected the same number of tab toggle buttons and panels.");
        }

        void TabSelected(int idx)
        {
            if (SelectedTab == idx)
                return;

            m_TabToggles[SelectedTab].SetValueWithoutNotify(false);
            m_TabPanels[SelectedTab].style.display = DisplayStyle.None;

            m_TabToggles[idx].SetValueWithoutNotify(true);
            m_TabPanels[idx].style.display = DisplayStyle.Flex;

            SelectedTab = idx;
        }
    }
}
