using System;
using System.Collections.Generic;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace UnityEngine.Localization
{
    class LoadTableOperation<TTable, TEntry> : WaitForCurrentOperationAsyncOperationBase<TTable>
        where TTable : DetailedLocalizationTable<TEntry>
        where TEntry : TableEntry
    {
        readonly Action<AsyncOperationHandle<SharedTableData>> m_LoadTableByGuidAction;
        readonly Action<AsyncOperationHandle<IList<IResourceLocation>>> m_LoadTableResourceAction;
        readonly Action<AsyncOperationHandle<TTable>> m_TableLoadedAction;

        LocalizedDatabase<TTable, TEntry> m_Database;
        TableReference m_TableReference;
        AsyncOperationHandle<TTable> m_LoadTableOperation;
        Locale m_SelectedLocale;
        string m_CollectionName;

        public Action<AsyncOperationHandle<TTable>> RegisterTableOperation { get; private set; }

        public LoadTableOperation()
        {
            RegisterTableOperation = RegisterTableOperationInternal;
            m_LoadTableByGuidAction = LoadTableByGuid;
            m_LoadTableResourceAction = LoadTableResource;
            m_TableLoadedAction = TableLoaded;
        }

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
                {
                    Complete(null, false, "SelectedLocale is null. Could not load table.");
                    return;
                }
            }

            // Extract the collection name
            if (m_TableReference.ReferenceType == TableReference.Type.Guid)
            {
                // We need to load the SharedTableData so we can resolve the name of the table
                var operation = m_Database.GetSharedTableData(m_TableReference.TableCollectionNameGuid);
                if (operation.IsDone)
                {
                    LoadTableByGuid(operation);
                }
                else
                {
                    CurrentOperation = operation;
                    operation.Completed += m_LoadTableByGuidAction;
                }
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

            // Check the table exists
            var tableResourceOp = AddressablesInterface.LoadTableLocationsAsync(m_CollectionName, m_SelectedLocale.Identifier, typeof(TTable));
            AddressablesInterface.Acquire(tableResourceOp);
            if (tableResourceOp.IsDone)
            {
                LoadTableResource(tableResourceOp);
            }
            else
            {
                CurrentOperation = tableResourceOp;
                tableResourceOp.Completed += m_LoadTableResourceAction;
            }
        }

        void LoadTableResource(AsyncOperationHandle<IList<IResourceLocation>> operationHandle)
        {
            if (operationHandle.Status != AsyncOperationStatus.Succeeded || operationHandle.Result.Count == 0)
            {
                AddressablesInterface.Release(operationHandle);
                Complete(null, true, $"Could not find a {m_SelectedLocale} table with the name '{m_CollectionName}`");
                return;
            }

            m_LoadTableOperation = AddressablesInterface.LoadTableFromLocation<TTable>(operationHandle.Result[0]);
            if (m_LoadTableOperation.IsDone)
            {
                TableLoaded(m_LoadTableOperation);
            }
            else
            {
                CurrentOperation = m_LoadTableOperation;
                m_LoadTableOperation.Completed += m_TableLoadedAction;
            }

            AddressablesInterface.Release(operationHandle);
        }

        void TableLoaded(AsyncOperationHandle<TTable> operationHandle)
        {
            Complete(operationHandle.Result, operationHandle.Status == AsyncOperationStatus.Succeeded, null);
        }

        void RegisterTableOperationInternal(AsyncOperationHandle<TTable> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
                m_Database.RegisterTableOperation(handle, m_SelectedLocale.Identifier, m_CollectionName);
        }

        protected override void Destroy()
        {
            base.Destroy();

            AddressablesInterface.SafeRelease(m_LoadTableOperation);
            m_LoadTableOperation = default;

            GenericPool<LoadTableOperation<TTable, TEntry>>.Release(this);
        }
    }
}
