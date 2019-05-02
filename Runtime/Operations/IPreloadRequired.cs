namespace UnityEngine.Localization
{
    public interface IPreloadRequired
    {
        AsyncOperationHandle PreloadOperation { get; }
    }
}