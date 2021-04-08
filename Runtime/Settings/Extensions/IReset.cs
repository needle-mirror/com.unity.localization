namespace UnityEngine.Localization.Settings
{
    /// <summary>
    /// Provides a reset callback which can be used to reset the internal state when exiting edit and play mode.
    /// </summary>
    public interface IReset
    {
        void ResetState();
    }
}
