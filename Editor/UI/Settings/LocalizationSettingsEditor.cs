using UnityEditor.Localization.UI.Toolkit;
using UnityEditor.UIElements;
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
            var root = new VisualElement();
            root.Add(new PropertyField(serializedObject.FindProperty("m_AvailableLocales")));

            var startupSelectors = new ManagedReferenceReorderableList(serializedObject.FindProperty("m_StartupSelectors"), typeof(IStartupLocaleSelector));
            startupSelectors.HeaderTitle = "Locale Selectors";
            root.Add(startupSelectors);

            root.Add(new PropertyField(serializedObject.FindProperty("m_AssetDatabase")));
            root.Add(new PropertyField(serializedObject.FindProperty("m_StringDatabase")));

            var metadata = new MetadataReorderableList(serializedObject.FindProperty("m_Metadata.m_Items"), new MetadataTypeAttribute(MetadataType.LocalizationSettings));
            metadata.HeaderTitle = "Metadata";
            root.Add(metadata);

            return root;
        }
    }
}
