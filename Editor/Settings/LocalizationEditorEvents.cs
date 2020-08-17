using System;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization
{
    public class LocalizationEditorEvents
    {
        public event Action<Locale> LocaleAdded;
        internal virtual void RaiseLocaleAdded(Locale locale) => LocaleAdded?.Invoke(locale);

        public event Action<Locale> LocaleRemoved;
        internal virtual void RaiseLocaleRemoved(Locale locale) => LocaleRemoved?.Invoke(locale);

        public event EventHandler<Locale> LocaleSortOrderChanged;
        internal virtual void RaiseLocaleSortOrderChanged(object sender, Locale locale) => LocaleSortOrderChanged?.Invoke(sender, locale);

        public event Action<LocalizationTableCollection, SharedTableData.SharedTableEntry> TableEntryAdded;
        internal virtual void RaiseTableEntryAdded(LocalizationTableCollection collection, SharedTableData.SharedTableEntry entry) => TableEntryAdded?.Invoke(collection, entry);

        public event Action<LocalizationTableCollection, SharedTableData.SharedTableEntry> TableEntryRemoved;
        internal virtual void RaiseTableEntryRemoved(LocalizationTableCollection collection, SharedTableData.SharedTableEntry entry) => TableEntryRemoved?.Invoke(collection, entry);

        public event Action<AssetTableCollection, AssetTable, AssetTableEntry> AssetTableEntryAdded;
        internal virtual void RaiseAssetTableEntryAdded(AssetTableCollection collection, AssetTable table, AssetTableEntry entry) => AssetTableEntryAdded?.Invoke(collection, table, entry);

        public event Action<AssetTableCollection, AssetTable, AssetTableEntry, string> AssetTableEntryRemoved;
        internal virtual void RaiseAssetTableEntryRemoved(AssetTableCollection collection, AssetTable table, AssetTableEntry entry, string assetGuid) => AssetTableEntryRemoved?.Invoke(collection, table, entry, assetGuid);

        public event EventHandler<LocalizationTableCollection> CollectionModified;
        internal virtual void RaiseCollectionModified(object sender, LocalizationTableCollection collection) => CollectionModified?.Invoke(sender, collection);

        public event Action<LocalizationTableCollection> CollectionAdded;
        internal virtual void RaiseCollectionAdded(LocalizationTableCollection collection) => CollectionAdded?.Invoke(collection);

        public event Action<LocalizationTableCollection> CollectionRemoved;
        internal virtual void RaiseCollectionRemoved(LocalizationTableCollection collection) => CollectionRemoved?.Invoke(collection);

        public event Action<LocalizationTableCollection, LocalizationTable> TableAddedToCollection;
        internal virtual void RaiseTableAddedToCollection(LocalizationTableCollection collection, LocalizationTable table) => TableAddedToCollection?.Invoke(collection, table);

        public event Action<LocalizationTableCollection, LocalizationTable> TableRemovedFromCollection;
        internal virtual void RaiseTableRemovedFromCollection(LocalizationTableCollection collection, LocalizationTable table) => TableRemovedFromCollection?.Invoke(collection, table);
    }
}
