using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Localization.UI;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using Object = UnityEngine.Object;

namespace UnityEditor.Localization
{
    /// <summary>
    /// Editor interface for modifying Localization settings and Localization based Addressables properties.
    /// </summary>
    public class LocalizationEditorSettings
    {
        internal const string LocaleGroupName = "Localization-Locales";
        internal const string AssetGroupName = "Localization-Assets-{0}";
        internal const string SharedAssetGroupName = "Localization-Assets-Shared";

        internal const string AssetTableGroupName = "Localization-AssetTables";
        internal const string StringTableGroupName = "Localization-StringTables";

        static class Styles
        {
            public static readonly string progressTitle = "Generating Asset Tables";
            public static readonly string saveTablesDialog = "Save tables to folder";
            public static readonly string saveTableDialog = "Save Table";
        }

        static LocalizationEditorSettings s_Instance;

        /// <summary>
        /// Event send in Editor when a modification is made to Localization assets.
        /// </summary>
        internal enum ModificationEvent
        {
                                     // Event type
            LocaleAdded,             // Locale
            LocaleRemoved,           // Locale
            TableAdded,              // LocalizedTable
            TableRemoved,            // LocalizedTable,
            TableEntryAdded,         // Tuple<KeyDatabase, KeyDatabase.KeyDatabaseEntry>
            TableEntryRemoved,       // Tuple<KeyDatabase, KeyDatabase.KeyDatabaseEntry>
            AssetTableEntryAdded,    // Tuple<AssetTable, AssetTableEntry, string>
            AssetTableEntryRemoved,  // Tuple<AssetTable, AssetTableEntry, string>
            AssetAdded,              // AddressableAssetEntry
            AssetUpdated,            // AddressableAssetEntry
            AssetRemoved,            // AddressableAssetEntry
        }
        internal delegate void Modification(ModificationEvent evt, object obj);
        event Modification m_OnModification;

        // Cached searches to help performance.
        ReadOnlyCollection<Locale> m_ProjectLocales;
        Dictionary<Type, ReadOnlyCollection<AssetTableCollection>> m_TableCollections = new Dictionary<Type, ReadOnlyCollection<AssetTableCollection>>(); 

        // Allows for overriding the default behavior, used for testing.
        internal static LocalizationEditorSettings Instance
        {
            get => s_Instance ?? (s_Instance = new LocalizationEditorSettings());
            set => s_Instance = value;
        }

        /// <summary>
        /// Event sent when modifications are made to Localization assets and Addressables.
        /// </summary>
        internal static event Modification OnModification
        {
            add => Instance.m_OnModification += value;
            remove => Instance.m_OnModification -= value;
        }

        /// <summary>
        /// The LocalizationSettings used for this project.
        /// </summary>
        /// <remarks>
        /// The activeLocalizationSettings will be available in any player builds
        /// and the editor when playing.
        /// During a build or entering play mode, the asset will be added to the preloaded assets list.
        /// Note: This needs to be an asset.
        /// </remarks>
        public static LocalizationSettings ActiveLocalizationSettings
        {
            get => Instance.ActiveLocalizationSettingsInternal;
            set => Instance.ActiveLocalizationSettingsInternal = value;
        }

        /// <summary>
        /// During play mode, in the editor a menu can be shown to allow for quickly changing the <see cref="LocalizationSettings.SelectedLocale"/>.
        /// </summary>
        public static bool ShowLocaleMenuInGameView
        {
            get => LocalizationSettings.ShowLocaleMenuInGameView;
            set => LocalizationSettings.ShowLocaleMenuInGameView = value;
        }

        /// <summary>
        /// Add the Locale to the Addressables system, so that it can be used by the Localization system during runtime.
        /// </summary>
        /// <param name="locale">The <see cref="Locale"/> to add to Addressables so that it can be used by the Localization system.</param>
        /// <param name="createUndo">Used to indicate if an Undo operation should be created.</param>
        public static void AddLocale(Locale locale, bool createUndo) => Instance.AddLocaleInternal(locale, createUndo);

        /// <summary>
        /// Removes the locale from the Addressables system.
        /// </summary>
        /// <param name="locale">The <see cref="Locale"/> to remove from Addressables so that it is no longer used by the Localization system.</param>
        /// /// <param name="createUndo">Used to indicate if an Undo operation should be created.</param>
        public static void RemoveLocale(Locale locale, bool createUndo) => Instance.RemoveLocaleInternal(locale, createUndo);

        /// <summary>
        /// Returns all locales that are part of the Addressables system.
        /// </summary>
        /// <returns></returns>
        public static ReadOnlyCollection<Locale> GetLocales() => Instance.GetLocalesInternal();

        /// <summary>
        /// Returns the locale for the code if it exists in the project.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static Locale GetLocale(string code) => Instance.GetLocaleInternal(code);

        /// <summary>
        /// Add a localized asset to the asset table.
        /// This function will ensure the localization system adds the asset to the Addressables system and sets the asset up for use.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="keyId"></param>
        /// <param name="asset"></param>
        /// <param name="createUndo">Used to indicate if an Undo operation should be created.</param>
        public static void AddAssetToTable(AssetTable table, uint keyId, Object asset, bool createUndo = false)
        {
            Instance.AddAssetToTableInternal(table, keyId, asset, createUndo);
        }
        
        /// <summary>
        /// Remove the asset mapping from the table and also cleanup the Addressables if necessary.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="keyId"></param>
        /// <param name="asset"></param>
        /// <param name="createUndo"></param>
        public static void RemoveAssetFromTable(AssetTable table, uint keyId, Object asset, bool createUndo = false)
        {
            Instance.RemoveAssetFromTableInternal(table, keyId, asset, createUndo);
        }

        /// <summary>
        /// Add or update the Addressables data for the table.
        /// Ensures the table is in the correct group and has all the required labels.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="createUndo">Used to indicate if an Undo operation should be created.</param>
        public static void AddOrUpdateTable(LocalizedTable table, bool createUndo)
        {
            Instance.AddOrUpdateTableInternal(table, createUndo);
        }

        /// <summary>
        /// Remove the table from Addressables and any associated assets if they are not used elsewhere.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="createUndo">Used to indicate if an Undo operation should be created.</param>
        public static void RemoveTable(LocalizedTable table, bool createUndo = false) => Instance.RemoveTableInternal(table, createUndo);

        /// <summary>
        /// Returns all asset tables in the project of type TLocalizedTable.
        /// </summary>
        /// <typeparam name="TLocalizedTable"></typeparam>
        /// <returns></returns>
        public static List<AddressableAssetEntry> GetAssetTables<TLocalizedTable>() where TLocalizedTable : LocalizedTable
        {
            return Instance.GetAssetTablesInternal(typeof(TLocalizedTable));
        }

        /// <summary>
        /// Returns all asset tables in the project of type tableType.
        /// </summary>
        /// <param name="tableType"></param>
        /// <returns></returns>
        public static List<AddressableAssetEntry> GetAssetTables(Type tableType) 
        {
            return Instance.GetAssetTablesInternal(tableType);
        }

        /// <summary>
        /// Returns all asset tables in the project sorted by type and table name.
        /// </summary>
        /// <typeparam name="TLocalizedTable"></typeparam>
        /// <returns></returns>
        public static ReadOnlyCollection<AssetTableCollection> GetAssetTablesCollection<TLocalizedTable>() where TLocalizedTable : LocalizedTable
        {
            return Instance.GetAssetTablesCollectionInternal(typeof(TLocalizedTable));
        }
        
        /// <summary>
        /// Returns all asset tables in the project sorted by type and table name.
        /// </summary>
        /// <param name="tableType"></param>
        /// <returns></returns>
        public static ReadOnlyCollection<AssetTableCollection> GetAssetTablesCollection(Type tableType)
        {
            return Instance.GetAssetTablesCollectionInternal(tableType);
        }

        /// <summary>
        /// Adds/Removes the preload flag for the table.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="preload"></param>
        /// <param name="createUndo"></param>
        public static void SetPreloadTableFlag(LocalizedTable table, bool preload, bool createUndo = false)
        {
            Instance.SetPreloadTableInternal(table, preload, createUndo);
        }

        /// <summary>
        /// Returns true if the table is marked for preloading.
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static bool GetPreloadTableFlag(LocalizedTable table)
        {
            return Instance.GetPreloadTableFlagInternal(table);
        }

        /// <summary>
        /// Returns the <see cref="AssetTableCollection"/> that contains a table entry with the closest match to the provided text.
        /// Uses the Levenshtein distance method.
        /// </summary>
        /// <typeparam name="TTable"></typeparam>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public static (AssetTableCollection collection, KeyDatabase.KeyDatabaseEntry entry, int matchDistance) FindSimilarKey<TTable>(string keyName) where TTable : LocalizedTable
        {
            return Instance.FindSimilarKeyInternal<TTable>(keyName);
        }

        /// <summary>
        /// Create a new AssetTable asset. This will create the table and ensure that the table is also added to the Addressables.
        /// </summary>
        /// <param name="selectedLocales">The locale the table should be created for.</param>
        /// <param name="keyDatabase"></param>
        /// <param name="tableType">The type of table to create. Must inherit from LocalizedTable.</param>
        /// <param name="assetPath">The path to save the asset to.</param>
        /// <returns></returns>
        public static LocalizedTable CreateAssetTable(Locale selectedLocales, KeyDatabase keyDatabase, Type tableType, string assetPath)
        {
            return Instance.CreateAssetTableInternal(selectedLocales, keyDatabase, tableType, assetPath);
        }

        /// <summary>
        /// Creates an AssetTable of type <paramref name="tableType"/> for each of the Locales provided in <paramref name="selectedLocales"/>.
        /// </summary>
        /// <param name="selectedLocales">The locales to use for generating the Tables.</param>
        /// <param name="tableName">The table name to be used for all tables.</param>
        /// <param name="tableType">The type of table to create. Must inherit from LocalizedTable.</param>
        /// <param name="assetDirectory">The directory to save all the generated asset files to.</param>
        /// <returns></returns>
        internal static List<LocalizedTable> CreateAssetTableColletion(IList<Locale> selectedLocales, string tableName, Type tableType, string assetDirectory)
        {
            return Instance.CreateAssetTableCollectionInternal(selectedLocales, tableName, tableType, assetDirectory, false, false);
        }

        internal void SendEvent(ModificationEvent evt, object context) => m_OnModification?.Invoke(evt, context);

        /// <summary>
        /// <inheritdoc cref="ActiveLocalizationSettings"/>
        /// </summary>
        protected virtual LocalizationSettings ActiveLocalizationSettingsInternal
        {
            get
            {
                EditorBuildSettings.TryGetConfigObject(LocalizationSettings.ConfigName, out LocalizationSettings settings);
                return settings;
            }
            set
            {
                if (value == null)
                {
                    EditorBuildSettings.RemoveConfigObject(LocalizationSettings.ConfigName);
                }
                else
                {
                    EditorBuildSettings.AddConfigObject(LocalizationSettings.ConfigName, value, true);
                }
            }
        }

        internal virtual AddressableAssetSettings GetAddressableAssetSettings(bool create)
        {
            return AddressableAssetSettingsDefaultObject.GetSettings(create);
        }

        /// <summary>
        /// Returns the name of the Addressables group that the table typse should be stored in.
        /// </summary>
        /// <param name="tableType"></param>
        /// <returns></returns>
        protected virtual string GetTableGroupName(Type tableType)
        {
            if (typeof(StringTable).IsAssignableFrom(tableType))
                return StringTableGroupName;
            else if (typeof(AssetTable).IsAssignableFrom(tableType))
                return AssetTableGroupName;
            throw new Exception($"Unknown table type \"{tableType.FullName}\"");
        }

        /// <summary>
        /// Returns the Addressables group with the matching name or creates a new one, if one could not be found.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="groupName"></param>
        /// <param name="create"></param>
        /// <param name="createUndo">Used to indicate if an Undo operation should be created.</param>
        /// <returns></returns>
        protected virtual AddressableAssetGroup GetGroup(AddressableAssetSettings settings, string groupName, bool create, bool createUndo)
        {
            var group = settings.FindGroup(groupName);
            if (group == null && create)
            {
                group = settings.CreateGroup(groupName, false, true, true, new List<AddressableAssetGroupSchema>(){ ScriptableObject.CreateInstance<ContentUpdateGroupSchema>() }, typeof(BundledAssetGroupSchema));

                if (createUndo)
                    Undo.RegisterCreatedObjectUndo(group, "Create group");
            }

            return group;
        }

        /// <summary>
        /// Tries to find a unique address for the asset to be stored in Addressables.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        protected virtual string FindUniqueAssetAddress(string address)
        {
            var aaSettings = GetAddressableAssetSettings(false);
            if (aaSettings == null)
                return address;

            var validAddress = address;
            var index = 1;
            var foundExisting = true;
            while (foundExisting)
            {
                if (index > 1000)
                {
                    Debug.LogError("Unable to create valid address for new Addressable Asset.");
                    return address;
                }
                foundExisting = false;
                foreach (var g in aaSettings.groups)
                {
                    if (g.Name == validAddress)
                    {
                        foundExisting = true;
                        validAddress = address + index;
                        index++;
                        break;
                    }
                }
            }

            return validAddress;
        }

        /// <summary>
        /// <inheritdoc cref="AddLocale"/>
        /// </summary>
        /// <param name="locale"></param>
        /// <param name="createUndo"></param>
        protected virtual void AddLocaleInternal(Locale locale, bool createUndo)
        {
            if (!EditorUtility.IsPersistent(locale))
            {
                Debug.LogError($"Only persistent assets can be addressable. The asset '{locale.name}' needs to be saved on disk.");
                return;
            }

            var aaSettings = GetAddressableAssetSettings(true);
            if (aaSettings == null)
                return;

            if (createUndo)
                Undo.RecordObject(aaSettings, "Add locale");

            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(locale));
            var assetEntry = aaSettings.FindAssetEntry(guid);

            if (assetEntry == null)
            {
                var group = GetGroup(aaSettings, LocaleGroupName, true, createUndo);

                if (createUndo)
                    Undo.RecordObject(group, "Add locale");

                assetEntry = aaSettings.CreateOrMoveEntry(guid, group, true);
                assetEntry.address = locale.name;
            }

            // Clear the locales cache.
            m_ProjectLocales = null;

            if (!assetEntry.labels.Contains(LocalizationSettings.LocaleLabel))
            {
                aaSettings.AddLabel(LocalizationSettings.LocaleLabel);
                assetEntry.SetLabel(LocalizationSettings.LocaleLabel, true);
                SendEvent(ModificationEvent.LocaleAdded, locale);
            }
        }

        /// <summary>
        /// <inheritdoc cref="RemoveLocale"/>
        /// </summary>
        /// <param name="locale"></param>
        /// <param name="createUndo"></param>
        protected virtual void RemoveLocaleInternal(Locale locale, bool createUndo)
        {
            var aaSettings = GetAddressableAssetSettings(false);
            if (aaSettings == null)
                return;

            if (createUndo)
                Undo.RecordObject(aaSettings, "Remove locale");

            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(locale));
            if (aaSettings.FindAssetEntry(guid) == null)
                return;

            if (createUndo)
            {
                var entry = aaSettings.FindAssetEntry(guid);
                Undo.RecordObject(entry.parentGroup, "Remove locale");
            }

            // Clear the locale cache
            m_ProjectLocales = null;

            aaSettings.RemoveAssetEntry(guid);
            SendEvent(ModificationEvent.LocaleRemoved, locale);
        }

        /// <summary>
        /// <inheritdoc cref="GetLocales"/>
        /// </summary>
        /// <returns></returns>
        protected virtual ReadOnlyCollection<Locale> GetLocalesInternal()
        {
            if (m_ProjectLocales != null)
                return m_ProjectLocales;

            var foundLocales = new List<Locale>();

            var aaSettings = GetAddressableAssetSettings(true);
            if (aaSettings == null)
                return null;

            var foundAssets = new List<AddressableAssetEntry>();
            aaSettings.GetAllAssets(foundAssets, false, null, entry =>
            {
                return entry.labels.Contains(LocalizationSettings.LocaleLabel);
            });

            foreach (var localeAddressable in foundAssets)
            {
                if (!string.IsNullOrEmpty(localeAddressable.guid))
                {
                    var locale = AssetDatabase.LoadAssetAtPath<Locale>(AssetDatabase.GUIDToAssetPath(localeAddressable.guid));
                    if (locale != null)
                        foundLocales.Add(locale);
                }
            }

            foundLocales.Sort();
            m_ProjectLocales = new ReadOnlyCollection<Locale>(foundLocales);
            return m_ProjectLocales;
        }

        /// <summary>
        /// <inheritdoc cref="GetLocale"/>
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        protected virtual Locale GetLocaleInternal(string code) => GetLocalesInternal().FirstOrDefault(loc => loc.Identifier.Code == code);

        static string FormatAssetTableName(LocaleIdentifier localeIdentifier)
        {
            return string.Format(AssetGroupName, localeIdentifier.Code);
        }

        /// <summary>
        /// <inheritdoc cref="AddAssetToTable"/>
        /// </summary>
        /// <param name="table"></param>
        /// <param name="keyId"></param>
        /// <param name="asset"></param>
        /// <param name="createUndo"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected virtual void AddAssetToTableInternal(AssetTable table, uint keyId, Object asset, bool createUndo)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            if (asset == null)
                throw new ArgumentNullException(nameof(asset));

            if (!EditorUtility.IsPersistent(table) || !EditorUtility.IsPersistent(asset))
            {
                var assetName = !EditorUtility.IsPersistent(table) ? table.name : asset.name;
                Debug.LogError($"Only persistent assets can be addressable. The asset '{assetName}' needs to be saved on disk.");
                return;
            }

            // Add the asset to the Addressables system and setup the table with the key to guid mapping.
            var aaSettings = GetAddressableAssetSettings(true);
            if (aaSettings == null)
                return;

            // Check we don't have mixed types for this entry.
            //var typeMetadata = table.Keys.GetSharedMetadata<AssetTypeMetadata>(keyId);
            //if (typeMetadata != null && typeMetadata.Type != asset.GetType())
            //    throw new Exception($"Can not add asset{asset.name}, expected a type of {typeMetadata.Type.Name} but got {asset.GetType()}.");

            if (createUndo)
                Undo.RecordObject(aaSettings, "Add asset to table");
            else
                EditorUtility.SetDirty(aaSettings);

            // Has the asset already been added? Perhaps it is being used by multiple tables or the user has added it manually.
            var assetGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));

            var tableEntry = table.GetEntry(keyId);
            if (tableEntry != null)
            {
                if(tableEntry.Guid == assetGuid)
                    return;

                // Remove the old asset first
                RemoveAssetFromTableInternal(table, keyId, tableEntry.Guid, createUndo);
            }

            var entry = aaSettings.FindAssetEntry(assetGuid);
            var entryLabel = AddressHelper.FormatAssetLabel(table.LocaleIdentifier.Code);
            aaSettings.AddLabel(entryLabel);

            if (entry == null)
            {
                var group = GetGroup(aaSettings, FormatAssetTableName(table.LocaleIdentifier), true, createUndo);

                entry = aaSettings.CreateOrMoveEntry(assetGuid, group, true);
                entry.SetLabel(entryLabel, true);
                entry.address = FindUniqueAssetAddress(asset.name);
            }
            else
            {
                Undo.RecordObject(entry.parentGroup, "Add asset to table");
                entry.SetLabel(entryLabel, true);
                UpdateAssetGroup(aaSettings, entry, createUndo);
            }

            if (createUndo)
            {
                Undo.RecordObject(table, "Add asset to table");
                Undo.RecordObject(table.Keys, "Add asset to table");
            }
            else
            {
                EditorUtility.SetDirty(table);
                EditorUtility.SetDirty(table.Keys);
            }

            tableEntry = table.AddEntry(keyId, assetGuid);

            // Update type metadata
            AssetTypeMetadata entryMetadata = null;
            AssetTypeMetadata typeMetadata = null;
            var assetType = asset.GetType();
            foreach(var md in table.Keys.Metadata.Entries)
            {
                if (md is AssetTypeMetadata at)
                {
                    if (at.Contains(keyId))
                    {
                        if (!at.Type.IsAssignableFrom(assetType))
                        {
                            Debug.LogWarning($"Table entry {keyId} contains the wrong type data ({at.Type}. It has been removed.");
                            tableEntry.RemoveSharedMetadata(at);
                        }
                        else
                        {
                            entryMetadata = at;
                        }
                    }

                    if (at.Type == assetType)
                        typeMetadata = at;
                }
            }
            var foundMetadata = entryMetadata ?? typeMetadata;
            if (foundMetadata == null)
            {
                foundMetadata = new AssetTypeMetadata() { Type = assetType };
            }
            tableEntry.AddSharedMetadata(foundMetadata);

            //EditorUtility.SetDirty(table); // Mark the table dirty or changes will not be saved.

            SendEvent(ModificationEvent.AssetTableEntryAdded, new Tuple<AssetTable, AssetTableEntry, string>(table, tableEntry, assetGuid));
        }

        /// <summary>
        /// <inheritdoc cref="RemoveAssetFromTable"/>
        /// </summary>
        /// <param name="table"></param>
        /// <param name="keyId"></param>
        /// <param name="assetGuid"></param>
        /// <param name="createUndo"></param>
        protected virtual void RemoveAssetFromTableInternal(AssetTable table, uint keyId, string assetGuid, bool createUndo)
        {
            if (createUndo)
                Undo.RecordObject(table, "Remove asset from table");
            else
                EditorUtility.SetDirty(table); // Mark the table dirty or changes will not be saved.

            // Clear the asset but keep the key
            var tableEntry = table.AddEntry(keyId, string.Empty);

            var aaSettings = GetAddressableAssetSettings(false);
            if (aaSettings == null)
                return;

            // Update type metadata
            // We cant use a foreach here as we are sometimes inside of a loop and exceptions will be thrown (Collection was modified).
            for (int i = 0; i <table.Keys.Metadata.Entries.Count; ++i)
            {
                var md = table.Keys.Metadata.Entries[i];
                if (md is AssetTypeMetadata at)
                {
                    if (at.Contains(keyId))
                    {
                        tableEntry.RemoveSharedMetadata(at);
                    }
                }
            }

            // Determine if the asset is being referenced by any other tables with the same locale, if not then we can
            // remove the locale label and if no other labels exist also remove the asset from the Addressables system.
            var tableGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(table));
            var tableGroup = GetGroup(aaSettings, AssetTableGroupName, false, createUndo);
            if (tableGroup != null)
            {
                foreach (var e in tableGroup.entries)
                {
                    var tableToCheck = e.guid == tableGuid ? table : AssetDatabase.LoadAssetAtPath<AssetTable>(AssetDatabase.GUIDToAssetPath(e.guid));
                    if (tableToCheck != null && tableToCheck.LocaleIdentifier == table.LocaleIdentifier)
                    {
                        foreach (var item in tableToCheck.TableEntries.Values)
                        {
                            // The asset is referenced elsewhere so we can not remove the label or asset.
                            if (item.Guid == assetGuid)
                            {
                                // Check this is not the entry currently being removed.
                                if (tableToCheck == table && item.Data.Id == keyId)
                                    continue;

                                return;
                            }
                        }
                    }
                }
            }

            var assetEntry = aaSettings.FindAssetEntry(assetGuid);
            if (assetEntry != null)
            {
                if (createUndo)
                    Undo.RecordObject(assetEntry.parentGroup, "Remove asset from table");

                var assetLabel = AddressHelper.FormatAssetLabel(table.LocaleIdentifier);
                assetEntry.SetLabel(assetLabel, false);
                UpdateAssetGroup(aaSettings, assetEntry, createUndo);
            }

            SendEvent(ModificationEvent.AssetTableEntryRemoved, new Tuple<AssetTable, AssetTableEntry, string>(table, tableEntry, assetGuid));
        }

        /// <summary>
        /// <inheritdoc cref="RemoveAssetFromTable"/>
        /// </summary>
        /// <param name="table"></param>
        /// <param name="keyId"></param>
        /// <param name="asset"></param>
        /// <param name="createUndo"></param>
        /// <typeparam name="TObject"></typeparam>
        protected virtual void RemoveAssetFromTableInternal<TObject>(AssetTable table, uint keyId, TObject asset, bool createUndo) where TObject : Object
        {
            var assetGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
            RemoveAssetFromTableInternal(table, keyId, assetGuid, createUndo);
        }

        /// <summary>
        /// Updates the group the asset should belong to. 
        /// If an asset is used by more than 1 <see cref="Locale"/> then it will be moved to the shared group.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="assetEntry"></param>
        /// <param name="createUndo">Used to indicate if an Undo operation should be created.</param>
        protected virtual void UpdateAssetGroup(AddressableAssetSettings settings, AddressableAssetEntry assetEntry, bool createUndo)
        {
            if (settings == null || assetEntry == null)
                return;

            var localesUsingAsset = assetEntry.labels.Where(AddressHelper.IsLocaleLabel).ToList();
            if (localesUsingAsset.Count == 0)
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

                SendEvent(ModificationEvent.AssetRemoved, assetEntry);
                return;
            }

            AddressableAssetGroup newGroup;
            if (localesUsingAsset.Count == 1)
            {
                // Add to a locale specific group
                var localeId = AddressHelper.LocaleLabelToId(localesUsingAsset[0]);
                newGroup = GetGroup(settings, FormatAssetTableName(localeId), true, createUndo);
            }
            else
            {
                // Add to the shared assets group
                newGroup = GetGroup(settings, SharedAssetGroupName, true, createUndo);
            }

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

                SendEvent(ModificationEvent.AssetUpdated, assetEntry);
            }
        }

        /// <summary>
        /// Update the Addressables data for the table.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="createUndo">Used to indicate if an Undo operation should be created.</param>
        protected virtual void AddOrUpdateTableInternal(LocalizedTable table, bool createUndo)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            if (!EditorUtility.IsPersistent(table))
            {
                Debug.LogError($"Only persistent assets can be addressable. The asset '{table.name}' needs to be saved on disk.");
                return;
            }

            var aaSettings = GetAddressableAssetSettings(true);
            if (aaSettings == null)
                return;

            if (createUndo)
                Undo.RecordObject(aaSettings, "Update table");

            // Add the key database
            var keyDbGuid = GetAssetGuid(table.Keys);
            var keyDbEntry = aaSettings.FindAssetEntry(keyDbGuid);
            if (keyDbEntry == null)
            {
                // Add to the shared assets group
                var sharedGroup = GetGroup(aaSettings, SharedAssetGroupName, true, createUndo);
                aaSettings.CreateOrMoveEntry(keyDbGuid, sharedGroup);
            }

            // Has the asset already been added?
            var tableGuid = GetAssetGuid(table);
            var tableEntry = aaSettings.FindAssetEntry(tableGuid);
            var tableAdded = tableEntry == null;
            if (tableEntry == null)
            {
                var groupName = GetTableGroupName(table.GetType());
                var group = GetGroup(aaSettings, groupName, true, createUndo);

                if (createUndo)
                    Undo.RecordObject(group, "Update table");

                tableEntry = aaSettings.CreateOrMoveEntry(tableGuid, group, true);

                // Clear cache
                m_TableCollections.Clear();
            }
            else if (createUndo)
            {
                Undo.RecordObject(tableEntry.parentGroup, "Update table");
            }

            tableEntry.address = AddressHelper.GetTableAddress(table.TableName, table.LocaleIdentifier);
            tableEntry.labels.Clear(); // Locale may have changed so clear the old one.

            // We store the KeyDatabase GUID in the importer so we can search through tables without loading them.
            var importer = AssetImporter.GetAtPath(tableEntry.AssetPath);
            if (importer.userData != keyDbGuid)
            {
                if (createUndo)
                    Undo.RecordObject(importer, "Update table");
                importer.userData = keyDbGuid;
                importer.SaveAndReimport();
            }

            // Label the locale
            var localeLabel = AddressHelper.FormatAssetLabel(table.LocaleIdentifier);
            aaSettings.AddLabel(localeLabel);
            tableEntry.SetLabel(localeLabel, true);

            if (tableAdded)
                SendEvent(ModificationEvent.TableAdded, table);
        }

        /// <summary>
        /// <inheritdoc cref=" RemoveTable"/>
        /// </summary>
        /// <param name="table"></param>
        /// <param name="createUndo"></param>
        protected virtual void RemoveTableInternal(LocalizedTable table, bool createUndo)
        {
            var aaSettings = GetAddressableAssetSettings(true);
            if (aaSettings == null)
                return;

            if (createUndo)
                Undo.RecordObject(aaSettings, "Remove table");

            var assetTable = table as AssetTable;
            if (assetTable != null)
            {
                foreach (var te in assetTable.TableEntries)
                {
                    RemoveAssetFromTableInternal(assetTable, te.Key, te.Value.Guid, createUndo);
                }
            }

            // Clear cache
            m_TableCollections.Clear();

            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(table));
            var entry = aaSettings.FindAssetEntry(guid);
            if (entry != null)
            {
                aaSettings.RemoveAssetEntry(guid);
                SendEvent(ModificationEvent.TableRemoved, table);
            }
        }

        /// <summary>
        /// <inheritdoc cref=" SetPreloadTable"/>
        /// </summary>
        /// <param name="table"></param>
        /// <param name="preload"></param>
        /// <param name="createUndo"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="AddressableEntryNotFoundException"></exception>
        protected virtual void SetPreloadTableInternal(LocalizedTable table, bool preload, bool createUndo = false)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var aaSettings = GetAddressableAssetSettings(true);
            if (aaSettings == null)
                return;

            var tabbleGuid = GetAssetGuid(table);
            var tableEntry = aaSettings.FindAssetEntry(tabbleGuid);
            if (tableEntry == null)
                throw new AddressableEntryNotFoundException(table);

            if (createUndo)
                Undo.RecordObjects(new Object[] {aaSettings, tableEntry.parentGroup }, "Set Preload flag");

            aaSettings.AddLabel(LocalizationSettings.PreloadLabel);
            tableEntry.SetLabel(LocalizationSettings.PreloadLabel, preload);
        }

        /// <summary>
        /// <inheritdoc cref=" GetPreloadTableFlag"/>
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="AddressableEntryNotFoundException"></exception>
        protected virtual bool GetPreloadTableFlagInternal(LocalizedTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            
            var aaSettings = GetAddressableAssetSettings(true);
            if (aaSettings == null)
                return false;

            var tabbleGuid = GetAssetGuid(table);
            var tableEntry = aaSettings.FindAssetEntry(tabbleGuid);
            if (tableEntry == null)
                throw new AddressableEntryNotFoundException(table);

            return tableEntry.labels.Contains(LocalizationSettings.PreloadLabel);
        }

        /// <summary>
        /// <inheritdoc cref=" GetAssetTables"/>
        /// </summary>
        /// <param name="tableType"></param>
        /// <returns></returns>
        protected virtual List<AddressableAssetEntry> GetAssetTablesInternal(Type tableType)
        {
            var foundTables = new List<AddressableAssetEntry>();
            var settings = GetAddressableAssetSettings(false);
            if (settings == null)
                return foundTables;

            settings.GetAllAssets(foundTables, false, null, (aae) =>
            {
                var type = AssetDatabase.GetMainAssetTypeAtPath(aae.AssetPath);
                return tableType.IsAssignableFrom(type);
            });
            return foundTables;
        }

        /// <summary>
        /// Returns all tables for the project that are part of Addressables. Tables are collated by <see cref="TableName"/> and type.
        /// </summary>
        /// Use <see cref="LocalizedTable"/> to get all types, <see cref="StringTable"/> for all string tables and <see cref="AssetTable"/> for all asset tables.</typeparam>
        /// <param name="tableType">The table base Type.</param>
        /// <returns></returns>
        protected virtual ReadOnlyCollection<AssetTableCollection> GetAssetTablesCollectionInternal(Type tableType)
        {
            if(!typeof(LocalizedTable).IsAssignableFrom(tableType))
                throw new Exception($"{nameof(tableType)} must inherit from {nameof(LocalizedTable)}");

            if (m_TableCollections.TryGetValue(tableType, out var foundCollection))
                return foundCollection;

            // We want to find all tables and sort them by type and name without loading the actual Tables(slow!).
            var assetTablesCollection = new List<AssetTableCollection>();
            var assetTables = GetAssetTablesInternal(tableType);

            // Collate by table type and table name
            var lookup = new Dictionary<(Type, string), AssetTableCollection>();
            foreach (var table in assetTables)
            {
                // Get the table name from the meta data so we do not need to load the whole asset.
                var importer = AssetImporter.GetAtPath(table.AssetPath);
                var keyDbGuid = importer.userData;
                if (string.IsNullOrEmpty(keyDbGuid))
                {
                    // Try and find the key db table name guid
                    var loadedTable = AssetDatabase.LoadAssetAtPath<LocalizedTable>(table.AssetPath);
                    keyDbGuid = GetAssetGuid(loadedTable.Keys);
                }

                var assetTableType = AssetDatabase.GetMainAssetTypeAtPath(table.AssetPath);
                var key = (assetTableType, keyDbGuid);
                AssetTableCollection tableCollection;
                if (!lookup.TryGetValue(key, out tableCollection))
                {
                    tableCollection = new AssetTableCollection() { TableType = assetTableType };
                    tableCollection.Keys = AssetDatabase.LoadAssetAtPath<KeyDatabase>(AssetDatabase.GUIDToAssetPath(key.keyDbGuid));

                    assetTablesCollection.Add(tableCollection);
                    lookup[key] = tableCollection;
                }

                tableCollection.TableEntries.Add(table);
            }

            var readOnly = new ReadOnlyCollection<AssetTableCollection>(assetTablesCollection);
            m_TableCollections[tableType] = readOnly;
            return readOnly;
        }

        /// <summary>
        ///  <inheritdoc cref=" FindSimilarKey"/>
        /// </summary>
        /// <param name="keyName"></param>
        /// <typeparam name="TTable"></typeparam>
        /// <returns></returns>
        protected virtual (AssetTableCollection collection, KeyDatabase.KeyDatabaseEntry entry, int matchDistance) FindSimilarKeyInternal<TTable>(string keyName) where TTable : LocalizedTable
        {
            // Check if we can find a matching key to the key name
            var tables = GetAssetTablesCollection<TTable>();
            int currentMatchDistance = int.MaxValue;
            KeyDatabase.KeyDatabaseEntry currentEntry = null;
            AssetTableCollection tableCollection = null;
            foreach (var assetTableCollection in tables)
            {
                var keys = assetTableCollection.Keys;
                var foundKey = keys.FindSimilarKey(keyName, out int distance);
                if (foundKey != null && distance < currentMatchDistance)
                {
                    currentMatchDistance = distance;
                    currentEntry = foundKey;
                    tableCollection = assetTableCollection;
                }
            }
            return (tableCollection, currentEntry, currentMatchDistance);
        }

        internal static LocalizedTable CreateAssetTableFilePanel(Locale selectedLocales, KeyDatabase keyDatabase, string tableName, Type tableType, string defaultDirectory)
        {
            return Instance.CreateAssetTableFilePanelInternal(selectedLocales, keyDatabase, tableName, tableType, defaultDirectory);
        }

        internal static void CreateAssetTablesFolderPanel(List<Locale> selectedLocales, string tableName, string tableType)
        {
            Instance.CreateAssetTableCollectionInternal(selectedLocales, tableName, Type.GetType(tableType));
        }

        internal static void CreateAssetTablesFolderPanel(List<Locale> selectedLocales, string tableName, Type tableType)
        {
            Instance.CreateAssetTableCollectionInternal(selectedLocales, tableName, tableType);
        }
        
        LocalizedTable CreateAssetTableFilePanelInternal(Locale selectedLocale, KeyDatabase keyDatabase, string tableName, Type tableType, string defaultDirectory)
        {
            var assetPath = EditorUtility.SaveFilePanel(Styles.saveTableDialog, defaultDirectory, AddressHelper.GetTableAddress(tableName, selectedLocale.Identifier), "asset");
            return string.IsNullOrEmpty(assetPath) ? null : CreateAssetTable(selectedLocale, keyDatabase, tableType, assetPath);
        }

        /// <summary>
        /// Creates a table using provided arguments.
        /// </summary>
        /// <param name="selectedLocales">The <see cref="Locale"/> the table represents.</param>
        /// <param name="keyDatabase"></param>
        /// <param name="tableType">The type of table, must derive from <see cref="LocalizedTable"/>.</param>
        /// <param name="assetPath">Where to save the asset, must be inside of the project Assets folder.</param>
        /// <returns>Returns the created table.</returns>
        protected virtual LocalizedTable CreateAssetTableInternal(Locale selectedLocales, KeyDatabase keyDatabase, Type tableType, string assetPath)
        {
            var relativePath = MakePathRelative(assetPath);
            var table = (LocalizedTable)ScriptableObject.CreateInstance(tableType);
            table.Keys = keyDatabase;
            table.LocaleIdentifier = selectedLocales.Identifier;
            table.name = Path.GetFileNameWithoutExtension(assetPath);

            CreateAsset(table, relativePath);

            AddOrUpdateTableInternal(table, false);
            return table;
        }

        void CreateAssetTableCollectionInternal(List<Locale> selectedLocales, string tableName, Type tableType)
        {
            var assetDirectory = EditorUtility.SaveFolderPanel(Styles.saveTablesDialog, "Assets/", "");
            if (!string.IsNullOrEmpty(assetDirectory))
                CreateAssetTableCollectionInternal(selectedLocales, tableName, tableType, assetDirectory, true, true);
        }

        /// <summary>
        /// Create multiple tables using the provided arguments.
        /// </summary>
        /// <param name="selectedLocales">List of <see cref="Locale"/>, a table will be created for each one.</param>
        /// <param name="tableName">The name of the table that will be applied to all tables.</param>
        /// <param name="tableType">The type of table, must derive from <see cref="LocalizedTable"/>.</param>
        /// <param name="assetDirectory">Directory where the created tables will be saved, must be in the project Assets folder.</param>
        /// <param name="showProgressBar">Should a progress bar be shown during the creation?</param>
        /// <param name="showInTablesWindow">Should the Asset Tables Window be opened with these new tables selected?</param>
        /// <returns></returns>
        protected virtual List<LocalizedTable> CreateAssetTableCollectionInternal(IList<Locale> selectedLocales, string tableName, Type tableType, string assetDirectory, bool showProgressBar, bool showInTablesWindow)
        {
            List<LocalizedTable> createdTables;

            try
            {
                // TODO: Check that no tables already exist with the same name, locale and type.
                AssetDatabase.StartAssetEditing(); // Batch the assets into a single asset operation
                var relativePath = MakePathRelative(assetDirectory);

                var keyDbPath = Path.Combine(relativePath, tableName + " Keys.asset");
                var keyDatabase = ScriptableObject.CreateInstance<KeyDatabase>();
                keyDatabase.TableName = tableName;
                CreateAsset(keyDatabase, keyDbPath);

                // Extract the KeyDatabase Guid and assign it so we can use it as a unique id for the table name.
                var keyDbGuid = GetAssetGuid(keyDatabase);
                keyDatabase.TableNameGuid = Guid.Parse(keyDbGuid); // We need to set it dirty so the change to TableNameGuid is saved.
                EditorUtility.SetDirty(keyDatabase);

                // Create the instances
                if (showProgressBar)
                    EditorUtility.DisplayProgressBar(Styles.progressTitle, "Creating Tables", 0);

                createdTables = new List<LocalizedTable>(selectedLocales.Count);
                foreach (var locale in selectedLocales)
                {
                    var table = (LocalizedTable)ScriptableObject.CreateInstance(tableType);
                    table.Keys = keyDatabase;
                    table.LocaleIdentifier = locale.Identifier;
                    table.name = AddressHelper.GetTableAddress(tableName, locale.Identifier);
                    createdTables.Add(table);
                }

                // Save as assets
                if (showProgressBar)
                    EditorUtility.DisplayProgressBar(Styles.progressTitle, "Saving Tables", 0);

                for (int i = 0; i < createdTables.Count; ++i)
                {
                    var tbl = createdTables[i];

                    if (showProgressBar)
                        EditorUtility.DisplayProgressBar(Styles.progressTitle, $"Saving Table {tbl.name}", i / (float)createdTables.Count);

                    var assetPath = Path.Combine(relativePath, tbl.name + ".asset");
                    assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
                    CreateAsset(tbl, assetPath);
                    AddOrUpdateTableInternal(tbl, false);
                }
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets(); // Required to ensure the change to KeyDatabase is written.

                if (showInTablesWindow)
                    AssetTablesWindow.ShowWindow(createdTables[0]);
            }
            finally
            {
                if (showProgressBar)
                    EditorUtility.ClearProgressBar();

                // Clear cache
                m_TableCollections.Clear();
            }

            return createdTables;
        }

        internal virtual void CreateAsset(Object asset, string path)
        {
            AssetDatabase.CreateAsset(asset, path);
        }

        internal virtual string GetAssetGuid(Object asset)
        {
            Debug.Assert(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string guid, out long _), "Failed to extract the asset Guid", asset);
            return guid;
        }

        internal static string MakePathRelative(string path)
        {
            if (path.Contains(Application.dataPath))
            {
                var length = Application.dataPath.Length - "Assets".Length;
                return path.Substring(length, path.Length - length);
            }

            return path;
        }
    }
}