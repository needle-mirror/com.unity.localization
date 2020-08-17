using UnityEngine.UIElements;

namespace UnityEditor.Localization.UI
{
    abstract class TableEditor : VisualElement
    {
        public LocalizationTableCollection TableCollection { get; set; }

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
            m_NameField.value = TableCollection.TableCollectionName;
            m_NameField.RegisterCallback<ChangeEvent<string>>(TableCollectionNameChanged);
            m_NameField.isDelayed = true; // Prevent an undo per char change.
        }

        public virtual void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoRedoPerformed;
        }

        ProjectTablesPopup FindTablesPopup()
        {
            // Find the root
            var root = parent;
            while (root.parent != null)
            {
                root = root.parent;
            }
            return root.Q<ProjectTablesPopup>();
        }

        void TableCollectionNameChanged(ChangeEvent<string> evt)
        {
            TableCollection.SetTableCollectionName(evt.newValue, true);

            // Force the label to update itself.
            var atf = FindTablesPopup();
            atf?.RefreshLabels();
        }

        protected virtual void UndoRedoPerformed()
        {
            TableCollection.RefreshAddressables();

            var name = TableCollection.TableCollectionName;
            m_NameField?.SetValueWithoutNotify(name);
            var atf = FindTablesPopup();
            atf?.RefreshLabels();
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
