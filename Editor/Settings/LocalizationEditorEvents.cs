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

        public event Action<LocalizedTableCollection, SharedTableData.SharedTableEntry> TableEntryAdded;
        internal virtual void RaiseTableEntryAdded(LocalizedTableCollection collection, SharedTableData.SharedTableEntry entry) => TableEntryAdded?.Invoke(collection, entry);

        public event Action<LocalizedTableCollection, SharedTableData.SharedTableEntry> TableEntryRemoved;
        internal virtual void RaiseTableEntryRemoved(LocalizedTableCollection collection, SharedTableData.SharedTableEntry entry) => TableEntryRemoved?.Invoke(collection, entry);

        public event Action<AssetTableCollection, AssetTable, AssetTableEntry> AssetTableEntryAdded;
        internal virtual void RaiseAssetTableEntryAdded(AssetTableCollection collection, AssetTable table, AssetTableEntry entry) => AssetTableEntryAdded?.Invoke(collection, table, entry);

        public event Action<AssetTableCollection, AssetTable, AssetTableEntry, string> AssetTableEntryRemoved;
        internal virtual void RaiseAssetTableEntryRemoved(AssetTableCollection collection, AssetTable table, AssetTableEntry entry, string assetGuid) => AssetTableEntryRemoved?.Invoke(collection, table, entry, assetGuid);

        public event EventHandler<LocalizedTableCollection> CollectionModified;
        internal virtual void RaiseCollectionModified(object sender, LocalizedTableCollection collection) => CollectionModified?.Invoke(sender, collection);

        public event Action<LocalizedTableCollection> CollectionAdded;
        internal virtual void RaiseCollectionAdded(LocalizedTableCollection collection) => CollectionAdded?.Invoke(collection);

        public event Action<LocalizedTableCollection> CollectionRemoved;
        internal virtual void RaiseCollectionRemoved(LocalizedTableCollection collection) => CollectionRemoved?.Invoke(collection);

        public event Action<LocalizedTableCollection, LocalizedTable> TableAddedToCollection;
        internal virtual void RaiseTableAddedToCollection(LocalizedTableCollection collection, LocalizedTable table) => TableAddedToCollection?.Invoke(collection, table);

        public event Action<LocalizedTableCollection, LocalizedTable> TableRemovedFromCollection;
        internal virtual void RaiseTableRemovedFromCollection(LocalizedTableCollection collection, LocalizedTable table) => TableRemovedFromCollection?.Invoke(collection, table);
    }
}
