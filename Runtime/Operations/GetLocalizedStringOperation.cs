using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization
{
    class GetLocalizedStringOperation : AsyncOperationBase<string>
    {
        LocalizedStringDatabase m_Database;
        AsyncOperationHandle<LocalizedDatabase<StringTable, StringTableEntry>.TableEntryResult> m_TableEntryOperation;
        TableReference m_TableReference;
        TableEntryReference m_TableEntryReference;
        Locale m_SelectedLocale;
        object[] m_Arguments;

        public void Init(AsyncOperationHandle<LocalizedDatabase<StringTable, StringTableEntry>.TableEntryResult> tableEntryOperation, Locale locale, LocalizedStringDatabase database, TableReference tableReference, TableEntryReference tableEntryReference, object[] arguments)
        {
            m_TableEntryOperation = tableEntryOperation;
            m_SelectedLocale = locale;
            AddressablesInterface.Acquire(m_TableEntryOperation);
            m_Database = database;
            m_TableReference = tableReference;
            m_TableEntryReference = tableEntryReference;
            m_Arguments = arguments;
        }

        protected override void Execute()
        {
            if (m_SelectedLocale == null)
            {
                m_SelectedLocale = LocalizationSettings.SelectedLocaleAsync.Result;
                if (m_SelectedLocale == null)
                {
                    Complete(null, false, "SelectedLocale is null");
                    AddressablesInterface.Release(m_TableEntryOperation);
                    return;
                }
            }

            if (m_TableEntryOperation.Status != AsyncOperationStatus.Succeeded)
            {
                Complete(null, false, "Load Table Operation Failed");
                AddressablesInterface.Release(m_TableEntryOperation);
                return;
            }

            var result = m_Database.GenerateLocalizedString(m_TableEntryOperation.Result.Table, m_TableEntryOperation.Result.Entry, m_TableReference, m_TableEntryReference, m_SelectedLocale, m_Arguments);
            Complete(result, true, null);
            AddressablesInterface.Release(m_TableEntryOperation);
        }

        protected override void Destroy()
        {
            base.Destroy();
            GenericPool<GetLocalizedStringOperation>.Release(this);
        }
    }
}
