using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization
{
    public class GetTableEntryOperation<TTable, TEntry> : AsyncOperationBase<LocalizedDatabase<TTable, TEntry>.TableEntryResult>
        where TTable : DetailedLocalizationTable<TEntry>
        where TEntry : TableEntry
    {
        AsyncOperationHandle<TTable> m_LoadTableOperation;
        TableReference m_TableReference;
        TableEntryReference m_TableEntryReference;
        LocalizedDatabase<TTable, TEntry> m_Database;
        Locale m_SelectedLocale;
        Locale m_CurrentLocale;
        bool m_UseFallback;

        public void Init(LocalizedDatabase<TTable, TEntry> database, AsyncOperationHandle<TTable> loadTableOperation, TableReference tableReference, TableEntryReference tableEntryReference, Locale selectedLoale, bool UseFallBack)
        {
            m_Database = database;
            m_LoadTableOperation = loadTableOperation;
            AddressablesInterface.Acquire(m_LoadTableOperation);
            m_TableReference = tableReference;
            m_TableEntryReference = tableEntryReference;
            m_SelectedLocale = selectedLoale;
            m_UseFallback = UseFallBack;
        }

        protected override void Execute()
        {
            if (m_SelectedLocale == null)
            {
                m_SelectedLocale = LocalizationSettings.SelectedLocaleAsync.Result;
                if (m_SelectedLocale == null)
                {
                    Complete(default, false, "SelectedLocale is null");
                    AddressablesInterface.Release(m_LoadTableOperation);
                    return;
                }
            }

            m_CurrentLocale = m_SelectedLocale;
            ExtractEntryFromTable(m_LoadTableOperation);
        }

        void ExtractEntryFromTable(AsyncOperationHandle<TTable> asyncOperation)
        {
            if (asyncOperation.Status != AsyncOperationStatus.Succeeded)
            {
                Complete(default, false, "Load Table Operation Failed");
                AddressablesInterface.Release(asyncOperation);
                return;
            }

            var entry = asyncOperation.Result?.GetEntryFromReference(m_TableEntryReference);
            if ((entry == null || string.IsNullOrEmpty(entry.Data.Localized)) && m_UseFallback)
            {
                var fallbackLocale = m_CurrentLocale.GetFallback();
                if (fallbackLocale != null)
                {
                    m_CurrentLocale = fallbackLocale;
                    AddressablesInterface.Release(asyncOperation);

                    asyncOperation = m_Database.GetTableAsync(m_TableReference, m_CurrentLocale);
                    AddressablesInterface.Acquire(asyncOperation);

                    //resetting the local 'm_UseFallback' variable to prevent an infinite loop of fallback entry load and also cyclic fallback Locale load.
                    m_UseFallback = false;

                    if (asyncOperation.IsDone)
                        ExtractEntryFromTable(asyncOperation);
                    else
                        asyncOperation.Completed += ExtractEntryFromTable;

                    return;
                }
            }

            Complete(new LocalizedDatabase<TTable, TEntry>.TableEntryResult(entry, asyncOperation.Result), true, null);
            AddressablesInterface.Release(asyncOperation);
        }

        protected override void Destroy()
        {
            GenericPool<GetTableEntryOperation<TTable, TEntry>>.Release(this);
            base.Destroy();
        }
    }
}
