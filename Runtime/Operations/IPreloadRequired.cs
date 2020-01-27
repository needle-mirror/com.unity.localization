using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization
{
    interface IPreloadRequired
    {
        AsyncOperationHandle PreloadOperation { get; }
    }
}
