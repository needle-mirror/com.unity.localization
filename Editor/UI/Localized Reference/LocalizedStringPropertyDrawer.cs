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
    class LocalizedStringPropertyDrawer : LocalizedReferencePropertyDrawer<StringTable>
    {
        public bool ShowPreview { get; set; } = true;

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
                            var table = SelectedTableCollection.Tables.FirstOrDefault(tbl => tbl.LocaleIdentifier == locale.Identifier);
                            SmartFormatField smartField = null;
                            if (previewArguments == null)
                                previewArguments = new SmartObjects();

                            if (table != null)
                            {
                                smartField = new SmartFormatField();
                                smartField.Table = (StringTable)table;
                                smartField.KeyId = SelectedTableEntry.Id;
                                smartField.RawText = SelectedTableEntry.Key;
                                smartField.ShowMetadataButton = false;
                                smartField.ShowPreviewTab = true;
                                smartField.MinHeight = EditorGUIUtility.singleLineHeight;
                                smartField.Arguments = previewArguments;
                                smartField.RefreshData();
                            }
                            m_SmartFormatFields.Add((locale, false, smartField));
                        }
                    }
                    return m_SmartFormatFields;
                }
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

            for (int i = 0; i < stringPropertyData.LocaleFields.Count; ++i)
            {
                var field = stringPropertyData.LocaleFields[i];

                // Locale label/foldout
                rowPosition.height = EditorGUIUtility.singleLineHeight;
                var label = new GUIContent(field.locale.Identifier.ToString());
                field.expanded = EditorGUI.BeginFoldoutHeaderGroup(rowPosition, field.expanded, label);

                // Preview label
                if (field.smartEditor != null)
                {
                    var labelWidth = EditorGUIUtility.labelWidth - ((EditorGUI.indentLevel + 1) * 15);
                    var labelPos = new Rect(rowPosition.x + labelWidth, rowPosition.y, rowPosition.width - labelWidth, rowPosition.height);
                    EditorGUI.LabelField(labelPos, field.smartEditor.Label);
                }
                rowPosition.y += rowPosition.height + EditorGUIUtility.standardVerticalSpacing;

                if (field.expanded)
                {
                    if (field.smartEditor != null)
                    {
                        rowPosition.height = field.smartEditor.Height;
                        field.smartEditor.Draw(rowPosition);
                        rowPosition.y += rowPosition.height + EditorGUIUtility.standardVerticalSpacing;
                    }
                    else // Missing table for the locale
                    {
                        rowPosition.height = EditorGUIUtility.singleLineHeight;
                    }
                }
                EditorGUI.EndFoldoutHeaderGroup();
                stringPropertyData.LocaleFields[i] = field;
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
