using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;

namespace UnityEngine.Localization
{
    public class LocalizedAssetDatabase : LocalizedDatabase, IPreloadRequired
    {        
        AssetDatabasePreloadOperation m_PreloadOperation;

        public const string AssetTableLabel = "AssetTable";
        
        // We track all tables either fully loaded or still loading here.
        Dictionary<Type, Dictionary<string, IAsyncOperation<LocalizedAssetTable>>> m_Tables = new Dictionary<Type, Dictionary<string, IAsyncOperation<LocalizedAssetTable>>>();

        public IAsyncOperation PreloadOperation
        {
            get
            {
                if(m_PreloadOperation == null)
                {
                    m_PreloadOperation = AsyncOperationCache.Instance.Acquire<AssetDatabasePreloadOperation>();
                    m_PreloadOperation.Retain();
                    m_PreloadOperation.Start(this);
                }
                return m_PreloadOperation;
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
        public virtual IAsyncOperation<LocalizedAssetTable> GetTable<TObject>(string tableName) where TObject : Object
        {
            var tables = GetTablesDict(typeof(TObject));
            IAsyncOperation<LocalizedAssetTable> operation;
            if (tables.TryGetValue(tableName, out operation))
            {
                return operation;
            }

            var tableAddress = string.Format("{0} - {1}", LocalizationSettings.SelectedLocale.Identifier.Code, tableName);
            var asyncOp = Addressables.LoadAsset<LocalizedAssetTable>(tableAddress);
            asyncOp.Retain();
            tables[tableName] = asyncOp;
            return asyncOp;
        }

        internal Dictionary<string, IAsyncOperation<LocalizedAssetTable>> GetTablesDict(Type assetType)
        {
            Dictionary<string, IAsyncOperation<LocalizedAssetTable>> tables;
            if (!m_Tables.TryGetValue(assetType, out tables))
            {
                tables = new Dictionary<string, IAsyncOperation<LocalizedAssetTable>>();
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
        public virtual IAsyncOperation<TObject> GetLocalizedAsset<TObject>(string tableName, string key) where TObject : Object
        {
            if (string.IsNullOrEmpty(tableName))
                return new CompletedOperation<TObject>().Start("tableName can not be empty or null.", key, null);

            if(string.IsNullOrEmpty(key))
                return new CompletedOperation<TObject>().Start("key can not be empty or null.", key, null);

            var initOp = LocalizationSettings.InitializationOperation;
            if (!initOp.IsDone)
                return AsyncOperationCache.Instance.Acquire<ChainOperation<TObject, LocalizationSettings>>().Start(null, key, initOp, (op) => GetLocalizedAsset_LoadTable<TObject>(tableName, key).Retain());
            return GetLocalizedAsset_LoadTable<TObject>(tableName, key);
        }

        protected virtual IAsyncOperation<TObject> GetLocalizedAsset_LoadTable<TObject>(string tableName, string key) where TObject : Object
        {
            // First get or load the table
            var tableOp = GetTable<TObject>(tableName);
            if (!tableOp.IsDone)
                return AsyncOperationCache.Instance.Acquire<ChainOperation<TObject, LocalizedAssetTable>>().Start(null, key, tableOp, (op) => GetLocalizedAsset_LoadAsset<TObject>(tableOp, key).Retain());
            return GetLocalizedAsset_LoadAsset<TObject>(tableOp, key);
        }

        static IAsyncOperation<TObject> GetLocalizedAsset_LoadAsset<TObject>(IAsyncOperation<LocalizedAssetTable> table, string key) where TObject : Object
        {
            if (table.HasLoadedSuccessfully())
            {
                var assetTable = (AddressableAssetTableT<TObject>)table.Result;
                return assetTable.GetAssetAsync(key);
            }
            return new CompletedOperation<TObject>().Start(table.Context, table.Key, null, table.OperationException);
        }

        /// <inheritdoc />
        public override void OnLocaleChanged(Locale locale)
        {
            foreach (var table in m_Tables)
            {
                foreach (var asyncOperation in table.Value)
                {
                    Addressables.ReleaseAsset(asyncOperation.Value.Result);
                    asyncOperation.Value.Release();
                }
            }
            m_Tables.Clear();

            if (m_PreloadOperation != null)
            {
                // TODO: Cancel loading operation if it is not completed. 

                m_PreloadOperation.Release();
                m_PreloadOperation = null;
            }
        }
    }
}