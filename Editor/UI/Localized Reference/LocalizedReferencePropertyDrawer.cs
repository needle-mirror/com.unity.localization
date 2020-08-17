using System;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.UI
{
    abstract class LocalizedReferencePropertyDrawer<TCollection> : PropertyDrawerExtended<LocalizedReferencePropertyDrawer<TCollection>.Data> where TCollection : LocalizationTableCollection
    {
        protected static class Styles
        {
            public static readonly GUIContent addLocale = new GUIContent("Add Locale", "Add a new Locale.");
            public static readonly GUIContent addTable = new GUIContent("Create Table", "Create a new table for the selected locale.");
            public static readonly GUIContent addTableCollection = new GUIContent("Create Table Collection", "Create a new table collection for every Locale in the project");
            public static readonly GUIContent addTableEntry = new GUIContent("Add Table Entry", "Create a new table entry in the selected table collection.");
            public static readonly GUIContent entryName = new GUIContent("Entry Name", "The name or key of the selected table entry");
            public static readonly GUIContent noTableSelected = new GUIContent($"None({typeof(TCollection).Name})");
            public static readonly GUIContent previewArguments = new GUIContent("Preview Arguments", "Arguments to pass to the string formatter. These are for preview purposes only and are not stored.");
            public static readonly GUIContent selectedTable = new GUIContent("Table Collection");
        }

        protected static Func<ReadOnlyCollection<TCollection>> GetProjectTableCollections { get; set; }

        public class Data
        {
            public SerializedObject serializedObject;
            public SerializedTableReference tableReference;
            public SerializedTableEntryReference tableEntryReference;
            public Type assetType;
            public GUIContent entryNameLabel;

            GUIContent m_FieldLabel;
            SharedTableData.SharedTableEntry m_SelectedEntry;
            TCollection m_SelectedTableCollection;
            int m_SelectedTableIdx = -1;

            public bool NeedsInitializing { get; protected set; } = true;

            public int SelectedTableIndex
            {
                get
                {
                    if (m_SelectedTableIdx < 0)
                    {
                        if (SelectedTableCollection == null)
                        {
                            m_SelectedTableIdx = 0;
                        }
                        else
                        {
                            for (int i = 1; i < ProjectTableLabels.Length; ++i)
                            {
                                if (SelectedTableCollection.TableCollectionName == ProjectTableLabels[i].text)
                                {
                                    m_SelectedTableIdx = i;
                                    break;
                                }
                            }
                        }
                    }
                    return m_SelectedTableIdx;
                }
                set
                {
                    m_SelectedTableIdx = value;
                    SelectedTableCollection = s_TableChoices[m_SelectedTableIdx];
                    m_FieldLabel = null;
                }
            }

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
                    m_SelectedTableIdx = -1;
                    SelectedTableEntry = null;
                    if (value != null)
                        tableReference.Reference = value.SharedData.TableCollectionNameGuid;
                    else
                        tableReference.Reference = string.Empty;
                }
            }

            public virtual SharedTableData.SharedTableEntry SelectedTableEntry
            {
                get
                {
                    if (m_SelectedEntry == null && SelectedTableCollection != null)
                    {
                        m_SelectedEntry = m_SelectedTableCollection.SharedData.GetEntryFromReference(tableEntryReference.Reference);
                    }
                    return m_SelectedEntry;
                }
                set
                {
                    m_FieldLabel = null;
                    m_SelectedEntry = value;
                    tableEntryReference.Reference = value != null ? value.Id : SharedTableData.EmptyId;
                }
            }

            public GUIContent FieldLabel
            {
                get
                {
                    if (m_FieldLabel == null)
                    {
                        var icon = EditorGUIUtility.ObjectContent(null, assetType);
                        if (SelectedTableCollection != null && SelectedTableEntry != null)
                        {
                            var key = SelectedTableEntry.Key;
                            var eol = key.IndexOf('\n');
                            if (eol > 0)
                            {
                                // We don't want a multiline label as it overflows in the UI.
                                key = key.Substring(0, eol);
                            }
                            m_FieldLabel = new GUIContent($"{SelectedTableCollection.TableCollectionName}/{key}", icon.image);
                        }
                        else
                        {
                            m_FieldLabel = new GUIContent($"None ({assetType.Name})", icon.image);
                        }
                    }
                    return m_FieldLabel;
                }
            }

            public virtual void Reset()
            {
                serializedObject = null;
                tableReference = null;
                tableEntryReference = null;

                // Clear cached values
                m_FieldLabel = null;
                m_SelectedEntry = null;
                m_SelectedTableCollection = null;
                m_SelectedTableIdx = -1;

                NeedsInitializing = true;
            }

            public virtual void Init(SerializedProperty property)
            {
                serializedObject = property.serializedObject;
                tableReference = new SerializedTableReference(property.FindPropertyRelative("m_TableReference"));
                tableEntryReference = new SerializedTableEntryReference(property.FindPropertyRelative("m_TableEntryReference"));
                NeedsInitializing = false;
            }
        }

        static GUIContent[] s_TableChoicesLabels;
        static TCollection[] s_TableChoices;

        const float k_OpenTableEditorButtonWidth = 30;

        static GUIContent[] ProjectTableLabels
        {
            get
            {
                if (s_TableChoicesLabels == null)
                {
                    var assetTables = GetProjectTableCollections();

                    s_TableChoices = new TCollection[assetTables.Count + 1];
                    assetTables.CopyTo(s_TableChoices, 1);

                    s_TableChoicesLabels = new GUIContent[assetTables.Count + 1];
                    s_TableChoicesLabels[0] = Styles.noTableSelected;
                    for (int i = 0; i < assetTables.Count; ++i)
                    {
                        s_TableChoicesLabels[i + 1] = new GUIContent(assetTables[i].TableCollectionName);
                    }
                }
                return s_TableChoicesLabels;
            }
        }

        public LocalizedReferencePropertyDrawer()
        {
            LocalizationEditorSettings.EditorEvents.CollectionAdded += EditorEvents_CollectionModified;
            LocalizationEditorSettings.EditorEvents.CollectionRemoved += EditorEvents_CollectionModified;
            LocalizationEditorSettings.EditorEvents.CollectionModified += EditorEvents_CollectionModifiedWithSender;
            LocalizationEditorSettings.EditorEvents.LocaleAdded += EditorEvents_LocaleAddedOrRemoved;
            LocalizationEditorSettings.EditorEvents.LocaleRemoved += EditorEvents_LocaleAddedOrRemoved;
            Undo.undoRedoPerformed += UndoRedoPerformed;
        }

        ~LocalizedReferencePropertyDrawer()
        {
            LocalizationEditorSettings.EditorEvents.CollectionAdded -= EditorEvents_CollectionModified;
            LocalizationEditorSettings.EditorEvents.CollectionRemoved -= EditorEvents_CollectionModified;
            LocalizationEditorSettings.EditorEvents.CollectionModified -= EditorEvents_CollectionModifiedWithSender;
            LocalizationEditorSettings.EditorEvents.LocaleAdded -= EditorEvents_LocaleAddedOrRemoved;
            LocalizationEditorSettings.EditorEvents.LocaleRemoved -= EditorEvents_LocaleAddedOrRemoved;
            Undo.undoRedoPerformed -= UndoRedoPerformed;
        }

        void UndoRedoPerformed()
        {
            foreach (var prop in PropertyData.Values)
            {
                prop.Reset();
            }
        }

        void EditorEvents_CollectionModifiedWithSender(object sender, LocalizationTableCollection collection)
        {
            if (sender != this)
                EditorEvents_CollectionModified(collection);
        }

        void EditorEvents_CollectionModified(LocalizationTableCollection obj)
        {
            s_TableChoicesLabels = null;
            s_TableChoices = null;
            ClearPropertyDataCache();
        }

        void EditorEvents_LocaleAddedOrRemoved(Locale obj)
        {
            s_TableChoicesLabels = null;
            s_TableChoices = null;
            ClearPropertyDataCache();
        }

        public override Data GetDataForProperty(SerializedProperty property)
        {
            var data = base.GetDataForProperty(property);
            if (data.NeedsInitializing)
                data.Init(property);
            return data;
        }

        public override Data CreatePropertyData(SerializedProperty property) => new Data { entryNameLabel = Styles.entryName };

        public override void OnGUI(Data data, Rect position, SerializedProperty property, GUIContent label)
        {
            var rowPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            var foldoutRect = new Rect(rowPosition.x, rowPosition.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            var dropDownPosition = new Rect(foldoutRect.xMax, rowPosition.y, rowPosition.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            rowPosition.y += rowPosition.height + EditorGUIUtility.standardVerticalSpacing;
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

            if (EditorGUI.DropdownButton(dropDownPosition, data.FieldLabel, FocusType.Passive))
            {
                var treeSelection = new TableEntryTreeView(data.assetType, (collection, entry) =>
                {
                    data.SelectedTableCollection = collection as TCollection;
                    data.SelectedTableEntry = entry;

                    // Will be called outside of OnGUI so we need to call ApplyModifiedProperties.
                    data.serializedObject.ApplyModifiedProperties();
                });

                PopupWindow.Show(dropDownPosition, new TreeViewPopupWindow(treeSelection) { Width = dropDownPosition.width });
            }

            DrawTableDetails(position, rowPosition, data, property);
        }

        void DrawTableDetails(Rect position, Rect rowPosition, Data data, SerializedProperty property)
        {
            if (!property.isExpanded)
                return;

            EditorGUI.indentLevel++;

            // Table selection
            var tableSelectionPos = data.SelectedTableIndex != 0 ? new Rect(rowPosition.x, rowPosition.y, rowPosition.width - k_OpenTableEditorButtonWidth, rowPosition.height) : rowPosition;
            EditorGUI.BeginChangeCheck();
            var selectedTableIndex = EditorGUI.Popup(tableSelectionPos, Styles.selectedTable, data.SelectedTableIndex, ProjectTableLabels);
            if (EditorGUI.EndChangeCheck())
            {
                data.SelectedTableIndex = selectedTableIndex;
            }

            if (data.SelectedTableIndex != 0)
            {
                var openTableEditorPos = new Rect(tableSelectionPos.xMax, tableSelectionPos.y, k_OpenTableEditorButtonWidth, tableSelectionPos.height);
                if (GUI.Button(openTableEditorPos, EditorIcons.StringTable))
                {
                    LocalizationTablesWindow.ShowWindow(data.SelectedTableCollection);
                }
            }

            rowPosition.y += rowPosition.height;
            var buttonPos = new Rect(rowPosition.x + EditorGUIUtility.labelWidth, rowPosition.y, rowPosition.width - EditorGUIUtility.labelWidth, rowPosition.height);
            if (selectedTableIndex == 0)
            {
                if (GUI.Button(buttonPos, Styles.addTableCollection, EditorStyles.miniButton))
                {
                    LocalizationTablesWindow.ShowTableCreator();
                }
            }
            else
            {
                if (GUI.Button(buttonPos, Styles.addTableEntry, EditorStyles.miniButton))
                {
                    // Attempt to create a new key with a set default name
                    var keys = data.SelectedTableCollection.SharedData;

                    const string newKeyName = "New Entry";
                    var keyToTry = newKeyName;
                    SharedTableData.SharedTableEntry entry = null;
                    int counter = 1;
                    while (entry == null)
                    {
                        if (keys.Contains(keyToTry))
                        {
                            keyToTry = $"{newKeyName} {counter++}";
                        }
                        else
                        {
                            Undo.RecordObject(keys, "Add entry.");
                            entry = keys.AddKey(keyToTry);
                            EditorUtility.SetDirty(keys);
                        }
                    }
                    data.SelectedTableEntry = entry;
                    LocalizationEditorSettings.EditorEvents.RaiseTableEntryAdded(data.SelectedTableCollection, entry);
                }
            }

            rowPosition.y += rowPosition.height;
            buttonPos.y = rowPosition.y;
            if (GUI.Button(buttonPos, Styles.addLocale))
            {
                LocaleGeneratorWindow.ShowWindow();
            }
            rowPosition.y += rowPosition.height + EditorGUIUtility.standardVerticalSpacing;

            if (data.SelectedTableEntry != null)
                DrawTableEntryDetails(ref rowPosition, data, position);
            EditorGUI.indentLevel--;
        }

        protected virtual void DrawTableEntryDetails(ref Rect rowPosition, Data data, Rect position)
        {
            // Entry name
            EditorGUI.BeginChangeCheck();
            rowPosition.height = EditorStyles.textField.CalcHeight(new GUIContent(data.SelectedTableEntry?.Key), rowPosition.width);
            var newName = EditorGUI.TextField(rowPosition, data.entryNameLabel, data.SelectedTableEntry?.Key);
            rowPosition.y += rowPosition.height + EditorGUIUtility.standardVerticalSpacing;
            if (EditorGUI.EndChangeCheck() && data.SelectedTableCollection != null)
            {
                // Prevent renaming to a new that is already taken.
                var sharedData = data.SelectedTableCollection.SharedData;
                var entry = sharedData.GetEntry(newName);

                if (string.IsNullOrEmpty(newName))
                {
                    data.entryNameLabel = new GUIContent(Styles.entryName.text, EditorIcons.WarningIcon, "Names can not be empty.");
                }
                else if (entry == null || data.SelectedTableEntry == entry)
                {
                    Undo.RecordObject(sharedData, "Rename key entry");
                    sharedData.RenameKey(data.SelectedTableEntry.Key, newName);
                    EditorUtility.SetDirty(sharedData);
                    data.entryNameLabel = Styles.entryName;
                }
                else
                {
                    data.entryNameLabel = new GUIContent(Styles.entryName.text, EditorIcons.WarningIcon, $"Can not rename key to '{newName}', the name is already in use by Key Id {entry.Id}.");
                }
            }
        }

        public override float GetPropertyHeight(Data data, SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // Foldout field height
            if (property.isExpanded)
            {
                height += EditorGUIUtility.singleLineHeight; // Selected table
                height += EditorGUIUtility.singleLineHeight; // create table/add entry button
                height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // Add locale button

                if (data.SelectedTableEntry != null)
                {
                    // Entry name
                    height += EditorStyles.textField.CalcHeight(new GUIContent(data.SelectedTableEntry?.Key), EditorGUIUtility.currentViewWidth) + EditorGUIUtility.standardVerticalSpacing;
                }
            }
            return height;
        }
    }
}
