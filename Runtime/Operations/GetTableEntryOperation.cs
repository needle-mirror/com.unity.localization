using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization
{
    class GetTableEntryOperation<TTable, TEntry> : WaitForCurrentOperationAsyncOperationBase<LocalizedDatabase<TTable, TEntry>.TableEntryResult>
        where TTable : DetailedLocalizationTable<TEntry>
        where TEntry : TableEntry
    {
        AsyncOperationHandle<TTable>? m_LoadTableOperation;
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
            AddressablesInterface.Acquire(m_LoadTableOperation.Value);
            m_TableReference = tableReference;
            m_TableEntryReference = tableEntryReference;
            m_SelectedLocale = selectedLoale;
            m_UseFallback = UseFallBack;
            CurrentOperation = null;
        }

        protected override void Execute()
        {
            var tableHandle = m_LoadTableOperation.Value;
            m_LoadTableOperation = null; // Don't hold on to it because it may change due to entry override, fallback etc. The final result will be assigned here later.

            if (m_SelectedLocale == null)
            {
                m_SelectedLocale = LocalizationSettings.SelectedLocaleAsync.Result;
                if (m_SelectedLocale == null)
                {
                    Complete(default, false, "SelectedLocale is null");
                    AddressablesInterface.SafeRelease(tableHandle);
                    return;
                }
            }

            m_CurrentLocale = m_SelectedLocale;
            ExtractEntryFromTable(tableHandle);
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

            if (HandleEntryOverride(asyncOperation, entry) || HandleFallback(asyncOperation, entry))
                return;

            m_LoadTableOperation = asyncOperation;
            Complete(new LocalizedDatabase<TTable, TEntry>.TableEntryResult(entry, asyncOperation.Result), true, null);
        }

        bool HandleEntryOverride(AsyncOperationHandle<TTable> asyncOperation, TEntry entry)
        {
            // First check for an Entry level override. This applies to Locale only.
            if (entry != null)
            {
                foreach (var md in entry.MetadataEntries)
                {
                    if (md is IEntryOverride entryOverride)
                    {
                        if (ApplyEntryOverride(entryOverride, asyncOperation, entry))
                            return true;
                    }
                }
            }

            // Now check for a shared override. This applies to all Locales for this table entry.
            var sharedEntry = entry?.SharedEntry ?? asyncOperation.Result?.SharedData.GetEntryFromReference(m_TableEntryReference);
            if (sharedEntry != null)
            {
                foreach (var md in sharedEntry.Metadata.MetadataEntries)
                {
                    if (md is IEntryOverride entryOverride)
                    {
                        if (ApplyEntryOverride(entryOverride, asyncOperation, entry))
                            return true;
                    }
                }
            }

            return false;
        }

        bool ApplyEntryOverride(IEntryOverride entryOverride, AsyncOperationHandle<TTable> asyncOperation, TEntry entry)
        {
            if (entryOverride == null)
                return false;

            var overrideType = entryOverride.GetOverride(out var tableReference, out var tableEntry);
            if (overrideType == EntryOverrideType.None)
                return false;

            if (overrideType == EntryOverrideType.Entry)
            {
                // Swap the entry but keep the same table.
                m_TableEntryReference = tableEntry;

                // Start the process again with the new entry
                ExtractEntryFromTable(asyncOperation);
                return true;
            }

            if (overrideType == EntryOverrideType.Table)
            {
                var sharedEntry = entry?.SharedEntry ?? asyncOperation.Result?.SharedData.GetEntryFromReference(m_TableEntryReference);

                // Use the key as the id may not be the same in both tables
                m_TableEntryReference = sharedEntry.Key;
            }
            else if (overrideType == EntryOverrideType.TableAndEntry)
            {
                m_TableEntryReference = tableEntry;
            }

            AddressablesInterface.Release(asyncOperation);
            asyncOperation = m_Database.GetTableAsync(tableReference, m_CurrentLocale);
            AddressablesInterface.Acquire(asyncOperation);

            if (asyncOperation.IsDone)
            {
                ExtractEntryFromTable(asyncOperation);
            }
            else
            {
                CurrentOperation = asyncOperation;
                asyncOperation.Completed += ExtractEntryFromTable;
            }

            return true;
        }

        bool HandleFallback(AsyncOperationHandle<TTable> asyncOperation, TEntry entry)
        {
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
                    {
                        ExtractEntryFromTable(asyncOperation);
                    }
                    else
                    {
                        CurrentOperation = asyncOperation;
                        asyncOperation.Completed += ExtractEntryFromTable;
                    }

                    return true;
                }
            }

            return false;
        }

        protected override void Destroy()
        {
            if (m_LoadTableOperation.HasValue)
            {
                AddressablesInterface.SafeRelease(m_LoadTableOperation.Value);
                m_LoadTableOperation = null;
            }

            base.Destroy();
            GenericPool<GetTableEntryOperation<TTable, TEntry>>.Release(this);
        }

        public override string ToString() => $"{GetType().Name}, Current Locale: {m_CurrentLocale}, Selected Locale: {m_SelectedLocale}, Table: {m_TableReference}, Entry: {m_TableEntryReference}, Fallback: {m_UseFallback}";
    }
}
