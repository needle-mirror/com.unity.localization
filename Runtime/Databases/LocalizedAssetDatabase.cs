using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization
{
    public class LocalizedAssetDatabase : LocalizedDatabase, IPreloadRequired
    {
        AsyncOperationHandle? m_PreloadOperation;

        public const string AssetTableLabel = "AssetTable";
        
        // We track all tables either fully loaded or still loading here.
        Dictionary<Type, Dictionary<string, AsyncOperationHandle<LocalizedAssetTable>>> m_Tables = new Dictionary<Type, Dictionary<string, AsyncOperationHandle<LocalizedAssetTable>>>();

        public AsyncOperationHandle PreloadOperation
        {
            get
            {
                if(m_PreloadOperation == null)
                {
                    m_PreloadOperation = LocalizationSettings.ResourceManager.StartOperation(new AssetDatabasePreloadOperation(this), default);
                }
                return m_PreloadOperation.Value;
            }
        }

        #if UNITY_EDITOR
        void OnEnable()
        {
            // ScriptableObject properties may persist during runs in the editor, so we reset them here to keep each play consistent.
            m_PreloadOperation = null;
            m_Tables.Clear();
        }

        void OnDisable()
        {
            m_PreloadOperation = null;
            m_Tables.Clear();
        }
        #endif

        /// <summary>
        /// Returns the loading operation for the selected table. If isDone is true then the table can be used immediately
        /// otherwise yield on the operation or use the callback to wait for it to be completed.
        /// </summary>
        public virtual AsyncOperationHandle<LocalizedAssetTable> GetTableAsync<TObject>(string tableName) where TObject : Object
        {
            var tables = GetTablesDict(typeof(TObject));
            if (tables.TryGetValue(tableName, out var operation))
            {
                return operation;
            }

            var tableAddress = $"{LocalizationSettings.SelectedLocale.Identifier.Code} - {tableName}";
            var asyncOp = Addressables.LoadAssetAsync<LocalizedAssetTable>(tableAddress);
            tables[tableName] = asyncOp;
            return asyncOp;
        }

        internal Dictionary<string, AsyncOperationHandle<LocalizedAssetTable>> GetTablesDict(Type assetType)
        {
            if (!m_Tables.TryGetValue(assetType, out var tables))
            {
                tables = new Dictionary<string, AsyncOperationHandle<LocalizedAssetTable>>();
                m_Tables[assetType] = tables;
            }
            return tables;
        }

        /// <summary>
        /// Loads the asset found in the table with the key.
        /// </summary>
        /// <typeparam name="TObject">Asset type</typeparam>
        /// <param name="tableName"></param>
        /// <param name="key"></param>
        public virtual AsyncOperationHandle<TObject> GetLocalizedAssetAsync<TObject>(string tableName, string key) where TObject : Object
        {
            if (string.IsNullOrEmpty(tableName))
                return LocalizationSettings.ResourceManager.CreateCompletedOperation<TObject>(null, nameof(tableName) + " can not be empty or null.");

            if (string.IsNullOrEmpty(key))
                return LocalizationSettings.ResourceManager.CreateCompletedOperation<TObject>(null, nameof(key) + " can not be null or empty.");

            var initOp = LocalizationSettings.InitializationOperation;
            if (!initOp.Value.IsDone)
                return LocalizationSettings.ResourceManager.CreateChainOperation<TObject>(initOp.Value, (op) => GetLocalizedAsset_LoadTableAsync<TObject>(tableName, key));

            return GetLocalizedAsset_LoadTableAsync<TObject>(tableName, key);
        }

        /// <summary>
        /// Loads the asset found in the table with the key Id, taken from the KeyDatabase.
        /// </summary>
        /// <typeparam name="TObject">Asset type</typeparam>
        /// <param name="tableName"></param>
        /// <param name="keyId"></param>
        public virtual AsyncOperationHandle<TObject> GetLocalizedAssetAsync<TObject>(string tableName, uint keyId) where TObject : Object
        {
            if (string.IsNullOrEmpty(tableName))
                return LocalizationSettings.ResourceManager.CreateCompletedOperation<TObject>(null, nameof(tableName) + " can not be empty or null.");

            if (keyId == KeyDatabase.EmptyId)
                return LocalizationSettings.ResourceManager.CreateCompletedOperation<TObject>(null, nameof(keyId) + " can not be empty.");

            var initOp = LocalizationSettings.InitializationOperation;
            if (!initOp.Value.IsDone)
                return LocalizationSettings.ResourceManager.CreateChainOperation<TObject>(initOp.Value, (op) => GetLocalizedAsset_LoadTableAsync<TObject>(tableName, keyId));

            return GetLocalizedAsset_LoadTableAsync<TObject>(tableName, keyId);
        }

        protected virtual AsyncOperationHandle<TObject> GetLocalizedAsset_LoadTableAsync<TObject>(string tableName, string key) where TObject : Object
        {
            // First get or load the table
            var tableOp = GetTableAsync<TObject>(tableName);
            if (!tableOp.IsDone)
                return LocalizationSettings.ResourceManager.CreateChainOperation<TObject>(tableOp, (op) => GetLocalizedAsset_LoadAsset<TObject>(tableOp, tableOp.Result.Keys.GetId(key)));

            return GetLocalizedAsset_LoadAsset<TObject>(tableOp, tableOp.Result.Keys.GetId(key));
        }

        protected virtual AsyncOperationHandle<TObject> GetLocalizedAsset_LoadTableAsync<TObject>(string tableName, uint keyId) where TObject : Object
        {
            // First get or load the table
            var tableOp = GetTableAsync<TObject>(tableName);
            if (!tableOp.IsDone)
                return LocalizationSettings.ResourceManager.CreateChainOperation<TObject>(tableOp, (op) => GetLocalizedAsset_LoadAsset<TObject>(tableOp, keyId));

            return GetLocalizedAsset_LoadAsset<TObject>(tableOp, keyId);
        }

        static AsyncOperationHandle<TObject> GetLocalizedAsset_LoadAsset<TObject>(AsyncOperationHandle<LocalizedAssetTable> table, uint key) where TObject : Object
        {
            if (table.Status != AsyncOperationStatus.Succeeded)
            {
                var error = "Failed to load table: " + table.DebugName;
                Debug.LogError(error);
                if (table.OperationException != null)
                {
                    error += "\n" + table.OperationException;
                    Debug.LogException(table.OperationException);
                }

                return LocalizationSettings.ResourceManager.CreateCompletedOperation<TObject>(null, error);
            }

            var assetTable = (AddressableAssetTableT<TObject>)table.Result;
            return assetTable.GetAssetAsync(key);
        }

        /// <inheritdoc />
        public override void OnLocaleChanged(Locale locale)
        {
            foreach (var table in m_Tables)
            {
                foreach (var asyncOperation in table.Value)
                {
                    LocalizationSettings.ResourceManager.Release(asyncOperation.Value);
                }
            }
            m_Tables.Clear();

            if (m_PreloadOperation != null)
            {
                // TODO: Cancel loading operation if it is not completed. https://unity.slack.com/archives/C8Z80RV4K/p1556632817070600
                LocalizationSettings.ResourceManager.Release(m_PreloadOperation.Value);
                m_PreloadOperation = null;
            }
        }
    }
}