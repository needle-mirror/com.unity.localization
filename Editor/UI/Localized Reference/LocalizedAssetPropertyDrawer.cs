using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using Object = UnityEngine.Object;

namespace UnityEditor.Localization.UI
{
    [CustomPropertyDrawer(typeof(LocalizedAsset<>), true)]
    class LocalizedAssetPropertyDrawer : LocalizedReferencePropertyDrawer<AssetTableCollection>
    {
        static LocalizedAssetPropertyDrawer()
        {
            GetProjectTableCollections = LocalizationEditorSettings.GetAssetTableCollections;
        }

        protected override PropertyData CreatePropertyData(SerializedProperty property)
        {
            var prop = base.CreatePropertyData(property);

            // Find the asset type.
            Type baseType = fieldInfo.FieldType;
            Type assetType = null;
            while (baseType != null)
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

            var projectLocales = LocalizationEditorSettings.GetLocales();

            foreach (var locale in projectLocales)
            {
                var table = m_Property.SelectedTableCollection.Tables.FirstOrDefault(tbl => tbl.asset?.LocaleIdentifier == locale.Identifier).asset as AssetTable;

                if (table != null)
                {
                    var tableEntry = table.GetEntry(m_Property.SelectedTableEntry.Id);
                    Object asset = null;
                    if (tableEntry != null && !tableEntry.IsEmpty)
                    {
                        asset = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(tableEntry.Guid));
                    }

                    EditorGUI.BeginChangeCheck();
                    var newAsset = EditorGUI.ObjectField(rowPosition, locale.Identifier.ToString(), asset, m_Property.assetType, false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (newAsset != null)
                            m_Property.SelectedTableCollection.AddAssetToTable(table, m_Property.SelectedTableEntry.Id, newAsset, true);
                        else
                            m_Property.SelectedTableCollection.RemoveAssetFromTable(table, m_Property.SelectedTableEntry.Id, true);
                    }
                }
                else
                {
                    var buttonPosition = EditorGUI.PrefixLabel(rowPosition, new GUIContent(locale.Identifier.ToString()));
                    if (GUI.Button(buttonPosition, "Create Table"))
                    {
                        m_Property.SelectedTableCollection.AddNewTable(locale.Identifier);
                        GUIUtility.ExitGUI();
                    }
                }

                rowPosition.y += rowPosition.height + EditorGUIUtility.standardVerticalSpacing;
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
