using UnityEditor.UIElements;
using UnityEngine.Localization.SmartFormat.GlobalVariables;
using UnityEngine.UIElements;

namespace UnityEditor.Localization.UI
{
    [CustomPropertyDrawer(typeof(GlobalVariable<>), true)]
    class GlobalVariablePropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var value = property.FindPropertyRelative("m_Value");

            var root = new VisualElement { style = { flexDirection = FlexDirection.Row } };
            root.Add(new Label(ManagedReferenceUtility.GetDisplayName(property.managedReferenceFullTypename).text) { style = { width = 50, alignSelf = Align.Center } });

            var valueField = new PropertyField(value){ style = { flexGrow = 1 } };

            // Workaround to handle the fact that PropertyField will always have a label.
            valueField.AddToClassList("unity-property-field--no-label");
            root.Add(valueField);
            return root;
        }
    }
}
