using System.Collections.Generic;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization.Operations
{
    class LoadAllTablesOperation<TTable, TEntry> : WaitForCurrentOperationAsyncOperationBase<IList<TTable>>
        where TTable : DetailedLocalizationTable<TEntry>
        where TEntry : TableEntry
    {
        AsyncOperationHandle<IList<TTable>> m_AllTablesOperation;
        LocalizedDatabase<TTable, TEntry> m_Database;
        Locale m_Locale;

        public void Init(LocalizedDatabase<TTable, TEntry> database, Locale locale)
        {
            m_Database = database;
            m_Locale = locale;
        }

        protected override void Execute()
        {
            var label = m_Locale != null ? AddressHelper.FormatAssetLabel(m_Locale.Identifier) : AddressHelper.FormatAssetLabel(LocalizationSettings.SelectedLocaleAsync.Result.Identifier);
            m_AllTablesOperation = AddressablesInterface.LoadAssetsWithLabel<TTable>(label, null);
            m_AllTablesOperation.Completed += LoadingCompleted;
            CurrentOperation = m_AllTablesOperation;
        }

        void LoadingCompleted(AsyncOperationHandle<IList<TTable>> obj)
        {
            // Cache the loading operations so we can release on a per asset basis.
            if (obj.Result != null)
            {
                foreach (var table in obj.Result)
                {
                    if (table == null)
                        continue;

                    var tableOp = m_Database.GetTableAsync(table.TableCollectionName, m_Locale);
                    Debug.Assert(tableOp.IsDone);
                }
            }

            Complete(obj.Result, obj.Status == AsyncOperationStatus.Succeeded, obj.OperationException?.Message);
        }

        protected override void Destroy()
        {
            base.Destroy();

            AddressablesInterface.ReleaseAndReset(ref m_AllTablesOperation);
            m_AllTablesOperation = default;
        }
    }
}
