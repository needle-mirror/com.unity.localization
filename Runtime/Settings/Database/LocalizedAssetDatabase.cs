using System;
using UnityEngine.Localization.Tables;
using UnityEngine.Pool;
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
        /// Once the Completed event has been called, during the next update, the internal operation will be returned to a pool so that it can be reused.
        /// If you do plan to keep hold of the handle after completion then you should call <see cref="Addressables.ResourceManager.Acquire(AsyncOperationHandle)"/>
        /// to prevent the operation being reused and <see cref="Addressables.Release(AsyncOperationHandle)"/> to finally return the operation back to the pool.
        /// </summary>
        /// <typeparam name="TObject">The type of asset that should be loaded.</typeparam>
        /// <param name="tableEntryReference">A reference to the entry in the <see cref="LocalizedAssetDatabase.DefaultTable"/></param>
        /// <param name="locale">The <see cref="Locale"/> to load the table from. Null will use <see cref="LocalizationSettings.SelectedLocale"/>.</param>
        /// <returns></returns>
        public AsyncOperationHandle<TObject> GetLocalizedAssetAsync<TObject>(TableEntryReference tableEntryReference, Locale locale = null) where TObject : Object
        {
            return GetLocalizedAssetAsync<TObject>(GetDefaultTable(), tableEntryReference, locale);
        }

        /// <summary>
        /// Returns a handle to a localized asset loading operation from the requested table.
        /// This function is asynchronous and may not have an immediate result available.
        /// Check IsDone to see if the data is available, if it is false then use the Completed event or yield on the operation.
        /// Once the Completed event has been called, during the next update, the internal operation will be returned to a pool so that it can be reused.
        /// If you do plan to keep hold of the handle after completion then you should call <see cref="Addressables.ResourceManager.Acquire(AsyncOperationHandle)"/>
        /// to prevent the operation being reused and <see cref="Addressables.Release(AsyncOperationHandle)"/> to finally return the operation back to the pool.
        /// </summary>
        /// <typeparam name="TObject">The type of asset that should be loaded.</typeparam>
        /// <param name="tableReference">A reference to the table that the asset should be loaded from.</param>
        /// <param name="tableEntryReference">A reference to the entry in the table.</param>
        /// <param name="locale">The <see cref="Locale"/> to load the table from. Null will use <see cref="LocalizationSettings.SelectedLocale"/>.</param>
        public virtual AsyncOperationHandle<TObject> GetLocalizedAssetAsync<TObject>(TableReference tableReference, TableEntryReference tableEntryReference, Locale locale = null) where TObject : Object
        {
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
            var loadTableOperation = GetTableAsync(tableReference, locale);

            var operation = GenericPool<LoadAssetOperation<TObject>>.Get();
            operation.Init(loadTableOperation, tableEntryReference);
            var handle = AddressablesInterface.ResourceManager.StartOperation(operation, loadTableOperation);

            // We don't want to force users to have to manage the reference counting so by default we will release the operation for reuse once completed in the next frame
            // If a user wants to hold onto it then they should call Acquire on the operation and later Release.
            handle.CompletedTypeless += ReleaseNextFrame;

            return handle;
        }

        /// <inheritdoc />
        public override void OnLocaleChanged(Locale locale)
        {
            foreach (var tableOp in TableOperations.Values)
            {
                tableOp.Result?.ReleaseAssets();
            }

            base.OnLocaleChanged(locale);
        }
    }
}
