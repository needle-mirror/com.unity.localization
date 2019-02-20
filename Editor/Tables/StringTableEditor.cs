using System.Linq;
using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Localization.UI;
using UnityEngine.Experimental.UIElements.StyleEnums;

namespace UnityEditor.Localization
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(StringTable), true)]
    public class StringTableEditor : LocalizedTableEditor
    {
        VisualElement m_Root;
        IMGUIContainer m_ImguiContainer;
        StringTableListView m_TreeView;
        VisualElement m_DetailedPanel;
        VisualElement m_DetailedPanelContainer;
        StringTableListViewItem m_SelectedItem;
        StringTable m_SelectedTable;
        Toggle m_PluralModeToggle;

        StringTableListView TreeView
        {
            get
            {
                if (m_TreeView == null)
                {
                    Tables = targets.Cast<LocalizedTable>().ToList();
                }
                return m_TreeView;
            }
        }

        public override List<LocalizedTable> Tables
        {
            set
            {
                base.Tables = value;
                m_TreeView = new StringTableListView() { Tables = value.Cast<StringTable>().ToList() };
                m_TreeView.editTargetTable += TreeViewSelectionChanged;
                m_TreeView.Initialize();
                m_TreeView.Reload();
            }
        }

        void TreeViewSelectionChanged(StringTableListViewItem row, StringTable col)
        {
            m_SelectedItem = row;
            m_SelectedTable = col;

            if (m_SelectedItem == null || m_SelectedTable == null)
            {
                if(m_DetailedPanel.parent != null)
                    m_DetailedPanelContainer.Remove(m_DetailedPanel);
            }
            else if (m_DetailedPanel.parent == null)
            {
                m_DetailedPanelContainer.Add(m_DetailedPanel);
            }

            if (m_DetailedPanel == null || m_DetailedPanelContainer == null)
                return;

            RefreshEditPanel();
        }

        protected override void UndoRedoPerformed()
        {
            if(m_TreeView != null)
                m_TreeView.Reload();

            RefreshEditPanel();
            base.UndoRedoPerformed();
        }

        public override VisualElement CreateInspectorGUI()
        {
            if (m_Root == null)
            {
                m_Root = Resources.GetTemplate("StringTableEditor");
                m_Root.Bind(serializedObject);
                m_Root.Q<PropertyField>("m_TableName").Q<TextField>().OnValueChanged(TableNameChanged);
                m_Root.Q<PropertyField>("m_TableName").Q<TextField>().isDelayed = true; // Prevent an undo for every char changed.
                var tableContainer = m_Root.Q("tableContainer");
                m_ImguiContainer = new IMGUIContainer(OnIMGUI);

                // Workaround for IMGUI resize bug https://unity.slack.com/archives/C3414V4UV/p1533657274000148
                m_ImguiContainer.style.overflow = Overflow.Visible;

                tableContainer.Add(m_ImguiContainer);
                m_ImguiContainer.StretchToParentSize();
                m_DetailedPanel = m_Root.Q("detailedPanel");
                m_DetailedPanelContainer = m_Root.Q("detailedPanelContainer");

                m_PluralModeToggle = m_DetailedPanelContainer.Q<Toggle>("pluralModeToggle");
                m_PluralModeToggle.OnValueChanged(evt => RefreshEditPanel());
            }
            return m_Root;
        }

        void RefreshEditPanel()
        {
            if (m_SelectedTable == null || m_SelectedItem == null)
                return;

            var contents = m_DetailedPanelContainer.Q("detailedPanelContents");
            contents.Clear();

            var entry = m_SelectedItem.GetEntry(m_SelectedTable);
            if (entry != null)
            {
                contents.Add(new Label("Source Text"));
                var sourceText = new TextField { isDelayed = true, value = entry.Id };
                sourceText.OnValueChanged(SourceTextChanged);
                contents.Add(sourceText);

                if (!m_PluralModeToggle.value)
                {
                    contents.Add(new Label("Translated Text(" + m_SelectedTable.LocaleIdentifier.Code + ")"));
                    var translated = new TextField { isDelayed = true, value = entry.Translated };
                    translated.OnValueChanged(evt =>
                    {
                        Undo.RecordObject(m_SelectedTable, "Change translated text");
                        entry.Translated = evt.newValue;
                        m_TreeView.Repaint();
                    });
                    contents.Add(translated);
                }
                else
                {
                    var pluralHandler = m_SelectedTable.PluralHandler;
                    if (pluralHandler != null)
                    {
                        for (int i = 0; i < pluralHandler.NumberOfPlurals; i++)
                        {
                            contents.Add(new Label("Plural " + i));
                            var plural = new TextField() { isDelayed = true, value = entry.GetPlural(i), multiline = true };
                            var pluralIndex = i;
                            plural.OnValueChanged(evt =>
                            {
                                Undo.RecordObject(m_SelectedTable, "Change translated text");
                                entry.SetPlural(pluralIndex, evt.newValue);
                                m_TreeView.Repaint();
                            });
                            contents.Add(plural);
                        }
                    }
                }
            }
        }

        void SourceTextChanged(ChangeEvent<string> evt)
        {
            foreach (var addressableAssetTable in Tables)
            {
                Undo.RecordObject(addressableAssetTable, "Rename table key");
                addressableAssetTable.ReplaceKey(evt.previousValue, evt.newValue);
                EditorUtility.SetDirty(addressableAssetTable);
                m_SelectedItem.Key = evt.newValue;
                m_TreeView.Repaint();
            }
        }

        void TableNameChanged(ChangeEvent<string> evt)
        {
            var atf = m_Root.panel.visualTree.Q<AssetTablesField>();
            if (atf != null)
            {
                // Force the label to update itself.
                atf.RefreshLabels();
            }
        }

        void OnIMGUI()
        {
            TreeView.OnGUI(m_ImguiContainer.layout);
        }
    }
}