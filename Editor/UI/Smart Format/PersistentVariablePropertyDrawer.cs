using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.UIElements;

namespace UnityEditor.Localization.UI
{
    [CustomPropertyDrawer(typeof(Variable<>), true)]
    class PersistentVariablePropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var value = property.FindPropertyRelative("m_Value");
            var label = property.propertyType == SerializedPropertyType.ManagedReference ? ManagedReferenceUtility.GetDisplayName(property.managedReferenceFullTypename).text : property.displayName;
            return new PropertyField(value, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.ManagedReference)
                label = ManagedReferenceUtility.GetDisplayName(property.managedReferenceFullTypename);
            EditorGUI.PropertyField(position, property.FindPropertyRelative("m_Value"), label, true);
        }
    }
}
