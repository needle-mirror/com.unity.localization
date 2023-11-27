#if MODULE_UITK && ENABLE_UITK_DATA_BINDING

using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEditor.Localization.UI;

[CustomPropertyDrawer(typeof(Variable<>.UxmlSerializedData), true)]
class PersistentVariableUxmlSerializedDataPropertyDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var value = property.FindPropertyRelative(nameof(Variable<int>.ValueUXML));
        return new PropertyField(value, ManagedReferenceUtility.GetDisplayName(property.managedReferenceFullTypename).text);
    }
}

#endif
