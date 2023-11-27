#if MODULE_UITK && ENABLE_UITK_DATA_BINDING

using UnityEngine.Localization;
using UnityEngine.UIElements;

namespace UnityEditor.Localization.UI
{
    [CustomPropertyDrawer(typeof(LocalizedString.UxmlSerializedData), true)]
    class LocalizedStringUxmlSerializedDataPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) => new LocalizedStringField("Localized String", property);
    }
}

#endif
