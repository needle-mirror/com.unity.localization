using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization
{
    /// <summary>
    /// Indicates that the class requires preloading during initialization before it is ready for use.
    /// </summary>
    public interface IPreloadRequired
    {
        AsyncOperationHandle PreloadOperation { get; }
    }
}
