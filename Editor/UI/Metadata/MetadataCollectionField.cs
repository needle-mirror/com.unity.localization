using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Localization.Metadata;

namespace UnityEditor.Localization.UI
{
    class MetadataCollectionFieldPropertyData : PropertyDrawerExtendedData
    {
        public SerializedProperty m_ItemsProperty;
        public Type m_DeferredAdd;
        public ReorderableListExtended m_List;
    }

    [CustomPropertyDrawer(typeof(MetadataTypeAttribute))]
    class MetadataCollectionField : PropertyDrawerExtended<MetadataCollectionFieldPropertyData>
    {
        MetadataTypeAttribute m_MetadataType;

        public MetadataTypeAttribute Type
        {
            get => m_MetadataType ?? attribute as MetadataTypeAttribute;
            set => m_MetadataType = value;
        }

        public override MetadataCollectionFieldPropertyData CreatePropertyData(SerializedProperty property)
        {
            var data = new MetadataCollectionFieldPropertyData
            {
                m_ItemsProperty = property.FindPropertyRelative("m_Items"),
            };

            data.m_List = new ReorderableListExtended(property.serializedObject, data.m_ItemsProperty);
            data.m_List.onAddDropdownCallback = (rect, _) => AddItem(rect, data);
            data.m_List.headerHeight = 2;
            return data;
        }

        public override void OnGUI(MetadataCollectionFieldPropertyData data, Rect position, SerializedProperty property, GUIContent label)
        {
            // Label
            var rowPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            data.m_ItemsProperty.isExpanded = EditorGUI.Foldout(rowPosition, data.m_ItemsProperty.isExpanded, label, true);
            rowPosition.y += rowPosition.height + EditorGUIUtility.standardVerticalSpacing;

            // Adding a new item is deferred so that Undo will work.
            if (data.m_DeferredAdd != null)
            {
                try
                {
                    var item = data.m_ItemsProperty.AddArrayElement();
                    var instance = Activator.CreateInstance(data.m_DeferredAdd);
                    item.managedReferenceValue = instance;
                }
                finally
                {
                    data.m_DeferredAdd = null;
                }
            }

            if (data.m_ItemsProperty.isExpanded)
            {
                rowPosition.height = position.height - rowPosition.height + EditorGUIUtility.standardVerticalSpacing;
                data.m_List.DoList(rowPosition);
            }
        }

        public override float GetPropertyHeight(MetadataCollectionFieldPropertyData data, SerializedProperty property, GUIContent label)
        {
            // Label
            var height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // List
            if (data.m_ItemsProperty.isExpanded)
                height += data.m_List.GetHeight();

            return height;
        }

        void AddItem(Rect rect, MetadataCollectionFieldPropertyData data)
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

                if ((itemAttribute.AllowedTypes & metadataType.Type) == 0)
                    continue;

                // Check if the item is already added.
                bool enabled = true;
                if (!itemAttribute.AllowMultiple)
                {
                    for (int j = 0; j < data.m_ItemsProperty.arraySize; ++j)
                    {
                        var typeName = data.m_ItemsProperty.GetArrayElementAtIndex(j).managedReferenceFullTypename;
                        if (!string.IsNullOrEmpty(typeName))
                        {
                            var type = ManagedReferenceUtility.GetType(data.m_ItemsProperty.GetArrayElementAtIndex(j).managedReferenceFullTypename);
                            if (type == md)
                            {
                                enabled = false;
                                break;
                            }
                        }
                    }
                }

                var name = itemAttribute.MenuItem;
                if (string.IsNullOrEmpty(name))
                    name = ObjectNames.NicifyVariableName(md.Name);

                var label = new GUIContent(name);
                if (enabled)
                {
                    menu.AddItem(label, false, () =>
                    {
                        data.m_DeferredAdd = md;
                    });
                }
                else
                {
                    menu.AddDisabledItem(label);
                }
            }

            menu.DropDown(rect);
        }
    }
}
