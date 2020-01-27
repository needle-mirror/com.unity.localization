using System;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor.Localization.UI
{
    class ReorderableListExtended : ReorderableList
    {
        Type m_AddType;

        public GUIContent Header
        {
            set
            {
                drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, value);
            }
        }

        public Type AddMenuType
        {
            set
            {
                m_AddType = value;
                onAddDropdownCallback = ShowAddMenu;
            }
        }

        public ReorderableListExtended(SerializedObject serializedObject, SerializedProperty elements) :
            base(serializedObject, elements)
        {
            drawElementCallback = DrawElement;
            elementHeightCallback = GetElementHeight;
            onCanRemoveCallback = CanRemoveElement;
        }

        void DrawElement(Rect rect, int idx, bool isActive, bool isFocused)
        {
            var element = serializedProperty.GetArrayElementAtIndex(idx);

            bool disabled = IsReadOnly(element);

            EditorGUI.BeginDisabledGroup(disabled);

            // Get the class name only
            var name = element.managedReferenceFullTypename;
            var namespaceIndex = name.LastIndexOf('.');

            if (namespaceIndex == -1)
            {
                // No namespace, so just remove the assembly name instead
                namespaceIndex = name.LastIndexOf(' ');
            }

            if (namespaceIndex > 0)
            {
                name = name.Substring(namespaceIndex + 1, name.Length - namespaceIndex - 1);
            }

            name = ObjectNames.NicifyVariableName(name);

            var label = new GUIContent(name);
            rect.xMin += 8; // Prevent the foldout arrow(>) being drawn over the reorder icon(=)
            EditorGUI.PropertyField(rect, element, label, true);
            EditorGUI.EndDisabledGroup();
        }

        bool CanRemoveElement(ReorderableList list)
        {
            var prop = serializedProperty.GetArrayElementAtIndex(list.index);
            return !IsReadOnly(prop);
        }

        float GetElementHeight(int idx)
        {
            var element = serializedProperty.GetArrayElementAtIndex(idx);
            return EditorGUI.GetPropertyHeight(element, true);
        }

        bool IsReadOnly(SerializedProperty prop)
        {
            bool isReadOnly = false;
            if (!string.IsNullOrEmpty(prop.managedReferenceFullTypename))
            {
                var typeNames = prop.managedReferenceFullTypename.Split(' ');
                if (typeNames?.Length == 2)
                {
                    var typ = Type.GetType($"{typeNames[1]}, {typeNames[0]}");
                    isReadOnly = Attribute.IsDefined(typ, typeof(HideInInspector));
                }
            }
            return isReadOnly;
        }

        void ShowAddMenu(Rect rect, ReorderableList lst)
        {
            var menu = new GenericMenu();

            var foundTypes = TypeCache.GetTypesDerivedFrom(m_AddType);
            for (int i = 0; i < foundTypes.Count; ++i)
            {
                var type = foundTypes[i];

                if (type.IsAbstract)
                    continue;

                menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(type.Name)), false, () =>
                {
                    var element = serializedProperty.AddArrayElement();
                    element.managedReferenceValue = Activator.CreateInstance(type);
                    serializedProperty.serializedObject.ApplyModifiedProperties();
                });
            }
            menu.ShowAsContext();
        }
    }
}
