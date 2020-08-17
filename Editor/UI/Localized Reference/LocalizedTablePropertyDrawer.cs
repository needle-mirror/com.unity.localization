using System;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.UI
{
    abstract class LocalizedTablePropertyDrawer<TCollection> : PropertyDrawerExtended<LocalizedTablePropertyDrawer<TCollection>.LocalizedTablePropertyDrawerPropertyData>
        where TCollection : LocalizationTableCollection
    {
        public class LocalizedTablePropertyDrawerPropertyData
        {
            GUIContent m_FieldLabel;
            TCollection m_SelectedTableCollection;

            public SerializedObject serializedObject;
            public SerializedTableReference tableReference;
            public Type assetType;

            public TCollection SelectedTableCollection
            {
                get
                {
                    if (m_SelectedTableCollection == null)
                    {
                        var tableCollections = GetProjectTableCollections();
                        if (tableReference.Reference.ReferenceType == TableReference.Type.Name)
                        {
                            m_SelectedTableCollection = tableCollections.FirstOrDefault(t => t.TableCollectionName == tableReference.Reference);
                        }
                        else if (tableReference.Reference.ReferenceType == TableReference.Type.Guid)
                        {
                            m_SelectedTableCollection = tableCollections.FirstOrDefault(t => t.SharedData.TableCollectionNameGuid == tableReference.Reference);
                        }
                    }
                    return m_SelectedTableCollection;
                }
                set
                {
                    m_SelectedTableCollection = value;
                    m_FieldLabel = null;
                    if (value != null)
                        tableReference.Reference = value.SharedData.TableCollectionNameGuid;
                    else
                        tableReference.Reference = string.Empty;
                }
            }

            public GUIContent FieldLabel
            {
                get
                {
                    if (m_FieldLabel == null)
                    {
                        if (SelectedTableCollection != null)
                        {
                            var icon = EditorGUIUtility.ObjectContent(m_SelectedTableCollection, assetType);
                            m_FieldLabel = new GUIContent(SelectedTableCollection.TableCollectionName, icon.image);
                        }
                        else
                        {
                            m_FieldLabel = new GUIContent($"None ({ObjectNames.NicifyVariableName(assetType.Name)})");
                        }
                    }
                    return m_FieldLabel;
                }
            }
        }

        protected static Func<ReadOnlyCollection<TCollection>> GetProjectTableCollections { get; set; }

        public LocalizedTablePropertyDrawer()
        {
            LocalizationEditorSettings.EditorEvents.CollectionAdded += EditorEvents_CollectionModified;
            LocalizationEditorSettings.EditorEvents.CollectionRemoved += EditorEvents_CollectionModified;
            Undo.undoRedoPerformed += ClearPropertyDataCache;
        }

        ~LocalizedTablePropertyDrawer()
        {
            LocalizationEditorSettings.EditorEvents.CollectionAdded -= EditorEvents_CollectionModified;
            LocalizationEditorSettings.EditorEvents.CollectionRemoved -= EditorEvents_CollectionModified;
            Undo.undoRedoPerformed -= ClearPropertyDataCache;
        }

        void EditorEvents_CollectionModified(LocalizationTableCollection obj) => ClearPropertyDataCache();

        public override LocalizedTablePropertyDrawerPropertyData CreatePropertyData(SerializedProperty property)
        {
            return new LocalizedTablePropertyDrawerPropertyData
            {
                serializedObject = property.serializedObject,
                tableReference = new SerializedTableReference(property.FindPropertyRelative("m_TableReference")),
                assetType = typeof(TCollection) == typeof(StringTableCollection) ? typeof(StringTable) : typeof(AssetTable)
            };
        }

        public override float GetPropertyHeight(LocalizedTablePropertyDrawerPropertyData data, SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;

        public override void OnGUI(LocalizedTablePropertyDrawerPropertyData data, Rect position, SerializedProperty property, GUIContent label)
        {
            var dropDownPosition = EditorGUI.PrefixLabel(position, label);

            if (EditorGUI.DropdownButton(dropDownPosition, data.FieldLabel, FocusType.Passive))
            {
                var treeSelection = new TableTreeView(typeof(TCollection) == typeof(StringTableCollection) ? typeof(StringTable) : typeof(AssetTable), collection =>
                {
                    data.SelectedTableCollection = collection as TCollection;

                    // Will be called outside of OnGUI so we need to call ApplyModifiedProperties.
                    data.serializedObject.ApplyModifiedProperties();
                });

                PopupWindow.Show(dropDownPosition, new TreeViewPopupWindow(treeSelection) { Width = dropDownPosition.width });
            }
        }
    }
}
