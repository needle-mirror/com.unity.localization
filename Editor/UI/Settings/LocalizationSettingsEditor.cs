using UnityEditor.Localization.UI.Toolkit;
using UnityEditor.UIElements;
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

            return root;
        }
    }
}
