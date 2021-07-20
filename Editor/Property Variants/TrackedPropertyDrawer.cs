#if ENABLE_PROPERTY_VARIANTS || PACKAGE_DOCS_GENERATION

using UnityEngine;
using UnityEngine.Localization.PropertyVariants;
using UnityEngine.Localization.PropertyVariants.TrackedProperties;
using UnityEngine.Localization.Settings;

namespace UnityEditor.Localization.PropertyVariants
{
    [CustomPropertyDrawer(typeof(TrackedProperty<>), true)]
    [CustomPropertyDrawer(typeof(UnityObjectProperty), true)]
    class TrackedPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var path = property.FindPropertyRelative("m_PropertyPath");
            var variants = property.FindPropertyRelative("m_VariantData");

            position.height = EditorGUIUtility.singleLineHeight;
            variants.isExpanded = EditorGUI.Foldout(position, variants.isExpanded, ObjectNames.NicifyVariableName(path.stringValue), true);
            position.MoveToNextLine();

            if (variants.isExpanded)
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < variants.arraySize; ++i)
                {
                    var item = variants.GetArrayElementAtIndex(i);

                    var code = item.FindPropertyRelative("localeIdentifier.m_Code");
                    var locale = LocalizationEditorSettings.GetLocale(code.stringValue);
                    var name = locale != null ? locale.LocaleName : code.stringValue;

                    var value = item.FindPropertyRelative("value");
                    EditorGUI.BeginChangeCheck();
                    var split = position.SplitHorizontalFixedWidthRight(17);
                    EditorGUI.PropertyField(split.left, value, new GUIContent(name));

                    if (EditorGUI.EndChangeCheck())
                    {
                        if (LocalizationSettings.SelectedLocale == null || LocalizationSettings.SelectedLocale != locale)
                            break;

                        if (property.serializedObject.targetObject is GameObjectLocalizer localizer)
                            localizer.ApplyLocaleVariant(locale);
                    }

                    if (GUI.Button(split.right, GUIContent.none, "OL Minus"))
                    {
                        variants.DeleteArrayElementAtIndex(i);
                    }

                    position.MoveToNextLine();
                }
                EditorGUI.indentLevel--;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var variants = property.FindPropertyRelative("m_VariantData");
            if (variants.isExpanded)
                height += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * variants.arraySize;
            return height;
        }
    }
}

#endif
