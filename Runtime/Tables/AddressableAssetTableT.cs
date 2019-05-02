using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

// TODO: Unload function for restoring/unloading all assets.
namespace UnityEngine.Localization
{
    /// <summary>
    /// Maps asset guid to key for a selected Locale. 
    /// The asset must also be managed by the Addressables system for it to be loaded at runtime.
    /// </summary>
    /// <typeparam name="TObject">The type of Asset being localized. For example Texture2D, AudioClip, Prefab etc.</typeparam>
    public class AddressableAssetTableT<TObject> : AddressableAssetTable where TObject : Object
    {
        public override Type SupportedAssetType => typeof(TObject);

        /// <summary>
        /// TODO: DOC
        /// </summary>
        public override AsyncOperationHandle PreloadOperation
        {
            get
            {
                if (m_PreloadOperation == null)
                {
                    var operations = new List<AsyncOperationHandle<TObject>>(AssetMap.Count);
                    foreach (var item in AssetMap.Values)
                    {
                        if (item.AsyncOperation == null && !string.IsNullOrEmpty(item.guid))
                        {
                            var op = Addressables.LoadAsset<TObject>(item.guid);
                            item.AsyncOperation = op;
                            operations.Add(op);
                        }
                    }

                    if (operations.Count == 0)
                    {
                        m_PreloadOperation = LocalizationSettings.ResourceManager.CreateCompletedOperation<TObject>(null, "No Assets To Load");
                    }
                    else
                    {
                        m_PreloadOperation = LocalizationSettings.ResourceManager.StartOperation(new GroupIAsyncOperation<TObject>(operations), default);
                    }
                }
                return m_PreloadOperation.Value;
            }
        }

        /// <summary>
        /// Force the table to load all assets(if they are not already loading or loaded.
        /// </summary>
        public virtual void LoadAllAssets()
        {
            foreach (var item in AssetMap.Values)
            {
                if (item.AsyncOperation == null && !string.IsNullOrEmpty(item.guid))
                {
                    item.AsyncOperation = Addressables.LoadAsset<TObject>(item.guid);
                }
            }
        }

        /// <summary>
        /// Returns the loading operation for the asset. 
        /// Check isDone to see if the asset is available for immediate use, if not you can yield on the operation or add a callback subscriber.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public AsyncOperationHandle<TObject> GetAssetAsync(uint key)
        {
            if (AssetMap.TryGetValue(key, out var id))
            {
                if (id.AsyncOperation == null)
                {
                    id.AsyncOperation = Addressables.LoadAsset<TObject>(id.guid);
                }

                return id.AsyncOperation.Value.Convert<TObject>();
            }
            return LocalizationSettings.ResourceManager.CreateCompletedOperation<TObject>(null, "Could not find asset with key:" + key);
        }
    }
}