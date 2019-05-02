using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#else
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#endif

namespace UnityEditor.Localization.UI
{
    class AssetTablesGenerator : VisualElement
    {
        internal new class UxmlFactory : UxmlFactory<AssetTablesGenerator> { }

        TextField m_TableName;
        ObjectField m_KeyDatabase;
        ScrollView m_LocalesList;
        AssetTableTypeField m_TableType;

        public AssetTablesGenerator()
        {
            var root = Resources.GetTemplate("AssetTablesGenerator");
            Add(root);
            root.StretchToParentSize();

            var locales = LocalizationEditorSettings.GetLocales();
            m_LocalesList = root.Q<ScrollView>("localesList");
            foreach (var locale in locales)
            {
                m_LocalesList.Add(new Toggle(){ name = locale.name,  text = locale.name, value = true});
            }

            root.Q<Button>("createTableButton").clickable.clicked += CreateTables;
            root.Q<Button>("selectAllLocales").clickable.clicked += () => SelectAllLocales(true);
            root.Q<Button>("selectNoLocales").clickable.clicked += () => SelectAllLocales(false);
            m_TableType = root.Q<AssetTableTypeField>();
            m_TableName = root.Q<TextField>("newTableName");
            m_KeyDatabase = root.Q<ObjectField>("newTableDatabase");
            m_KeyDatabase.objectType = typeof(KeyDatabase);

            LocalizationEditorSettings.OnModification += LocalizationEditorSettingsOnOnModification;
        }

        ~AssetTablesGenerator() => LocalizationEditorSettings.OnModification -= LocalizationEditorSettingsOnOnModification;

        private void LocalizationEditorSettingsOnOnModification(LocalizationEditorSettings.ModificationEvent evt, object obj)
        {
            if (evt == LocalizationEditorSettings.ModificationEvent.LocaleAdded)
            {
                var locale = (Locale)obj;
                m_LocalesList.Add(new Toggle(){ name = locale.name, text = locale.name, value = true});
            }
            else if (evt == LocalizationEditorSettings.ModificationEvent.LocaleRemoved)
            {
                var locale = (Locale)obj;
                var toggle = m_LocalesList.Q<Toggle>(locale.name);
                if (toggle != null)
                    m_LocalesList.Remove(toggle);
            }
        }

        void SelectAllLocales(bool selected)
        {
            for (int i = 0; i < m_LocalesList.contentContainer.childCount; ++i)
            {
                var toggle = m_LocalesList.contentContainer.ElementAt(i) as Toggle;
                toggle.value = selected;
            }
        }

        List<Locale> GetSelectedLocales()
        {
            var locales = LocalizationEditorSettings.GetLocales();
            var selectedLocales = new List<Locale>();

            for (int i = 0; i < m_LocalesList.contentContainer.childCount; ++i)
            {
                var toggle = m_LocalesList.contentContainer.ElementAt(i) as Toggle;
                if (toggle != null && toggle.value)
                {
                    Debug.Assert(locales[i].name == toggle.text, $"Expected locale to match toggle. Expected {locales[i].name} but got {toggle.name}");
                    selectedLocales.Add(locales[i]);
                }
            }

            return selectedLocales;
        }

        void CreateTables()
        {
            LocalizationEditorSettings.CreateAssetTablesFolderPanel(GetSelectedLocales(), (KeyDatabase)m_KeyDatabase.value, m_TableName.value, m_TableType.value);
        }
    }
}
