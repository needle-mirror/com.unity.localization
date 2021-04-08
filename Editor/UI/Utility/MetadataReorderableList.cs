using System.Reflection;
using UnityEngine;
using UnityEngine.Localization.Metadata;

namespace UnityEditor.Localization.UI.Toolkit
{
    internal class MetadataReorderableList : ManagedReferenceReorderableList
    {
        MetadataTypeAttribute m_MetadataType;

        public MetadataReorderableList(SerializedProperty property, MetadataTypeAttribute metadataType) :
            base(property, typeof(IMetadata))
        {
            m_MetadataType = metadataType;
            AddCallback = ShowAddItemMenu;
        }

        void ShowAddItemMenu(ReorderableList list, int index)
        {
            var menu = new GenericMenu();
            var metadataTypes = TypeCache.GetTypesDerivedFrom<IMetadata>();

            for (int i = 0; i < metadataTypes.Count; ++i)
            {
                var md = metadataTypes[i];
                if (md.IsAbstract)
                    continue;

                var itemAttribute = md.GetCustomAttribute<MetadataAttribute>();
                if (itemAttribute == null)
                    continue;

                if ((itemAttribute.AllowedTypes & m_MetadataType.Type) == 0)
                    continue;

                bool enabled = true;
                if (!itemAttribute.AllowMultiple)
                {
                    for (int j = 0; j < list.ListProperty.arraySize; ++j)
                    {
                        var typeName = list.ListProperty.GetArrayElementAtIndex(j).managedReferenceFullTypename;
                        if (!string.IsNullOrEmpty(typeName))
                        {
                            var type = ManagedReferenceUtility.GetType(list.ListProperty.GetArrayElementAtIndex(j).managedReferenceFullTypename);
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
                        AddManagedItem(list, md, index);
                    });
                }
                else
                {
                    menu.AddDisabledItem(label);
                }
            }

            menu.ShowAsContext();
        }
    }
}
