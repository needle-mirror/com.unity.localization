using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace UnityEngine.Localization
{
    class PreloadDatabaseOperation<TTable, TEntry> : AsyncOperationBase<LocalizedDatabase<TTable, TEntry>>
        where TTable : DetailedLocalizationTable<TEntry>
        where TEntry : TableEntry
    {
        LocalizedDatabase<TTable, TEntry> m_Database;
        AsyncOperationHandle<IList<IResourceLocation>> m_LoadResourcesOperation;
        AsyncOperationHandle<IList<TTable>> m_LoadTablesOperation;

        GroupIAsyncOperation m_PreloadTableContentsGroupOperation;
        List<AsyncOperationHandle> m_PreloadTableContentsOperations = new List<AsyncOperationHandle>();
        float m_Progress;

        protected override float Progress => m_Progress;

        protected override string DebugName => $"Preload {m_Database.GetType()}";

        public void Init(LocalizedDatabase<TTable, TEntry> database)
        {
            m_Database = database;
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
                Debug.LogError("Can not preload when `LocalizationSettings.SelectedLocale` is null.");
                return;
            }

            m_Progress = 0;
            var localeLabel = AddressHelper.FormatAssetLabel(selectedLocale.Identifier);
            m_LoadResourcesOperation = Addressables.LoadResourceLocationsAsync(new object[] { localeLabel, LocalizationSettings.PreloadLabel }, Addressables.MergeMode.Intersection, typeof(TTable));

            if (!m_LoadResourcesOperation.IsDone)
                m_LoadResourcesOperation.Completed += LoadTables;
            else
                LoadTables(m_LoadResourcesOperation);
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
            m_LoadTablesOperation = Addressables.LoadAssetsAsync<TTable>(loadResourcesOperation.Result, TableLoaded);
            if (!m_LoadTablesOperation.IsDone)
                m_LoadTablesOperation.Completed += LoadTableContents;
            else
                LoadTableContents(m_LoadTablesOperation);
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
                Debug.Assert(!m_Database.TableOperations.ContainsKey((table.LocaleIdentifier, table.TableCollectionName)), $"A table with the same key `{table.TableCollectionName}` already exists. Something went wrong during preloading.");
                m_Database.RegisterTableOperation(LocalizationSettings.ResourceManager.CreateCompletedOperation(table, null), table.LocaleIdentifier, table.TableCollectionName);

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

            m_PreloadTableContentsGroupOperation = m_PreloadTableContentsGroupOperation ?? (GroupIAsyncOperation)LocalizationSettings.ResourceManager.Allocator.New(typeof(GroupIAsyncOperation), typeof(GroupIAsyncOperation).GetHashCode());
            m_PreloadTableContentsGroupOperation.Init(m_PreloadTableContentsOperations);
            var groupOperation = LocalizationSettings.ResourceManager.StartOperation(m_PreloadTableContentsGroupOperation, default);

            if (!groupOperation.IsDone)
                groupOperation.CompletedTypeless += FinishPreloading;
            FinishPreloading(groupOperation);
        }

        void FinishPreloading(AsyncOperationHandle op)
        {
            m_Progress = 1;
            Complete(m_Database, op.Status == AsyncOperationStatus.Succeeded, null);
        }
    }
}
