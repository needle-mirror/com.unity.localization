using System;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Tables;
using Object = UnityEngine.Object;

namespace UnityEditor.Localization
{
    /// <summary>
    /// Contains information on all the tables associated with a table collection name and type.
    /// </summary>
    public class AssetTableCollection : LocalizationTableCollection
    {
        static readonly Type kTableType = typeof(AssetTable);
        protected internal override  Type TableType => kTableType;

        protected internal override Type RequiredExtensionAttribute => typeof(AssetTableCollectionExtensionAttribute);

        protected override string DefaultAddressablesGroupName => "Localization-AssetTables";

        /// <summary>
        /// A helper property which is the contents of <see cref="Tables"/> loaded and cast to <see cref="AssetTable"/>.
        /// </summary>
        public virtual ReadOnlyCollection<AssetTable> AssetTables => new ReadOnlyCollection<AssetTable>(Tables.Select(t => t.asset as AssetTable).ToList().AsReadOnly());

        protected internal override string DefaultGroupName => "Asset Table";

        protected override void RemoveTableFromAddressables(LocalizationTable table, bool createUndo)
        {
            if (table is AssetTable assetTable)
            {
                // This can be called when a table is about to be deleted or when it is removed from the collection.
                // If the asset is removed from the collection it will still exists but not be part of a build, essentially is is turned off.
                // A user may want to turn the asset back on at a later time and have all the assets restored, so when we remove the table we need
                // to keep its entries so that they can be added again.
                // We can do this by saving the state using JsonUtility and then restoring it afterwards.

                // Save the state
                var json = JsonUtility.ToJson(table);

                // We need to make a copy as the original will be modified as we remove each item.
                var valuesCopy = assetTable.Values.ToArray();
                foreach (var entry in valuesCopy)
                {
                    RemoveAssetFromTable(assetTable, entry.KeyId);
                }

                // Restore the entries
                JsonUtility.FromJsonOverwrite(json, table);
            }

            base.RemoveTableFromAddressables(table, createUndo);
        }

        /// <summary>
        /// Add a localized asset to the asset table.
        /// This function will ensure the localization system adds the asset to the Addressables system and sets the asset up for use.
        /// </summary>
        /// <param name="table">The table to add the asset to, must be part of the collection.</param>
        /// <param name="entryReference">The table entry Key or Key Id.</param>
        /// <param name="asset">The asset to add.</param>
        /// <param name="createUndo">Should an undo operation be created?</param>
        public virtual void AddAssetToTable(AssetTable table, TableEntryReference entryReference, Object asset, bool createUndo = false)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table), "Can not add asset to null table");

            if (asset == null)
                throw new ArgumentNullException(nameof(asset), "Can not add null asset to table");

            if (!ContainsTable(table))
            {
                throw new Exception("The table does not belong to this collection.");
            }

            if (!EditorUtility.IsPersistent(table))
                throw new AssetNotPersistentException(table);

            if (!EditorUtility.IsPersistent(asset))
                throw new AssetNotPersistentException(asset);

            // Add the asset to the Addressables system and setup the table with the key to guid mapping.
            var aaSettings = LocalizationEditorSettings.Instance.GetAddressableAssetSettings(true);
            if (aaSettings == null)
                return;

            var undoGroup = Undo.GetCurrentGroup();
            if (createUndo)
                Undo.RecordObject(aaSettings, "Add asset to table");

            // Remove the old asset first
            var assetGuid = LocalizationEditorSettings.Instance.GetAssetGuid(asset);
            var tableEntry = table.GetEntryFromReference(entryReference);
            if (tableEntry != null)
            {
                if (tableEntry.Guid == assetGuid)
                    return;

                RemoveAssetFromTable(table, entryReference, createUndo);
            }

            // Has the asset already been added? Perhaps it is being used by multiple tables or the user has added it manually.
            var entry = aaSettings.FindAssetEntry(assetGuid);
            var entryLabel = AddressHelper.FormatAssetLabel(table.LocaleIdentifier.Code);
            aaSettings.AddLabel(entryLabel);

            if (entry == null)
            {
                var group = LocalizationEditorSettings.Instance.GetGroup(aaSettings, FormatAssetTableCollectionName(table.LocaleIdentifier), true, createUndo);

                if (createUndo)
                    Undo.RecordObject(group,  "Add asset to table");

                entry = aaSettings.CreateOrMoveEntry(assetGuid, group, true);
                entry.SetLabel(entryLabel, true);
                entry.address = LocalizationEditorSettings.Instance.FindUniqueAssetAddress(asset.name);
            }
            else
            {
                Undo.RecordObject(entry.parentGroup, "Add asset to table");
                entry.SetLabel(entryLabel, true);
                UpdateAssetGroup(aaSettings, entry, createUndo);
            }

            // Update the table
            if (createUndo)
            {
                Undo.RecordObject(table, "Add asset to table");
                Undo.RecordObject(table.SharedData, "Add asset to table");
            }
            //else // Asset changes are not being saved correctly at the moment when using Undo. (LOC-82)
            {
                EditorUtility.SetDirty(table);
                EditorUtility.SetDirty(table.SharedData);
            }

            tableEntry = table.AddEntryFromReference(entryReference, assetGuid);

            // Update type metadata
            AssetTypeMetadata entryMetadata = null;
            AssetTypeMetadata typeMetadata = null;
            var assetType = asset.GetType();
            // We cant use a foreach here as we are sometimes inside of a loop and exceptions will be thrown (Collection was modified).
            for (int i = 0; i < table.SharedData.Metadata.MetadataEntries.Count; ++i)
            {
                var md = table.SharedData.Metadata.MetadataEntries[i];
                if (md is AssetTypeMetadata at)
                {
                    if (at.Contains(tableEntry.KeyId))
                    {
                        if (!at.Type.IsAssignableFrom(assetType))
                        {
                            tableEntry.RemoveSharedMetadata(at);

                            // Are other tables still using the type for the same id?
                            if (at.Contains(tableEntry.KeyId))
                            {
                                var name = SharedData.GetEntry(tableEntry.KeyId);
                                Debug.LogWarning($"Table entry {name}({tableEntry.KeyId}) contains mixed types. Both {at.Type} and {assetType} are used.");
                            }
                        }
                        else
                        {
                            entryMetadata = at;
                            break;
                        }
                    }

                    if (at.Type == assetType)
                    {
                        typeMetadata = at;
                        break;
                    }
                }
            }
            var foundMetadata = entryMetadata ?? typeMetadata;
            if (foundMetadata == null)
            {
                foundMetadata = new AssetTypeMetadata() { Type = assetType };
            }
            tableEntry.AddSharedMetadata(foundMetadata);

            if (createUndo)
            {
                Undo.CollapseUndoOperations(undoGroup);
            }

            LocalizationEditorSettings.EditorEvents.RaiseAssetTableEntryAdded(this, table, tableEntry);
        }

        /// <summary>
        /// Remove the asset mapping from the table entry and also cleans up the Addressables if necessary.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="entryReference"></param>
        /// <param name="createUndo"></param>
        public void RemoveAssetFromTable(AssetTable table, TableEntryReference entryReference, bool createUndo = false)
        {
            var undoGroup = Undo.GetCurrentGroup();
            if (createUndo)
            {
                Undo.RecordObject(table, "Remove asset from table"); // We modify the table entry.
                Undo.RecordObject(table.SharedData, "Remove asset from table"); // We modify the shared table metadata.
            }
            //else // Asset changes are not being saved correctly at the moment when using Undo. (LOC-82)
            {
                EditorUtility.SetDirty(table);
                EditorUtility.SetDirty(table.SharedData);
            }

            // Clear the asset but keep the key
            var tableEntry = table.GetEntryFromReference(entryReference);
            if (tableEntry == null)
                return;

            var removedAssetGuid = tableEntry.Guid;
            tableEntry.Guid = string.Empty;

            var aaSettings = LocalizationEditorSettings.Instance.GetAddressableAssetSettings(false);
            if (aaSettings == null)
                return;

            // Update type metadata
            // We cant use a foreach here as we are sometimes inside of a loop and exceptions will be thrown (Collection was modified).
            for (int i = 0; i < table.SharedData.Metadata.MetadataEntries.Count; ++i)
            {
                var md = table.SharedData.Metadata.MetadataEntries[i];
                if (md is AssetTypeMetadata at)
                {
                    if (at.Contains(tableEntry.KeyId))
                    {
                        tableEntry.RemoveSharedMetadata(at);
                    }
                }
            }

            // If the entry has metadata then we will leave an empty entry otherwise we just remove the whole thing.
            if (tableEntry.MetadataEntries.Count == 0)
                table.RemoveEntry(tableEntry.KeyId);

            // Determine if the asset is being referenced by any entries or tables with the same locale, if not then we can
            // remove the locale label and if no other labels exist also remove the asset from the Addressables system.
            var assetTableCollections = LocalizationEditorSettings.GetAssetTableCollections();
            foreach (var collection in assetTableCollections)
            {
                var tableWithMatchingLocaleId = collection.GetTable(table.LocaleIdentifier) as AssetTable;
                if (tableWithMatchingLocaleId == null)
                    continue;

                if (tableWithMatchingLocaleId.ContainsValue(removedAssetGuid))
                {
                    // The asset is referenced elsewhere so we can not remove the label or asset.
                    return;
                }
            }

            // Remove the locale label for this asset
            var assetEntry = aaSettings.FindAssetEntry(removedAssetGuid);
            if (assetEntry != null)
            {
                if (createUndo)
                {
                    Undo.RecordObject(assetEntry.parentGroup, "Remove asset from table");
                }

                var assetLabel = AddressHelper.FormatAssetLabel(table.LocaleIdentifier);
                assetEntry.SetLabel(assetLabel, false);
                UpdateAssetGroup(aaSettings, assetEntry, createUndo);
            }

            if (createUndo)
            {
                Undo.CollapseUndoOperations(undoGroup);
            }

            LocalizationEditorSettings.EditorEvents.RaiseAssetTableEntryRemoved(this, table, tableEntry, removedAssetGuid);
        }

        /// <summary>
        /// Updates the group the asset should belong to.
        /// Assets will be stored in groups for the Locale they are used by unless they are used
        /// by more than 1 <see cref="Locale"/>, then they will be moved to the shared group.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="assetEntry"></param>
        /// <param name="createUndo">Used to indicate if an Undo operation should be created.</param>
        protected virtual void UpdateAssetGroup(AddressableAssetSettings settings, AddressableAssetEntry assetEntry, bool createUndo)
        {
            if (settings == null || assetEntry == null)
                return;

            // Find all the locales that are using the asset using the Addressable labels.
            var localesUsingAsset = assetEntry.labels.Where(AddressHelper.IsLocaleLabel);
            if (localesUsingAsset.Count() == 0)
            {
                var oldGroup = assetEntry.parentGroup;
                settings.RemoveAssetEntry(assetEntry.guid);
                if (oldGroup.entries.Count == 0)
                {
                    if (createUndo)
                    {
                        // We cant use undo asset deletion so we will leave an empty group instead of deleting it.
                        Undo.RecordObject(oldGroup, "Remove group");
                    }
                    else
                    {
                        settings.RemoveGroup(oldGroup);
                    }
                }

                return;
            }

            AddressableAssetGroup newGroup;
            if (localesUsingAsset.Count() == 1)
            {
                // If only 1 locale is using the asset then we will add it to a locale specific group.
                var localeId = AddressHelper.LocaleLabelToId(localesUsingAsset.First());
                newGroup = LocalizationEditorSettings.Instance.GetGroup(settings, FormatAssetTableCollectionName(localeId), true, createUndo);
            }
            else
            {
                // More than one locale uses the asset so it goes to the shared assets group.
                newGroup = LocalizationEditorSettings.Instance.GetGroup(settings, LocalizationEditorSettings.SharedAssetGroupName, true, createUndo);
            }

            // Do we need to change the asset's group?
            if (newGroup != assetEntry.parentGroup)
            {
                if (createUndo)
                {
                    Undo.RecordObject(newGroup, "Update asset group");
                    Undo.RecordObject(assetEntry.parentGroup, "Update asset group");
                }

                var oldGroup = assetEntry.parentGroup;
                settings.MoveEntry(assetEntry, newGroup, true);
                if (oldGroup.entries.Count == 0)
                {
                    // We only delete the asset when not creating an undo as we can not undo asset deletion.
                    if (!createUndo)
                    {
                        settings.RemoveGroup(oldGroup);
                    }
                }
            }
        }

        static string FormatAssetTableCollectionName(LocaleIdentifier localeIdentifier) => string.Format(LocalizationEditorSettings.AssetGroupName, localeIdentifier.Code);
    }
}
