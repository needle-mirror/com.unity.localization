using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Localization.Tables;
using UnityEngine.UIElements;

namespace UnityEditor.Localization.UI
{
    class EditAssetTables : VisualElement
    {
        internal new class UxmlFactory : UxmlFactory<EditAssetTables> { }

        VisualElement m_TableContents;
        ProjectTablesPopup m_AssetTablesField;
        TableEditor m_CurrentEditor;

        public EditAssetTables()
        {
            var asset = Resources.GetTemplateAsset(nameof(EditAssetTables));
            asset.CloneTree(this);

            m_TableContents = this.Q("table-contents");
            m_AssetTablesField = this.Q<ProjectTablesPopup>();
            m_AssetTablesField.RegisterValueChangedCallback((evt) => TableCollectionSelected(evt.newValue));
            TableCollectionSelected(m_AssetTablesField.value);

            LocalizationEditorSettings.OnModification += LocalizationEditorSettingsOnOnModification;
        }

        ~EditAssetTables() => LocalizationEditorSettings.OnModification -= LocalizationEditorSettingsOnOnModification;

        void TableCollectionSelected(AssetTableCollection atc)
        {
            m_TableContents.Clear();

            if (m_CurrentEditor != null)
            {
                m_CurrentEditor.OnDisable();
            }

            if (atc == null || atc.TableType == null)
                return;

            var editorType = GetEditorType(atc.TableType);
            if (editorType == null)
                return;

            m_CurrentEditor = (TableEditor)Activator.CreateInstance(editorType);
            m_CurrentEditor.TableCollection = atc;

            if (m_CurrentEditor != null)
            {
                m_TableContents.Add(m_CurrentEditor);
                m_CurrentEditor.StretchToParentSize();
                m_CurrentEditor.OnEnable();
            }
        }

        static Type GetEditorType(Type tableType)
        {
            var editors = TypeCache.GetTypesWithAttribute<TableEditorAttribute>();
            Type editorType = null;
            foreach (var e in editors)
            {
                var attribute = e.GetCustomAttribute<TableEditorAttribute>();
                if (attribute.EditorTargetType == tableType)
                {
                    editorType = e;
                    break;
                }
            }

            if (editorType == null)
                Debug.LogError($"Table Type {tableType.Name} does not have a Table Editor. Please use the TableEditorAttribute to assign one.");

            return editorType;
        }

        void LocalizationEditorSettingsOnOnModification(LocalizationEditorSettings.ModificationEvent evt, object obj)
        {
            m_AssetTablesField.RefreshLabels();
        }

        /// <summary>
        /// TODO: DOC
        /// </summary>
        /// <param name="table"></param>
        public void Select(LocalizedTable table)
        {
            m_AssetTablesField.SetValueFromTable(table);
        }
    }
}
