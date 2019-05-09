using System;
using UnityEditor.AddressableAssets;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using Object = UnityEngine.Object;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

namespace UnityEditor.Localization
{
    public class LocalizationEditorSettings
    {
        internal const string LocaleGroupName = "Localization-Locales";
        internal const string AssetGroupName = "Localization-Assets-{0}";
        internal const string SharedAssetGroupName = "Localization-Assets-Shared";

        internal const string AssetTableGroupName = "Localization-AssetTables";
        internal const string StringTableGroupName = "Localization-StringTables";

        const string k_AssetTableNameFormat = "{0}-{1}-{2}"; // [locale code] [table name] [type]
        const string k_AssetLabelPrefix = "Locale-";

        class Texts
        {
            public readonly string progressTitle = "Generating Asset Tables";
            public readonly string saveTablesDialog = "Save tables to folder";
            public readonly string saveTableDialog = "Save Table";
        }
        static Texts s_Texts = new Texts();

        static LocalizationEditorSettings s_Instance;

        public enum ModificationEvent
        {
                                // Object type
            LocaleAdded,        // Locale
            LocaleRemoved,      // Locale
            TableAdded,         // LocalizedTable
            TableRemoved,       // LocalizedTable
            AssetAdded,         // AddressableAssetEntry
            AssetUpdated,       // AddressableAssetEntry
            AssetRemoved,       // AddressableAssetEntry
        }
        public delegate void Modification(ModificationEvent evt, object obj);
        event Modification m_OnModification;

        // Allows for overriding the default behavior, used for testing.
        internal static LocalizationEditorSettings Instance
        {
            get => s_Instance ?? (s_Instance = new LocalizationEditorSettings());
            set => s_Instance = value;
        }

        /// <summary>
        /// Event sent when modifications are made to Localization assets and Addressables.
        /// </summary>
        public static event Modification OnModification
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
        /// Add the Locale to the Addressables system, so that it can be used by the Localization system during runtime.
        /// </summary>
        /// <param name="locale"></param>
        public static void AddLocale(Locale locale) => Instance.AddLocaleInternal(locale);

        /// <summary>
        /// Removes the locale from the Addressables system.
        /// </summary>
        /// <param name="locale"></param>
        public static void RemoveLocale(Locale locale) => Instance.RemoveLocaleInternal(locale);

        /// <summary>
        /// Returns all locales that are part of the Addressables system.
        /// </summary>
        /// <returns></returns>
        public static List<Locale> GetLocales() => Instance.GetLocalesInternal();

        /// <summary>
        /// Add a localized asset to the asset table.
        /// This function will ensure the localization system adds the asset to the Addressables system and sets the asset up for use.
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="table"></param>
        /// <param name="keyId"></param>
        /// <param name="asset"></param>
        public static void AddAssetToTable<TObject>(AddressableAssetTableT<TObject> table, uint keyId, TObject asset) where TObject : Object
        {
            Instance.AddAssetToTableInternal(table, keyId, asset);
        }

        /// <summary>
        /// Remove the asset mapping from the table and also cleanup the Addressables if necessary.
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="table"></param>
        /// <param name="keyId"></param>
        /// <param name="asset"></param>
        public static void RemoveAssetFromTable<TObject>(AddressableAssetTableT<TObject> table, uint keyId, TObject asset) where TObject : Object
        {
            Instance.RemoveAssetFromTableInternal(table, keyId, asset);
        }

        /// <summary>
        /// Remove the asset mapping from the table and also cleanup the Addressables if necessary.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="keyId"></param>
        /// <param name="assetGuid"></param>
        public static void RemoveAssetFromTable(AddressableAssetTable table, uint keyId, string assetGuid)
        {
            Instance.RemoveAssetFromTableInternal(table, keyId, assetGuid);
        }

        /// <summary>
        /// Add or update the Addressables data for the table.
        /// Ensures the table is in the correct group and has all the required labels.
        /// </summary>
        /// <param name="table"></param>
        public static void AddOrUpdateTable(LocalizedTable table)
        {
            Instance.AddOrUpdateTableInternal(table);
        }

        /// <summary>
        /// Remove the table from Addressables and any associated assets if they are not used elsewhere.
        /// </summary>
        /// <param name="table"></param>
        public static void RemoveTable(LocalizedTable table) => Instance.RemoveTableInternal(table);

        /// <summary>
        /// Returns all asset tables in the project of type TLocalizedTable.
        /// </summary>
        /// <typeparam name="TLocalizedTable"></typeparam>
        /// <returns></returns>
        public static List<TLocalizedTable> GetAssetTables<TLocalizedTable>() where TLocalizedTable : LocalizedTable
        {
            return Instance.GetAssetTablesInternal<TLocalizedTable>();
        }

        /// <summary>
        /// Returns all asset tables in the project.
        /// </summary>
        /// <returns></returns>
        public static List<LocalizedTable> GetAllAssetTables() => Instance.GetAssetTablesInternal<LocalizedTable>();

        /// <summary>
        /// Returns all asset tables in the project sorted by type and table name.
        /// </summary>
        /// <typeparam name="TLocalizedTable"></typeparam>
        /// <returns></returns>
        public static List<AssetTableCollection> GetAssetTablesCollection<TLocalizedTable>() where TLocalizedTable : LocalizedTable
        {
            return Instance.GetAssetTablesCollectionInternal<TLocalizedTable>();
        }

        /// <summary>
        /// Create a new AssetTable asset. This will create the table and ensure that the table is also added to the Addressables.
        /// </summary>
        /// <param name="selectedLocales">The locale the table should be created for.</param>
        /// <param name="keyDatabase">The Key Database for this table.</param>
        /// <param name="tableName">The name of the table. Tables are collated by their type and name.</param>
        /// <param name="tableType">The type of table to create. Must inherit from LocalizedTable.</param>
        /// <param name="assetPath">The path to save the asset to.</param>
        /// <returns></returns>
        public static LocalizedTable CreateAssetTable(Locale selectedLocales, KeyDatabase keyDatabase, string tableName, Type tableType, string assetPath)
        {
            return Instance.CreateAssetTableInternal(selectedLocales, keyDatabase, tableName, tableType, assetPath);
        }

        /// <summary>
        /// Creates an AssetTable of type <paramref name="tableType"/> for each of the Locales provided in <paramref name="selectedLocales"/>.
        /// </summary>
        /// <param name="selectedLocales">The locales to use for generating the Tables.</param>
        /// <param name="keyDatabase">The key database to be used for all the tables. If null a new one will be created.</param>
        /// <param name="tableName">The table name to be used for all tables.</param>
        /// <param name="tableType">The type of table to create. Must inherit from LocalizedTable.</param>
        /// <param name="assetDirectory">The directory to save all the generated asset files to.</param>
        /// <returns></returns>
        internal static List<LocalizedTable> CreateAssetTables(List<Locale> selectedLocales, KeyDatabase keyDatabase, string tableName, Type tableType, string assetDirectory)
        {
            return Instance.CreateAssetTablesInternal(selectedLocales, keyDatabase, tableName, tableType, assetDirectory, false, false);
        }

        protected void SendEvent(ModificationEvent evt, object context) => m_OnModification?.Invoke(evt, context);

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

        protected virtual AddressableAssetSettings GetAddressableAssetSettings(bool create)
        {
            return AddressableAssetSettingsDefaultObject.GetSettings(create);
        }

        protected virtual AddressableAssetGroup GetGroup(AddressableAssetSettings settings, string groupName, bool create = false)
        {
            var group = settings.FindGroup(groupName);
            if (group == null && create)
            {
                group = settings.CreateGroup(groupName, false, false, true, new List<AddressableAssetGroupSchema>(){ ScriptableObject.CreateInstance<ContentUpdateGroupSchema>() }, typeof(BundledAssetGroupSchema));
            }

            return group;
        }

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

        protected virtual void AddLocaleInternal(Locale locale)
        {
            if (!EditorUtility.IsPersistent(locale))
            {
                Debug.LogError($"Only persistent assets can be addressable. The asset '{locale.name}' needs to be saved on disk.");
                return;
            }

            var aaSettings = GetAddressableAssetSettings(true);
            if (aaSettings == null)
                return;

            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(locale));
            var assetEntry = aaSettings.FindAssetEntry(guid);

            if (assetEntry == null)
            {
                var group = GetGroup(aaSettings, LocaleGroupName, true);
                assetEntry = aaSettings.CreateOrMoveEntry(guid, group, true);
                assetEntry.address = locale.name;
            }

            if (!assetEntry.labels.Contains(LocalizationSettings.LocaleLabel))
            {
                aaSettings.AddLabel(LocalizationSettings.LocaleLabel);
                assetEntry.SetLabel(LocalizationSettings.LocaleLabel, true);
                SendEvent(ModificationEvent.LocaleAdded, locale);
            }
        }

        protected virtual void RemoveLocaleInternal(Locale locale)
        {
            var aaSettings = GetAddressableAssetSettings(false);
            if (aaSettings == null)
                return;

            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(locale));
            if (aaSettings.FindAssetEntry(guid) == null)
                return;

            aaSettings.RemoveAssetEntry(guid);
            SendEvent(ModificationEvent.LocaleRemoved, locale);
        }

        protected virtual List<Locale> GetLocalesInternal()
        {
            var locales = new List<Locale>();

            var aaSettings = GetAddressableAssetSettings(false);
            if (aaSettings == null)
                return locales;

            // TODO: Use Addressables instead of AssetDatabase. Waiting on new filter type to be added to `GetAllAssets`
            var localeGuids = AssetDatabase.FindAssets("t:Locale");

            foreach (var localeGuid in localeGuids)
            {
                var assetEntry = aaSettings.FindAssetEntry(localeGuid);
                if (assetEntry != null && assetEntry.labels.Contains(LocalizationSettings.LocaleLabel))
                {
                    var locale = AssetDatabase.LoadAssetAtPath<Locale>(AssetDatabase.GUIDToAssetPath(localeGuid));
                    locales.Add(locale);
                }
            }
            return locales;
        }

        internal static bool IsLocaleLabel(string label) => label.StartsWith(k_AssetLabelPrefix);

        internal static LocaleIdentifier LocaleLabelToId(string label)
        {
            Debug.Assert(IsLocaleLabel(label));
            return label.Substring(k_AssetLabelPrefix.Length, label.Length - k_AssetLabelPrefix.Length);
        }

        internal static string FormatAssetLabel(LocaleIdentifier localeIdentifier) => k_AssetLabelPrefix + localeIdentifier.Code;

        protected static string FormatAssetTableName(LocaleIdentifier localeIdentifier)
        {
            return string.Format(AssetGroupName, localeIdentifier.Code);
        }

        protected virtual void AddAssetToTableInternal<TObject>(AddressableAssetTableT<TObject> table, uint keyId, TObject asset) where TObject : Object
        {
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

            // Has the asset already been added? Perhaps it is being used by multiple tables or the user has added it manually.
            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
            var entry = aaSettings.FindAssetEntry(guid);

            var entryLabel = FormatAssetLabel(table.LocaleIdentifier.Code);
            aaSettings.AddLabel(entryLabel);

            if (entry == null)
            {
                var group = GetGroup(aaSettings, FormatAssetTableName(table.LocaleIdentifier), true);
                entry = aaSettings.CreateOrMoveEntry(guid, group, true);
                entry.SetLabel(entryLabel, true);
                entry.address = FindUniqueAssetAddress(asset.name);

            }
            else
            {
                entry.SetLabel(entryLabel, true);
                UpdateAssetGroup(aaSettings, entry);
            }

            table.AddAsset(keyId, guid);
            EditorUtility.SetDirty(table); // Mark the table dirty or changes will not be saved.
        }

        protected virtual void RemoveAssetFromTableInternal(AddressableAssetTable table, uint keyId, string assetGuid)
        {
            // Clear the asset but keep the key
            table.AddAsset(keyId, string.Empty);
            EditorUtility.SetDirty(table); // Mark the table dirty or changes will not be saved.

            var aaSettings = GetAddressableAssetSettings(false);
            if (aaSettings == null)
                return;

            // Determine if the asset is being referenced by any other tables with the same locale, if not then we can
            // remove the locale label and if no other labels exist also remove the asset from the Addressables system.
            var tableGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(table));
            var tableGroup = GetGroup(aaSettings, AssetTableGroupName);
            if (tableGroup != null)
            {
                foreach (var tableEntry in tableGroup.entries)
                {
                    var tableToCheck = tableEntry.guid == tableGuid ? table : AssetDatabase.LoadAssetAtPath<AddressableAssetTable>(AssetDatabase.GUIDToAssetPath(tableEntry.guid));
                    if (tableToCheck != null && tableToCheck.LocaleIdentifier == table.LocaleIdentifier)
                    {
                        foreach (var item in tableToCheck.AssetMap.Values)
                        {
                            // The asset is referenced elsewhere so we can not remove the label or asset.
                            if (item.guid == assetGuid)
                            {
                                // Check this is not the entry currently being removed.
                                if (tableToCheck == table && item.key == keyId)
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
                var assetLabel = FormatAssetLabel(table.LocaleIdentifier);
                aaSettings.AddLabel(assetLabel);
                assetEntry.SetLabel(assetLabel, false);
                UpdateAssetGroup(aaSettings, assetEntry);
            }
        }

        protected virtual void RemoveAssetFromTableInternal<TObject>(AddressableAssetTableT<TObject> table, uint keyId, TObject asset) where TObject : Object
        {
            var assetGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
            RemoveAssetFromTableInternal(table, keyId, assetGuid);
        }

        protected virtual void UpdateAssetGroup(AddressableAssetSettings settings, AddressableAssetEntry assetEntry)
        {
            if (settings == null ||assetEntry == null)
                return;

            var localesUsingAsset = assetEntry.labels.Where(IsLocaleLabel).ToList();
            if (localesUsingAsset.Count == 0)
            {
                var oldGroup = assetEntry.parentGroup;
                settings.RemoveAssetEntry(assetEntry.guid);
                if (oldGroup.entries.Count == 0)
                    settings.RemoveGroup(oldGroup);
                SendEvent(ModificationEvent.AssetRemoved, assetEntry);
                return;
            }

            AddressableAssetGroup newGroup;
            if (localesUsingAsset.Count == 1)
            {
                // Add to a locale specific group
                var localeId = LocaleLabelToId(localesUsingAsset[0]);
                newGroup = GetGroup(settings, FormatAssetTableName(localeId), true);
            }
            else
            {
                // Add to the shared assets group
                newGroup = GetGroup(settings, SharedAssetGroupName, true);
            }

            if (newGroup != assetEntry.parentGroup)
            {
                var oldGroup = assetEntry.parentGroup;
                settings.MoveEntry(assetEntry, newGroup, true);
                if (oldGroup.entries.Count == 0)
                    settings.RemoveGroup(oldGroup);

                SendEvent(ModificationEvent.AssetUpdated, assetEntry);
            }
        }

        protected virtual void AddOrUpdateTableInternal(LocalizedTable table)
        {
            if (!EditorUtility.IsPersistent(table))
            {
                Debug.LogError($"Only persistent assets can be addressable. The asset '{table.name}' needs to be saved on disk.");
                return;
            }

            var aaSettings = GetAddressableAssetSettings(true);
            if (aaSettings == null)
                return;

            var isStringTable = table is StringTableBase;

            // Has the asset already been added?
            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(table));
            var entry = aaSettings.FindAssetEntry(guid);
            var tableAdded = entry == null;
            if (entry == null)
            {
                var groupName = isStringTable ? StringTableGroupName : AssetTableGroupName;
                var group = GetGroup(aaSettings, groupName, true);
                entry = aaSettings.CreateOrMoveEntry(guid, group, true);
            }

            entry.address = $"{table.LocaleIdentifier.Code} - {table.TableName}";
            entry.labels.Clear(); // Locale may have changed so clear the old one.

            // Label the table type
            var label = isStringTable ? LocalizedStringDatabase.StringTableLabel : LocalizedAssetDatabase.AssetTableLabel;
            aaSettings.AddLabel(label);
            entry.SetLabel(label, true);

            // Label the locale
            var localeLabel = FormatAssetLabel(table.LocaleIdentifier);
            aaSettings.AddLabel(localeLabel);
            entry.SetLabel(localeLabel, true);

            if (tableAdded)
                SendEvent(ModificationEvent.TableAdded, table);
        }

        protected virtual void RemoveTableInternal(LocalizedTable table)
        {
            var aaSettings = GetAddressableAssetSettings(true);
            if (aaSettings == null)
                return;

            var assetTable = table as AddressableAssetTable;
            if (assetTable != null)
            {
                foreach (var entries in assetTable.AssetMap)
                {
                    RemoveAssetFromTableInternal(assetTable, entries.Key, entries.Value.guid);
                }
            }

            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(table));
            var entry = aaSettings.FindAssetEntry(guid);
            if (entry != null)
            {
                aaSettings.RemoveAssetEntry(guid);
                SendEvent(ModificationEvent.TableRemoved, table);
            }
        }

        protected virtual List<TLocalizedTable> GetAssetTablesInternal<TLocalizedTable>() where TLocalizedTable : LocalizedTable
        {
            var foundTables = new List<TLocalizedTable>();

            var settings = GetAddressableAssetSettings(false);
            if (settings == null)
                return foundTables;

            var allEntries = new List<AddressableAssetEntry>();
            settings.GetAllAssets(allEntries);
            foreach (var entry in allEntries)
            {
                if (entry.labels.Contains(LocalizedStringDatabase.StringTableLabel) || entry.labels.Contains(LocalizedAssetDatabase.AssetTableLabel))
                {
                    var asset = AssetDatabase.LoadAssetAtPath<TLocalizedTable>(entry.AssetPath);
                    if (asset != null)
                        foundTables.Add(asset);
                }
            }

            return foundTables;
        }

        protected virtual List<AssetTableCollection> GetAssetTablesCollectionInternal<TLocalizedTable>() where TLocalizedTable : LocalizedTable
        {
            var assetTablesCollection = new List<AssetTableCollection>();
            var assetTables = GetAssetTablesInternal<TLocalizedTable>();

            // Collate by type and table name
            var typeLookup = new Dictionary<Type, Dictionary<string, AssetTableCollection>>();
            foreach (var table in assetTables)
            {
                AssetTableCollection tableCollection;
                if (typeLookup.TryGetValue(table.GetType(), out var nameLookup))
                {
                    if (!nameLookup.TryGetValue(table.TableName, out tableCollection))
                    {
                        tableCollection = new AssetTableCollection() { TableType = table.GetType(), Keys = table.Keys };
                        assetTablesCollection.Add(tableCollection);
                        nameLookup[table.TableName] = tableCollection;
                    }
                }
                else
                {
                    tableCollection = new AssetTableCollection() { TableType = table.GetType(), Keys = table.Keys };
                    assetTablesCollection.Add(tableCollection);

                    nameLookup = new Dictionary<string, AssetTableCollection>();
                    nameLookup[table.TableName] = tableCollection;
                    typeLookup[table.GetType()] = nameLookup;
                }

                if (tableCollection.Keys != table.Keys)
                {
                    Debug.LogError($"Table '{table.TableName}' does not use the same KeyDatabase as other tables with the same name and type. Tables must share the same KeyDatabase. This table will be ignored.", table);
                    continue;
                }

                // Add to table
                tableCollection.Tables.Add(table);
            }
            return assetTablesCollection;
        }

        internal static LocalizedTable CreateAssetTableFilePanel(Locale selectedLocales, KeyDatabase keyDatabase, string tableName, Type tableType, string defaultDirectory)
        {
            return Instance.CreateAssetTableFilePanelInternal(selectedLocales, keyDatabase, tableName, tableType, defaultDirectory);
        }

        internal static void CreateAssetTablesFolderPanel(List<Locale> selectedLocales, KeyDatabase keyDatabase, string tableName, string tableType)
        {
            Instance.CreateAssetTablesFolderPanelInternal(selectedLocales, keyDatabase, tableName, Type.GetType(tableType));
        }

        internal static void CreateAssetTablesFolderPanel(List<Locale> selectedLocales, KeyDatabase keyDatabase, string tableName, Type tableType)
        {
            Instance.CreateAssetTablesFolderPanelInternal(selectedLocales, keyDatabase, tableName, tableType);
        }

        protected virtual LocalizedTable CreateAssetTableFilePanelInternal(Locale selectedLocales, KeyDatabase keyDatabase, string tableName, Type tableType, string defaultDirectory)
        {
            var name = string.Format(k_AssetTableNameFormat, selectedLocales.Identifier.Code, tableName, ObjectNames.NicifyVariableName(tableType.Name));
            var assetPath = EditorUtility.SaveFilePanel(s_Texts.saveTableDialog, defaultDirectory, name, "asset");
            return string.IsNullOrEmpty(assetPath) ? null : CreateAssetTable(selectedLocales, keyDatabase, tableName, tableType, assetPath);
        }

        protected virtual LocalizedTable CreateAssetTableInternal(Locale selectedLocales, KeyDatabase keyDatabase, string tableName, Type tableType, string assetPath)
        {
            var relativePath = MakePathRelative(assetPath);
            var table = (LocalizedTable)ScriptableObject.CreateInstance(tableType);
            table.TableName = tableName;
            table.Keys = keyDatabase;
            table.LocaleIdentifier = selectedLocales.Identifier;
            table.name = Path.GetFileNameWithoutExtension(assetPath);

            AssetDatabase.CreateAsset(table, relativePath);
            AddOrUpdateTableInternal(table);
            return table;
        }

        protected virtual void CreateAssetTablesFolderPanelInternal(List<Locale> selectedLocales, KeyDatabase keyDatabase, string tableName, Type tableType)
        {
            var assetDirectory = EditorUtility.SaveFolderPanel(s_Texts.saveTablesDialog, "Assets/", "");
            if (!string.IsNullOrEmpty(assetDirectory))
                CreateAssetTablesInternal(selectedLocales, keyDatabase, tableName, tableType, assetDirectory, true, true);
        }

        protected virtual List<LocalizedTable> CreateAssetTablesInternal(List<Locale> selectedLocales, KeyDatabase keyDatabase, string tableName, Type tableType, string assetDirectory, bool showProgressBar, bool showInTablesWindow)
        {
            List<LocalizedTable> createdTables;

            try
            {
                // TODO: Check that no tables already exist with the same name, locale and type.
                AssetDatabase.StartAssetEditing(); // Batch the assets into a single asset operation
                var relativePath = MakePathRelative(assetDirectory);

                // Create a new Key Database?
                if (keyDatabase == null)
                {
                    var keyDbPath = Path.Combine(relativePath, tableName + " Keys.asset");
                    keyDatabase = ScriptableObject.CreateInstance<KeyDatabase>();
                    CreateAsset(keyDatabase, keyDbPath);
                }

                Debug.Assert(keyDatabase != null, "Expected a key database");

                // Create the instances
                if (showProgressBar)
                    EditorUtility.DisplayProgressBar(s_Texts.progressTitle, "Creating Tables", 0);

                createdTables = new List<LocalizedTable>(selectedLocales.Count);
                foreach (var locale in selectedLocales)
                {
                    var table = (LocalizedTable)ScriptableObject.CreateInstance(tableType);
                    table.TableName = tableName;
                    table.Keys = keyDatabase;
                    table.LocaleIdentifier = locale.Identifier;
                    table.name = string.Format(k_AssetTableNameFormat, locale.Identifier.Code, tableName, ObjectNames.NicifyVariableName(tableType.Name));
                    createdTables.Add(table);
                }

                // Save as assets
                if (showProgressBar)
                    EditorUtility.DisplayProgressBar(s_Texts.progressTitle, "Saving Tables", 0);

                for (int i = 0; i < createdTables.Count; ++i)
                {
                    var tbl = createdTables[i];

                    if (showProgressBar)
                        EditorUtility.DisplayProgressBar(s_Texts.progressTitle, $"Saving Table {tbl.name}", i / (float)createdTables.Count);

                    var assetPath = Path.Combine(relativePath, tbl.name + ".asset");
                    assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
                    CreateAsset(tbl, assetPath);
                    AddOrUpdateTableInternal(tbl);
                }
                AssetDatabase.StopAssetEditing();

                if (showInTablesWindow)
                    AssetTablesWindow.ShowWindow(createdTables[0]);
            }
            finally
            {
                if (showProgressBar)
                    EditorUtility.ClearProgressBar();
            }

            return createdTables;
        }

        protected virtual void CreateAsset(Object asset, string path)
        {
            AssetDatabase.CreateAsset(asset, path);
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