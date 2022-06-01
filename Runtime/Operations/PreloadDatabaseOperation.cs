using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace UnityEngine.Localization
{
    class PreloadDatabaseOperation<TTable, TEntry> : WaitForCurrentOperationAsyncOperationBase<LocalizedDatabase<TTable, TEntry>>
        where TTable : DetailedLocalizationTable<TEntry>
        where TEntry : TableEntry
    {
        readonly Action<AsyncOperationHandle<IList<IResourceLocation>>> m_LoadTablesAction;
        readonly Action<AsyncOperationHandle<TTable>> m_LoadTableContentsAction;
        readonly Action<AsyncOperationHandle> m_FinishPreloadingAction;
        readonly Action<AsyncOperationHandle<IList<AsyncOperationHandle>>> m_PreloadTablesCompletedAction;

        LocalizedDatabase<TTable, TEntry> m_Database;
        AsyncOperationHandle<IList<IResourceLocation>> m_LoadResourcesOperation;
        readonly List<AsyncOperationHandle> m_LoadTablesOperations = new List<AsyncOperationHandle>();
        readonly List<AsyncOperationHandle> m_PreloadTableContentsOperations = new List<AsyncOperationHandle>();
        readonly List<string> m_ResourceLabels = new List<string>();
        float m_Progress;

        protected override float Progress => m_Progress;

        protected override string DebugName => $"Preload {m_Database.GetType()}";

        public PreloadDatabaseOperation()
        {
            m_LoadTablesAction = LoadTables;
            m_LoadTableContentsAction = LoadTableContents;
            m_FinishPreloadingAction = FinishPreloading;
            m_PreloadTablesCompletedAction = PreloadTablesCompleted;
        }

        public void Init(LocalizedDatabase<TTable, TEntry> database)
        {
            m_Database = database;
            m_LoadTablesOperations.Clear();
            m_PreloadTableContentsOperations.Clear();
        }

        /// <summary>
        /// Preloads a LocalizedDatabase.
        /// The following steps are performed for a full preload:
        ///     1 - BeginPreloading: Load preload resources - these are the tables that are marked with the preload tag.
        ///     2 - LoadTables: Load the tables using the resources, if any exist
        ///     3 - LoadTableContents: Preload the tables contents, if a table implements IPreloadRequired
        /// </summary>
        protected override void Execute()
        {
            BeginPreloading();
        }

        /// <summary>
        /// We need to check if there are any resources to preload. If we call Load when there is nothing to load an error will be thrown.
        /// </summary>
        /// <returns></returns>
        void BeginPreloading()
        {
            var selectedLocale = LocalizationSettings.SelectedLocale;
            if (selectedLocale == null)
            {
                Complete(m_Database, true, null);
                return;
            }

            m_Progress = 0;
            var localeLabel = AddressHelper.FormatAssetLabel(selectedLocale.Identifier);
            m_ResourceLabels.Clear();
            m_ResourceLabels.Add(localeLabel);
            m_ResourceLabels.Add(LocalizationSettings.PreloadLabel);
            m_LoadResourcesOperation = AddressablesInterface.LoadResourceLocationsWithLabelsAsync(m_ResourceLabels, Addressables.MergeMode.Intersection, typeof(TTable));

            if (m_LoadResourcesOperation.IsDone)
            {
                LoadTables(m_LoadResourcesOperation);
            }
            else
            {
                CurrentOperation = m_LoadResourcesOperation;
                m_LoadResourcesOperation.Completed += m_LoadTablesAction;
            }
        }

        void LoadTables(AsyncOperationHandle<IList<IResourceLocation>> loadResourcesOperation)
        {
            if (loadResourcesOperation.Status != AsyncOperationStatus.Succeeded)
            {
                Complete(m_Database, false, "Failed to locate preload tables.");
                return;
            }

            // Do we need to preload any tables?
            if (loadResourcesOperation.Result.Count == 0)
            {
                m_Progress = 1;
                Complete(m_Database, true, null);
                return;
            }

            // Load the tables
            foreach (var resourceLocation in loadResourcesOperation.Result)
            {
                var tableOperation = AddressablesInterface.LoadTableFromLocation<TTable>(resourceLocation);
                m_LoadTablesOperations.Add(tableOperation);

                if (tableOperation.IsDone)
                    LoadTableContents(tableOperation);
                else
                    tableOperation.Completed += m_LoadTableContentsAction;
            }

            var loadTablesOperation = Addressables.ResourceManager.CreateGenericGroupOperation(m_LoadTablesOperations, true);
            if (loadTablesOperation.IsDone)
            {
                PreloadTablesCompleted(loadTablesOperation);
            }
            else
            {
                CurrentOperation = loadTablesOperation;
                loadTablesOperation.Completed += m_PreloadTablesCompletedAction;
            }
        }

        void LoadTableContents(AsyncOperationHandle<TTable> operation)
        {
            // Update progress.
            m_Progress += 1.0f / m_LoadTablesOperations.Count;

            if (operation.Result == null)
                return;

            var table = operation.Result;
            var tableCollectionName = table.TableCollectionName;

            if (m_Database.TableOperations.TryGetValue((table.LocaleIdentifier, tableCollectionName), out var tableOp))
            {
                // Remove the extra reference
                AddressablesInterface.Release(operation);

                // If the operation is still loading then we can leave it to continue, no need to register this operation.
                if (tableOp.IsDone && !ReferenceEquals(tableOp.Result, table))
                {
                    Debug.LogError($"A table with the same key `{tableCollectionName}` already exists. Something went wrong during preloading. Table {table} does not match {tableOp.Result}.");
                    return;
                }
            }
            else
            {
                m_Database.RegisterTableOperation(operation, table.LocaleIdentifier, tableCollectionName);
            }

            if (table is IPreloadRequired preloadRequired)
            {
                var preloadOperation = preloadRequired.PreloadOperation;
                if (!preloadOperation.IsDone)
                {
                    m_PreloadTableContentsOperations.Add(preloadOperation);
                }
            }
        }

        void PreloadTablesCompleted(AsyncOperationHandle<IList<AsyncOperationHandle>> obj)
        {
            AddressablesInterface.SafeRelease(m_LoadResourcesOperation);

            if (m_PreloadTableContentsOperations.Count == 0)
            {
                Complete(m_Database, true, null);
                return;
            }

            var groupOperation = AddressablesInterface.ResourceManager.CreateGenericGroupOperation(m_PreloadTableContentsOperations);
            if (groupOperation.IsDone)
            {
                FinishPreloading(groupOperation);
            }
            else
            {
                CurrentOperation = groupOperation;
                groupOperation.CompletedTypeless += m_FinishPreloadingAction;
            }
        }

        void FinishPreloading(AsyncOperationHandle op)
        {
            m_Progress = 1;
            Complete(m_Database, op.Status == AsyncOperationStatus.Succeeded, null);
        }

        protected override void Destroy()
        {
            base.Destroy();
            GenericPool<PreloadDatabaseOperation<TTable, TEntry>>.Release(this);
        }
    }
}
