using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.Localization;
using Object = UnityEngine.Object;

namespace UnityEditor.Localization.Tests
{
    /// <summary>
    /// Overrides the Localization Editor Settings so tests can be run that wont interfere with an existing project.
    /// Ignores all Addressable operations.
    /// </summary>
    public class FakedLocalizationEditorSettings : LocalizationEditorSettings
    {
        public List<LocalizedTable> CreatedTables { get; set; }
        public List<KeyDatabase> CreatedKeyDatabases { get; set; }
        public List<LocalizedTable> AddOrUpdateTables { get; set; }

        public FakedLocalizationEditorSettings()
        {
            CreatedTables = new List<LocalizedTable>();
            CreatedKeyDatabases = new List<KeyDatabase>();
            AddOrUpdateTables = new List<LocalizedTable>();
        }

        protected override AddressableAssetSettings GetAddressableAssetSettings(bool create)
        {
            Assert.Fail("Should Not Be Called");
            return null;
        }

        protected override void AddOrUpdateTableInternal(LocalizedTable table)
        {
            AddOrUpdateTables.Add(table);
        }

        protected override void CreateAsset(Object asset, string path)
        {
            var table = asset as LocalizedAssetTable;
            if (table != null)
            {
                CreatedTables.Add(table);
                return;
            }

            var keyDb = asset as KeyDatabase;
            if (keyDb != null)
            {
                CreatedKeyDatabases.Add(keyDb);
            }
        }

        protected override List<TLocalizedTable> GetAssetTablesInternal<TLocalizedTable>()
        {
            var tables = new List<TLocalizedTable>();

            foreach(var created in CreatedTables)
            {
                var tbl = created as TLocalizedTable;
                if (tbl != null)
                {
                    tables.Add(tbl);
                }
            }

            foreach(var added in AddOrUpdateTables)
            {
                var tbl = added as TLocalizedTable;
                if (tbl != null && !tables.Contains(tbl))
                {
                    tables.Add(tbl);
                }
            }
            return tables;
        }

        public void Teardown()
        {
            CreatedTables.ForEach(tbl => Object.DestroyImmediate(tbl));
            CreatedTables.Clear();

            CreatedKeyDatabases.ForEach(keyDb => Object.DestroyImmediate(keyDb));
            CreatedKeyDatabases.Clear();

            AddOrUpdateTables.Clear();
        }
    }

    /// <summary>
    /// Overrides the Localization Editor Settings so tests can be run that wont interfere with an existing project.
    /// Supports a Testable AddressableAssetSettings instance.
    /// </summary>
    public class FakedAddressableLocalizationEditorSettings : LocalizationEditorSettings
    {
        public AddressableAssetSettings TestAddressableAssetSettings { get; set; }

        protected override AddressableAssetSettings GetAddressableAssetSettings(bool create)
        {
            Assert.IsNotNull(TestAddressableAssetSettings);
            return TestAddressableAssetSettings;
        }

        public bool LocaleLabel(string label)
        {
            return IsLocaleLabel(label);
        }
    }
}