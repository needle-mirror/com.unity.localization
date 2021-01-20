using System.Collections.Generic;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace UnityEngine.Localization
{
    class LoadTableOperation<TTable, TEntry> : AsyncOperationBase<TTable>
        where TTable : DetailedLocalizationTable<TEntry>
        where TEntry : TableEntry
    {
        LocalizedDatabase<TTable, TEntry> m_Database;
        TableReference m_TableReference;
        Locale m_SelectedLocale;
        string m_CollectionName;

        public void Init(LocalizedDatabase<TTable, TEntry> database, TableReference tableReference, Locale locale)
        {
            m_Database = database;
            m_TableReference = tableReference;
            m_SelectedLocale = locale;
        }

        protected override void Execute()
        {
            if (m_SelectedLocale == null)
            {
                m_SelectedLocale = LocalizationSettings.SelectedLocale;
                if (m_SelectedLocale == null)
                    Complete(null, false, "SelectedLocale is null");
            }

            // Extract the collection name
            if (m_TableReference.ReferenceType == TableReference.Type.Guid)
            {
                // We need to load the SharedTableData so we can resolve the name of the table
                var operation = m_Database.GetSharedTableData(m_TableReference.TableCollectionNameGuid);
                if (operation.IsDone)
                    LoadTableByGuid(operation);
                else
                    operation.Completed += LoadTableByGuid;
            }
            else
            {
                FindTableByName(m_TableReference.TableCollectionName);
            }
        }

        void LoadTableByGuid(AsyncOperationHandle<SharedTableData> operationHandle)
        {
            if (operationHandle.Status == AsyncOperationStatus.Succeeded)
            {
                FindTableByName(operationHandle.Result.TableCollectionName);
            }
            else
            {
                Complete(null, false, $"Failed to extract table name from shared table data {m_TableReference}. Load Shared Table data operation failed.");
            }
        }

        void FindTableByName(string collectionName)
        {
            m_CollectionName = collectionName;
            var tableAddress = AddressHelper.GetTableAddress(m_CollectionName, m_SelectedLocale.Identifier);

            // Check the table exists
            var tableResourceOp = AddressablesInterface.LoadResourceLocationsAsync(tableAddress, typeof(TTable));
            if (!tableResourceOp.IsDone)
                tableResourceOp.Completed += LoadTableResource;
            else
                LoadTableResource(tableResourceOp);
        }

        void LoadTableResource(AsyncOperationHandle<IList<IResourceLocation>> operationHandle)
        {
            // TODO: Fallback if table is not found
            if (operationHandle.Status != AsyncOperationStatus.Succeeded || operationHandle.Result.Count == 0)
            {
                Complete(null, true, $"Could not find a {m_SelectedLocale} table with the name '{m_CollectionName}`");
                return;
            }

            var loadTableOperation = AddressablesInterface.LoadAssetFromLocation<TTable>(operationHandle.Result[0]);
            if (loadTableOperation.IsDone)
                TableLoaded(loadTableOperation);
            else
                loadTableOperation.Completed += TableLoaded;
        }

        void TableLoaded(AsyncOperationHandle<TTable> operationHandle)
        {
            Complete(operationHandle.Result, operationHandle.Status == AsyncOperationStatus.Succeeded, null);
        }

        public void RegisterTableOperation(AsyncOperationHandle<TTable> handle)
        {
            m_Database.RegisterTableOperation(handle, m_SelectedLocale.Identifier, m_CollectionName);
        }

        protected override void Destroy()
        {
            GenericPool<LoadTableOperation<TTable, TEntry>>.Release(this);
        }
    }
}
