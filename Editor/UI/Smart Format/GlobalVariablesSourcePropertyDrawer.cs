using UnityEditor.Localization.UI.Toolkit;
using UnityEditor.UIElements;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat.Extensions;
using UnityEngine.Localization.SmartFormat.GlobalVariables;
using UnityEngine.UIElements;

namespace UnityEditor.Localization.UI
{
    [CustomPropertyDrawer(typeof(GlobalVariablesSource), true)]
    class GlobalVariablesSourcePropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var list = new ReorderableList(property.FindPropertyRelative("m_Groups"));
            list.HeaderTitle = "Global Variables Source";
            list.IsNestedList = true;
            list.CreateItemCallback = CreateListItem;
            list.AddCallback = AddItem;
            list.RemoveCallback = RemoveItem;
            list.ReorderCallback = SwapItems;

            return list;
        }

        static void CreateListItem(ReorderableList list, int index, VisualElement root)
        {
            root.style.flexDirection = FlexDirection.Row;

            var element = list.ListProperty.GetArrayElementAtIndex(index);
            var nameField = new TextField { multiline = false, style = { width = 150, alignItems = Align.FlexStart } };
            nameField.RegisterValueChangedCallback(evt =>
            {
                // Variable must not contain any spaces or the smart format parser will not be able to correctly parse them as selectors
                nameField.SetValueWithoutNotify(evt.newValue.ReplaceWhiteSpaces("-"));
            });
            nameField.BindProperty(element.FindPropertyRelative("name"));
            root.Add(nameField);

            var group = new ObjectField { style = { flexGrow = 1 } };
            group.objectType = typeof(GlobalVariablesGroup);
            group.BindProperty(element.FindPropertyRelative("group"));
            root.Add(group);
        }

        static void AddItem(ReorderableList list, int index)
        {
            var element = list.ListProperty.InsertArrayElement(index);

            var name = element.FindPropertyRelative("name");
            name.stringValue = list.ListProperty.arraySize > 1 ? $"global-{list.ListProperty.arraySize - 1}" : "global";

            element.serializedObject.ApplyModifiedProperties();
            list.RefreshList();
        }

        static void RemoveItem(ReorderableList list, int index)
        {
            list.ListProperty.DeleteArrayElementAtIndex(list.Selected);
            list.ListProperty.serializedObject.ApplyModifiedProperties();

            // The bindings will have changed
            list.RefreshList();
        }

        static void SwapItems(ReorderableList list, int from, int to)
        {
            list.ListProperty.MoveArrayElement(from, to);
            list.ListProperty.serializedObject.ApplyModifiedProperties();
        }
    }
}
