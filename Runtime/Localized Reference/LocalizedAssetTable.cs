using System;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace UnityEngine.Localization
{
    [Serializable]
    public class LocalizedAssetTable : LocalizedTable<AssetTable, AssetTableEntry>
    {
        protected override LocalizedDatabase<AssetTable, AssetTableEntry> Database => LocalizationSettings.AssetDatabase;

        /// <summary>
        /// Initializes and returns an empty instance of a <see cref="LocalizedAssetTable"/>.
        /// </summary>
        public LocalizedAssetTable() {}

        /// <summary>
        /// Initializes and returns an instance of a <see cref="LocalizedAssetTable"/>.
        /// </summary>
        /// <param name="tableReference">Reference to the Asset Table Collection.
        /// This can either be the name of the collection as a <c>string</c> or the Collection Guid as a [System.Guid](https://docs.microsoft.com/en-us/dotnet/api/system.guid).</param>
        public LocalizedAssetTable(TableReference tableReference) => TableReference = tableReference;
    }
}
