using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization.Settings
{
    [Serializable]
    public abstract class LocalizedDatabase<TTable, TEntry> : IPreloadRequired
        where TTable : LocalizedTableT<TEntry>
        where TEntry : TableEntry, new()
    {
        /// <summary>
        /// Contains the results of a request. The found entry and the table the entry was found in,
        /// this may be different if a fall back occurred.
        /// </summary>
        public struct TableEntryResult
        {
            public TEntry Entry{ get; private set; }
            public TTable Table { get; private set; }

            internal TableEntryResult(TEntry entry, TTable table)
            {
                Entry = entry;
                Table = table;
            }
        }

        /// <summary>
        /// Preload operation.
        /// Loads all tables and their contents(when applicable) marked with the preload label for the selected locale.
        /// </summary>
        public AsyncOperationHandle PreloadOperation
        {
            get
            {
                if (!m_PreloadOperationHandle.HasValue)
                {
                    // TODO: We need to reuse these operations however its not possible to reset the internal state at the moment.
                    //if (m_PreloadOperation == null)
                        m_PreloadOperation = new PreloadDatabaseOperation<TTable, TEntry>();
                    m_PreloadOperation.Init(this);
                    m_PreloadOperationHandle = ResourceManager.StartOperation(m_PreloadOperation, default);
                }
                return m_PreloadOperationHandle.Value;
            }
        }

        [SerializeField]
        TableReference m_DefaultTableReference;

        PreloadDatabaseOperation<TTable, TEntry> m_PreloadOperation;
        AsyncOperationHandle? m_PreloadOperationHandle;

        /// <summary>
        /// The <see cref="ResourceManager"/> to use when generating loading operations.
        /// By default uses <see cref="LocalizationSettings.ResourceManager"/>
        /// </summary>
        protected virtual ResourceManager ResourceManager => LocalizationSettings.ResourceManager;

        // These values should never go null however they currently do due to bug 1193322. As a workaround we check them in a property.
        Dictionary<string, AsyncOperationHandle<TTable>> m_TableOperations = new Dictionary<string, AsyncOperationHandle<TTable>>();
        Dictionary<Guid, AsyncOperationHandle<KeyDatabase>> m_KeyDatabaseOperations = new Dictionary<Guid, AsyncOperationHandle<KeyDatabase>>();

        internal Dictionary<string, AsyncOperationHandle<TTable>> TableOperations
        {
            get
            {
                if (m_TableOperations == null)
                    m_TableOperations = new Dictionary<string, AsyncOperationHandle<TTable>>();
                return m_TableOperations;
            }
        }

        Dictionary<Guid, AsyncOperationHandle<KeyDatabase>> KeyDatabaseOperations
        {
            get
            {
                if (m_KeyDatabaseOperations == null)
                    m_KeyDatabaseOperations = new Dictionary<Guid, AsyncOperationHandle<KeyDatabase>>();
                return m_KeyDatabaseOperations;
            }
        }

        /// <summary>
        /// The default table to use when no table name is provided.
        /// </summary>
        public TableReference DefaultTable
        {
            get => m_DefaultTableReference;
            set => m_DefaultTableReference = value;
        }

        internal void RegisterTableOperation(AsyncOperationHandle<TTable> handle, string tableName)
        {
            TableOperations[tableName] = handle;
        }

        /// <summary>
        /// Returns the named table.
        /// This function is asynchronous and may not have an immediate result available.
        /// Check IsDone to see if the data is available, if it is false then use the Completed event or yield on the operation.
        /// </summary>
        /// <param name="tableReference">The table identifier. Can be either the name of the table or the table name Guid.</param>
        /// <returns></returns>
        public AsyncOperationHandle<TTable> GetTableAsync(TableReference tableReference)
        {
            var initOp = LocalizationSettings.InitializationOperation;
            if (!initOp.IsDone)
                return ResourceManager.CreateChainOperation(initOp, (op) => GetTable_LoadTable(tableReference));
            return GetTable_LoadTable(tableReference);
        }

        /// <summary>
        /// Returns the entry from the requested table. A table entry will contain the localized item and metadata.
        /// This function is asynchronous and may not have an immediate result available.
        /// Check IsDone to see if the data is available, if it is false then use the Completed event or yield on the operation.
        /// </summary>
        /// <param name="tableReference">The table identifier. Can be either the name of the table or the table name Guid.</param>
        /// <param name="tableEntryReference">A reference to the entry in the table.</param>
        /// <returns></returns>
        public AsyncOperationHandle<TableEntryResult> GetTableEntryAsync(TableReference tableReference, TableEntryReference tableEntryReference)
        {
            tableReference.Validate();
            tableEntryReference.Validate();

            var tableOp = GetTableAsync(tableReference);
            if (!tableOp.IsDone)
                return ResourceManager.CreateChainOperation(tableOp, (op) => GetTableEntry_FindEntry(tableOp, tableEntryReference));
            return GetTableEntry_FindEntry(tableOp, tableEntryReference);
        }

        AsyncOperationHandle<KeyDatabase> GetKeyDatabase(Guid tableNameGuid)
        {
            if (KeyDatabaseOperations.TryGetValue(tableNameGuid, out var keyDatabaseOp))
                return keyDatabaseOp;

            keyDatabaseOp = Addressables.LoadAssetAsync<KeyDatabase>(TableReference.StringFromGuid(tableNameGuid));
            KeyDatabaseOperations[tableNameGuid] = keyDatabaseOp;
            return keyDatabaseOp;
        }

        AsyncOperationHandle<TTable> GetTable_LoadTable(AsyncOperationHandle<KeyDatabase> keyDatabaseOperation)
        {
            if (keyDatabaseOperation.Status != AsyncOperationStatus.Succeeded)
            {
                var error = $"Failed to load key database: {keyDatabaseOperation.DebugName}";
                Debug.LogError(error);
                if (keyDatabaseOperation.OperationException != null)
                {
                    Debug.LogException(keyDatabaseOperation.OperationException);
                }
                return ResourceManager.CreateCompletedOperation(default(TTable), error);
            }
            return GetTable_LoadTable(keyDatabaseOperation.Result.TableName);
        }

        AsyncOperationHandle<TTable> GetTable_LoadTable(TableReference tableReference)
        {
            if (tableReference.ReferenceType == TableReference.Type.Guid)
            {
                var keyDatabaseOperation = GetKeyDatabase(tableReference);
                if (keyDatabaseOperation.IsDone)
                    return GetTable_LoadTable(keyDatabaseOperation);
                return ResourceManager.CreateChainOperation(keyDatabaseOperation, op => GetTable_LoadTable(op));
            }

            if (TableOperations.TryGetValue(tableReference, out var asyncOp))
                return asyncOp;

            var tableAddress = AddressHelper.GetTableAddress(tableReference, LocalizationSettings.SelectedLocale.Identifier);
            asyncOp = Addressables.LoadAssetAsync<TTable>(tableAddress);
            RegisterTableOperation(asyncOp, tableReference.TableName);
            return asyncOp;
        }

        AsyncOperationHandle<TableEntryResult> GetTableEntry_FindEntry(AsyncOperationHandle<TTable> tableOperation, TableEntryReference tableEntryReference)
        {
            if (tableOperation.Status != AsyncOperationStatus.Succeeded)
            {
                var error = $"Failed to load table: {tableOperation.DebugName}";
                Debug.LogError(error);
                if (tableOperation.OperationException != null)
                {
                    Debug.LogException(tableOperation.OperationException);
                }
                return ResourceManager.CreateCompletedOperation(default(TableEntryResult), error);
            }

            var table = tableOperation.Result;

            // We need either a key of keyId. If the key is null then we use the keyId.
            var entry = tableEntryReference.ReferenceType == TableEntryReference.Type.Name ? table.GetEntry(tableEntryReference.Key) : table.GetEntry(tableEntryReference.KeyId);

            // TODO: Fallback if null
            return ResourceManager.CreateCompletedOperation(new TableEntryResult(entry, tableOperation.Result), null);
        }

        /// <summary>
        /// Called before the LocaleChanged event is sent out in order to give the database a chance to prepare.
        /// </summary>
        public virtual void OnLocaleChanged(Locale locale)
        {
            foreach (var to in TableOperations.Values)
            {
                Addressables.Release(to);
            }

            m_PreloadOperationHandle = null;
            TableOperations.Clear();
        }
    }
}