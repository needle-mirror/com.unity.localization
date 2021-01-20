using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace UnityEngine.Localization
{
    class PreLoadTablesOperation<TTable, TEntry> : AsyncOperationBase<LocalizedDatabase<TTable, TEntry>>
        where TTable : DetailedLocalizationTable<TEntry>
        where TEntry : TableEntry
    {
        LocalizedDatabase<TTable, TEntry> m_Database;
        List<AsyncOperationHandle<TTable>> loadTables = new List<AsyncOperationHandle<TTable>>();
        List<AsyncOperationHandle> loadTablesOperation = new List<AsyncOperationHandle>();
        List<AsyncOperationHandle> m_PreloadTablesOperations = new List<AsyncOperationHandle>();

        IList<TableReference> m_TableReferences;
        Locale m_SelectedLocale;
        string m_CollectionName;

        public void Init(LocalizedDatabase<TTable, TEntry> database, IList<TableReference> tableReference, Locale locale = null)
        {
            m_Database = database;
            m_TableReferences = tableReference;
            m_SelectedLocale = locale;
        }

        protected override void Execute()
        {
            BeginPreLoadingTables();
        }

        void BeginPreLoadingTables()
        {
            foreach (var tableReference in m_TableReferences)
            {
                var table = m_Database.GetTableAsync(tableReference, m_SelectedLocale);
                loadTables.Add(table);

                if (!table.IsDone)
                {
                    loadTablesOperation.Add(table);
                }
            }

            if (loadTablesOperation.Count > 0)
            {
                var loadTablesOperationHandle = AddressablesInterface.ResourceManager.CreateGenericGroupOperation(loadTablesOperation);

                if (!loadTablesOperationHandle.IsDone)
                {
                    loadTablesOperationHandle.CompletedTypeless += LoadTableContents;
                    return;
                }
            }

            LoadTableContents();
        }

        void LoadTableContents(AsyncOperationHandle handle)
        {
            LoadTableContents();
        }

        void LoadTableContents()
        {
            // Iterate through the loaded tables, add them to our known tables and preload the actual table contents if required.
            foreach (var table in loadTables)
            {
                if (table is IPreloadRequired preloadRequired)
                {
                    m_PreloadTablesOperations.Add(preloadRequired.PreloadOperation);
                }
            }


            if (m_PreloadTablesOperations.Count == 0)
            {
                Complete(m_Database, true, null);
                return;
            }

            var m_PreloadTablesContents = AddressablesInterface.ResourceManager.CreateGenericGroupOperation(m_PreloadTablesOperations);
            if (!m_PreloadTablesContents.IsDone)
                m_PreloadTablesContents.CompletedTypeless += FinishPreloading;
            else
                FinishPreloading(m_PreloadTablesContents);
        }

        void FinishPreloading(AsyncOperationHandle op)
        {
            Complete(m_Database, op.Status == AsyncOperationStatus.Succeeded, null);
        }

        protected override void Destroy()
        {
            loadTables.Clear();
            loadTablesOperation.Clear();
            m_PreloadTablesOperations.Clear();
            m_TableReferences.Clear();
            GenericPool<PreLoadTablesOperation<TTable, TEntry>>.Release(this);
        }
    }
}
