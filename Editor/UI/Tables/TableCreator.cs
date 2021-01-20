using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Pseudo;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace UnityEditor.Localization.UI
{
    class TableCreator : VisualElement
    {
        internal new class UxmlFactory : UxmlFactory<TableCreator> {}

        class LocaleLabel : Label
        {
            public Locale boundLocale;
        }

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
            for (int i = 0; i < m_LocalesList.childCount; ++i)
            {
                var localeLabel = m_LocalesList[i].Q<Label>();
                if (localeLabel != null && localeLabel.text == locale.name)
                {
                    m_LocalesList.Remove(m_LocalesList[i]);
                    UpdateCreateButtonState();
                }
            }
        }

        void AddLocaleElement(Locale locale)
        {
            if (locale is PseudoLocale) // Don't include pseudo locales
                return;

            var so = new SerializedObject(locale);

            var visualElement = new VisualElement() { style = { flexDirection = FlexDirection.Row, alignItems = Align.Center }, };
            var toggle = new Toggle() { value = true };
            toggle.RegisterValueChangedCallback((evt) => UpdateCreateButtonState());
            var label = new LocaleLabel() { boundLocale = locale, bindingPath = "m_Name" };
            visualElement.Add(toggle);
            visualElement.Add(label);
            visualElement.Bind(so);

            m_LocalesList.Add(visualElement);
        }

        void UpdateCreateButtonState()
        {
            // If we have no active Locales then the buttons should be disabled.
            foreach (var localeItem in m_LocalesList.Children())
            {
                var toggle = localeItem.Q<Toggle>();
                if (toggle.value)
                {
                    m_CreateStringTablesButton.SetEnabled(true);
                    m_CreateAssetTablesButton.SetEnabled(true);
                    return;
                }
            }
            m_CreateStringTablesButton.SetEnabled(false);
            m_CreateAssetTablesButton.SetEnabled(false);
        }

        void SelectAllLocales(bool selected)
        {
            foreach (var localeItem in m_LocalesList.Children())
            {
                var toggle = localeItem.Q<Toggle>();
                toggle.value = selected;
            }
        }

        List<Locale> GetSelectedLocales()
        {
            var selectedLocales = new List<Locale>();

            foreach (var localeItem in m_LocalesList.Children())
            {
                var toggle = localeItem.Q<Toggle>();
                var label = localeItem.Q<LocaleLabel>();

                if (toggle.value)
                {
                    if (label.boundLocale != null)
                        selectedLocales.Add(label.boundLocale);
                    else
                        Debug.LogError($"Expected locale to match toggle. Expected {label.boundLocale.name} but got {label.text}");
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
