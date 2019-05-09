using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization
{
    class AssetDatabasePreloadOperation : AsyncOperationBase<LocalizedAssetDatabase>
    {
        int m_PreloadingOperations;
        LocalizedAssetDatabase m_Db;
        string m_Error;

        public AssetDatabasePreloadOperation(LocalizedAssetDatabase db)
        {
            m_Db = db;
        }

        protected override void Execute()
        {
            var loadTablesOperation = Addressables.LoadAssetsAsync<LocalizedAssetTable>(new object[] { LocalizedAssetDatabase.AssetTableLabel, LocalizationSettings.SelectedLocale.Identifier.Code }, TableLoaded, Addressables.MergeMode.Intersection);
            loadTablesOperation.Completed += PreloadTablesCompleted;
        }

        void TableLoaded(LocalizedAssetTable table)
        {
            var tables = m_Db.GetTablesDict(table.SupportedAssetType);
            Debug.AssertFormat(!tables.ContainsKey(table.TableName), "A table with the same key `{0}` already exists for this type `{1}`. Something went wrong during preloading.", table.TableName, table.SupportedAssetType);
            tables[table.TableName] = LocalizationSettings.ResourceManager.CreateCompletedOperation(table, string.Empty);
        }

        void PreloadTablesCompleted(AsyncOperationHandle<IList<LocalizedAssetTable>> asyncOperation)
        {
            if (asyncOperation.Status != AsyncOperationStatus.Succeeded)
            {
                var error = "Failed to preload table: " + asyncOperation.DebugName + "\n";
                m_Error += error;
                Debug.LogError(error);
                if (asyncOperation.OperationException != null)
                {
                    Debug.LogException(asyncOperation.OperationException);
                    m_Error += asyncOperation.OperationException + "\n";
                }
                return;
            }

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
                FinishInitializing();
        }

        void PreloadOperationCompleted(AsyncOperationHandle obj)
        {
            m_PreloadingOperations--;

            if (obj.Status != AsyncOperationStatus.Succeeded)
            {
                m_Error += "Failed to preload: " + obj.DebugName + "\n";
                if (obj.OperationException != null)
                    m_Error += obj.OperationException + "\n";
            }

            Debug.Assert(m_PreloadingOperations >= 0);
            if (m_PreloadingOperations == 0)
                FinishInitializing();
        }

        void FinishInitializing() => Complete(m_Db, string.IsNullOrEmpty(m_Error), m_Error);
    }
}