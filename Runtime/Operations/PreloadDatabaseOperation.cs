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
        LocalizedDatabase<TTable, TEntry> m_Database;
        AsyncOperationHandle<IList<IResourceLocation>> m_LoadResourcesOperation;
        AsyncOperationHandle<IList<TTable>> m_LoadTablesOperation;
        readonly List<AsyncOperationHandle> m_PreloadTableContentsOperations = new List<AsyncOperationHandle>();
        readonly List<string> m_ResourceLabels = new List<string>();
        float m_Progress;

        protected override float Progress => m_Progress;

        protected override string DebugName => $"Preload {m_Database.GetType()}";

        public void Init(LocalizedDatabase<TTable, TEntry> database)
        {
            m_Database = database;
            m_PreloadTableContentsOperations.Clear();
            CurrentOperation = null;
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

            if (!m_LoadResourcesOperation.IsDone)
            {
                CurrentOperation = m_LoadResourcesOperation;
                m_LoadResourcesOperation.Completed += LoadTables;
            }
            else
            {
                LoadTables(m_LoadResourcesOperation);
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
            m_LoadTablesOperation = AddressablesInterface.LoadAssetsFromLocations<TTable>(loadResourcesOperation.Result, TableLoaded);
            if (!m_LoadTablesOperation.IsDone)
            {
                CurrentOperation = m_LoadTablesOperation;
                m_LoadTablesOperation.Completed += LoadTableContents;
            }
            else
            {
                LoadTableContents(m_LoadTablesOperation);
            }
        }

        void TableLoaded(TTable table)
        {
            // We only update the progress here.
            m_Progress += 1.0f / m_LoadResourcesOperation.Result.Count;
        }

        void LoadTableContents(AsyncOperationHandle<IList<TTable>> loadTablesOperation)
        {
            if (loadTablesOperation.Status != AsyncOperationStatus.Succeeded)
            {
                Complete(m_Database, false, "Failed to preload tables.");
                return;
            }

            // Iterate through the loaded tables, add them to our known tables and preload the actual table contents if required.
            foreach (var table in loadTablesOperation.Result)
            {
                if (m_Database.TableOperations.TryGetValue((table.LocaleIdentifier, table.TableCollectionName), out var tableOp))
                {
                    if (tableOp.Result != table)
                        Debug.LogError($"A table with the same key `{table.TableCollectionName}` already exists. Something went wrong during preloading.");
                    continue;
                }

                m_Database.RegisterTableOperation(AddressablesInterface.ResourceManager.CreateCompletedOperation(table, null), table.LocaleIdentifier, table.TableCollectionName);

                if (table is IPreloadRequired preloadRequired)
                {
                    var preloadOperation = preloadRequired.PreloadOperation;
                    if (!preloadOperation.IsDone)
                    {
                        m_PreloadTableContentsOperations.Add(preloadOperation);
                    }
                }
            }

            if (m_PreloadTableContentsOperations.Count == 0)
            {
                Complete(m_Database, true, null);
                return;
            }

            var groupOperation = AddressablesInterface.ResourceManager.CreateGenericGroupOperation(m_PreloadTableContentsOperations);
            if (!groupOperation.IsDone)
            {
                CurrentOperation = groupOperation;
                groupOperation.CompletedTypeless += FinishPreloading;
            }
            else
                FinishPreloading(groupOperation);
        }

        void FinishPreloading(AsyncOperationHandle op)
        {
            AddressablesInterface.SafeRelease(m_LoadResourcesOperation);
            AddressablesInterface.SafeRelease(m_LoadTablesOperation);

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
