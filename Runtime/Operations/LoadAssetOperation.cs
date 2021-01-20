using UnityEngine.Localization.Tables;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization
{
    public class LoadAssetOperation<TObject> : AsyncOperationBase<TObject> where TObject : Object
    {
        AsyncOperationHandle<AssetTable> m_LoadTableOperation;
        TableEntryReference m_TableEntryReference;

        public void Init(AsyncOperationHandle<AssetTable> loadTableOperation, TableEntryReference tableEntryReference)
        {
            m_LoadTableOperation = loadTableOperation;
            AddressablesInterface.Acquire(m_LoadTableOperation);

            m_TableEntryReference = tableEntryReference;
        }

        protected override void Execute()
        {
            if (m_LoadTableOperation.Status != AsyncOperationStatus.Succeeded)
            {
                Complete(null, false, "Load Table Operation Failed");
            }
            else
            {
                var entry = m_LoadTableOperation.Result?.GetEntryFromReference(m_TableEntryReference);

                if (entry == null)
                {
                    // Empty entries are treated as null objects
                    Complete(null, true, null);
                }
                else
                {
                    var handle = m_LoadTableOperation.Result.GetAssetAsync<TObject>(entry);
                    if (handle.IsDone)
                        AssetLoaded(handle);
                    else
                        handle.Completed += AssetLoaded;
                }
            }
        }

        void AssetLoaded(AsyncOperationHandle<TObject> handle)
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
                Complete(null, false, "GetAssetAsync failed");
            else
                Complete(handle.Result, true, null);
        }

        protected override void Destroy()
        {
            AddressablesInterface.Release(m_LoadTableOperation);
            GenericPool<LoadAssetOperation<TObject>>.Release(this);
        }
    }
}
