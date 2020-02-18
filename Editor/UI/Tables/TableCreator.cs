using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Pseudo;
using UnityEngine.UIElements;

namespace UnityEditor.Localization.UI
{
    class TableCreator : VisualElement
    {
        internal new class UxmlFactory : UxmlFactory<TableCreator> {}

        readonly TextField m_TableName;
        readonly ScrollView m_LocalesList;
        readonly TableTypePopup m_TableType;
        readonly Button m_CreateButton;

        public TableCreator()
        {
            var asset = Resources.GetTemplateAsset(nameof(TableCreator));
            asset.CloneTree(this);

            var locales = LocalizationEditorSettings.GetLocales();
            m_LocalesList = this.Q<ScrollView>("locales-list");
            foreach (var locale in locales)
            {
                AddLocaleElement(locale);
            }

            m_CreateButton = this.Q<Button>("create-table-button");
            m_CreateButton.clickable.clicked += CreateTables;
            UpdateCreateButtonState();

            this.Q<Button>("select-all-button").clickable.clicked += () => SelectAllLocales(true);
            this.Q<Button>("select-none-button").clickable.clicked += () => SelectAllLocales(false);
            this.Q<Button>("locale-generator-button").clickable.clicked += () => LocaleGeneratorWindow.ShowWindow();

            m_TableType = this.Q<TableTypePopup>();
            m_TableName = this.Q<TextField>("new-table-name-field");

            LocalizationEditorSettings.OnModification += LocalizationEditorSettingsOnOnModification;
        }

        ~TableCreator() => LocalizationEditorSettings.OnModification -= LocalizationEditorSettingsOnOnModification;

        void AddLocaleElement(Locale locale)
        {
            if (locale is PseudoLocale) // Don't include pseudo locales
                return;

            var toggle = new Toggle() { name = locale.name, text = locale.name, value = true };
            toggle.RegisterValueChangedCallback((evt) => UpdateCreateButtonState());
            m_LocalesList.Add(toggle);
        }

        void UpdateCreateButtonState()
        {
            // If we have no active Locales then the button should be disabled.
            foreach (var child in m_LocalesList.Children())
            {
                if (child is Toggle toggle)
                {
                    if (toggle.value)
                    {
                        m_CreateButton.SetEnabled(true);
                        return;
                    }
                }
            }
            m_CreateButton.SetEnabled(false);
        }

        void LocalizationEditorSettingsOnOnModification(LocalizationEditorSettings.ModificationEvent evt, object obj)
        {
            if (evt == LocalizationEditorSettings.ModificationEvent.LocaleAdded)
            {
                var locale = (Locale)obj;
                AddLocaleElement(locale);
                UpdateCreateButtonState();
            }
            else if (evt == LocalizationEditorSettings.ModificationEvent.LocaleRemoved)
            {
                var locale = (Locale)obj;
                var toggle = m_LocalesList.Q<Toggle>(locale.name);
                if (toggle != null)
                {
                    m_LocalesList.Remove(toggle);
                    UpdateCreateButtonState();
                }
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
                if (m_LocalesList.contentContainer.ElementAt(i) is Toggle toggle && toggle.value)
                {
                    Debug.Assert(locales[i].name == toggle.text, $"Expected locale to match toggle. Expected {locales[i].name} but got {toggle.name}");
                    selectedLocales.Add(locales[i]);
                }
            }

            return selectedLocales;
        }

        void CreateTables()
        {
            LocalizationEditorSettings.CreateAssetTablesFolderPanel(GetSelectedLocales(), m_TableName.value, m_TableType.value);
        }
    }
}
