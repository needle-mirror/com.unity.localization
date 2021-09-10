using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Tables;
using UnityEngine.UIElements;

namespace UnityEditor.Localization.UI
{
    class TableEntrySelected : ISelectable
    {
        LocalizationTable m_Table;
        Locale m_Locale;
        VisualElement m_Editor;
        MetadataTypeAttribute m_SharedMetadataType;
        MetadataTypeAttribute m_MetadataType;

        public long KeyId { get; private set; }

        public bool Selected { get; set; }

        public TableEntrySelected(LocalizationTable table, long id, Locale locale, MetadataType supportedType)
        {
            m_Table = table;
            m_Locale = locale;
            KeyId = id;

            m_SharedMetadataType = new MetadataTypeAttribute(supportedType);

            // Remove the shared entry flag
            var entryFlags = supportedType & ~MetadataType.AllSharedTableEntries;
            m_MetadataType = new MetadataTypeAttribute(entryFlags);
        }

        public VisualElement CreateEditor()
        {
            if (m_Editor == null)
            {
                m_Editor = new VisualElement() { style = { marginLeft = 5, marginRight = 5, marginTop = 5, marginBottom = 5 } };
                var metadataLabel = new GUIContent("Metadata");

                // Shared data
                var sharedIndex = m_Table.SharedData.Entries.FindIndex(e => e.Id == KeyId);
                Debug.Assert(sharedIndex != -1, $"Could not find index of key {KeyId}");
                var sharedSerializedObject = new SerializedObject(m_Table.SharedData);
                var sharedSerializedEditor = new MetadataCollectionField(){ Type = m_SharedMetadataType };
                var sharedEntryProperty = sharedSerializedObject.FindProperty($"m_Entries.Array.data[{sharedIndex}].m_Metadata");
                var sharedLabel = new GUIContent("Shared Entry");
                var sharedEditor = new IMGUIContainer(() =>
                {
                    sharedSerializedObject.Update();

                    EditorGUILayout.LabelField(sharedLabel, EditorStyles.boldLabel);
                    var rect = EditorGUILayout.GetControlRect(true, sharedSerializedEditor.GetPropertyHeight(sharedEntryProperty, metadataLabel));
                    sharedSerializedEditor.OnGUI(rect, sharedEntryProperty, metadataLabel);
                    sharedSerializedObject.ApplyModifiedProperties();
                });
                m_Editor.Add(sharedEditor);

                // Table data
                var tableIndex = m_Table.TableData.FindIndex(d => d.Id == KeyId);
                var tableSerializedObject = new SerializedObject(m_Table);
                if (tableIndex == -1)
                {
                    EditorUtility.SetDirty(m_Table);
                    m_Table.CreateEmpty(KeyId);
                    tableSerializedObject.Update();

                    tableIndex = m_Table.TableData.FindIndex(d => d.Id == KeyId);
                    if (tableIndex == -1)
                        throw new System.Exception($"Failed to create or find a new entry for {KeyId} in the table");
                }

                var tableEntryProperty = tableSerializedObject.FindProperty($"m_TableData.Array.data[{tableIndex}].m_Metadata");
                var tableSerializedEditor = new MetadataCollectionField(){ Type = m_MetadataType };
                var tableLabel = new GUIContent($"{m_Locale?.ToString()} Entry Metadata");
                var tableEditor = new IMGUIContainer(() =>
                {
                    tableSerializedObject.Update();

                    EditorGUILayout.LabelField(tableLabel, EditorStyles.boldLabel);
                    var rect = EditorGUILayout.GetControlRect(true, tableSerializedEditor.GetPropertyHeight(tableEntryProperty, metadataLabel));
                    tableSerializedEditor.OnGUI(rect, tableEntryProperty, metadataLabel);
                    tableSerializedObject.ApplyModifiedProperties();
                });
                m_Editor.Add(tableEditor);
            }

            return m_Editor;
        }
    }
}
