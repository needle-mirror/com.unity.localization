using System;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization.Settings
{
    /// <summary>
    /// The Localized Asset Database provides a single point of access for all localized assets.
    /// </summary>
    /// <remarks>
    /// A localized asset must derive from <see cref="UnityEngine.Object"/>.
    /// </remarks>
    [Serializable]
    public class LocalizedAssetDatabase : LocalizedDatabase<AssetTable, AssetTableEntry>
    {
        /// <summary>
        /// Returns a handle to a localized asset loading operation from the <see cref="LocalizedAssetDatabase.DefaultTable"/>.
        /// This function is asynchronous and may not have an immediate result available.
        /// Check IsDone to see if the data is available, if it is false then use the Completed event or yield on the operation.
        /// </summary>
        /// <typeparam name="TObject">The type of asset that should be loaded.</typeparam>
        /// <param name="tableEntryReference">A reference to the entry in the <see cref="LocalizedAssetDatabase.DefaultTable"/></param>
        /// <param name="locale">The <see cref="Locale"/> to use instead of the default <see cref="LocalizationSettings.SelectedLocale"/></param>
        /// <returns></returns>
        public AsyncOperationHandle<TObject> GetLocalizedAssetAsync<TObject>(TableEntryReference tableEntryReference) where TObject : Object
        {
            return GetLocalizedAssetAsync<TObject>(DefaultTable, tableEntryReference);
        }

        /// <summary>
        /// Returns a handle to a localized asset loading operation from the <see cref="LocalizedAssetDatabase.DefaultTable"/>.
        /// This function is asynchronous and may not have an immediate result available.
        /// Check IsDone to see if the data is available, if it is false then use the Completed event or yield on the operation.
        /// </summary>
        /// <typeparam name="TObject">The type of asset that should be loaded.</typeparam>
        /// <param name="tableEntryReference">A reference to the entry in the <see cref="LocalizedAssetDatabase.DefaultTable"/></param>
        /// <returns></returns>
        public AsyncOperationHandle<TObject> GetLocalizedAssetAsync<TObject>(TableEntryReference tableEntryReference, Locale locale) where TObject : Object
        {
            if (locale == null)
                throw new ArgumentNullException(nameof(locale));

            return GetLocalizedAssetAsync<TObject>(DefaultTable, tableEntryReference, locale);
        }

        /// <summary>
        /// Returns a handle to a localized asset loading operation from the requested table.
        /// This function is asynchronous and may not have an immediate result available.
        /// Check IsDone to see if the data is available, if it is false then use the Completed event or yield on the operation.
        /// </summary>
        /// <typeparam name="TObject">The type of asset that should be loaded.</typeparam>
        /// <param name="tableReference">A reference to the table that the asset should be loaded from.</param>
        /// <param name="tableEntryReference">A reference to the entry in the table.</param>
        public virtual AsyncOperationHandle<TObject> GetLocalizedAssetAsync<TObject>(TableReference tableReference, TableEntryReference tableEntryReference) where TObject : Object
        {
            var locale = LocalizationSettings.SelectedLocaleAsync;
            if (!locale.IsDone)
                return ResourceManager.CreateChainOperation(locale, (op) => GetLocalizedAssetAsync<TObject>(tableReference, tableEntryReference, op.Result));
            return GetLocalizedAssetAsync<TObject>(tableReference, tableEntryReference, locale.Result);
        }

        /// <summary>
        /// Returns a handle to a localized asset loading operation from the requested table.
        /// This function is asynchronous and may not have an immediate result available.
        /// Check IsDone to see if the data is available, if it is false then use the Completed event or yield on the operation.
        /// </summary>
        /// <typeparam name="TObject">The type of asset that should be loaded.</typeparam>
        /// <param name="tableReference">A reference to the table that the asset should be loaded from.</param>
        /// <param name="tableEntryReference">A reference to the entry in the table.</param>
        /// <param name="locale">The <see cref="Locale"/> to use instead of the default <see cref="LocalizationSettings.SelectedLocale"/></param>
        public AsyncOperationHandle<TObject> GetLocalizedAssetAsync<TObject>(TableReference tableReference, TableEntryReference tableEntryReference, Locale locale) where TObject : Object
        {
            if (locale == null)
                throw new ArgumentNullException(nameof(locale));
            return GetLocalizedAssetAsyncInternal<TObject>(tableReference, tableEntryReference, locale);
        }

        /// <summary>
        /// Implementation for all versions of <see cref="GetLocalizedAssetAsync"/>.
        /// </summary>
        /// <typeparam name="TObject">The type of asset that should be loaded.</typeparam>
        /// <param name="tableReference">A reference to the table that the asset should be loaded from.</param>
        /// <param name="tableEntryReference">A reference to the entry in the table.</param>
        /// <param name="locale">The <see cref="Locale"/> to use instead of the default <see cref="LocalizationSettings.SelectedLocale"/></param>
        protected virtual AsyncOperationHandle<TObject> GetLocalizedAssetAsyncInternal<TObject>(TableReference tableReference, TableEntryReference tableEntryReference, Locale locale) where TObject : Object
        {
            var tableEntryOp = GetTableEntryAsync(tableReference, tableEntryReference, locale);
            if (!tableEntryOp.IsDone)
                return ResourceManager.CreateChainOperation<TObject>(tableEntryOp, (op) => GetLocalizedAssetLoadAsset<TObject>(tableEntryOp, tableEntryReference));
            return GetLocalizedAssetLoadAsset<TObject>(tableEntryOp, tableEntryReference);
        }

        /// <summary>
        /// /// Performs the final step after the <see cref="AssetTableEntry"/> has been found. Starts the table asset loading operation.
        /// </summary>
        /// <typeparam name="TObject">The type of asset that should be loaded.</typeparam>
        /// <param name="entryOp">The table entry operation returned from <see cref="LocalizedAssetDatabase.GetTableEntryAsync"/></param>
        /// <param name="tableEntryReference">A reference to the entry in the table.</param>
        /// <returns></returns>
        internal protected virtual AsyncOperationHandle<TObject> GetLocalizedAssetLoadAsset<TObject>(AsyncOperationHandle<TableEntryResult> entryOp, TableEntryReference tableEntryReference) where TObject : Object
        {
            tableEntryReference.Validate();

            if (entryOp.Status != AsyncOperationStatus.Succeeded || entryOp.Result.Entry == null)
            {
                // Find the key so we can provide a better error message.
                var table = entryOp.Result.Table;
                string key = tableEntryReference.ResolveKeyName(table?.SharedData);
                var message = $"Could not load asset {key}";
                return ResourceManager.CreateCompletedOperation<TObject>(null, message);
            }

            return entryOp.Result.Table.GetAssetAsync<TObject>(entryOp.Result.Entry);
        }

        /// <inheritdoc />
        public override void OnLocaleChanged(Locale locale)
        {
            foreach (var tableOp in TableOperations.Values)
            {
                var table = tableOp.Result;
                if (table != null)
                {
                    foreach (var tableEntry in table.Values)
                    {
                        if (tableEntry.AsyncOperation.HasValue)
                        {
                            Addressables.Release(tableEntry.AsyncOperation.Value);
                            tableEntry.AsyncOperation = null;
                        }
                    }
                }
            }

            base.OnLocaleChanged(locale);
        }
    }
}
