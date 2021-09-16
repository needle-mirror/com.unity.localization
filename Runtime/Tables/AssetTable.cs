using System;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Settings;
using UnityEngine.Pool;
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

        /// <summary>
        /// Provides support for overriding the localized asset for the Entry. Note this is only temporary and will not persist in the Editor or if the table is reloaded.
        /// This allows for a table to be updated in the player.
        /// </summary>
        /// <typeparam name="T">The type to store the asset as locally.</typeparam>
        /// <param name="asset">The asset reference to use instead of <see cref="Guid"/>.</param>
        /// <example>
        /// This example shows how you can update the AssetTable entry values when the Locale is changed.
        /// <code source="../../DocCodeSamples.Tests/LocalizedAssetSamples.cs" region="override-asset-entry-1"/>
        /// </example>
        /// <example>
        /// This example shows how you can update all AssetTable entries at the start and ensure that the tables are never unloaded so that the changes are persistent throughtout the lifetime of the player.
        /// <code source="../../DocCodeSamples.Tests/LocalizedAssetSamples.cs" region="override-asset-entry-2"/>
        /// </example>
        public void SetAssetOverride<T>(T asset) where T : Object
        {
            if (AsyncOperation.HasValue)
                AddressablesInterface.Release(AsyncOperation.Value);
            AsyncOperation = AddressablesInterface.ResourceManager.CreateCompletedOperation(asset, null);
        }
    }

    /// <summary>
    /// An AssetTable
    /// </summary>
    public class AssetTable : DetailedLocalizationTable<AssetTableEntry>, IPreloadRequired
    {
        AsyncOperationHandle? m_PreloadOperationHandle;

        ResourceManager ResourceManager => AddressablesInterface.ResourceManager;

        /// <summary>
        /// Handle to the preload operation for this table.
        /// Calling this will start the asset tables preloading operation, it will preload all assets referenced
        /// unless the Metadata <see cref="PreloadAssetTableMetadata"/> is found and has the
        /// <see cref="PreloadAssetTableMetadata.PreloadBehaviour.NoPreload"/> behavior.
        /// </summary>
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
                var handleList = ListPool<AsyncOperationHandle>.Get();

                // Preload all
                foreach (var entry in Values)
                {
                    if (!entry.IsEmpty && !entry.AsyncOperation.HasValue)
                    {
                        // We have to preload the asset as an array so that we get all sub objects. The reason for this is that some assets,
                        // such as Sprite can contain multiple sub objects and if we load it as Object then we may get the wrong one.
                        // For example, if we load a Sprite as an Object and it has the same name as its Texture asset then it will load as a Texture,
                        // not Sprite. So if we load as an array we get both, we can then pick the one we need later based on the type passed into GetAssetAsync. (LOC-143)
                        entry.AsyncOperation = AddressablesInterface.LoadAssetFromGUID<Object[]>(entry.Guid);
                        ResourceManager.Acquire(entry.AsyncOperation.Value);
                        handleList.Add(entry.AsyncOperation.Value);
                    }
                }
                if (handleList.Count > 0)
                {
                    return ResourceManager.CreateGenericGroupOperation(handleList);
                }
                else
                {
                    ListPool<AsyncOperationHandle>.Release(handleList);
                }
            }

            // Nothing to preload, we are done
            return ResourceManager.CreateCompletedOperation(this, null);
        }

        /// <summary>
        /// Returns the loading operation for the asset.
        /// Check isDone to see if the asset is available for immediate use, if not you can yield on the operation or add a callback subscriber.
        /// </summary>
        /// <param name="entryReference"></param>
        /// <typeparam name="TObject">The type of object to load.</typeparam>
        /// <returns></returns>
        public AsyncOperationHandle<TObject> GetAssetAsync<TObject>(TableEntryReference entryReference) where TObject : Object
        {
            var entry = GetEntryFromReference(entryReference);
            if (entry == null)
            {
                var keyName = entryReference.ResolveKeyName(SharedData);
                return ResourceManager.CreateCompletedOperation<TObject>(null, $"Could not find asset with key \"{keyName}\"");
            }
            return GetAssetAsync<TObject>(entry);
        }

        internal AsyncOperationHandle<TObject> GetAssetAsync<TObject>(AssetTableEntry entry) where TObject : Object
        {
            if (!entry.AsyncOperation.HasValue)
            {
                // Empty entries are treated as null.
                if (string.IsNullOrEmpty(entry.Guid))
                    entry.AsyncOperation = ResourceManager.CreateCompletedOperation<TObject>(null, null);
                else
                    entry.AsyncOperation = AddressablesInterface.LoadAssetFromGUID<TObject>(entry.Guid);
            }

            var operation = entry.AsyncOperation.Value;

            try
            {
                return operation.Convert<TObject>();
            }
            catch (InvalidCastException)
            {
                // If we preloaded then the operation will be of type AsyncOperationHandle<Object[]> however we now
                // need to extract the asset and convert to AsyncOperationHandle<TObject>.

                if (operation.IsDone)
                {
                    if (operation.Status != AsyncOperationStatus.Succeeded)
                    {
                        return ResourceManager.CreateCompletedOperation<TObject>(null, operation.OperationException.Message);
                    }

                    // Extract the asset from the array of preloaded sub objects.
                    if (operation.Result is Object[] subObjects)
                    {
                        foreach (var obj in subObjects)
                        {
                            if (obj is TObject target)
                            {
                                var convertedCompletedOperation = ResourceManager.CreateCompletedOperation(target, null);
                                entry.AsyncOperation = convertedCompletedOperation;
                                AddressablesInterface.Release(operation); // Release the old operation
                                return convertedCompletedOperation;
                            }
                        }
                    }
                    throw new InvalidCastException($"Could not convert asset of type {operation.Result.GetType().Name} to {typeof(TObject).Name}.");
                }

                // Wait for the operation to complete before attempting again
                var convertedOperation = ResourceManager.CreateChainOperation(operation, (op) => GetAssetAsync<TObject>(entry));
                entry.AsyncOperation = convertedOperation;
                AddressablesInterface.Release(operation); // Release the old operation
                return convertedOperation;
            }
        }

        /// <summary>
        /// Releases all assets that have been preloaded or cached resets the preload state so it can be performed again.
        /// Note: This is called automatically by <see cref="LocalizedAssetDatabase"/> when the <see cref="LocalizationSettings.SelectedLocale"/> is changed.
        /// </summary>
        public void ReleaseAssets()
        {
            if (m_PreloadOperationHandle.HasValue)
            {
                AddressablesInterface.SafeRelease(m_PreloadOperationHandle.Value);
                m_PreloadOperationHandle = null;
            }

            foreach (var entry in Values)
            {
                if (entry.AsyncOperation.HasValue)
                {
                    AddressablesInterface.SafeRelease(entry.AsyncOperation.Value);
                    entry.AsyncOperation = null;
                }
            }
        }

        /// <summary>
        /// Release an asset for a single entry that have been preloaded  or cached
        /// </summary>
        /// <param name="entry">A reference to the entry in the table.</param>
        public void ReleaseAsset(AssetTableEntry entry)
        {
            if (entry == null)
                return;

            if (m_PreloadOperationHandle.HasValue)
            {
                AddressablesInterface.SafeRelease(m_PreloadOperationHandle.Value);
                m_PreloadOperationHandle = null;
            }

            if (entry.AsyncOperation.HasValue)
            {
                AddressablesInterface.SafeRelease(entry.AsyncOperation.Value);
                entry.AsyncOperation = null;
            }
        }

        /// <summary>
        /// Release an asset for a single entry that have been preloaded  or cached
        /// </summary>
        /// <param name="entry">A reference to the entry in the table.</param>
        public void ReleaseAsset(TableEntryReference entry)
        {
            ReleaseAsset(GetEntryFromReference(entry));
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
