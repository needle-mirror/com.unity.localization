using System;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat;
using UnityEngine.Localization.SmartFormat.Core.Extensions;

namespace UnityEditor.Localization.UI
{
    class SmartFormatterPropertyFieldPropertyData
    {
        public SerializedProperty settings;
        public SerializedProperty parser;
        public SerializedProperty sources;
        public SerializedProperty formatters;
        public SmartFormatter smartFormatterInstance;
        public ReorderableListExtended sourcesList;
        public ReorderableListExtended formattersList;
    }

    [CustomPropertyDrawer(typeof(SmartFormatter))]
    class SmartFormatterPropertyField : PropertyDrawerExtended<SmartFormatterPropertyFieldPropertyData>
    {
        class Styles
        {
            public static readonly GUIContent sourcesHeader = new GUIContent("Sources", "Used to evaluate a selector. Checked in order of the list, top first.");
            public static readonly GUIContent formattersHeader = new GUIContent("Formatters", "Used to convert an object to a string. Checked in order of the list, top first.");
            public static readonly GUIContent smartFormatHeader = new GUIContent("Smart Format");
        }

        void AddClassMenu(Rect rect, ReorderableList list, System.Type baseType, SmartFormatterPropertyFieldPropertyData data)
        {
            var menu = new GenericMenu();

            var foundTypes = TypeCache.GetTypesDerivedFrom(baseType);
            for (int i = 0; i < foundTypes.Count; ++i)
            {
                var type = foundTypes[i];
                menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(type.Name)), false, () =>
                {
                    // We only support 2 types of constructor. A default and one that takes a SmartFormatter.
                    var hasDefaultConstructor = type.GetConstructors().Any(c => c.GetParameters().Length == 0);
                    var hasSmartFormatterConstructor = type.GetConstructors().Any(c => c.GetParameters().Length == 1 && c.GetParameters()[0].ParameterType == typeof(SmartFormatter));

                    if (hasSmartFormatterConstructor || hasDefaultConstructor)
                    {
                        var elementProp = list.serializedProperty.AddArrayElement();
                        elementProp.managedReferenceValue = hasDefaultConstructor ? Activator.CreateInstance(type) : Activator.CreateInstance(type, data.smartFormatterInstance);
                        list.serializedProperty.serializedObject.ApplyModifiedProperties();
                    }
                    else
                    {
                        Debug.LogWarning($"Can not create an instance of {type}, it does not have a default constructor or a constructor that takes a SmartFormatter parameter.");
                    }
                });
            }
            menu.DropDown(rect);
        }

        public override SmartFormatterPropertyFieldPropertyData CreatePropertyData(SerializedProperty property)
        {
            var data = new SmartFormatterPropertyFieldPropertyData();
            data.settings = property.FindPropertyRelative("m_Settings");
            data.parser = property.FindPropertyRelative("m_Parser");
            data.sources = property.FindPropertyRelative("m_Sources");
            data.formatters = property.FindPropertyRelative("m_Formatters");

            var settings = property.serializedObject.targetObject as LocalizationSettings;
            if (settings != null)
            {
                data.smartFormatterInstance = settings.GetStringDatabase()?.SmartFormatter;
            }
            else
            {
                data.smartFormatterInstance = property.GetActualObjectForSerializedProperty<SmartFormatter>(fieldInfo);
            }
            Debug.Assert(data.smartFormatterInstance != null, $"Failed to extract {nameof(SmartFormatter)} instance.");

            data.sourcesList = new ReorderableListExtended(data.sources.serializedObject, data.sources);
            data.sourcesList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, Styles.sourcesHeader);
            data.sourcesList.onAddDropdownCallback = (rect, list) => AddClassMenu(rect, list, typeof(ISource), data);

            data.formattersList = new ReorderableListExtended(data.formatters.serializedObject, data.formatters);
            data.formattersList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, Styles.formattersHeader);
            data.formattersList.onAddDropdownCallback = (rect, list) => AddClassMenu(rect, list, typeof(IFormatter), data);
            return data;
        }

        public override void OnGUI(SmartFormatterPropertyFieldPropertyData data, Rect position, SerializedProperty property, GUIContent label)
        {
            // Header
            position.height = EditorStyles.boldLabel.lineHeight;
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, Styles.smartFormatHeader);
            position.yMin += position.height + EditorGUIUtility.standardVerticalSpacing;

            if (!property.isExpanded)
                return;

            EditorGUI.indentLevel++;

            // Settings
            position.height = EditorGUI.GetPropertyHeight(data.settings);
            EditorGUI.PropertyField(position, data.settings, true);
            position.yMin += position.height + EditorGUIUtility.standardVerticalSpacing;

            // Parser
            position.height = EditorGUI.GetPropertyHeight(data.parser);
            EditorGUI.PropertyField(position, data.parser, true);
            position.yMin += position.height + EditorGUIUtility.standardVerticalSpacing;

            position.height = data.sourcesList.GetHeight();
            var listPos = EditorGUI.PrefixLabel(position, GUIContent.none);
            data.sourcesList.DoList(listPos);

            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

            position.height = data.formattersList.GetHeight();
            listPos = EditorGUI.PrefixLabel(position, GUIContent.none);
            data.formattersList.DoList(listPos);

            EditorGUI.indentLevel--;
        }

        public override float GetPropertyHeight(SmartFormatterPropertyFieldPropertyData data, SerializedProperty property, GUIContent label)
        {
            float height = EditorStyles.foldout.lineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (property.isExpanded)
            {
                height += EditorGUI.GetPropertyHeight(data.settings) + EditorGUIUtility.standardVerticalSpacing;
                height += EditorGUI.GetPropertyHeight(data.parser) + EditorGUIUtility.standardVerticalSpacing;
                height += data.sourcesList.GetHeight() + EditorGUIUtility.standardVerticalSpacing;
                height += data.formattersList.GetHeight() + EditorGUIUtility.standardVerticalSpacing;
            }
            return height;
        }
    }
}
