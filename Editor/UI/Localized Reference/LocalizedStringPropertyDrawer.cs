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

        class StringPropertyData : PropertyData
        {
            List<(Locale locale, bool expanded, SmartFormatField)> m_SmartFormatFields;

            public ReorderableList previewArgumentsList;
            public bool previewExpanded;
            public SmartObjects previewArguments;

            public List<(Locale locale, bool expanded, SmartFormatField smartEditor)> LocaleFields
            {
                get
                {
                    if (m_SmartFormatFields == null && SelectedTableEntry != null)
                    {
                        m_SmartFormatFields = new List<(Locale locale, bool expanded, SmartFormatField)>();
                        var projectLocales = LocalizationEditorSettings.GetLocales();

                        foreach (var locale in projectLocales)
                        {
                            var table = SelectedTableCollection.Tables.FirstOrDefault(tbl => tbl.asset?.LocaleIdentifier == locale.Identifier);
                            if (previewArguments == null)
                                previewArguments = new SmartObjects();
                            m_SmartFormatFields.Add((locale, false, CreateSmartFormatFieldForTable(table.asset)));
                        }
                    }
                    return m_SmartFormatFields;
                }
            }

            public SmartFormatField CreateSmartFormatFieldForTable(LocalizedTable table)
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

            public StringPropertyData()
            {
                assetType = typeof(string);
                previewArgumentsList = new ReorderableList(new List<Object>(), typeof(Object));
                previewArgumentsList.headerHeight = 1;
                previewArgumentsList.drawElementCallback = DrawPreviewElement;
                previewArgumentsList.onChangedCallback = UpdateArguments;
            }

            public void UpdateArguments(ReorderableList _)
            {
                previewArguments.Clear();
                previewArguments.AddRange(previewArgumentsList.list as List<Object>);
                LocaleFields.ForEach(sf => sf.smartEditor.ResetCache());
            }

            void DrawPreviewElement(Rect rect, int index, bool isActive, bool isFocused)
            {
                var list = previewArgumentsList.list as List<Object>;
                EditorGUI.BeginChangeCheck();
                list[index] = EditorGUI.ObjectField(rect, list[index], typeof(Object), true);
                if (EditorGUI.EndChangeCheck())
                    UpdateArguments(previewArgumentsList);
            }
        }

        static LocalizedStringPropertyDrawer()
        {
            GetProjectTableCollections = LocalizationEditorSettings.GetStringTableCollections;
        }

        protected override PropertyData CreatePropertyData(SerializedProperty property)
        {
            var prop = new StringPropertyData()
            {
                serializedObject = property.serializedObject,
                tableReference = new SerializedTableReference(property.FindPropertyRelative("m_TableReference")),
                tableEntryReference = new SerializedTableEntryReference(property.FindPropertyRelative("m_TableEntryReference")),
                entryNameLabel = Styles.entryName,
                previewArguments = Arguments,
            };
            return prop;
        }

        protected override void DrawTableEntryDetails(ref Rect rowPosition, Rect position)
        {
            base.DrawTableEntryDetails(ref rowPosition, position);
            var stringPropertyData = (StringPropertyData)m_Property;

            // We want to clip long labels (case LOC-84)
            if (s_FoldoutStyle == null)
                s_FoldoutStyle = new GUIStyle(EditorStyles.foldoutHeader) { clipping = TextClipping.Clip };

            for (int i = 0; i < stringPropertyData.LocaleFields.Count; ++i)
            {
                var field = stringPropertyData.LocaleFields[i];
                var label = new GUIContent(field.locale.Identifier.ToString());

                // Is this a missing table?
                if (field.smartEditor == null)
                {
                    rowPosition.height = EditorGUIUtility.singleLineHeight;
                    var buttonPosition = EditorGUI.PrefixLabel(rowPosition, label);
                    if (GUI.Button(buttonPosition, "Create Table"))
                    {
                        var table = m_Property.SelectedTableCollection.AddNewTable(field.locale.Identifier);
                        field.smartEditor = stringPropertyData.CreateSmartFormatFieldForTable(table);
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
                field.expanded = EditorGUI.BeginFoldoutHeaderGroup(foldoutPos, field.expanded, label, s_FoldoutStyle);

                // Preview label
                EditorGUI.LabelField(labelPos, field.smartEditor.Label);
                rowPosition.y += rowPosition.height + EditorGUIUtility.standardVerticalSpacing;

                if (field.expanded)
                {
                    rowPosition.height = field.smartEditor.Height;
                    field.smartEditor.Draw(rowPosition);
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

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = base.GetPropertyHeight(property, label);

            if (property.isExpanded && m_Property.SelectedTableEntry != null)
            {
                var stringPropertyData = (StringPropertyData)m_Property;

                foreach (var field in stringPropertyData.LocaleFields)
                {
                    height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // Locale label/foldout
                    if (field.expanded)
                    {
                        if (field.smartEditor != null)
                        {
                            height += field.smartEditor.Height + EditorGUIUtility.standardVerticalSpacing;
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
