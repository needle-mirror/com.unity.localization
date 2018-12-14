using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;

namespace UnityEngine.Localization
{
    class AssetDatabasePreloadOperation : AsyncOperationBase<LocalizedAssetDatabase>
    {
        int m_PreloadingOperations;
        LocalizedAssetDatabase m_Db;

        /// <inheritdoc />
        public override void ResetStatus()
        {
            base.ResetStatus();
            m_PreloadingOperations = 0;
            m_Db = null;
        }

        public virtual AssetDatabasePreloadOperation Start(LocalizedAssetDatabase db)
        {
            m_Db = db;
            var loadTablesOperation = Addressables.LoadAssets<LocalizedAssetTable>(new object[] { LocalizedAssetDatabase.AssetTableLabel, LocalizationSettings.SelectedLocale.Identifier.Code }, TableLoaded, Addressables.MergeMode.Intersection);
            loadTablesOperation.Completed += PreloadTablesCompleted;
            return this;
        }

        void TableLoaded(IAsyncOperation<LocalizedAssetTable> asyncOperation)
        {
            if (!asyncOperation.HasLoadedSuccessfully())
                return;
            var tables = m_Db.GetTablesDict(asyncOperation.Result.SupportedAssetType);
            Debug.AssertFormat(!tables.ContainsKey(asyncOperation.Result.TableName), "A table with the same key `{0}` already exists for this type `{1}`. Something went wrong during preloading.", asyncOperation.Result.TableName, asyncOperation.Result.SupportedAssetType);
            asyncOperation.Retain();
            tables[asyncOperation.Result.TableName] = asyncOperation;
        }

        void PreloadTablesCompleted(IAsyncOperation<IList<LocalizedAssetTable>> asyncOperation)
        {
            asyncOperation.HasLoadedSuccessfully();

            // Preload table data
            m_PreloadingOperations = 0;
            foreach (var table in asyncOperation.Result)
            {
                var preloadTable = table as IPreloadRequired;
                if (preloadTable != null && !preloadTable.PreloadOperation.IsDone)
                {
                    m_PreloadingOperations++;
                    preloadTable.PreloadOperation.Completed += PreloadOperationCompleted;
                }
            }

            if (m_PreloadingOperations == 0)
                InvokeCompletionEvent();
        }

        private void PreloadOperationCompleted(IAsyncOperation obj)
        {
            m_PreloadingOperations--;

            if (obj.Status != AsyncOperationStatus.Succeeded)
            {
                Status = obj.Status;
                m_error = obj.OperationException;
            }

            Debug.Assert(m_PreloadingOperations >= 0);
            if (m_PreloadingOperations == 0)
                FinishInitializing();
        }

        void FinishInitializing()
        {
            if (Status != AsyncOperationStatus.Failed)
                Status = AsyncOperationStatus.Succeeded;
            InvokeCompletionEvent();
        }
    }
}