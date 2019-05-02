#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#else
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#endif

namespace UnityEditor.Localization.UI
{
    class AssetTablesWindowToolbar : VisualElement
    {
        const string k_SelectedTabPref = "Localization-AssetTablesWindowToolbar-SelectedTab";

        internal new class UxmlFactory : UxmlFactory<AssetTablesWindowToolbar> { }

        public enum SelectedPanel
        {
            NewTables,
            EditTables
        }

        public SelectedPanel selectedPanel { get; private set; }

        public delegate void SelectionDelegate(SelectedPanel panel);
        public event SelectionDelegate selectionChanged;

        public ToolbarToggle NewButton { get; }
        public ToolbarToggle EditButton { get; }

        public AssetTablesWindowToolbar()
        {
            #if UNITY_2019_1_OR_NEWER
            var root = Resources.GetTemplate("AssetTablesWindowToolbar");
            #else
            var root = Resources.GetTemplate("AssetTablesWindowToolbar_2018_3");
            #endif

            Add(root);
            root.StretchToParentSize();

            NewButton = root.Q<ToolbarToggle>("toolbarButtonNew");
            EditButton = root.Q<ToolbarToggle>("toolbarButtonEdit");

            NewButton.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                if (evt.newValue) ShowNewCallback();
            });
            EditButton.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                if (evt.newValue) ShowEditCallback();
            });

            selectedPanel = (SelectedPanel)EditorPrefs.GetInt(k_SelectedTabPref, 0);

            if (selectedPanel == SelectedPanel.NewTables)
            {
                NewButton.value = true;
                ShowNewCallback();
            }
            else
            {
                EditButton.value = true;
                ShowEditCallback();
            }
        }
        
        void ShowEditCallback()
        {
            EditorPrefs.SetInt(k_SelectedTabPref, (int)SelectedPanel.EditTables);
            NewButton.SetValueWithoutNotify(false);
            NewButton.SetEnabled(true);
            selectedPanel = SelectedPanel.EditTables;
            EditButton.SetEnabled(false);
            selectionChanged?.Invoke(selectedPanel);
        }

        void ShowNewCallback()
        {
            EditorPrefs.SetInt(k_SelectedTabPref, (int)SelectedPanel.NewTables);
            EditButton.SetValueWithoutNotify(false);
            EditButton.SetEnabled(true);
            selectedPanel = SelectedPanel.NewTables;
            NewButton.SetEnabled(false);
            selectionChanged?.Invoke(selectedPanel);
        }
    }
}