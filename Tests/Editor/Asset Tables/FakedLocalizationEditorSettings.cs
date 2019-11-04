using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.Localization.Tables;
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

        public List<AssetTableCollection> Collections { get; set; } = new List<AssetTableCollection>();

        public FakedLocalizationEditorSettings()
        {
            CreatedTables = new List<LocalizedTable>();
            CreatedKeyDatabases = new List<KeyDatabase>();
            AddOrUpdateTables = new List<LocalizedTable>();
        }

        internal override AddressableAssetSettings GetAddressableAssetSettings(bool create)
        {
            Assert.Fail("Should Not Be Called");
            return null;
        }

        protected override void AddOrUpdateTableInternal(LocalizedTable table, bool createUndo)
        {
            AddOrUpdateTables.Add(table);
        }

        internal override void CreateAsset(Object asset, string path)
        {
            var table = asset as AssetTable;
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

        internal override string GetAssetGuid(Object asset)
        {
            if (asset is KeyDatabase keys)
            {
                return CreatedKeyDatabases.Find(k => k == keys).TableNameGuid.ToString("N");
            }
            Assert.Fail("Should not be called");
            return null;
        }

        protected override ReadOnlyCollection<AssetTableCollection> GetAssetTablesCollectionInternal(Type tableType)
        {
            return new ReadOnlyCollection<AssetTableCollection>(Collections);
        }

        protected override List<AddressableAssetEntry> GetAssetTablesInternal(Type tableType)
        {
            Assert.Fail("Should not be called");
            return null;
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
}