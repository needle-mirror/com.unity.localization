using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Localization.UI
{
    class AssetTablesGenerator : VisualElement
    {
        internal new class UxmlFactory : UxmlFactory<AssetTablesGenerator> { }

        class Texts
        {
            public readonly string progressTitle = "Generating Asset Tables";
            public readonly string saveDialog = "Save tables to folder";
        }
        static Texts s_Texts = new Texts();

        const string k_AssetNameFormat = "{0}-{1}-{2}"; // [locale code] [table name] [type]

        TextField m_TableName;
        ScrollView m_LocalesList;
        AssetTableTypeField m_TableType;

        public AssetTablesGenerator()
        {
            var root = Resources.GetTemplate("AssetTablesGenerator");
            Add(root);
            root.StretchToParentSize();

            var locales = LocalizationAddressableSettings.GetLocales();
            m_LocalesList = root.Q<ScrollView>("localesList");
            foreach (var locale in locales)
            {
                m_LocalesList.Add(new Toggle(){ text = locale.name, value = true});
            }

            root.Q<Button>("createTableButton").clickable.clicked += CreateTables;
            root.Q<Button>("selectAllLocales").clickable.clicked += () => SelectAllLocales(true);
            root.Q<Button>("selectNoLocales").clickable.clicked += () => SelectAllLocales(false);
            m_TableType = root.Q<AssetTableTypeField>();
            m_TableName = root.Q<TextField>("newTableName");
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
            var locales = LocalizationAddressableSettings.GetLocales();
            List<Locale> selectedLocales = new List<Locale>();

            for (int i = 0; i < m_LocalesList.contentContainer.childCount; ++i)
            {
                var toggle = m_LocalesList.contentContainer.ElementAt(i) as Toggle;
                if (toggle != null && toggle.value)
                {
                    Debug.Assert(locales[i].name == toggle.text, string.Format("Expected locale to match toggle. Expected {0} but got {1}", locales[i].name, toggle.name));
                    selectedLocales.Add(locales[i]);
                }
            }

            return selectedLocales;
        }

        void CreateTables()
        {
            string path = EditorUtility.SaveFolderPanel(s_Texts.saveDialog, "Assets/", "");
            if (string.IsNullOrEmpty(path))
                return;

            try
            {
                // TODO: Check that no tables already exist with the same name, locale and type. If they do skip them and print a warning.
                // Still add the ones that don't exist though. Perhaps show the warning during editing?

                var selectedLocales = GetSelectedLocales();

                // Create the instances
                EditorUtility.DisplayProgressBar(s_Texts.progressTitle, "Creating Tables", 0);
                var tables = new List<LocalizedTable>(selectedLocales.Count);
                foreach (var locale in selectedLocales)
                {
                    var t = (LocalizedTable)ScriptableObject.CreateInstance(m_TableType.value);
                    t.TableName = m_TableName.value;
                    t.LocaleIdentifier = locale.Identifier;
                    t.name = string.Format(k_AssetNameFormat, locale.Identifier.Code, m_TableName.value, m_TableType.text);
                    tables.Add(t);
                }

                // Save as assets
                EditorUtility.DisplayProgressBar(s_Texts.progressTitle, "Saving Tables", 0);
                AssetDatabase.StartAssetEditing(); // Batch the assets into a single asset operation
                var relativePath = MakePathRelative(path);
                for (int i = 0; i < tables.Count; ++i)
                {
                    var tbl = tables[i];
                    EditorUtility.DisplayProgressBar(s_Texts.progressTitle, "Saving Table " + tbl.name, i / (float)tables.Count);
                    var assetPath = Path.Combine(relativePath, tbl.name + ".asset");
                    assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
                    AssetDatabase.CreateAsset(tbl, assetPath);
                    LocalizationAddressableSettings.AddOrUpdateAssetTable(tbl);
                }
                AssetDatabase.StopAssetEditing();
                AssetTablesWindow.ShowWindow(tables[0]);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        static string MakePathRelative(string path)
        {
            if (path.Contains(Application.dataPath))
            {
                int length = Application.dataPath.Length - "Assets".Length;
                return path.Substring(length, path.Length - length);
            }

            return path;
        }
    }
}