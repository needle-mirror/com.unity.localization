using NUnit.Framework;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization.Tests
{
    public static class AsyncOperationHelper
    {
        public static void CompletedWithoutErrors(AsyncOperationHandle handle, string message = "")
        {
            Assert.True(handle.IsDone, $"Expected operation to be done but but it was not. {message}\n{handle.OperationException}");
            Assert.AreEqual(AsyncOperationStatus.Succeeded, handle.Status, $"Expected operation to be completed with no errors. {message}\n{handle.OperationException}");
        }
    }
}
