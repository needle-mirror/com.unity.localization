using System;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace UnityEngine.Localization
{
    /// <summary>
    /// Provides runtime access to an <see cref="AssetTable"/> for the current selected <see cref="Locale"/>.
    /// When accessing multiple localized assets it may be more convenient to use a <see cref="LocalizedAssetTable"/> instead of multiple <see cref="LocalizedAsset{TObject}"/>.
    /// This will fetch the table on demand or provide a callback whenever the table has finished loading, such as when the selected locale was changed.
    /// </summary>
    [Serializable]
    public class LocalizedAssetTable : LocalizedTable<AssetTable, AssetTableEntry>
    {
        protected override LocalizedDatabase<AssetTable, AssetTableEntry> Database => LocalizationSettings.AssetDatabase;
    }
}
