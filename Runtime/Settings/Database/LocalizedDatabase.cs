using System;
using System.Collections.Generic;
using UnityEngine.Localization.Tables;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization.Settings
{
    public enum FallbackBehavior
    {
        UseProjectSettings,
        DontUseFallback,
        UseFallback
    }

    [Flags]
    public enum MissingTranslationBehavior
    {
        ShowMissingTranslationMessage = 1,
        PrintWarning = 2
    }

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
            public TEntry Entry { get; }
            public TTable Table { get; }

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
                    var operation = GenericPool<PreloadDatabaseOperation<TTable, TEntry>>.Get();
                    operation.Init(this);
                    m_PreloadOperationHandle = AddressablesInterface.ResourceManager.StartOperation(operation, default);
                }
                return m_PreloadOperationHandle.Value;
            }
        }

        [SerializeField]
        TableReference m_DefaultTableReference;

        [SerializeField]
        bool m_UseFallback;

        AsyncOperationHandle? m_PreloadOperationHandle;

        static Action<AsyncOperationHandle> s_ReleaseNextFrame;
        internal static Action<AsyncOperationHandle> ReleaseNextFrame
        {
            get
            {
                if (s_ReleaseNextFrame == null)
                {
                    s_ReleaseNextFrame = OperationHandleDeferedRelease.Instance.ReleaseNextFrame;
                }
                return s_ReleaseNextFrame;
            }
        }

        // Used in place of the actual selected locale when it is still being loaded.
        static readonly LocaleIdentifier s_SelectedLocaleId = new LocaleIdentifier("selected locale placeholder");

        // These values should never go null however they currently do due to bug 1193322. As a workaround we check them in a property.
        Dictionary<(LocaleIdentifier, string), AsyncOperationHandle<TTable>> m_TableOperations = new Dictionary<(LocaleIdentifier, string), AsyncOperationHandle<TTable>>();
        Dictionary<Guid, AsyncOperationHandle<SharedTableData>> m_SharedTableDataOperations = new Dictionary<Guid, AsyncOperationHandle<SharedTableData>>();

        internal Dictionary<(LocaleIdentifier localeIdentifier, string tableNameOrGuid), AsyncOperationHandle<TTable>> TableOperations
        {
            get
            {
                if (m_TableOperations == null)
                    m_TableOperations = new Dictionary<(LocaleIdentifier, string), AsyncOperationHandle<TTable>>();
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
        public virtual TableReference DefaultTable
        {
            get => m_DefaultTableReference;
            set => m_DefaultTableReference = value;
        }


        /// <summary>
        /// Should the fallback Locale be used when a translation could not be found?.
        /// </summary>
        public bool UseFallback
        {
            get => m_UseFallback;
            set => m_UseFallback = value;
        }

        internal TableReference GetDefaultTable()
        {
            if (m_DefaultTableReference.ReferenceType == TableReference.Type.Empty)
                throw new Exception($"Trying to get the DefaultTable however the {GetType().Name} DefaulTable value has not been set. This can be configured in the Localization Settings.");

            return m_DefaultTableReference;
        }

        internal void RegisterTableOperation(AsyncOperationHandle<TTable> handle, LocaleIdentifier localeIdentifier, string tableName)
        {
            AddressablesInterface.Acquire(handle);
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
                // Register the shared table data Guid
                var sharedTableData = tableOperation.Result.SharedData;
                var tableNameGuid = sharedTableData.TableCollectionNameGuid;
                if (!SharedTableDataOperations.ContainsKey(tableNameGuid))
                    SharedTableDataOperations[tableNameGuid] = AddressablesInterface.ResourceManager.CreateCompletedOperation(sharedTableData, null);

                // Register the table via the guid
                AddressablesInterface.Acquire(tableOperation);
                TableOperations[(tableOperation.Result.LocaleIdentifier, TableReference.StringFromGuid(tableNameGuid))] = tableOperation;
            }
        }

        /// <summary>
        /// Returns the Default table.
        /// This function is asynchronous and may not have an immediate result available.
        /// Check IsDone to see if the data is available, if it is false then use the Completed event or yield on the operation.
        /// </summary>
        /// <returns></returns>
        public AsyncOperationHandle<TTable> GetDefaultTableAsync()
        {
            return GetTableAsync(GetDefaultTable());
        }

        /// <summary>
        /// Returns the named table.
        /// This function is asynchronous and may not have an immediate result available.
        /// Check IsDone to see if the data is available, if it is false then use the Completed event or yield on the operation.
        /// </summary>
        /// <param name="tableReference">The table identifier. Can be either the name of the table or the table collection name Guid.</param>
        /// <param name="locale">The <see cref="Locale"/> to load the table from, use null to default to cref="LocalizationSettings.SelectedLocale"/>.</param>
        /// <returns></returns>
        public virtual AsyncOperationHandle<TTable> GetTableAsync(TableReference tableReference, Locale locale = null)
        {
            // Extract the Locale Id or use a placeholder if we are using the selected locale and it is not ready yet.
            bool localeAvailable = locale != null || LocalizationSettings.SelectedLocaleAsync.IsDone;
            bool useSelectedLocalePlaceholder = true;
            if (localeAvailable)
            {
                if (locale == null)
                {
                    if (LocalizationSettings.SelectedLocaleAsync.Result == null)
                        return AddressablesInterface.ResourceManager.CreateCompletedOperation<TTable>(null, "SelectedLocale is null");
                    locale = LocalizationSettings.SelectedLocaleAsync.Result;
                }
                useSelectedLocalePlaceholder = false;
            }

            // Do we have a cached operation already running?
            tableReference.Validate();
            var tableIdString = tableReference.ReferenceType == TableReference.Type.Guid ? TableReference.StringFromGuid(tableReference.TableCollectionNameGuid) : tableReference.TableCollectionName;
            var localeId = useSelectedLocalePlaceholder ? s_SelectedLocaleId : locale.Identifier;
            if (TableOperations.TryGetValue((localeId, tableIdString), out var operationHandle))
                return operationHandle;

            // Start a new operation
            var operation = CreateLoadTableOperation();
            operation.Init(this, tableReference, locale);
            var handle = AddressablesInterface.ResourceManager.StartOperation(operation, LocalizationSettings.InitializationOperation);

            // Register this operation for reuse
            TableOperations[(localeId, tableIdString)] = handle;

            // If we are using a placeholder then we need to register the correct locale so that this operation can be reused
            if (useSelectedLocalePlaceholder)
            {
                if (handle.IsDone)
                    operation.RegisterTableOperation(handle);
                else
                    handle.Completed += operation.RegisterTableOperation;
            }

            return handle;
        }

        // <summary>
        /// Attempts to retrieve all the Tables at once
        /// This function is asynchronous and may not have an immediate result.
        /// Check IsDone to see if the data is available, if it is false then use the Completed event or yield on the operation.
        /// </summary>
        /// <param name="tableReference">A reference to the table to check for the string.</param>
        /// <param name="locale">The <see cref="Locale"/> to use instead of the default <see cref="LocalizationSettings.SelectedLocale"/></param>
        /// <returns></returns>
        public AsyncOperationHandle PreLoadTables(TableReference tableReference, Locale locale = null)
        {
            // Start a new operation
            var operation = CreatePreLoadTablesOperation();
            operation.Init(this, new List<TableReference> { tableReference }, locale);
            var handle = AddressablesInterface.ResourceManager.StartOperation(operation, LocalizationSettings.InitializationOperation);
            handle.CompletedTypeless += ReleaseNextFrame;
            return handle;
        }

        /// <summary>
        /// Attempts to retrieve all the Tables at once
        /// This function is asynchronous and may not have an immediate result.
        /// Check IsDone to see if the data is available, if it is false then use the Completed event or yield on the operation.
        /// </summary>
        /// <param name="tableReferences">An IList of tableReferences to check for the string.</param>
        /// <param name="locale">The <see cref="Locale"/> to use instead of the default <see cref="LocalizationSettings.SelectedLocale"/></param>
        /// <returns></returns>
        public AsyncOperationHandle PreLoadTables(IList<TableReference> tableReferences, Locale locale = null)
        {
            // Start a new operation
            var operation = CreatePreLoadTablesOperation();
            operation.Init(this, tableReferences, locale);
            var handle = AddressablesInterface.ResourceManager.StartOperation(operation, LocalizationSettings.InitializationOperation);
            handle.CompletedTypeless += ReleaseNextFrame;
            return handle;
        }

        internal virtual LoadTableOperation<TTable, TEntry> CreateLoadTableOperation() => GenericPool<LoadTableOperation<TTable, TEntry>>.Get();
        internal virtual PreLoadTablesOperation<TTable, TEntry> CreatePreLoadTablesOperation() => GenericPool<PreLoadTablesOperation<TTable, TEntry>>.Get();

        /// <summary>
        /// Returns the entry from the requested table. A table entry will contain the localized item and metadata.
        /// This function is asynchronous and may not have an immediate result available.
        /// Check IsDone to see if the data is available, if it is false then use the Completed event or yield on the operation.
        /// Once the Completed event has been called, during the next update, the internal operation will be returned to a pool so that it can be reused.
        /// If you do plan to keep hold of the handle after completion then you should call <see cref="Addressables.ResourceManager.Acquire(AsyncOperationHandle)"/>
        /// to prevent the operation being reused and <see cref="Addressables.Release(AsyncOperationHandle)"/> to finally return the operation back to the pool.
        /// </summary>
        /// <param name="tableReference">The table identifier. Can be either the name of the table or the table collection name Guid.</param>
        /// <param name="tableEntryReference">A reference to the entry in the table.</param>
        /// <param name="locale">The <see cref="Locale"/> to load the table from. Null will use <see cref="LocalizationSettings.SelectedLocale"/>.</param>
        /// <param name="fallbackBehavior">A Enum which determines if a Fallback should be used when no value could be found for the Locale.</param>
        /// <returns></returns>
        public virtual AsyncOperationHandle<TableEntryResult> GetTableEntryAsync(TableReference tableReference, TableEntryReference tableEntryReference, Locale locale = null, FallbackBehavior fallbackBehavior = FallbackBehavior.UseProjectSettings)
        {
            var loadTableOperation = GetTableAsync(tableReference, locale);
            var getTableEntryOperation = GenericPool<GetTableEntryOperation<TTable, TEntry>>.Get();
            var useFallback = fallbackBehavior != FallbackBehavior.UseProjectSettings ? fallbackBehavior == FallbackBehavior.UseFallback : UseFallback;

            getTableEntryOperation.Init(this, loadTableOperation, tableReference, tableEntryReference, locale, useFallback);
            var handle = AddressablesInterface.ResourceManager.StartOperation(getTableEntryOperation, loadTableOperation);

            // We don't want to force users to have to manage the reference counting so by default we will release the operation for reuse once completed in the next frame
            // If a user wants to hold onto it then they should call Acquire on the operation and later Release.
            handle.CompletedTypeless += ReleaseNextFrame;

            return handle;
        }

        internal AsyncOperationHandle<SharedTableData> GetSharedTableData(Guid tableNameGuid)
        {
            if (SharedTableDataOperations.TryGetValue(tableNameGuid, out var sharedTableDataOp))
                return sharedTableDataOp;

            sharedTableDataOp = AddressablesInterface.LoadAssetFromGUID<SharedTableData>(TableReference.StringFromGuid(tableNameGuid));
            SharedTableDataOperations[tableNameGuid] = sharedTableDataOp;
            return sharedTableDataOp;
        }

        /// <summary>
        /// Called before the LocaleChanged event is sent out in order to give the database a chance to prepare.
        /// </summary>
        public virtual void OnLocaleChanged(Locale locale)
        {
            foreach (var to in TableOperations.Values)
            {
                AddressablesInterface.Release(to);
            }

            if (m_PreloadOperationHandle.HasValue)
            {
                AddressablesInterface.Release(m_PreloadOperationHandle.Value);
                m_PreloadOperationHandle = null;
            }

            TableOperations.Clear();
        }
    }
}
