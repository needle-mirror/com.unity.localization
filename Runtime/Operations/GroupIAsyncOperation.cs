using System;
using System.Collections.Generic;
using UnityEngine.ResourceManagement;

namespace UnityEngine.Localization
{
    // TODO: Addressables will have support for this in the future so we can remove this class then.
    public class GroupIAsyncOperation<TObject> : AsyncOperationBase<IList<TObject>> where TObject : class
    {
        Action<IAsyncOperation<TObject>> m_Callback;
        Action<IAsyncOperation<TObject>> m_InternalOnComplete;
        IList<IAsyncOperation<TObject>> m_Operations;
        int m_LoadedCount;

        public GroupIAsyncOperation()
        {
            m_InternalOnComplete = OnOperationCompleted;
            m_result = new List<TObject>();
        }

        /// <inheritdoc />
        public override bool IsDone
        {
            get
            {
                Validate();
                return Result.Count == m_LoadedCount;
            }
        }

        /// <inheritdoc />
        public override void SetResult(IList<TObject> result)
        {
            Validate();
        }

        /// <inheritdoc />
        public override void ResetStatus()
        {
            m_releaseToCacheOnCompletion = true;
            m_status = AsyncOperationStatus.None;
            m_error = null;
            m_context = null;

            Result.Clear();
            m_Operations = null;
            m_Callback = null;
        }

        /// <inheritdoc />
        public override object Key
        {
            get
            {
                Validate();
                return m_key;
            }
            set
            {
                Validate();
                m_key = value;
                if (m_Operations != null)
                {
                    foreach (var op in m_Operations)
                        op.Key = Key;
                }
            }
        }

        public virtual IAsyncOperation<IList<TObject>> Start(IList<IAsyncOperation<TObject>> operations, Action<IAsyncOperation<TObject>> callback)
        {
            m_Operations = operations;
            m_Callback = callback;

            foreach(var op in operations)
            {
                op.Completed += m_InternalOnComplete;
                Result.Add(default(TObject));
            }
            return this;
        }

        void OnOperationCompleted(IAsyncOperation<TObject> op)
        {
            if (m_Callback != null)
                m_Callback(op);
            m_LoadedCount++;
            for (int i = 0; i < m_Operations.Count; i++)
            {
                if (m_Operations[i] == op)
                {
                    Result[i] = op.Result;
                    if (op.Status != AsyncOperationStatus.Succeeded)
                    {
                        Status = op.Status;
                        m_error = op.OperationException;
                    }
                    break;
                }
            }

            op.Release();
            if (IsDone)
            {
                if (Status != AsyncOperationStatus.Failed)
                    Status = AsyncOperationStatus.Succeeded;
                InvokeCompletionEvent();
            }
        }
    }
}