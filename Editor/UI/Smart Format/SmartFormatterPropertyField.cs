using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat;
using UnityEngine.Localization.SmartFormat.Core.Extensions;

namespace UnityEditor.Localization.UI
{
    [CustomPropertyDrawer(typeof(SmartFormatter))]
    class SmartFormatterPropertyField : PropertyDrawer
    {
        class Styles
        {
            public static readonly GUIContent sourcesHeader = new GUIContent("Sources", "Used to evaluate a selector. Checked in order of the list, top first.");
            public static readonly GUIContent formattersHeader = new GUIContent("Formatters", "Used to convert an object to a string. Checked in order of the list, top first.");
            public static readonly GUIContent smartFormatHeader = new GUIContent("Smart Format");
        }

        class PropertyData
        {
            public SerializedProperty settings;
            public SerializedProperty parser;
            public SerializedProperty sources;
            public SerializedProperty formatters;
            public SmartFormatter smartFormatterInstance;
            public ReorderableListExtended sourcesList;
            public ReorderableListExtended formattersList;
        }

        // Its possible that the PropertyDrawer may be used to draw more than one item (arrays, lists)
        Dictionary<string, PropertyData> m_PropertyDataPerPropertyPath = new Dictionary<string, PropertyData>();
        PropertyData m_Property;

        void Init(SerializedProperty property)
        {
            if (m_PropertyDataPerPropertyPath.TryGetValue(property.propertyPath, out m_Property))
                return;

            m_Property = new PropertyData();
            m_Property.settings = property.FindPropertyRelative("m_Settings");
            m_Property.parser = property.FindPropertyRelative("m_Parser");
            m_Property.sources = property.FindPropertyRelative("m_Sources");
            m_Property.formatters = property.FindPropertyRelative("m_Formatters");

            var settings = property.serializedObject.targetObject as LocalizationSettings;
            if (settings != null)
            {
                m_Property.smartFormatterInstance = settings.GetStringDatabase()?.SmartFormatter;
            }
            else
            {
                m_Property.smartFormatterInstance = property.GetActualObjectForSerializedProperty<SmartFormatter>(fieldInfo);
            }
            Debug.Assert(m_Property.smartFormatterInstance != null, $"Failed to extract {nameof(SmartFormatter)} instance.");

            m_Property.sourcesList = new ReorderableListExtended(m_Property.sources.serializedObject, m_Property.sources);
            m_Property.sourcesList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, Styles.sourcesHeader);
            m_Property.sourcesList.onAddDropdownCallback = (rect, list) => AddClassMenu(rect, list, typeof(ISource));

            m_Property.formattersList = new ReorderableListExtended(m_Property.formatters.serializedObject, m_Property.formatters);
            m_Property.formattersList.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, Styles.formattersHeader);
            m_Property.formattersList.onAddDropdownCallback = (rect, list) => AddClassMenu(rect, list, typeof(IFormatter));
            m_PropertyDataPerPropertyPath[property.propertyPath] = m_Property;
        }

        void AddClassMenu(Rect rect, ReorderableList list, System.Type baseType)
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
                        elementProp.managedReferenceValue = hasDefaultConstructor ? Activator.CreateInstance(type) : Activator.CreateInstance(type, m_Property.smartFormatterInstance);
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

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Init(property);

            // Header
            position.height = EditorStyles.boldLabel.lineHeight;
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, Styles.smartFormatHeader);
            position.yMin += position.height + EditorGUIUtility.standardVerticalSpacing;

            if (!property.isExpanded)
                return;

            EditorGUI.indentLevel++;

            // Settings
            position.height = EditorGUI.GetPropertyHeight(m_Property.settings);
            EditorGUI.PropertyField(position, m_Property.settings, true);
            position.yMin += position.height + EditorGUIUtility.standardVerticalSpacing;

            // Parser
            position.height = EditorGUI.GetPropertyHeight(m_Property.parser);
            EditorGUI.PropertyField(position, m_Property.parser, true);
            position.yMin += position.height + EditorGUIUtility.standardVerticalSpacing;

            position.height = m_Property.sourcesList.GetHeight();
            var listPos = EditorGUI.PrefixLabel(position, GUIContent.none);
            m_Property.sourcesList.DoList(listPos);

            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

            position.height = m_Property.formattersList.GetHeight();
            listPos = EditorGUI.PrefixLabel(position, GUIContent.none);
            m_Property.formattersList.DoList(listPos);

            EditorGUI.indentLevel--;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Init(property);
            float height = EditorStyles.foldout.lineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (property.isExpanded)
            {
                height += EditorGUI.GetPropertyHeight(m_Property.settings) + EditorGUIUtility.standardVerticalSpacing;
                height += EditorGUI.GetPropertyHeight(m_Property.parser) + EditorGUIUtility.standardVerticalSpacing;
                height += m_Property.sourcesList.GetHeight() + EditorGUIUtility.standardVerticalSpacing;
                height += m_Property.formattersList.GetHeight() + EditorGUIUtility.standardVerticalSpacing;
            }
            return height;
        }
    }
}
