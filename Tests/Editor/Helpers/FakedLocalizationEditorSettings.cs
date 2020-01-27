using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using UnityEditor.AddressableAssets.Settings;
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
        public List<SharedTableData> CreatedSharedTableDatas { get; set; }
        public List<LocalizedTable> AddOrUpdateTables { get; set; }

        public List<AssetTableCollection> Collections { get; set; } = new List<AssetTableCollection>();

        public FakedLocalizationEditorSettings()
        {
            CreatedTables = new List<LocalizedTable>();
            CreatedSharedTableDatas = new List<SharedTableData>();
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

            var sharedData = asset as SharedTableData;
            if (sharedData != null)
            {
                CreatedSharedTableDatas.Add(sharedData);
            }
        }

        internal override string GetAssetGuid(Object asset)
        {
            if (asset is SharedTableData sharedData)
            {
                return CreatedSharedTableDatas.Find(k => k == sharedData).TableNameGuid.ToString("N");
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

            CreatedSharedTableDatas.ForEach(sharedData => Object.DestroyImmediate(sharedData));
            CreatedSharedTableDatas.Clear();

            AddOrUpdateTables.Clear();
        }
    }
}
