using UnityEngine.ResourceManagement;

namespace UnityEngine.Localization
{
    public static class AsyncOperationMethods
    {
        /// <summary>
        /// Checks the operation had no errors and was successful. Returns true if its OK to use else false.
        /// </summary>
        /// <param name="logErrors">Send any error messages to Debug.Log and Debug.LogException.</param>
        /// <returns></returns>
        public static bool HasLoadedSuccessfully(this IAsyncOperation operation, bool logErrors = true)
        {
            if (operation.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("Failed to load asset: " + operation.Context);
                if (operation.OperationException != null)
                    Debug.LogException(operation.OperationException);
                return false;
            }
            return true;
        }
    }
}