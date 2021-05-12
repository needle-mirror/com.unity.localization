using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization
{
    /// <summary>
    /// Extended version of <seealso cref="AsyncOperationBase{TObject}"/> which includes generic <seealso cref="InvokeWaitForCompletion"/> support.
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    abstract class WaitForCurrentOperationAsyncOperationBase<TObject> : AsyncOperationBase<TObject>
    {
        protected AsyncOperationHandle? CurrentOperation { get; set; }

        public AsyncOperationHandle? Dependency { get; set; }

        protected override bool InvokeWaitForCompletion()
        {
            if (!IsRunning)
                return true;

            // If we have dependencies then we need to wait for them.
            if (!HasExecuted && Dependency.HasValue)
            {
                Dependency.Value.WaitForCompletion();
            }

            if (!CurrentOperation.HasValue)
            {
                Complete(default, false, "Expected to have a current operation to wait on. Can not complete.");
                return true;
            }

            CurrentOperation.Value.WaitForCompletion();
            return false;
        }

        protected override void Destroy()
        {
            base.Destroy();
            Dependency = null;
            CurrentOperation = null;

            // HasExecuted does not get reset so we need to do it. https://unity.slack.com/archives/C8Z80RV4K/p1620124726080400
            HasExecuted = false;
        }
    }
}
