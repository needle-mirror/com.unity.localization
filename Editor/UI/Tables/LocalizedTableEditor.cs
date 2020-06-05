using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.UI
{
    [CustomEditor(typeof(LocalizedTable), true)]
    class LocalizedTableEditor : UnityEditor.Editor
    {
        LocalizedTable m_TargetTable;
        GUIContent m_TableEditorButton;
        GUIContent m_CollectionButton;
        SerializedProperty m_LocaleId;
        SerializedProperty m_SharedTableData;

        SerializedObject m_SharedTableDataSerializedObject;
        SerializedProperty m_TableCollectionName;

        LocalizedTableCollection m_Collection;
        LocalizedTableCollection m_SharedTableDataCollection;
        List<LocalizedTable> m_PossibleTableCollection = new List<LocalizedTable>(); // If m_Collection is null

        class Styles
        {
            public static readonly GUIContent addToCollection = new GUIContent("Add table to collection", "Adds the table to the collection that shares the same Shared Table Data");
            public static readonly GUIContent createAssetTableCollection = new GUIContent("Create Asset Table Collection", "Creates a new asset table collection from the loose tables.");
            public static readonly GUIContent createStringTableCollection = new GUIContent("Create String Table Collection", "Creates a new string table collection from the loose tables.");
            public static readonly GUIContent removeFromCollection = new GUIContent("Remove table from collection", "Removes the table from the collection so that is is not used in the project");
            public static readonly GUIContent removeTableFromList = new GUIContent("-");
            public static readonly GUIContent sharedTableData = new GUIContent("Shared Table");
        }

        public virtual void OnEnable()
        {
            Undo.undoRedoPerformed += ResolveTableCollection;
            LocalizationEditorSettings.EditorEvents.TableAddedToCollection += OnCollectionTableChange;
            LocalizationEditorSettings.EditorEvents.TableRemovedFromCollection += OnCollectionTableChange;
            LocalizationEditorSettings.EditorEvents.CollectionAdded += OnCollectionChange;
            LocalizationEditorSettings.EditorEvents.CollectionRemoved += OnCollectionChange;

            if (target == null)
                return;

            m_TargetTable = target as LocalizedTable;
            m_LocaleId = serializedObject.FindProperty("m_LocaleId");
            m_SharedTableData = serializedObject.FindProperty("m_SharedData");
            m_TableEditorButton = new GUIContent("Open Table Editor", EditorGUIUtility.ObjectContent(target, target.GetType()).image);
            ResolveTableCollection();
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= ResolveTableCollection;
            LocalizationEditorSettings.EditorEvents.TableAddedToCollection -= OnCollectionTableChange;
            LocalizationEditorSettings.EditorEvents.TableRemovedFromCollection -= OnCollectionTableChange;
            LocalizationEditorSettings.EditorEvents.CollectionAdded -= OnCollectionChange;
            LocalizationEditorSettings.EditorEvents.CollectionRemoved -= OnCollectionChange;
        }

        void OnCollectionTableChange(LocalizedTableCollection col, LocalizedTable table) => ResolveTableCollection();
        void OnCollectionChange(LocalizedTableCollection col) => ResolveTableCollection();

        void ResolveTableCollection()
        {
            m_PossibleTableCollection.Clear();
            m_Collection = LocalizationEditorSettings.GetCollectionFromTable(m_TargetTable);

            if (m_TargetTable.SharedData == null)
                return;

            m_SharedTableDataSerializedObject = new SerializedObject(m_TargetTable.SharedData);
            m_TableCollectionName = m_SharedTableDataSerializedObject.FindProperty("m_TableCollectionName");

            if (m_Collection != null)
            {
                m_CollectionButton = new GUIContent("Select Collection", EditorGUIUtility.ObjectContent(m_Collection, m_Collection.GetType()).image);
                return;
            }

            m_SharedTableDataCollection = LocalizationEditorSettings.GetCollectionForSharedTableData(m_TargetTable.SharedData);
            if (m_SharedTableDataCollection != null)
                return;

            LocalizationEditorSettings.FindLooseStringTablesUsingSharedTableData(m_TargetTable.SharedData, m_PossibleTableCollection);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            m_SharedTableDataSerializedObject?.Update();

            EditorGUILayout.LabelField("Table Collection Name", m_TableCollectionName?.stringValue);
            EditorGUILayout.PropertyField(m_LocaleId);

            EditorGUILayout.Space();

            if (m_Collection == null)
            {
                DrawLooseTableGUI();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(m_CollectionButton, EditorStyles.miniButton, GUILayout.Width(150), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                {
                    Selection.activeObject = m_Collection;
                }
                if (GUILayout.Button(m_TableEditorButton, EditorStyles.miniButton, GUILayout.Width(150), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                {
                    LocalizationTablesWindow.ShowWindow(m_Collection);
                }
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button(Styles.removeFromCollection))
                {
                    m_Collection.RemoveTable(m_TargetTable, true);
                }
            }

            EditorGUILayout.Space();
            serializedObject.ApplyModifiedProperties();
        }

        void DrawLooseTableGUI()
        {
            if (m_TargetTable.SharedData == null)
            {
                EditorGUILayout.HelpBox("Shared Table Data is missing. Please add the missing asset or create a new one.", MessageType.Error);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.ObjectField(m_SharedTableData);
                if (EditorGUI.EndChangeCheck())
                {
                    ResolveTableCollection();
                }
                return;
            }

            EditorGUILayout.HelpBox("Table does not belong to a Collection and will be ignored.", MessageType.Warning);
            if (m_SharedTableDataCollection != null)
            {
                if (GUILayout.Button(Styles.addToCollection))
                {
                    m_SharedTableDataCollection.AddTable(m_TargetTable, true);
                }
                return;
            }

            if (m_PossibleTableCollection.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("The following loose tables could be combined into a new Table Collection", MessageType.Info);
                for (int i = 0; i < m_PossibleTableCollection.Count; ++i)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(m_PossibleTableCollection[i].name, EditorStyles.label))
                    {
                        EditorGUIUtility.PingObject(m_PossibleTableCollection[i]);
                    }
                    if (GUILayout.Button(Styles.removeTableFromList, GUILayout.Width(40)))
                    {
                        m_PossibleTableCollection.RemoveAt(i);
                        EditorGUIUtility.ExitGUI();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            if (m_PossibleTableCollection.Count > 0)
            {
                var isStringTable = typeof(StringTable).IsAssignableFrom(m_PossibleTableCollection[0].GetType());
                var label = isStringTable ? Styles.createStringTableCollection : Styles.createAssetTableCollection;
                if (GUILayout.Button(label))
                {
                    var defaultDirectory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(m_TargetTable.SharedData));
                    var path = EditorUtility.SaveFilePanel("Create Table Collection", defaultDirectory, m_TargetTable.TableCollectionName, "asset");
                    if (string.IsNullOrEmpty(path))
                        return;
                    LocalizationEditorSettings.CreateCollectionFromLooseTables(m_PossibleTableCollection, path);
                }
            }
        }
    }
}
