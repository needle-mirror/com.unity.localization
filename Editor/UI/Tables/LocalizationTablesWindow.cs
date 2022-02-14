using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Localization.UI
{
    /// <summary>
    /// Window used to edit localization tables.
    /// </summary>
    public class LocalizationTablesWindow : EditorWindow, IHasCustomMenu
    {
        const string k_EditorPrefValueKey = "Localization-LocalizationTablesWindow-Selected-Tab";
        const string k_WindowTitle = "Localization Tables";

        static readonly Vector2 k_MinSize = new Vector2(900, 600);
        List<ToolbarToggle> m_TabToggles;
        List<VisualElement> m_TabPanels;

        internal int SelectedTab
        {
            get => EditorPrefs.GetInt(k_EditorPrefValueKey, 0);
            set => EditorPrefs.SetInt(k_EditorPrefValueKey, value);
        }

        /// <summary>
        /// Opens the window in the Editor.
        /// </summary>
        [MenuItem("Window/Asset Management/Localization Tables")]
        public static void ShowWindow()
        {
            var window = GetWindow<LocalizationTablesWindow>(false, k_WindowTitle, true);
            window.titleContent = EditorGUIUtility.TrTextContent("Localization Tables", EditorIcons.TableWindow);
            window.Show();
        }

        /// <summary>
        /// Opens the window in the Editor and selects <paramref name="selectedcollection"/> for editing.
        /// </summary>
        /// <param name="selectedcollection"></param>
        public static void ShowWindow(LocalizationTableCollection selectedcollection)
        {
            var window = GetWindow<LocalizationTablesWindow>(false, k_WindowTitle, true);
            window.titleContent = EditorGUIUtility.TrTextContent("Localization Tables", EditorIcons.TableWindow);
            window.Show();
            window.EditCollection(selectedcollection);
        }

        /// <summary>
        /// Opens the window in the Editor with the table creator tab active.
        /// </summary>
        public static void ShowTableCreator()
        {
            var window = GetWindow<LocalizationTablesWindow>(false, k_WindowTitle, true);
            window.Show();
            if (LocalizationEditorSettings.ActiveLocalizationSettings != null)
            {
                int idx = window.m_TabPanels.FindIndex(p => p is TableCreator);
                window.m_TabToggles[idx].value = true;
            }
        }

        /// <summary>
        /// Selects the collection for editing.
        /// </summary>
        /// <param name="selectedCollection"></param>
        public void EditCollection(LocalizationTableCollection selectedCollection)
        {
            if (LocalizationEditorSettings.ActiveLocalizationSettings == null)
                return;

            int idx = m_TabPanels.FindIndex(p => p is EditAssetTables);
            TabSelected(idx);

            var panel = m_TabPanels[idx] as EditAssetTables;
            panel.Select(selectedCollection);
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
                var asset = Resources.GetTemplateAsset(nameof(LocalizationTablesWindow));
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

        /// <summary>
        /// Adds menu items to the table context window.
        /// </summary>
        /// <param name="menu"></param>
        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(EditorGUIUtility.TrTextContent("Import/XLIFF Directory..."), false, Plugins.XLIFF.MenuItems.ImportXliffDirectory);

            if (LocalizationEditorSettings.ActiveLocalizationSettings == null)
                return;

            int idx = m_TabPanels.FindIndex(p => p is EditAssetTables);

            var panel = m_TabPanels[idx] as EditAssetTables;
            if (SelectedTab != idx)
                return;

            var selectedCollection = panel.SelectedCollection as StringTableCollection;
            if (selectedCollection != null)
            {
                menu.AddItem(EditorGUIUtility.TrTextContent("Import/XLIFF into Collection..."), false, () => Plugins.XLIFF.MenuItems.ImportIntoCollection(new MenuCommand(selectedCollection)));
                menu.AddItem(EditorGUIUtility.TrTextContent("Import/XLIFF..."), false, () => Plugins.XLIFF.MenuItems.ImportXliffFile());
                menu.AddItem(EditorGUIUtility.TrTextContent("Import/CSV..."), false, () => Plugins.CSV.MenuItems.ImportCollection(new MenuCommand(selectedCollection)));
                menu.AddItem(EditorGUIUtility.TrTextContent("Import/CSV(Merge)..."), false, () => Plugins.CSV.MenuItems.ImportCollectionMerge(new MenuCommand(selectedCollection)));
                menu.AddItem(EditorGUIUtility.TrTextContent("Export/XLIFF..."), false, () => Plugins.XLIFF.MenuItems.ExportCollection(new MenuCommand(selectedCollection)));
                menu.AddItem(EditorGUIUtility.TrTextContent("Export/CSV..."), false, () => Plugins.CSV.MenuItems.ExportCollection(new MenuCommand(selectedCollection)));
                menu.AddItem(EditorGUIUtility.TrTextContent("Export/CSV(With Comments)..."), false, () => Plugins.CSV.MenuItems.ExportCollectionWithComments(new MenuCommand(selectedCollection)));
                menu.AddItem(EditorGUIUtility.TrTextContent("Export/Character Set..."), false, () => ExportCharacterSetWindow.ShowWindow().SelectedTables.SetSelection(selectedCollection));
            }
        }
    }
}
