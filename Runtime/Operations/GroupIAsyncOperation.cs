using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization
{
    class GroupIAsyncOperation<TObject> : AsyncOperationBase<IList<TObject>> where TObject : class
    {
        IList<AsyncOperationHandle<TObject>> m_Operations;
        List<TObject> m_Results = new List<TObject>();
        int m_LoadingObjects;
        string m_Error;

        public GroupIAsyncOperation(IList<AsyncOperationHandle<TObject>> operations)
        {
            m_Operations = operations;
        }

        protected override void Execute()
        {
            m_LoadingObjects = 0;
            foreach (var asyncOperationHandle in m_Operations)
            {
                if (!asyncOperationHandle.IsDone)
                {
                    m_LoadingObjects++;
                    asyncOperationHandle.Completed += OnOperationCompleted;
                }
            }
        }

        void OnOperationCompleted(AsyncOperationHandle<TObject> asyncOperation)
        {
            if (asyncOperation.Status != AsyncOperationStatus.Succeeded)
            {
                m_Error += "Failed to load table: " + asyncOperation.DebugName + "\n";
                if (asyncOperation.OperationException != null)
                    m_Error += asyncOperation.OperationException + "\n";
            }
            else
            {
                m_Results.Add(asyncOperation.Result);
            }

            m_LoadingObjects--;
            
            if (m_LoadingObjects == 0)
                Complete(m_Results, string.IsNullOrEmpty(m_Error), m_Error);
        }
    }
}