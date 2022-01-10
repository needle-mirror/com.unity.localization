using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

class LoadAllTablesOperation<TTable, TEntry> : WaitForCurrentOperationAsyncOperationBase<IList<TTable>>
    where TTable : DetailedLocalizationTable<TEntry>
    where TEntry : TableEntry
{
    AsyncOperationHandle<IList<TTable>>? m_AllTablesOperation;
    Locale m_locale;

    public void Init(Locale locale)
    {
        m_locale = locale;
    }

    protected override void Execute()
    {
        var label = m_locale != null ? AddressHelper.FormatAssetLabel(m_locale.Identifier) : AddressHelper.FormatAssetLabel(LocalizationSettings.SelectedLocaleAsync.Result.Identifier);
        m_AllTablesOperation = AddressablesInterface.LoadAssetsWithLabel<TTable>(label, null);
        m_AllTablesOperation.Value.Completed += LoadingCompleted;
        CurrentOperation = m_AllTablesOperation.Value;
    }

    void LoadingCompleted(AsyncOperationHandle<IList<TTable>> obj)
    {
        Complete(obj.Result, obj.Status == AsyncOperationStatus.Succeeded, obj.OperationException?.Message);
    }

    protected override void Destroy()
    {
        base.Destroy();

        if (m_AllTablesOperation.HasValue)
        {
            AddressablesInterface.SafeRelease(m_AllTablesOperation.Value);
            m_AllTablesOperation = null;
        }
    }
}
