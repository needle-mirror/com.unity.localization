using System;

namespace UnityEngine.Localization
{
    public abstract class LocalizedAssetTable : LocalizedTable
    {
        /// <summary>
        /// TODO: DOC
        /// </summary>
        public abstract Type SupportedAssetType { get; }
    }
}