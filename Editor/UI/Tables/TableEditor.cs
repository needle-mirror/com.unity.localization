using UnityEngine.UIElements;

namespace UnityEditor.Localization.UI
{
    abstract class TableEditor : VisualElement
    {
        public AssetTableCollection TableCollection { get; set; }

        protected VisualElement m_TableContentsPanel;
        protected VisualElement m_PropertiesPanel;
        protected TextField m_NameField;

        public virtual void OnEnable()
        {
            Undo.undoRedoPerformed += UndoRedoPerformed;

            var asset = Resources.GetTemplateAsset(GetType().Name);
            asset.CloneTree(this);

            m_TableContentsPanel = this.Q("table-contents-panel");
            m_PropertiesPanel = this.Q("properties-panel");

            m_NameField = this.Q<TextField>("table-name-field");
            m_NameField.value = TableCollection.TableName;
            m_NameField.RegisterCallback<ChangeEvent<string>>(TableNameChanged);
            m_NameField.isDelayed = true; // Prevent an undo per char change.
        }

        public virtual void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoRedoPerformed;
        }

        void TableNameChanged(ChangeEvent<string> evt)
        {
            var group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Change table collection name");
            Undo.RecordObject(TableCollection.SharedData, "Change table collection name");

            TableCollection.SharedData.TableName = evt.newValue;
            for (int i = 0; i < TableCollection.Tables.Count; ++i)
            {
                var table = TableCollection.Tables[i];
                LocalizationEditorSettings.AddOrUpdateTable(table, true);
            }
            Undo.CollapseUndoOperations(group);

            // Force the label to update itself.
            var atf = this.Q<ProjectTablesPopup>();
            atf?.RefreshLabels();
        }

        protected virtual void UndoRedoPerformed()
        {
            // Table name
            var name = TableCollection.TableName;
            if (m_NameField != null && m_NameField.value != name)
            {
                m_NameField.SetValueWithoutNotify(name);
                TableCollection.Tables.ForEach(tbl => LocalizationEditorSettings.AddOrUpdateTable(tbl, false));

                var atf = this.Q<ProjectTablesPopup>();
                atf?.RefreshLabels();
            }
        }

        protected virtual void TableListViewOnSelectedForEditing(ISelectable selected)
        {
            m_PropertiesPanel.Clear();

            if (selected != null)
            {
                m_PropertiesPanel.style.display = DisplayStyle.Flex;
                var editor = selected.CreateEditor();
                m_PropertiesPanel.Add(editor);
                editor.StretchToParentSize();
            }
            else
            {
                m_PropertiesPanel.style.display = DisplayStyle.None;
            }
        }
    }
}
