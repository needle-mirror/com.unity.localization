using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor.Localization.Plugins.Google.Columns;
using UnityEditor.Localization.Reporting;
using UnityEditor.Localization.UI;
using UnityEngine;

namespace UnityEditor.Localization.Plugins.Google
{
    class GoogleSheetsExtensionPropertyDrawerData
    {
        public SerializedProperty m_Collection;
        public SerializedProperty m_SheetsServiceProvider;
        public SerializedProperty m_SpreadSheetId;
        public SerializedProperty m_SheetId;
        public SerializedProperty m_Columns;
        public SerializedProperty m_RemoveMissingPulledKeys;

        public string m_NewSheetName;
        public ReorderableListExtended columnsList;

        public Task pushTask;

        public SheetsServiceProvider Provider => m_SheetsServiceProvider.objectReferenceValue as SheetsServiceProvider;
    }

    [CustomPropertyDrawer(typeof(GoogleSheetsExtension))]
    class GoogleSheetsExtensionPropertyDrawer : PropertyDrawerExtended<GoogleSheetsExtensionPropertyDrawerData>
    {
        class Styles
        {
            public static readonly GUIContent addDefaultColumns = new GUIContent("Add Default Columns");
            public static readonly GUIContent addLocalesInSheet = new GUIContent("Add Project Locales Found In Sheet");
            public static readonly GUIContent addSheet = new GUIContent("Add Sheet");
            public static readonly GUIContent createNewSpredsheet = new GUIContent("Create New Spreadsheet");
            public static readonly GUIContent extractColumns = new GUIContent("Extract Columns From Sheet");
            public static readonly GUIContent header = new GUIContent("Google Sheets");
            public static readonly GUIContent newSheetName = new GUIContent("Sheet Name");
            public static readonly GUIContent noSheetsFound = new GUIContent("No Sheets Could Be Found");
            public static readonly GUIContent mappedColumns = new GUIContent("Mapped Columns");
            public static readonly GUIContent openSpreadsheet = new GUIContent("Open", "Opens the sheet in an external browser");
            public static readonly GUIContent push = new GUIContent("Push");
            public static readonly GUIContent pull = new GUIContent("Pull");
            public static readonly GUIContent pushSelected = new GUIContent("Push Selected");
            public static readonly GUIContent pullSelected = new GUIContent("Pull Selected");
        }

        /// <summary>
        /// Keep track of any Async push requests in case the property drawer is closed/reopened.
        /// </summary>
        static List<(LocalizationTableCollection collection, Task pushTask)> s_PushRequests = new List<(LocalizationTableCollection, Task)>();

        public override GoogleSheetsExtensionPropertyDrawerData CreatePropertyData(SerializedProperty property)
        {
            var data =  new GoogleSheetsExtensionPropertyDrawerData
            {
                m_Collection = property.FindPropertyRelative("m_Collection"),
                m_SheetsServiceProvider = property.FindPropertyRelative("m_SheetsServiceProvider"),
                m_SpreadSheetId = property.FindPropertyRelative("m_SpreadsheetId"),
                m_SheetId = property.FindPropertyRelative("m_SheetId"),
                m_Columns = property.FindPropertyRelative("m_Columns"),
                m_RemoveMissingPulledKeys = property.FindPropertyRelative("m_RemoveMissingPulledKeys")
            };

            var ltc = data.m_Collection.objectReferenceValue as LocalizationTableCollection;
            data.m_NewSheetName = ltc.TableCollectionName;
            data.columnsList = new ReorderableListExtended(property.serializedObject, data.m_Columns);
            data.columnsList.Header = Styles.mappedColumns;
            data.columnsList.AddMenuType = typeof(SheetColumn);

            var currentPushRequest = s_PushRequests.FirstOrDefault(tc => ReferenceEquals(tc.collection, ltc));
            if (currentPushRequest != default)
            {
                if (currentPushRequest.pushTask.IsCompleted)
                    s_PushRequests.Remove(currentPushRequest);
                else
                    data.pushTask = currentPushRequest.pushTask;
            }

            data.columnsList.AddMenuItems = menu =>
            {
                menu.AddSeparator(string.Empty);
                menu.AddItem(Styles.addDefaultColumns, false, () =>
                {
                    var columns = ColumnMapping.CreateDefaultMapping();

                    data.m_Columns.ClearArray();
                    foreach (var c in columns)
                    {
                        var colElement = data.m_Columns.AddArrayElement();
                        colElement.managedReferenceValue = c;
                    }
                    data.m_Columns.serializedObject.ApplyModifiedProperties();
                });

                if (string.IsNullOrEmpty(data.m_SpreadSheetId.stringValue) || data.m_SheetId.intValue <= 0)
                {
                    menu.AddDisabledItem(Styles.extractColumns);
                }
                else
                {
                    menu.AddItem(Styles.extractColumns, false, () =>
                    {
                        var target = property.GetActualObjectForSerializedProperty<GoogleSheetsExtension>(fieldInfo);
                        var google = GetGoogleSheets(data);
                        var titles = google.GetColumnTitles(data.m_SheetId.intValue);
                        List<string> unused = new List<string>();
                        var columns = ColumnMapping.CreateMappingsFromColumnNames(titles, unused);

                        if (unused.Count > 0)
                        {
                            Debug.Log($"Could not map: {string.Join(", ", unused)}");
                        }

                        data.m_Columns.ClearArray();
                        foreach (var c in columns)
                        {
                            var colElement = data.m_Columns.AddArrayElement();
                            colElement.managedReferenceValue = c;
                        }
                        data.m_Columns.serializedObject.ApplyModifiedProperties();
                    });
                }
            };

            data.columnsList.CreateNewInstance = type =>
            {
                // Try to extract the next free column
                var instance = Activator.CreateInstance(type) as SheetColumn;
                try
                {
                    var target = property.GetActualObjectForSerializedProperty<GoogleSheetsExtension>(fieldInfo);
                    var usedColumns = target.Columns.Where(c => !string.IsNullOrEmpty(c.Column)).Select(c => c.Column);
                    instance.Column = ColumnMapping.GetNextAvailableColumn(usedColumns.ToArray());
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                return instance;
            };
            return data;
        }

        public override void OnGUI(GoogleSheetsExtensionPropertyDrawerData data, Rect position, SerializedProperty property, GUIContent label)
        {
            position.yMin += EditorGUIUtility.standardVerticalSpacing;
            position.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.LabelField(position, Styles.header, EditorStyles.boldLabel);
            position.MoveToNextLine();

            EditorGUI.PropertyField(position, data.m_SheetsServiceProvider);
            position.MoveToNextLine();

            EditorGUI.BeginDisabledGroup(data.m_SheetsServiceProvider.objectReferenceValue == null);
            using (new EditorGUI.DisabledGroupScope(data.m_SheetsServiceProvider.objectReferenceValue == null))
            {
                if (GUI.Button(position, Styles.createNewSpredsheet))
                {
                    var google = GetGoogleSheets(data);
                    var results = google.CreateSpreadsheet(PlayerSettings.productName, data.m_NewSheetName, data.Provider.NewSheetProperties, new ProgressBarReporter { ReportTaskSummaryInConsole = true });
                    data.m_SpreadSheetId.stringValue = results.spreadSheetId;
                    data.m_SheetId.intValue = results.sheetId;
                }
                position.MoveToNextLine();

                var spreadSheetPos = position.SplitHorizontalFixedWidthRight(50);
                EditorGUI.PropertyField(spreadSheetPos.left, data.m_SpreadSheetId, true);

                using (new EditorGUI.DisabledGroupScope(string.IsNullOrEmpty(data.m_SpreadSheetId.stringValue)))
                {
                    if (GUI.Button(spreadSheetPos.right, Styles.openSpreadsheet))
                    {
                        GoogleSheets.OpenSheetInBrowser(data.m_SpreadSheetId.stringValue, data.m_SheetId.intValue);
                    }
                }

                position.MoveToNextLine();
                DrawSheetField(data, ref position);
                DrawColumnsField(data, ref position);
                DrawSyncControls(data, property, ref position);
            }
        }

        void DrawSheetField(GoogleSheetsExtensionPropertyDrawerData data, ref Rect position)
        {
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(data.m_SpreadSheetId.stringValue));
            // Add new sheet, select sheet
            EditorGUI.PropertyField(position, data.m_SheetId);
            position.MoveToNextLine();

            var sheetNamePos = position.SplitHorizontal();
            var buttonPos = sheetNamePos.right.SplitHorizontal();
            data.m_NewSheetName = EditorGUI.TextField(sheetNamePos.left, data.m_NewSheetName);

            if (GUI.Button(buttonPos.left, Styles.addSheet))
            {
                try
                {
                    EditorUtility.DisplayProgressBar("Add Sheet", string.Empty, 0);
                    var google = GetGoogleSheets(data);
                    data.m_SheetId.intValue = google.AddSheet(data.m_NewSheetName, data.Provider.NewSheetProperties);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }
            }

            if (EditorGUI.DropdownButton(buttonPos.right, new GUIContent("Select Sheet"), FocusType.Passive))
            {
                try
                {
                    EditorUtility.DisplayProgressBar("Fetching Sheet", string.Empty, 0);
                    var google = GetGoogleSheets(data);
                    var sheets = google.GetSheets();
                    EditorUtility.ClearProgressBar();

                    var menu = new GenericMenu();
                    foreach (var s in sheets)
                    {
                        menu.AddItem(new GUIContent(s.name), data.m_SheetId.intValue == s.id, () =>
                        {
                            data.m_SheetId.intValue = s.id;
                            data.m_SheetId.serializedObject.ApplyModifiedProperties();
                        });
                    }

                    if (menu.GetItemCount() == 0)
                        menu.AddDisabledItem(Styles.noSheetsFound);

                    menu.DropDown(buttonPos.right);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                }
            }

            position.MoveToNextLine();
            EditorGUI.EndDisabledGroup();
        }

        void DrawColumnsField(GoogleSheetsExtensionPropertyDrawerData data, ref Rect position)
        {
            position.height = data.columnsList.GetHeight();
            data.columnsList.DoList(position);
            position.MoveToNextLine();
            position.height = EditorGUIUtility.singleLineHeight;
            position.MoveToNextLine();
        }

        void DrawSyncControls(GoogleSheetsExtensionPropertyDrawerData data, SerializedProperty property, ref Rect position)
        {
            EditorGUI.PropertyField(position, data.m_RemoveMissingPulledKeys);
            position.MoveToNextLine();

            // Disable if we have no destination sheet.
            var splitRow = position.SplitHorizontal();
            position.MoveToNextLine();

            if (data.pushTask != null && data.pushTask.IsCompleted)
            {
                var target = property.GetActualObjectForSerializedProperty<GoogleSheetsExtension>(fieldInfo);
                var collection = target.TargetCollection as StringTableCollection;
                var currentPushRequest = s_PushRequests.FirstOrDefault(tc => ReferenceEquals(tc.collection, collection));
                s_PushRequests.Remove(currentPushRequest);
                data.pushTask = null;
            }

            using (new EditorGUI.DisabledGroupScope(data.pushTask != null || string.IsNullOrEmpty(data.m_SpreadSheetId.stringValue) || data.m_SheetId.intValue <= 0 || data.columnsList.count == 0))
            {
                using (new EditorGUI.DisabledGroupScope(data.columnsList.index < 0))
                {
                    if (GUI.Button(splitRow.left, Styles.pushSelected))
                    {
                        var google = GetGoogleSheets(data);
                        var target = property.GetActualObjectForSerializedProperty<GoogleSheetsExtension>(fieldInfo);
                        var selectedCollection = GetSelectedColumns(data.columnsList.index, property);
                        var collection = target.TargetCollection as StringTableCollection;
                        data.pushTask = google.PushStringTableCollectionAsync(data.m_SheetId.intValue, collection, selectedCollection, TaskReporter.CreateDefaultReporter());
                        s_PushRequests.Add((collection, data.pushTask));
                    }
                    if (GUI.Button(splitRow.right, Styles.pullSelected))
                    {
                        var google = GetGoogleSheets(data);
                        var target = property.GetActualObjectForSerializedProperty<GoogleSheetsExtension>(fieldInfo);
                        var selectedCollection = GetSelectedColumns(data.columnsList.index, property);
                        google.PullIntoStringTableCollection(data.m_SheetId.intValue, target.TargetCollection as StringTableCollection, selectedCollection, data.m_RemoveMissingPulledKeys.boolValue, TaskReporter.CreateDefaultReporter(), true);
                    }
                }

                splitRow = position.SplitHorizontal();
                position.MoveToNextLine();
                if (GUI.Button(splitRow.left, Styles.push))
                {
                    var google = GetGoogleSheets(data);
                    var target = property.GetActualObjectForSerializedProperty<GoogleSheetsExtension>(fieldInfo);
                    var collection = target.TargetCollection as StringTableCollection;
                    data.pushTask = google.PushStringTableCollectionAsync(data.m_SheetId.intValue, collection, target.Columns, TaskReporter.CreateDefaultReporter());
                    s_PushRequests.Add((collection, data.pushTask));
                }
                if (GUI.Button(splitRow.right, Styles.pull))
                {
                    var google = GetGoogleSheets(data);
                    var target = property.GetActualObjectForSerializedProperty<GoogleSheetsExtension>(fieldInfo);
                    google.PullIntoStringTableCollection(data.m_SheetId.intValue, target.TargetCollection as StringTableCollection, target.Columns, data.m_RemoveMissingPulledKeys.boolValue, TaskReporter.CreateDefaultReporter(), true);
                }
            }
        }

        IList<SheetColumn> GetSelectedColumns(int selectedIndex, SerializedProperty property)
        {
            // Return the selected column and a key column
            var target = property.GetActualObjectForSerializedProperty<GoogleSheetsExtension>(fieldInfo);
            var selectedCol = target.Columns[selectedIndex];

            // If the selected column is the key column then thats all we need.
            if (selectedCol is IPullKeyColumn)
                return new SheetColumn[] { selectedCol };

            // Find the Key column and add it.
            var keyCol = target.Columns.FirstOrDefault(c => c is IPullKeyColumn);
            if (keyCol == null)
            {
                Debug.LogError($"Missing Key Column. A Key Column is required in order to pull and push, please add one to '{Styles.mappedColumns.text}'");
                return null;
            }

            return new SheetColumn[] { keyCol, selectedCol };
        }

        GoogleSheets GetGoogleSheets(GoogleSheetsExtensionPropertyDrawerData data)
        {
            var google = new GoogleSheets(data.Provider);
            google.SpreadSheetId = data.m_SpreadSheetId.stringValue;
            return google;
        }

        public override float GetPropertyHeight(GoogleSheetsExtensionPropertyDrawerData data, SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.standardVerticalSpacing; // top padding
            height += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 10;
            height += data.columnsList.GetHeight() + EditorGUIUtility.standardVerticalSpacing;
            return height;
        }
    }
}
