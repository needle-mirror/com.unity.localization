using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Localization.Metadata;

namespace UnityEditor.Localization.UI
{
    [CustomPropertyDrawer(typeof(MetadataTypeAttribute))]
    class MetadataCollectionField : PropertyDrawer
    {
        class PropertyData
        {
            public Type DefferedAdd { get; set; }
            public ReorderableListExtended List { get; set; }
        }

        Dictionary<string, PropertyData> m_PropertyDataPerPropertyPath = new Dictionary<string, PropertyData>();
        PropertyData m_Property;
        MetadataTypeAttribute m_MetadataType;

        public MetadataTypeAttribute Type
        {
            get => m_MetadataType ?? attribute as MetadataTypeAttribute;
            set => m_MetadataType = value;
        }

        public MetadataCollectionField()
        {
        }

        void Init(SerializedProperty property)
        {
            if (m_PropertyDataPerPropertyPath.TryGetValue(property.propertyPath, out m_Property))
                return;

            m_Property = new PropertyData();
            var list = new ReorderableListExtended(property.serializedObject, property);
            list.headerHeight = 2;
            list.onAddDropdownCallback = AddItem;

            m_Property.List = list;
            m_PropertyDataPerPropertyPath.Add(property.propertyPath, m_Property);
        }

        void AddItem(Rect rect, ReorderableList list)
        {
            var menu = new GenericMenu();
            var metadataTypes = TypeCache.GetTypesDerivedFrom<IMetadata>();

            var metadataType = Type;
            Debug.Assert(metadataType != null);
            for (int i = 0; i < metadataTypes.Count; ++i)
            {
                var md = metadataTypes[i];
                if (md.IsAbstract)
                    continue;

                var itemAttribute = md.GetCustomAttribute<MetadataAttribute>();
                if (itemAttribute == null)
                    continue;

                if (!itemAttribute.AllowedTypes.HasFlag(metadataType.Type))
                    continue;

                //bool enabled = true;
                // Check if the item is already added.
                //if (!attribute.AllowMultiple)
                //{
                //    for (int j = 0; j < m_ItemsProperty.arraySize; ++j)
                //    {
                //        var metadata = m_ItemsProperty.GetArrayElementAtIndex(j).objectReferenceValue;
                //        if (metadata.GetType() == md.GetType())
                //            enabled = false;
                //    }
                //}

                var name = itemAttribute.MenuItem;
                if (string.IsNullOrEmpty(name))
                    name = ObjectNames.NicifyVariableName(md.Name);
                var prop = m_Property;
                menu.AddItem(new GUIContent(name), false, () =>
                {
                    prop.DefferedAdd = md;
                });
            }

            menu.DropDown(rect);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var prop = property.FindPropertyRelative("m_Items");
            Init(prop);

            // Label
            var rowPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            prop.isExpanded = EditorGUI.Foldout(rowPosition, prop.isExpanded, label);
            rowPosition.y += rowPosition.height + EditorGUIUtility.standardVerticalSpacing;

            if (m_Property.DefferedAdd != null)
            {
                try
                {
                    prop.InsertArrayElementAtIndex(prop.arraySize);
                    var item = prop.GetArrayElementAtIndex(prop.arraySize - 1);

                    var instance = Activator.CreateInstance(m_Property.DefferedAdd);
                    item.managedReferenceValue = instance;
                }
                finally
                {
                    m_Property.DefferedAdd = null;
                }
            }

            if (prop.isExpanded)
            {
                rowPosition.height = position.height - rowPosition.height + EditorGUIUtility.standardVerticalSpacing;
                m_Property.List.DoList(rowPosition);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var prop = property.FindPropertyRelative("m_Items");
            Init(prop);

            // Label
            var height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // List
            if (prop.isExpanded)
                height += m_Property.List.GetHeight();

            return height;
        }
    }
}