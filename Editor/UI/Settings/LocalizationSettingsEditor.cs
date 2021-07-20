using UnityEditor.Localization.UI.Toolkit;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

namespace UnityEditor.Localization.UI
{
    [CustomEditor(typeof(LocalizationSettings))]
    class LocalizationSettingsEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = Resources.GetTemplate(nameof(LocalizationSettings));

            var startupSelectors = new ManagedReferenceReorderableList(serializedObject.FindProperty("m_StartupSelectors"), typeof(IStartupLocaleSelector));
            startupSelectors.HeaderTitle = "Locale Selectors";
            root.Insert(1, startupSelectors);

            var metadata = new MetadataReorderableList(serializedObject.FindProperty("m_Metadata.m_Items"), new MetadataTypeAttribute(MetadataType.LocalizationSettings));
            metadata.HeaderTitle = "Metadata";
            root.Add(metadata);

            return root;
        }
    }
}
