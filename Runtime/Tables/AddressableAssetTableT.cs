using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;

// TODO: Unload function for restoring/unloading all assets.
namespace UnityEngine.Localization
{
    [Serializable]
    public class AssetTableItemData
    {
        Hash128 m_GuidHash;

        // We map the key to the asset guid
        public string key;
        public string guid;

        public Hash128 GuidHash
        {
            get
            {
                if(!m_GuidHash.isValid)
                    m_GuidHash = Hash128.Parse(guid);
                return m_GuidHash;
            }
            set
            {
                m_GuidHash = value;
                guid = m_GuidHash.ToString();
            }
        }

        public IAsyncOperation AsyncOperation { get; set; }
    }

    /// <summary>
    /// Maps asset guid to key for a selected Locale. 
    /// The asset must also be managed by the Addressables system for it to be loaded at runtime.
    /// </summary>
    /// <typeparam name="TObject">The type of Asset being localized. For example Texture2D, AudioClip, Prefab etc.</typeparam>
    public class AddressableAssetTableT<TObject> : LocalizedAssetTable, IPreloadRequired, ISerializationCallbackReceiver where TObject : Object
    {
        // We map the key to the guid of the asset.
        // TODO: Use Hash128 for key instead of string
        Dictionary<string, AssetTableItemData> m_ItemsMap = new Dictionary<string, AssetTableItemData>();

        [SerializeField] List<AssetTableItemData> m_Data = new List<AssetTableItemData>();

        IAsyncOperation m_PreloadOperation;

        public override Type SupportedAssetType
        {
            get { return typeof(TObject); }
        }

        /// <summary>
        /// TODO: DOC
        /// </summary>
        public IAsyncOperation PreloadOperation
        {
            get
            {
                if (m_PreloadOperation == null)
                {
                    var operations = new List<IAsyncOperation<TObject>>(AssetMap.Count);
                    foreach (var item in AssetMap.Values)
                    {
                        if (item.AsyncOperation == null && item.GuidHash.isValid)
                        {
                            var op = Addressables.LoadAsset<TObject>(item.GuidHash);
                            item.AsyncOperation = op;
                            operations.Add(op);
                        }
                    }

                    if (operations.Count == 0)
                    {
                        m_PreloadOperation = new CompletedOperation<IList<TObject>>().Start("No Assets To Load", null, null);
                    }
                    else
                    {
                        var preloadOp = AsyncOperationCache.Instance.Acquire<GroupIAsyncOperation<TObject>>().Start(operations, null);
                        preloadOp.Retain();
                        m_PreloadOperation = preloadOp;
                    }
                }
                return m_PreloadOperation;
            }
        }

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
        public Dictionary<string, AssetTableItemData> AssetMap
        {
            get { return m_ItemsMap; }
            set { m_ItemsMap = value; }
        }

        /// <summary>
        /// Force the table to load all assets(if they are not already loading or loaded.
        /// </summary>
        public virtual void LoadAllAssets()
        {
            foreach (var item in AssetMap.Values)
            {
                if (item.AsyncOperation == null && item.GuidHash.isValid)
                {
                    item.AsyncOperation = Addressables.LoadAsset<TObject>(item.GuidHash);
                }
            }
        }

        /// <summary>
        /// Synchronous version of GetAssetAsync. The table and its assets must have been preloaded to use this. See See <see cref="LocalizationSettings.PreloadBehavior"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TObject GetAsset(string key)
        {
            Debug.Assert(m_PreloadOperation != null && !m_PreloadOperation.IsDone, "The Synchronous version of GetAsset cannot be used without preloading first.");
            AssetTableItemData id;
            if (m_ItemsMap.TryGetValue(key, out id))
            {
                if (id.AsyncOperation != null)
                {
                    Debug.Assert(id.AsyncOperation.IsDone);
                    return (TObject)id.AsyncOperation.Result;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the loading operation for the asset. 
        /// Check isDone to see if the asset is available for immediate use, if not you can yield on the operation or add a callback subscriber.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IAsyncOperation<TObject> GetAssetAsync(string key)
        {
            AssetTableItemData id;
            if (m_ItemsMap.TryGetValue(key, out id) && id.GuidHash.isValid)
            {
                if (id.AsyncOperation == null)
                {
                    var operation = Addressables.LoadAsset<TObject>(id.GuidHash);
                    operation.Retain();
                    id.AsyncOperation = operation;
                }

                return (IAsyncOperation<TObject>)id.AsyncOperation;
            }
            return null;
        }

        /// <summary>
        /// Returns the asset guid for a specific key.
        /// </summary>
        /// <param name="assetKey"></param>
        /// <returns>guid or string.Empty if it was not found.</returns>
        public string GetGuidFromKey(string assetKey)
        {
            AssetTableItemData id;
            if (m_ItemsMap.TryGetValue(assetKey, out id))
            {
                return id.GuidHash.ToString();
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
            AssetTableItemData id;
            if(!m_ItemsMap.TryGetValue(assetKey, out id))
            {
                id = new AssetTableItemData() { key = assetKey };
                m_ItemsMap[assetKey] = id;
            }

            id.GuidHash = Hash128.Parse(assetGuid);
        }

        /// <inheritdoc/>
        public override void AddKey(string key)
        {
            if (m_ItemsMap.ContainsKey(key))
                return;
            m_ItemsMap[key] = new AssetTableItemData();
        }

        /// <inheritdoc/>
        public override void ReplaceKey(string key, string newKey)
        {
            AssetTableItemData id;
            if (m_ItemsMap.TryGetValue(key, out id))
            {
                m_ItemsMap.Remove(key);
            }
            else
            {
                id = new AssetTableItemData();
            }
            m_ItemsMap[newKey] = id;
        }

        /// <inheritdoc/>
        public override void RemoveKey(string key)
        {
            m_ItemsMap.Remove(key);
        }

        /// <inheritdoc/>
        public override void GetKeys(HashSet<string> keySet)
        {
            foreach (var item in m_ItemsMap)
            {
                keySet.Add(item.Key);
            }
        }

        public virtual void OnBeforeSerialize()
        {
            m_Data.Clear();
            foreach (var item in m_ItemsMap)
            {
                m_Data.Add(new AssetTableItemData() { key = item.Key, guid = item.Value.guid });
            }
        }

        public virtual void OnAfterDeserialize()
        {
            m_ItemsMap.Clear();
            foreach (var itemData in m_Data)
            {
                m_ItemsMap[itemData.key] = itemData;
            }
        }
    }
}