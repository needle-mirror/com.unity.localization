using System;

namespace UnityEngine.Localization
{
    public abstract class LocalizedAssetTable : LocalizedTable
    {
        /// <summary>
        /// The asset type supported by this table.
        /// </summary>
        public abstract Type SupportedAssetType { get; }
    }
}