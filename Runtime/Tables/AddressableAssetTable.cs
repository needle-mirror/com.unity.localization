using System;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization
{
    [Serializable]
    public class AssetTableItemData
    {
        // We map the key to the asset guid
        public uint key;
        public string guid;

        public AsyncOperationHandle? AsyncOperation { get; set; }
    }

    /// <summary>
    /// Maps asset guid to key for a selected Locale.
    /// </summary>
    public abstract class AddressableAssetTable : LocalizedAssetTable, IPreloadRequired, ISerializationCallbackReceiver
    {
        [SerializeField] List<AssetTableItemData> m_Data = new List<AssetTableItemData>();

        protected AsyncOperationHandle? m_PreloadOperation;

        #if UNITY_EDITOR
        void OnEnable()
        {
            // ScriptableObject properties may persist during runs in the editor, so we reset them here to keep each play consistent.
            m_PreloadOperation = null;
        }
#endif

        /// <summary>
        /// The internal map used to reference assets by key.
        /// </summary>
        public Dictionary<uint, AssetTableItemData> AssetMap { get; set; } = new Dictionary<uint, AssetTableItemData>();

        public abstract AsyncOperationHandle PreloadOperation { get; }

        /// <summary>
        /// Returns the asset guid for a specific key.
        /// </summary>
        /// <param name="assetKey"></param>
        /// <returns>guid or string.Empty if it was not found.</returns>
        public string GetGuidFromKey(uint assetKey)
        {
            if (AssetMap.TryGetValue(assetKey, out var id))
            {
                return id.guid.ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// Maps the asset to the key for this LocaleId.
        /// </summary>
        /// <param name="assetKey">The key to map the asset to.</param>
        /// <param name="assetGuid">The guid of the asset. The asset will also need to be controlled by the Addressables system to be found.</param>
        public virtual void AddAsset(string assetKey, string assetGuid)
        {
            if (Keys == null)
            {
                Debug.LogError("Can not add asset with Key'" + assetKey + "'. The Asset Table does not have a Key Database.");
                return;
            }
            var keyId = Keys.GetId(assetKey, true);
            AddAsset(keyId, assetGuid);
        }

        /// <summary>
        /// Maps the asset to the key for this LocaleId.
        /// </summary>
        /// <param name="assetKeyId">The key Id to map the asset to.</param>
        /// <param name="assetGuid">The guid of the asset. The asset will also need to be controlled by the Addressables system to be found.</param>
        public virtual void AddAsset(uint assetKeyId, string assetGuid)
        {
            if (!AssetMap.TryGetValue(assetKeyId, out var id))
            {
                id = new AssetTableItemData() { key = assetKeyId };
                AssetMap[assetKeyId] = id;
            }

            id.guid = assetGuid;
        }

        public virtual void OnBeforeSerialize()
        {
            m_Data.Clear();
            foreach (var item in AssetMap)
            {
                m_Data.Add(new AssetTableItemData() { key = item.Key, guid = item.Value.guid });
            }
        }

        public virtual void OnAfterDeserialize()
        {
            AssetMap.Clear();
            foreach (var itemData in m_Data)
            {
                AssetMap[itemData.key] = itemData;
            }
        }
    }
}