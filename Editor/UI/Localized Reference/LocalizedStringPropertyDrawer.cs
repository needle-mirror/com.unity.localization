using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.UI
{
    [CustomPropertyDrawer(typeof(LocalizedString), true)]
    class LocalizedStringPropertyDrawer : LocalizedReferencePropertyDrawer<StringTableCollection>
    {
        public bool ShowPreview { get; set; } = true;

        static GUIStyle s_FoldoutStyle;

        /// <summary>
        /// Optional arguments list.
        /// </summary>
        public SmartObjects Arguments { get; set; }

        public class StringPropertyData : Data
        {
            List<LocaleField> m_SmartFormatFields;

            public ReorderableList previewArgumentsList;
            public bool previewExpanded;
            public SmartObjects previewArguments;

            public class LocaleField
            {
                public Locale Locale { get; set; }
                public bool Expanded { get; set; }
                public SmartFormatField SmartEditor { get; set; }
            }

            public List<LocaleField> LocaleFields
            {
                get
                {
                    if (m_SmartFormatFields == null && SelectedTableEntry != null)
                    {
                        m_SmartFormatFields = new List<LocaleField>();
                        var projectLocales = LocalizationEditorSettings.GetLocales();

                        foreach (var locale in projectLocales)
                        {
                            var table = SelectedTableCollection.Tables.FirstOrDefault(tbl => tbl.asset?.LocaleIdentifier == locale.Identifier);
                            if (previewArguments == null)
                                previewArguments = new SmartObjects();
                            m_SmartFormatFields.Add(new LocaleField { Locale = locale, Expanded = false, SmartEditor = CreateSmartFormatFieldForTable(table.asset) });
                        }
                    }
                    return m_SmartFormatFields;
                }
            }

            public StringPropertyData()
            {
                assetType = typeof(string);
            }

            public SmartFormatField CreateSmartFormatFieldForTable(LocalizationTable table)
            {
                if (table is StringTable stringTable)
                {
                    var smartField = new SmartFormatField();
                    smartField.Table = stringTable;
                    smartField.KeyId = SelectedTableEntry.Id;
                    smartField.RawText = SelectedTableEntry.Key;
                    smartField.ShowMetadataButton = false;
                    smartField.ShowPreviewTab = true;
                    smartField.MinHeight = EditorGUIUtility.singleLineHeight;
                    smartField.Arguments = previewArguments;
                    smartField.RefreshData();
                    return smartField;
                }
                return null;
            }

            public override SharedTableData.SharedTableEntry SelectedTableEntry
            {
                set
                {
                    m_SmartFormatFields = null; // Reset cache.
                    base.SelectedTableEntry = value;
                }
            }

            public void UpdateArguments(ReorderableList _)
            {
                previewArguments.Clear();
                previewArguments.AddRange(previewArgumentsList.list as List<Object>);
                LocaleFields.ForEach(sf => sf.SmartEditor.ResetCache());
            }

            void DrawPreviewElement(Rect rect, int index, bool isActive, bool isFocused)
            {
                var list = previewArgumentsList.list as List<Object>;
                EditorGUI.BeginChangeCheck();
                list[index] = EditorGUI.ObjectField(rect, list[index], typeof(Object), true);
                if (EditorGUI.EndChangeCheck())
                    UpdateArguments(previewArgumentsList);
            }

            public override void Init(SerializedProperty property)
            {
                base.Init(property);

                previewArgumentsList = new ReorderableList(new List<Object>(), typeof(Object));
                previewArgumentsList.headerHeight = 1;
                previewArgumentsList.drawElementCallback = DrawPreviewElement;
                previewArgumentsList.onChangedCallback = UpdateArguments;

                serializedObject = property.serializedObject;
                tableReference = new SerializedTableReference(property.FindPropertyRelative("m_TableReference"));
                tableEntryReference = new SerializedTableEntryReference(property.FindPropertyRelative("m_TableEntryReference"));
                NeedsInitializing = true;

                if (LocaleFields != null)
                {
                    foreach (var field in LocaleFields)
                    {
                        field.SmartEditor.RefreshData();
                    }
                }
            }
        }

        static LocalizedStringPropertyDrawer()
        {
            GetProjectTableCollections = LocalizationEditorSettings.GetStringTableCollections;
        }

        public override Data CreatePropertyData(SerializedProperty property)
        {
            var prop = new StringPropertyData()
            {
                entryNameLabel = Styles.entryName,
                previewArguments = Arguments,
            };
            return prop;
        }

        protected override void DrawTableEntryDetails(ref Rect rowPosition, Data data, Rect position)
        {
            base.DrawTableEntryDetails(ref rowPosition, data, position);
            var stringPropertyData = (StringPropertyData)data;

            // We want to clip long labels (case LOC-84)
            if (s_FoldoutStyle == null)
                s_FoldoutStyle = new GUIStyle(EditorStyles.foldoutHeader) { clipping = TextClipping.Clip };

            for (int i = 0; i < stringPropertyData.LocaleFields.Count; ++i)
            {
                var field = stringPropertyData.LocaleFields[i];
                var label = new GUIContent(field.Locale.Identifier.ToString());

                // Is this a missing table?
                if (field.SmartEditor == null)
                {
                    rowPosition.height = EditorGUIUtility.singleLineHeight;
                    var buttonPosition = EditorGUI.PrefixLabel(rowPosition, label);
                    if (GUI.Button(buttonPosition, "Create Table"))
                    {
                        var table = stringPropertyData.SelectedTableCollection.AddNewTable(field.Locale.Identifier);
                        field.SmartEditor = stringPropertyData.CreateSmartFormatFieldForTable(table);
                        stringPropertyData.LocaleFields[i] = field;
                        GUIUtility.ExitGUI();
                    }
                    rowPosition.y += rowPosition.height + EditorGUIUtility.standardVerticalSpacing;
                    continue;
                }

                // Locale label/foldout
                rowPosition.height = EditorGUIUtility.singleLineHeight;

                float xMin = rowPosition.xMin; // Store the x position so we can restore it at the end.
                rowPosition = EditorGUI.PrefixLabel(rowPosition, GUIContent.none);

                var labelWidth = EditorGUIUtility.labelWidth - ((EditorGUI.indentLevel + 1) * 15);
                var foldoutPos = new Rect(rowPosition.x, rowPosition.y, labelWidth, rowPosition.height);
                var labelPos = new Rect(rowPosition.x + labelWidth, rowPosition.y, rowPosition.width - labelWidth, rowPosition.height);
                field.Expanded = EditorGUI.BeginFoldoutHeaderGroup(foldoutPos, field.Expanded, label, s_FoldoutStyle);

                // Preview label
                EditorGUI.LabelField(labelPos, field.SmartEditor.Label);
                rowPosition.y += rowPosition.height + EditorGUIUtility.standardVerticalSpacing;

                if (field.Expanded)
                {
                    rowPosition.height = field.SmartEditor.Height;
                    field.SmartEditor.Draw(rowPosition);
                    rowPosition.y += rowPosition.height + EditorGUIUtility.standardVerticalSpacing;
                }
                EditorGUI.EndFoldoutHeaderGroup();
                stringPropertyData.LocaleFields[i] = field;
                rowPosition.xMin = xMin;
            }

            // Preview tab
            if (ShowPreview)
            {
                rowPosition.height = EditorGUIUtility.singleLineHeight;
                stringPropertyData.previewExpanded = EditorGUI.BeginFoldoutHeaderGroup(rowPosition, stringPropertyData.previewExpanded, Styles.previewArguments);
                rowPosition.y += rowPosition.height + EditorGUIUtility.standardVerticalSpacing;
                if (stringPropertyData.previewExpanded)
                {
                    rowPosition.height = stringPropertyData.previewArgumentsList.GetHeight();
                    var listPos = EditorGUI.PrefixLabel(rowPosition, GUIContent.none);
                    stringPropertyData.previewArgumentsList.DoList(listPos);
                }
            }
        }

        public override float GetPropertyHeight(Data data, SerializedProperty property, GUIContent label)
        {
            float height = base.GetPropertyHeight(data, property, label);

            if (property.isExpanded && data.SelectedTableEntry != null)
            {
                var stringPropertyData = (StringPropertyData)data;

                foreach (var field in stringPropertyData.LocaleFields)
                {
                    height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // Locale label/foldout
                    if (field.Expanded)
                    {
                        if (field.SmartEditor != null)
                        {
                            height += field.SmartEditor.Height + EditorGUIUtility.standardVerticalSpacing;
                        }
                        else
                        {
                            height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // Locale label/foldout
                        }
                    }
                }

                // Preview field
                if (ShowPreview)
                {
                    height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // Preview foldout
                    if (stringPropertyData.previewExpanded)
                    {
                        height += stringPropertyData.previewArgumentsList.GetHeight() + EditorGUIUtility.standardVerticalSpacing; // Preview foldout
                    }
                }
            }
            return height;
        }
    }
}
