using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace UnityEngine.Localization
{
    /// <summary>
    /// Allows for intercepting calls to Addressables.
    /// Used for testing and so we can provide Edit mode support without going through Addressables.
    /// </summary>
    internal class AddressablesInterface
    {
        static AddressablesInterface s_Instance;

        public static AddressablesInterface Instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = new AddressablesInterface();
                return s_Instance;
            }
            set => s_Instance = value;
        }

        public static ResourceManager ResourceManager => Addressables.ResourceManager;
        public static void Acquire(AsyncOperationHandle handle) => Addressables.ResourceManager.Acquire(handle);
        public static void Release(AsyncOperationHandle handle) => Addressables.Release(handle);

        public static AsyncOperationHandle<IList<IResourceLocation>> LoadResourceLocationsAsync(IEnumerable keys, Addressables.MergeMode mode, Type type = null) => Instance.LoadResourceLocationsAsyncInternal(keys, mode, type);
        public static AsyncOperationHandle<IList<IResourceLocation>> LoadResourceLocationsAsync(object key, Type type = null) => Instance.LoadResourceLocationsAsyncInternal(key, type);
        public static AsyncOperationHandle<IList<TObject>> LoadAssetsFromLocations<TObject>(IList<IResourceLocation> locations, Action<TObject> callback) => Instance.LoadAssetsFromLocationsInternal(locations, callback);
        public static AsyncOperationHandle<TObject> LoadAssetFromGUID<TObject>(string guid) => Instance.LoadAssetFromGUIDInternal<TObject>(guid);
        public static AsyncOperationHandle<TObject> LoadAssetFromName<TObject>(string name) => Instance.LoadAssetFromNameInternal<TObject>(name);
        public static AsyncOperationHandle<TObject> LoadAssetFromLocation<TObject>(IResourceLocation location) => Instance.LoadAssetFromLocationInternal<TObject>(location);
        public static AsyncOperationHandle<IList<TObject>> LoadAssetsWithLabel<TObject>(string label, Action<TObject> callback) => Instance.LoadAssetsWithLabelInternal(label, callback);

        internal virtual AsyncOperationHandle<IList<IResourceLocation>> LoadResourceLocationsAsyncInternal(IEnumerable keys, Addressables.MergeMode mode, Type type = null) => Addressables.LoadResourceLocationsAsync(keys, mode, type);
        internal virtual AsyncOperationHandle<IList<IResourceLocation>> LoadResourceLocationsAsyncInternal(object key, Type type = null) => Addressables.LoadResourceLocationsAsync(key, type);
        internal virtual AsyncOperationHandle<IList<TObject>> LoadAssetsFromLocationsInternal<TObject>(IList<IResourceLocation> locations, Action<TObject> callback) => Addressables.LoadAssetsAsync(locations, callback);
        internal virtual AsyncOperationHandle<TObject> LoadAssetFromGUIDInternal<TObject>(string guid) => Addressables.LoadAssetAsync<TObject>(guid);
        internal virtual AsyncOperationHandle<TObject> LoadAssetFromNameInternal<TObject>(string name) => Addressables.LoadAssetAsync<TObject>(name);
        internal virtual AsyncOperationHandle<TObject> LoadAssetFromLocationInternal<TObject>(IResourceLocation location) => Addressables.LoadAssetAsync<TObject>(location);
        internal virtual AsyncOperationHandle<IList<TObject>> LoadAssetsWithLabelInternal<TObject>(string label, Action<TObject> callback) => Addressables.LoadAssetsAsync(label, callback);
    }
}
