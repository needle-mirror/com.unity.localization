using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization.Tables
{
    /// <summary>
    /// Run time representation of an entry in a <see cref="AssetTable"/>.
    /// </summary>
    public class AssetTableEntry : TableEntry
    {
        internal AsyncOperationHandle? AsyncOperation { get; set; }

        /// <summary>
        /// The asset Guid, used to load the asset by the Addressables system.
        /// </summary>
        public string Guid { get => Data.Localized; set => Data.Localized = value; }

        /// <summary>
        /// Does this entry contain any data? Checks if <see cref="Guid"/> is empty.
        /// </summary>
        public bool IsEmpty => string.IsNullOrEmpty(Guid);

        internal AssetTableEntry()
        {
        }

        /// <summary>
        /// Attempts to remove the entry from the <see cref="AssetTable"/> that it belongs to.
        /// If <see cref="TableEntry.Table"/> is null then a warning will be produced.
        /// </summary>
        public void RemoveFromTable()
        {
            var assetTable = Table as AssetTable;
            if (assetTable == null)
            {
                Debug.LogWarning($"Failed to remove {nameof(AssetTableEntry)} with id {KeyId} and asset guid `{Guid}` as it does not belong to a table.");
            }
            else
            {
                assetTable.Remove(KeyId);
            }
        }
    }

    /// <summary>
    /// An AssetTable
    /// </summary>
    public class AssetTable : DetailedLocalizationTable<AssetTableEntry>, IPreloadRequired
    {
        AsyncOperationHandle? m_PreloadOperationHandle;
        List<AsyncOperationHandle> m_AssetPreloadOperations;

        ResourceManager ResourceManager => LocalizationSettings.ResourceManager;

        public virtual AsyncOperationHandle PreloadOperation
        {
            get
            {
                if (m_PreloadOperationHandle == null)
                {
                    m_PreloadOperationHandle = PreloadAssets();
                }
                return m_PreloadOperationHandle.Value;
            }
        }

        AsyncOperationHandle PreloadAssets()
        {
            // Check the metadata to see if we should preload. First we check the table preload data, if one does not exist then check the key database(global).
            var preload = GetMetadata<PreloadAssetTableMetadata>() ?? SharedData.Metadata.GetMetadata<PreloadAssetTableMetadata>();

            // If no preload metadata was found then we will preload all assets by default.
            if (preload?.Behaviour != PreloadAssetTableMetadata.PreloadBehaviour.NoPreload)
            {
                m_AssetPreloadOperations = m_AssetPreloadOperations ?? new List<AsyncOperationHandle>();
                m_AssetPreloadOperations.Clear();

                // Preload all
                foreach (var entry in Values)
                {
                    if (!entry.IsEmpty && !entry.AsyncOperation.HasValue)
                    {
                        entry.AsyncOperation = Addressables.LoadAssetAsync<Object>(entry.Guid);
                        m_AssetPreloadOperations.Add(entry.AsyncOperation.Value);
                    }
                }

                // TODO: Preload selected
                //{
                //    foreach (var entry in TableEntries.Values)
                //    {
                //        if (!entry.IsEmpty && !entry.AsyncOperation.HasValue && entry.Data.Metadata.GetMetadata<PreloadAssetMetadata>() != null)
                //        {
                //            entry.AsyncOperation = Addressables.LoadAssetAsync<Object>(entry.Guid);
                //            m_AssetPreloadOperations.Add(entry.AsyncOperation.Value);
                //        }
                //    }
                //}

                if (m_AssetPreloadOperations.Count > 0)
                {
                    return ResourceManager.CreateGenericGroupOperation(m_AssetPreloadOperations, true);
                }
            }

            // Nothing to preload, we are done
            return ResourceManager.CreateCompletedOperation(this, null);
        }

        /// <summary>
        /// Returns the loading operation for the asset.
        /// Check isDone to see if the asset is available for immediate use, if not you can yield on the operation or add a callback subscriber.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public AsyncOperationHandle<TObject> GetAssetAsync<TObject>(long keyId) where TObject : Object
        {
            var entry = GetEntry(keyId);
            if (entry == null)
            {
                var keyName = SharedData.GetKey(keyId);

                return ResourceManager.CreateCompletedOperation<TObject>(null, $"Could not find asset with key \"{keyName}({keyId})\"");
            }
            return GetAssetAsync<TObject>(entry);
        }

        internal AsyncOperationHandle<TObject> GetAssetAsync<TObject>(AssetTableEntry entry) where TObject : Object
        {
            if (!entry.AsyncOperation.HasValue)
            {
                if (string.IsNullOrEmpty(entry.Guid))
                {
                    var keyName = SharedData.GetKey(entry.Data.Id);
                    return ResourceManager.CreateCompletedOperation<TObject>(null, $"The asset table entry \"{keyName}({entry.Data.Id})\" is empty, no asset can be loaded from the table \"{ToString()}\"");
                }
                entry.AsyncOperation = Addressables.LoadAssetAsync<TObject>(entry.Guid);
            }

            var operation = entry.AsyncOperation.Value;

            try
            {
                return operation.Convert<TObject>();
            }
            catch (InvalidCastException)
            {
                // If we preloaded then the operation will be of type AsyncOperationHandle<Object> however we now need to
                // convert to AsyncOperationHandle<TObject>.

                if (operation.IsDone)
                {
                    if (operation.Status != AsyncOperationStatus.Succeeded)
                    {
                        return ResourceManager.CreateCompletedOperation<TObject>(null, operation.OperationException.Message);
                    }

                    // Convert the operation
                    if (operation.Result is TObject)
                    {
                        var convertedCompletedOperation = ResourceManager.CreateCompletedOperation<TObject>(operation.Result as TObject, null);
                        entry.AsyncOperation = convertedCompletedOperation;
                        return convertedCompletedOperation;
                    }
                    else
                    {
                        throw new InvalidCastException($"Could not convert asset of type {operation.Result.GetType().Name} to {typeof(TObject).Name}.");
                    }
                }

                // Wait for the operation to complete before attempting again
                var convertedOperation = ResourceManager.CreateChainOperation(operation, (op) => GetAssetAsync<TObject>(entry));
                entry.AsyncOperation = convertedOperation;
                return convertedOperation;
            }
        }

        /// <summary>
        /// Releases all assets that have been preloaded or cached resets the preload state so it can be performed again.
        /// Note: This is called automatically by <see cref="LocalizedAssetDatabase"/> when the <see cref="LocalizationSettings.SelectedLocale"/> is changed.
        /// </summary>
        public void ReleaseAssets()
        {
            m_PreloadOperationHandle = null;
            foreach (var entry in Values)
            {
                if (entry.AsyncOperation.HasValue)
                {
                    Addressables.Release(entry.AsyncOperation.Value);
                    entry.AsyncOperation = null;
                }
            }
        }

        /// <summary>
        /// Creates a new, empty AssetTableEntry.
        /// </summary>
        /// <returns></returns>
        public override AssetTableEntry CreateTableEntry()
        {
            return new AssetTableEntry() { Table = this, Data = new TableEntryData() };
        }

        #if UNITY_EDITOR
        /// <summary>
        // ScriptableObject properties may persist during runs in the editor, so we reset them here to keep each play consistent.
        /// </summary>
        void OnEnable()
        {
            m_PreloadOperationHandle = null;
        }

        #endif
    }
}
