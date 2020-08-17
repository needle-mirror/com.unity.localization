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
        where TTable : DetailedLocalizationTable<TEntry>
        where TEntry : TableEntry
    {
        /// <summary>
        /// Contains the results of a request. The found entry and the table the entry was found in,
        /// this may be different if a fall back occurred.
        /// </summary>
        public struct TableEntryResult
        {
            public TEntry Entry { get; private set; }
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
        Dictionary<(LocaleIdentifier localeIdentifier, string tableName), AsyncOperationHandle<TTable>> m_TableOperations = new Dictionary<(LocaleIdentifier localeIdentifier, string tableName), AsyncOperationHandle<TTable>>();
        Dictionary<Guid, AsyncOperationHandle<SharedTableData>> m_SharedTableDataOperations = new Dictionary<Guid, AsyncOperationHandle<SharedTableData>>();

        internal Dictionary<(LocaleIdentifier localeIdentifier, string tableName), AsyncOperationHandle<TTable>> TableOperations
        {
            get
            {
                if (m_TableOperations == null)
                    m_TableOperations = new Dictionary<(LocaleIdentifier localeIdentifier, string tableName), AsyncOperationHandle<TTable>>();
                return m_TableOperations;
            }
        }

        Dictionary<Guid, AsyncOperationHandle<SharedTableData>> SharedTableDataOperations
        {
            get
            {
                if (m_SharedTableDataOperations == null)
                    m_SharedTableDataOperations = new Dictionary<Guid, AsyncOperationHandle<SharedTableData>>();
                return m_SharedTableDataOperations;
            }
        }

        /// <summary>
        /// The default table to use when no table collection name is provided.
        /// </summary>
        public TableReference DefaultTable
        {
            get => m_DefaultTableReference;
            set => m_DefaultTableReference = value;
        }

        internal void RegisterTableOperation(AsyncOperationHandle<TTable> handle, LocaleIdentifier localeIdentifier, string tableName)
        {
            TableOperations[(localeIdentifier, tableName)] = handle;

            if (handle.IsDone)
                RegisterSharedTableDataOperation(handle);
            else
                handle.Completed += RegisterSharedTableDataOperation;
        }

        void RegisterSharedTableDataOperation(AsyncOperationHandle<TTable> tableOperation)
        {
            if (tableOperation.Result != null)
            {
                var sharedTableData = tableOperation.Result.SharedData;
                var tableNameGuid = sharedTableData.TableCollectionNameGuid;
                if (!SharedTableDataOperations.ContainsKey(tableNameGuid))
                    SharedTableDataOperations[tableNameGuid] = ResourceManager.CreateCompletedOperation(sharedTableData, null);
            }
        }

        /// <summary>
        /// Returns the named table.
        /// This function is asynchronous and may not have an immediate result available.
        /// Check IsDone to see if the data is available, if it is false then use the Completed event or yield on the operation.
        /// </summary>
        /// <param name="tableReference">The table identifier. Can be either the name of the table or the table collection name Guid.</param>
        /// <param name="locale">The <see cref="Locale"/> to load the table from if you do not wish to use <see cref="LocalizationSettings.SelectedLocale"/>.</param>
        /// <returns></returns>
        public virtual AsyncOperationHandle<TTable> GetTableAsync(TableReference tableReference, Locale locale)
        {
            if (locale == null)
                throw new ArgumentNullException(nameof(locale));

            var initOp = LocalizationSettings.InitializationOperation;
            if (!initOp.IsDone)
                return ResourceManager.CreateChainOperation(initOp, (op) => GetTableLoadTable(tableReference, locale));
            return GetTableLoadTable(tableReference, locale);
        }

        /// <summary>
        /// Returns the named table.
        /// This function is asynchronous and may not have an immediate result available.
        /// Check IsDone to see if the data is available, if it is false then use the Completed event or yield on the operation.
        /// </summary>
        /// <param name="tableReference">The table identifier. Can be either the name of the table or the table collection name Guid.</param>
        /// <returns></returns>
        public AsyncOperationHandle<TTable> GetTableAsync(TableReference tableReference)
        {
            tableReference.Validate();

            // We need to initialize before we can use LocalizationSettings.SelectedLocale.
            var initOp = LocalizationSettings.InitializationOperation;
            if (!initOp.IsDone)
                return ResourceManager.CreateChainOperation(initOp, (op) => GetTableAsync(tableReference, LocalizationSettings.SelectedLocale));
            return GetTableAsync(tableReference, LocalizationSettings.SelectedLocale);
        }

        /// <summary>
        /// Returns the entry from the requested table. A table entry will contain the localized item and metadata.
        /// This function is asynchronous and may not have an immediate result available.
        /// Check IsDone to see if the data is available, if it is false then use the Completed event or yield on the operation.
        /// </summary>
        /// <param name="tableReference">The table identifier. Can be either the name of the table or the table collection name Guid.</param>
        /// <param name="tableEntryReference">A reference to the entry in the table.</param>
        /// <param name="locale">The <see cref="Locale"/> to load the table from if you do not wish to use <see cref="LocalizationSettings.SelectedLocale"/>.</param>
        /// <returns></returns>
        public AsyncOperationHandle<TableEntryResult> GetTableEntryAsync(TableReference tableReference, TableEntryReference tableEntryReference, Locale locale)
        {
            var tableOp = GetTableAsync(tableReference, locale);
            if (!tableOp.IsDone)
                return ResourceManager.CreateChainOperation(tableOp, (op) => GetTableEntryFindEntry(tableOp, tableEntryReference));
            return GetTableEntryFindEntry(tableOp, tableEntryReference);
        }

        /// <summary>
        /// Returns the entry from the requested table. A table entry will contain the localized item and metadata.
        /// This function is asynchronous and may not have an immediate result available.
        /// Check IsDone to see if the data is available, if it is false then use the Completed event or yield on the operation.
        /// </summary>
        /// <param name="tableReference">The table identifier. Can be either the name of the table or the table collection name Guid.</param>
        /// <param name="tableEntryReference">A reference to the entry in the table.</param>
        /// <returns></returns>
        public virtual AsyncOperationHandle<TableEntryResult> GetTableEntryAsync(TableReference tableReference, TableEntryReference tableEntryReference)
        {
            var selectedLocaleOp = LocalizationSettings.SelectedLocaleAsync;
            if (!selectedLocaleOp.IsDone)
                return ResourceManager.CreateChainOperation(selectedLocaleOp, (op) => GetTableEntryAsync(tableReference, tableEntryReference, selectedLocaleOp.Result));
            return GetTableEntryAsync(tableReference, tableEntryReference, selectedLocaleOp.Result);
        }

        AsyncOperationHandle<SharedTableData> GetSharedTableData(Guid tableNameGuid)
        {
            if (SharedTableDataOperations.TryGetValue(tableNameGuid, out var sharedTableDataOp))
                return sharedTableDataOp;

            sharedTableDataOp = Addressables.LoadAssetAsync<SharedTableData>(TableReference.StringFromGuid(tableNameGuid));
            SharedTableDataOperations[tableNameGuid] = sharedTableDataOp;
            return sharedTableDataOp;
        }

        AsyncOperationHandle<TTable> GetTableLoadTable(TableReference tableReference, Locale locale)
        {
            if (tableReference.ReferenceType == TableReference.Type.Guid)
            {
                // We need to load the SharedTableData so we can resolve the name of the table
                var sharedTableDataOperation = GetSharedTableData(tableReference);
                if (sharedTableDataOperation.IsDone)
                    return GetTableLoadTable(sharedTableDataOperation, locale);
                return ResourceManager.CreateChainOperation(sharedTableDataOperation, op => GetTableLoadTable(op, locale));
            }

            if (TableOperations.TryGetValue((locale.Identifier, tableReference), out var asyncOp))
                return asyncOp;

            var tableAddress = AddressHelper.GetTableAddress(tableReference, locale.Identifier);
            asyncOp = Addressables.LoadAssetAsync<TTable>(tableAddress);
            RegisterTableOperation(asyncOp, locale.Identifier, tableReference.TableCollectionName);
            return asyncOp;
        }

        AsyncOperationHandle<TTable> GetTableLoadTable(AsyncOperationHandle<SharedTableData> sharedTableDataOperation, Locale locale)
        {
            if (sharedTableDataOperation.Status != AsyncOperationStatus.Succeeded)
            {
                var error = $"Failed to load SharedTableData: {sharedTableDataOperation.DebugName}";
                Debug.LogError(error);
                if (sharedTableDataOperation.OperationException != null)
                {
                    Debug.LogException(sharedTableDataOperation.OperationException);
                }
                return ResourceManager.CreateCompletedOperation(default(TTable), error);
            }
            return GetTableLoadTable(sharedTableDataOperation.Result.TableCollectionName, locale);
        }

        AsyncOperationHandle<TableEntryResult> GetTableEntryFindEntry(AsyncOperationHandle<TTable> tableOperation, TableEntryReference tableEntryReference)
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
            tableEntryReference.Validate();
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
