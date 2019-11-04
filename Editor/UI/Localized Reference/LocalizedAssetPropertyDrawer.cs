using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using Object = UnityEngine.Object;

namespace UnityEditor.Localization.UI
{
    [CustomPropertyDrawer(typeof(LocalizedAsset<>), true)]
    class LocalizedAssetPropertyDrawer : LocalizedReferencePropertyDrawer<AssetTable>
    {
        protected override PropertyData CreatePropertyData(SerializedProperty property)
        {
            var prop = base.CreatePropertyData(property);

            // Find the asset type.
            Type baseType = fieldInfo.FieldType;
            Type assetType = null;
            while(baseType != null)
            {
                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(LocalizedAsset<>))
                {
                    assetType = baseType.GetGenericArguments()[0];
                    break;
                }
                baseType = baseType.BaseType;
            }
            Debug.Assert(assetType != null, "Could not determine the asset type for " + fieldInfo.FieldType.Name);
            prop.assetType = assetType;
            return prop;
        }

        protected override void DrawTableEntryDetails(ref Rect rowPosition, Rect position)
        {
            base.DrawTableEntryDetails(ref rowPosition, position);

            rowPosition.xMin += EditorGUI.indentLevel * 15;

            var labelWidth = EditorGUIUtility.labelWidth - ((EditorGUI.indentLevel + 1) * 15);
            var labelRect = new Rect(rowPosition.x, rowPosition.y, labelWidth, EditorGUIUtility.singleLineHeight);
            var fieldRect = new Rect(labelRect.xMax, rowPosition.y, rowPosition.width - labelRect.width, labelRect.height);

            var projectLocales = LocalizationEditorSettings.GetLocales();

            foreach (var locale in projectLocales)
            {
                var table = m_Property.SelectedTableCollection.Tables.FirstOrDefault(tbl => tbl.LocaleIdentifier == locale.Identifier) as AssetTable;

                if (table != null)
                {
                    if (GUI.Button(labelRect, locale.Identifier.ToString(), EditorStyles.label))
                    {
                        EditorGUIUtility.PingObject(table);
                    }

                    var tableEntry = table.GetEntry(m_Property.SelectedTableEntry.Id);
                    Object asset = null;
                    if (tableEntry != null && !tableEntry.IsEmpty)
                    {
                        asset = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(tableEntry.Guid));
                    }

                    EditorGUI.BeginChangeCheck();
                    var newAsset = EditorGUI.ObjectField(fieldRect, asset, m_Property.assetType, false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (newAsset != null)
                            LocalizationEditorSettings.AddAssetToTable(table, m_Property.SelectedTableEntry.Id, newAsset, true);
                        else
                            LocalizationEditorSettings.RemoveAssetFromTable(table, m_Property.SelectedTableEntry.Id, asset, true);
                    }
                }
                else
                {
                    EditorGUI.LabelField(labelRect, locale.Identifier.ToString());
                    if (GUI.Button(fieldRect, "Add Table"))
                    {
                        // Get the path of one of the tables so we can start the file picker in the directory.
                        var tableToCopyPath = m_Property.SelectedTableCollection.TableEntries[0].AssetPath;
                        string tableToCopyDir = Path.GetDirectoryName(tableToCopyPath);
                        var tc = m_Property.SelectedTableCollection;
                        LocalizationEditorSettings.CreateAssetTableFilePanel(locale, tc.Keys, tc.TableName, tc.TableType, tableToCopyDir);
                    }
                }

                labelRect.y += labelRect.height + EditorGUIUtility.standardVerticalSpacing;
                fieldRect.y += fieldRect.height + EditorGUIUtility.standardVerticalSpacing;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = base.GetPropertyHeight(property, label);

            if (property.isExpanded && m_Property.SelectedTableEntry != null)
            {
                height += LocalizationEditorSettings.GetLocales().Count * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            }
            return height;
        }
    }
}
