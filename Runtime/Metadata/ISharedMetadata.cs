namespace UnityEngine.Localization.Metadata
{
    [HideInInspector]
    public interface ISharedMetadata : IMetadata
    {
        /// <summary>
        /// Is the entry using this shared metadata?
        /// </summary>
        /// <param name="keyId"></param>
        /// <returns></returns>
        bool Contains(uint keyId);

        /// <summary>
        /// Mark the entry as using this shared metadata.
        /// </summary>
        /// <param name="keyId"></param>
        void AddEntry(uint keyId);

        /// <summary>
        /// Remove the entry from the shared metadata.
        /// </summary>
        /// <param name="keyId"></param>
        void RemoveEntry(uint keyId);
    }
}
