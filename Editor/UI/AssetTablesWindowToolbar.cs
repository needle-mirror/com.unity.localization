using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;

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

        public ToolbarToggle NewButton { get; private set; }
        public ToolbarToggle EditButton { get; private set; }

        public AssetTablesWindowToolbar()
        {
            var root = Resources.GetTemplate("AssetTablesWindowToolbar");
            Add(root);
            root.StretchToParentSize();

            NewButton = root.Q<ToolbarToggle>("toolbarButtonNew");
            EditButton = root.Q<ToolbarToggle>("toolbarButtonEdit");

            NewButton.OnValueChanged(evt =>
            {
                if (evt.newValue) ShowNewCallback();
            });
            EditButton.OnValueChanged(evt =>
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
            if (selectionChanged != null)
                selectionChanged(selectedPanel);
        }

        void ShowNewCallback()
        {
            EditorPrefs.SetInt(k_SelectedTabPref, (int)SelectedPanel.NewTables);
            EditButton.SetValueWithoutNotify(false);
            EditButton.SetEnabled(true);
            selectedPanel = SelectedPanel.NewTables;
            NewButton.SetEnabled(false);
            if (selectionChanged != null)
                selectionChanged(selectedPanel);
        }
    }
}