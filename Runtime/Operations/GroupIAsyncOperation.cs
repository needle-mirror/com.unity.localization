using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization
{
    class GroupIAsyncOperation : AsyncOperationBase<IList<AsyncOperationHandle>>
    {
        IList<AsyncOperationHandle> m_Operations;
        List<AsyncOperationHandle> m_Results = new List<AsyncOperationHandle>();
        int m_RemainingLoadingObjects;
        int m_TotalLoadingObjects;
        string m_Error;
        float m_Progress;

        protected override float Progress => m_Progress;

        protected override string DebugName => nameof(GroupIAsyncOperation);

        public void Init(IList<AsyncOperationHandle> operations)
        {
            m_Operations = operations;
            m_Results.Clear();
            m_RemainingLoadingObjects = 0;
            m_TotalLoadingObjects = 0;
            m_Error = null;
        }

        protected override void Execute()
        {
            foreach (var asyncOperationHandle in m_Operations)
            {
                if (!asyncOperationHandle.IsDone)
                {
                    m_RemainingLoadingObjects++;
                    asyncOperationHandle.Completed += OnOperationCompleted;
                }
            }

            m_TotalLoadingObjects = m_RemainingLoadingObjects;
        }

        void OnOperationCompleted(AsyncOperationHandle asyncOperation)
        {
            if (asyncOperation.Status != AsyncOperationStatus.Succeeded)
            {
                m_Error += "Failed to load table: " + asyncOperation.DebugName + "\n";
                if (asyncOperation.OperationException != null)
                    m_Error += asyncOperation.OperationException + "\n";
            }
            else
            {
                m_Results.Add(asyncOperation);
            }

            m_RemainingLoadingObjects--;
            m_Progress = 1.0f - ((float)m_RemainingLoadingObjects / m_TotalLoadingObjects);

            if (m_RemainingLoadingObjects == 0)
                Complete(m_Results, string.IsNullOrEmpty(m_Error), m_Error);
        }
    }
}
