using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Pseudo;
using UnityEngine.UIElements;

namespace UnityEditor.Localization.UI
{
    class TableCreator : VisualElement
    {
        internal new class UxmlFactory : UxmlFactory<TableCreator> {}

        class LocaleLabel : Label
        {
            public Locale boundLocale;
        }

        readonly VisualElement m_CollectionTypeContainer;
        readonly TextField m_TableCollectionName;
        readonly ScrollView m_LocalesList;
        readonly Button m_CreateTableCollectionButton;
        readonly PopupField<Type> m_CollectionTypePopup;
        readonly VisualElement m_LocaleHelpBox = HelpBoxFactory.CreateDefaultHelpBox("Must have at least one Locale selected.");
        readonly VisualElement m_LocaleHelpBoxContainer;
        readonly VisualElement m_TableNameHelpBoxContainer;
        VisualElement m_TableNameHelpBox;

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
            m_LocaleHelpBoxContainer = this.Q("locale-help-box-container");

            var items = new List<Type> {typeof(StringTableCollection), typeof(AssetTableCollection)};
            m_CollectionTypeContainer = this.Q<VisualElement>("table-collection-type-container");
            m_CollectionTypePopup = new PopupField<Type>("Type", items, 0)
            {
                formatListItemCallback = type => ObjectNames.NicifyVariableName(type.Name),
                formatSelectedValueCallback = type => ObjectNames.NicifyVariableName(type.Name)
            };
            m_CollectionTypePopup.RegisterValueChangedCallback(it => UpdateCreateButtonState());
            m_CollectionTypeContainer.Add(m_CollectionTypePopup);

            m_CreateTableCollectionButton = this.Q<Button>("create-table-collection-button");
            m_CreateTableCollectionButton.clickable.clicked += CreateCollection;

            this.Q<Button>("select-all-button").clickable.clicked += () => SelectAllLocales(true);
            this.Q<Button>("select-none-button").clickable.clicked += () => SelectAllLocales(false);
            this.Q<Button>("locale-generator-button").clickable.clicked += () => LocaleGeneratorWindow.ShowWindow();

            m_TableCollectionName = this.Q<TextField>("new-table-name-field");
            m_TableCollectionName.RegisterValueChangedCallback(it => UpdateCreateButtonState());
            InitializeTableName();

            m_TableNameHelpBoxContainer = this.Q("table-name-help-box-container");

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
            var label = new LocaleLabel() { boundLocale = locale, bindingPath = "m_LocaleName" };
            visualElement.Add(toggle);
            visualElement.Add(label);
            visualElement.Bind(so);

            m_LocalesList.Add(visualElement);
        }

        void UpdateCreateButtonState()
        {
            RemoveHelpBoxes();

            var atLeastOneActiveLocale = m_LocalesList.Children().Any(it => it.Q<Toggle>().value);
            var tableNameError = LocalizationEditorSettings.Instance.IsTableNameValid(m_CollectionTypePopup.value, m_TableCollectionName?.value);

            var createEnabled = atLeastOneActiveLocale && tableNameError == null;
            if (!createEnabled)
            {
                if (!atLeastOneActiveLocale)
                {
                    m_LocaleHelpBoxContainer.Add(m_LocaleHelpBox);
                }
                if (tableNameError != null)
                {
                    m_TableNameHelpBox = HelpBoxFactory.CreateDefaultHelpBox(tableNameError);
                    m_TableNameHelpBoxContainer.Add(m_TableNameHelpBox);
                }
            }
            m_CreateTableCollectionButton.SetEnabled(createEnabled);
        }

        void RemoveHelpBoxes()
        {
            m_TableNameHelpBox?.RemoveFromHierarchy();
            m_LocaleHelpBox.RemoveFromHierarchy();
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

        void CreateCollection()
        {
            var assetDirectory = EditorUtility.SaveFolderPanel("Create Table Collection", "Assets/", "");
            if (string.IsNullOrEmpty(assetDirectory))
                return;

            LocalizationTableCollection createdCollection = null;
            if (m_CollectionTypePopup.value == typeof(StringTableCollection))
            {
                createdCollection = LocalizationEditorSettings.CreateStringTableCollection(m_TableCollectionName.value, assetDirectory, GetSelectedLocales());
            }
            if (m_CollectionTypePopup.value == typeof(AssetTableCollection))
            {
                createdCollection = LocalizationEditorSettings.CreateAssetTableCollection(m_TableCollectionName.value, assetDirectory, GetSelectedLocales());
            }

            // Select the root asset and open the table editor window.
            Selection.activeObject = createdCollection;
            LocalizationTablesWindow.ShowWindow(createdCollection);
            InitializeTableName();
        }

        void InitializeTableName()
        {
            m_TableCollectionName.value = LocalizationEditorSettings.Instance.GetUniqueTableCollectionName(
                m_CollectionTypePopup.value,
                "New Table");
            UpdateCreateButtonState();
        }
    }
}
