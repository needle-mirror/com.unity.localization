using System;
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

        readonly TextField m_TableCollectionName;
        readonly ScrollView m_LocalesList;
        readonly Button m_CreateStringTablesButton;
        readonly Button m_CreateAssetTablesButton;

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

            m_CreateStringTablesButton = this.Q<Button>("create-string-tables-button");
            m_CreateStringTablesButton.clickable.clicked += () => CreateCollection(LocalizationEditorSettings.CreateStringTableCollection);

            m_CreateAssetTablesButton = this.Q<Button>("create-asset-tables-button");
            m_CreateAssetTablesButton.clickable.clicked += () => CreateCollection(LocalizationEditorSettings.CreateAssetTableCollection);

            UpdateCreateButtonState();

            this.Q<Button>("select-all-button").clickable.clicked += () => SelectAllLocales(true);
            this.Q<Button>("select-none-button").clickable.clicked += () => SelectAllLocales(false);
            this.Q<Button>("locale-generator-button").clickable.clicked += () => LocaleGeneratorWindow.ShowWindow();

            m_TableCollectionName = this.Q<TextField>("new-table-name-field");

            LocalizationEditorSettings.EditorEvents.LocaleAdded += OnLocaleAdded;
            LocalizationEditorSettings.EditorEvents.LocaleRemoved += OnLocaleRemoved;
        }

        ~TableCreator()
        {
            LocalizationEditorSettings.EditorEvents.LocaleAdded -= OnLocaleAdded;
            LocalizationEditorSettings.EditorEvents.LocaleRemoved -= OnLocaleRemoved;
        }

        void OnLocaleAdded(Locale locale)
        {
            AddLocaleElement(locale);
            UpdateCreateButtonState();
        }

        void OnLocaleRemoved(Locale locale)
        {
            var toggle = m_LocalesList.Q<Toggle>(locale.name);
            if (toggle != null)
            {
                m_LocalesList.Remove(toggle);
                UpdateCreateButtonState();
            }
        }

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
            // If we have no active Locales then the buttons should be disabled.
            foreach (var child in m_LocalesList.Children())
            {
                if (child is Toggle toggle)
                {
                    if (toggle.value)
                    {
                        m_CreateStringTablesButton.SetEnabled(true);
                        m_CreateAssetTablesButton.SetEnabled(true);
                        return;
                    }
                }
            }
            m_CreateStringTablesButton.SetEnabled(false);
            m_CreateAssetTablesButton.SetEnabled(false);
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

        void CreateCollection(Func<string, string, IList<Locale>, LocalizationTableCollection> create)
        {
            var assetDirectory = EditorUtility.SaveFolderPanel("Create Table Collection", "Assets/", "");
            if (string.IsNullOrEmpty(assetDirectory))
                return;
            var createdCollection = create(m_TableCollectionName.value, assetDirectory, GetSelectedLocales());

            // Select the root asset and open the table editor window.
            Selection.activeObject = createdCollection;
            LocalizationTablesWindow.ShowWindow(createdCollection);
        }
    }
}
