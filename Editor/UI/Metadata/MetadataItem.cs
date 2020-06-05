using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace UnityEditor.Localization.UI
{
    class MetadataItem : Foldout
    {
        internal new class UxmlFactory : UxmlFactory<MetadataItem> {}

        Object m_Asset;

        public Button RemoveItemsButton => this.Q<Button>("remove-item-button");

        Object Asset
        {
            get => m_Asset;
            set
            {
                m_Asset = value;
                contentContainer.Clear();

                if (m_Asset == null)
                {
                    var objectField = new ObjectField("Reference");
                    objectField.objectType = typeof(Object);
                    objectField.RegisterValueChangedCallback(o => Asset = o.newValue);
                    contentContainer.Add(objectField);
                }
                else
                {
                    // InspectorElement has a bug where it uses the whole window size to calculate the label width.
                    var editor = UnityEditor.Editor.CreateEditor(m_Asset);
                    var imgui = new IMGUIContainer(() =>
                    {
                        EditorGUIUtility.labelWidth = 120;
                        editor.OnInspectorGUI();
                    });
                    contentContainer.Add(imgui);
                }
            }
        }

        public MetadataItem()
        {
            var toggle = this.Q<Toggle>();
            var button = new Button(){text = "-", name = "remove-item-button", style = { width = 15}};
            button.RemoveFromClassList("unity-text-element"); // Centers the button text
            toggle.Add(button);
        }

        public MetadataItem(SerializedProperty arrayProperty) : this()
        {
            value = arrayProperty.isExpanded;
            Asset = arrayProperty.objectReferenceValue;
            text = Asset == null ? "No data" : Asset.name;
        }
    }
}
