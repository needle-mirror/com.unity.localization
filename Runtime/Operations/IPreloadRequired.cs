using UnityEngine.ResourceManagement;

namespace UnityEngine.Localization
{
    public interface IPreloadRequired
    {
        IAsyncOperation PreloadOperation { get; }
    }
}