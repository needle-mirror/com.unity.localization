using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Pseudo;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Pool;
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

        internal const string k_GameViewPref = "Localization-ShowLocaleMenuInGameView";

        static LocalizationEditorSettings s_Instance;

        // Cached searches to help performance.
        ReadOnlyCollection<Locale> m_ProjectLocales;

        // Allows for overriding the default behavior, used for testing.
        internal static LocalizationEditorSettings Instance
        {
            get => s_Instance ?? (s_Instance = new LocalizationEditorSettings());
            set => s_Instance = value;
        }

        internal LocalizationTableCollectionCache TableCollectionCache { get; set; } = new LocalizationTableCollectionCache();

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
            get => EditorPrefs.GetBool(k_GameViewPref, true);
            set => EditorPrefs.SetBool(k_GameViewPref, value);
        }

        /// <summary>
        /// Localization modification events.
        /// </summary>
        public static LocalizationEditorEvents EditorEvents { get; internal set; } = new LocalizationEditorEvents();

        internal static bool EnableAddressablesCreation { get; set; } = true;

        public LocalizationEditorSettings()
        {
            EditorEvents.LocaleSortOrderChanged += (sender, locale) => SortLocales();
            Undo.undoRedoPerformed += UndoRedoPerformed;
        }

        ~LocalizationEditorSettings()
        {
            Undo.undoRedoPerformed -= UndoRedoPerformed;
        }

        /// <summary>
        /// Add the Locale to the Addressables system, so that it can be used by the Localization system during runtime.
        /// </summary>
        /// <param name="locale">The <see cref="Locale"/> to add to Addressables so that it can be used by the Localization system.</param>
        /// <param name="createUndo">Used to indicate if an Undo operation should be created.</param>
        public static void AddLocale(Locale locale, bool createUndo = false) => Instance.AddLocaleInternal(locale, createUndo);

        /// <summary>
        /// Removes the locale from the Addressables system.
        /// </summary>
        /// <param name="locale">The <see cref="Locale"/> to remove from Addressables so that it is no longer used by the Localization system.</param>
        /// /// <param name="createUndo">Used to indicate if an Undo operation should be created.</param>
        public static void RemoveLocale(Locale locale, bool createUndo = false) => Instance.RemoveLocaleInternal(locale, createUndo);

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
        /// Returns all <see cref="StringTableCollection"/> assets in the project.
        /// </summary>
        /// <returns></returns>
        public static ReadOnlyCollection<StringTableCollection> GetStringTableCollections() => Instance.TableCollectionCache.StringTableCollections.AsReadOnly();

        /// <summary>
        /// Returns a <see cref="StringTableCollection"/> with the matching <see cref="TableReference"/>.
        /// </summary>
        /// <param name="tableNameOrGuid"></param>
        /// <returns>Found collection or null if one could not be found.</returns>
        public static StringTableCollection GetStringTableCollection(TableReference tableNameOrGuid) => Instance.TableCollectionCache.FindStringTableCollection(tableNameOrGuid);

        /// <summary>
        /// Returns all <see cref="AssetTableCollection"/> assets in the project.
        /// </summary>
        /// <returns></returns>
        public static ReadOnlyCollection<AssetTableCollection> GetAssetTableCollections() => Instance.TableCollectionCache.AssetTableCollections.AsReadOnly();

        /// <summary>
        /// Returns a <see cref="AssetTableCollection"/> with the matching <see cref="TableReference"/>.
        /// </summary>
        /// <param name="tableNameOrGuid"></param>
        /// <returns></returns>
        public static AssetTableCollection GetAssetTableCollection(TableReference tableNameOrGuid) => Instance.TableCollectionCache.FindAssetTableCollection(tableNameOrGuid);

        /// <summary>
        /// Returns the <see cref="LocalizationTableCollection"/> that the table is part of or null if the table has no collection.
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static LocalizationTableCollection GetCollectionFromTable(LocalizationTable table) => Instance.TableCollectionCache.FindCollectionForTable(table);

        /// <summary>
        /// Returns the <see cref="LocalizationTableCollection"/> that the <see cref="SharedTableData"/> is part of or null if one could not be found.
        /// </summary>
        /// <param name="sharedTableData"></param>
        /// <returns></returns>
        public static LocalizationTableCollection GetCollectionForSharedTableData(SharedTableData sharedTableData) => Instance.TableCollectionCache.FindCollectionForSharedTableData(sharedTableData);

        /// <summary>
        /// If a table does not belong to a <see cref="LocalizationTableCollection"/> then it is considered to be loose, it has no parent collection and will be ignored.
        /// This returns all loose tables that use the same <see cref="SharedTableData"/>, they can then be converted into a <see cref="LocalizationTableCollection"/>.
        /// </summary>
        /// <param name="sharedTableData"></param>
        /// <param name="foundTables"></param>
        public static void FindLooseStringTablesUsingSharedTableData(SharedTableData sharedTableData, IList<LocalizationTable> foundTables) => Instance.TableCollectionCache.FindLooseTablesUsingSharedTableData(sharedTableData, foundTables);

        /// <summary>
        /// Creates a <see cref="StringTableCollection"/> or <see cref="AssetTableCollection"/> from the loose tables.
        /// </summary>
        /// <param name="looseTables">Tables to create the collection from. All tables must be of the same type.</param>
        /// <returns></returns>
        public static LocalizationTableCollection CreateCollectionFromLooseTables(IList<LocalizationTable> looseTables, string path) => Instance.CreateCollectionFromLooseTablesInternal(looseTables, path);

        /// <summary>
        /// Creates a <see cref="StringTableCollection"/> using the project Locales.
        /// </summary>
        /// <param name="tableName">The name of the new collection.</param>
        /// <param name="assetDirectory">The directory to save the generated assets, must be in the project Assets directory.</param>
        /// <returns></returns>
        public static StringTableCollection CreateStringTableCollection(string tableName, string assetDirectory) => CreateStringTableCollection(tableName, assetDirectory, GetLocales());

        /// <summary>
        /// Creates a <see cref="StringTableCollection"/> using the provided Locales.
        /// </summary>
        /// <param name="tableName">The name of the new collection.</param>
        /// <param name="assetDirectory">The directory to save the generated assets, must be in the project Assets directory.</param>
        /// <param name="selectedLocales">The locales to generate the collection with. A <see cref="StringTable"/> will be created for each Locale.</param>
        /// <returns></returns>
        public static StringTableCollection CreateStringTableCollection(string tableName, string assetDirectory, IList<Locale> selectedLocales) => Instance.CreateCollection(typeof(StringTableCollection), tableName, assetDirectory, selectedLocales) as StringTableCollection;

        /// <summary>
        /// Creates a <see cref="AssetTableCollection"/> using the project Locales.
        /// </summary>
        /// <param name="tableName">The name of the new collection.</param>
        /// <param name="assetDirectory">The directory to save the generated assets, must be in the project Assets directory.</param>
        /// <returns></returns>
        public static AssetTableCollection CreateAssetTableCollection(string tableName, string assetDirectory) => CreateAssetTableCollection(tableName, assetDirectory, GetLocales());

        /// <summary>
        /// Creates a <see cref="AssetTableCollection"/> using the provided Locales.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="assetDirectory"></param>
        /// <param name="selectedLocales">The locales to generate the collection with. A <see cref="AssetTable"/> will be created for each Locale</param>
        /// <returns></returns>
        public static AssetTableCollection CreateAssetTableCollection(string tableName, string assetDirectory, IList<Locale> selectedLocales) => Instance.CreateCollection(typeof(AssetTableCollection), tableName, assetDirectory, selectedLocales) as AssetTableCollection;

        internal protected virtual LocalizationTableCollection CreateCollection(Type collectionType, string tableName, string assetDirectory, IList<Locale> selectedLocales)
        {
            if (collectionType.IsAssignableFrom(typeof(LocalizationTableCollection)))
                throw new ArgumentException($"{collectionType.Name} Must be derived from {nameof(LocalizationTableCollection)}", nameof(collectionType));

            if (string.IsNullOrEmpty(tableName))
                throw new ArgumentException("Can not be null or empty", nameof(tableName));

            var collection = ScriptableObject.CreateInstance(collectionType) as LocalizationTableCollection;
            List<LocalizationTable> createdTables;

            AssetDatabase.StartAssetEditing();

            // TODO: Check that no tables already exist with the same name, locale and type.
            var relativePath = MakePathRelative(assetDirectory);
            Directory.CreateDirectory(relativePath);

            var sharedDataPath = Path.Combine(relativePath, tableName + " Shared Data.asset");
            var SharedTableData = ScriptableObject.CreateInstance<SharedTableData>();
            SharedTableData.TableCollectionName = tableName;
            CreateAsset(SharedTableData, sharedDataPath);
            collection.SharedData = SharedTableData;
            collection.AddSharedTableDataToAddressables();

            // Extract the SharedTableData Guid and assign it so we can use it as a unique id for the table collection name.
            var sharedDataGuid = GetAssetGuid(SharedTableData);
            SharedTableData.TableCollectionNameGuid = Guid.Parse(sharedDataGuid);
            EditorUtility.SetDirty(SharedTableData); // We need to set it dirty so the change to TableCollectionNameGuid is saved.

            if (selectedLocales?.Count > 0)
            {
                createdTables = new List<LocalizationTable>(selectedLocales.Count);
                foreach (var locale in selectedLocales)
                {
                    var table = ScriptableObject.CreateInstance(collection.TableType) as LocalizationTable;
                    table.SharedData = SharedTableData;
                    table.LocaleIdentifier = locale.Identifier;
                    table.name = AddressHelper.GetTableAddress(tableName, locale.Identifier);
                    createdTables.Add(table);
                }

                for (int i = 0; i < createdTables.Count; ++i)
                {
                    var tbl = createdTables[i];

                    var assetPath = Path.Combine(relativePath, tbl.name + ".asset");
                    assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
                    CreateAsset(tbl, assetPath);
                    collection.AddTable(tbl, postEvent: false);
                }
            }

            // Save the collection
            collection.name = tableName;
            var collectionPath = Path.Combine(relativePath, collection.name + ".asset");
            CreateAsset(collection, collectionPath);

            AssetDatabase.StopAssetEditing();

            EditorEvents.RaiseCollectionAdded(collection);

            return collection;
        }

        /// <summary>
        /// Creates a new <see cref="StringTableCollection"/> or <see cref="AssetTableCollection"/> from the provided list of tables that do not currently belong to a collection.
        /// </summary>
        /// <param name="looseTables"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        protected virtual LocalizationTableCollection CreateCollectionFromLooseTablesInternal(IList<LocalizationTable> looseTables, string path)
        {
            if (looseTables == null || looseTables.Count == 0)
                return null;

            var isStringTable = typeof(StringTable).IsAssignableFrom(looseTables[0].GetType());

            var collectionType = isStringTable ? typeof(StringTableCollection) : typeof(AssetTableCollection);
            var collection = ScriptableObject.CreateInstance(collectionType) as LocalizationTableCollection;
            collection.SharedData = looseTables[0].SharedData;

            foreach (var table in looseTables)
            {
                if (table.SharedData != collection.SharedData)
                {
                    Debug.LogError($"Table {table.name} does not share the same Shared Table Data and can not be part of the new collection", table);
                    continue;
                }
                collection.AddTable(table, postEvent: false); // Don't post the event, we will send the Collection added only event
            }

            var relativePath = MakePathRelative(path);
            CreateAsset(collection, relativePath);
            EditorEvents.RaiseCollectionAdded(collection);
            return collection;
        }

        /// <summary>
        /// Adds/Removes the preload flag for the table.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="preload"></param>
        /// <param name="createUndo"></param>
        public static void SetPreloadTableFlag(LocalizationTable table, bool preload, bool createUndo = false)
        {
            Instance.SetPreloadTableInternal(table, preload, createUndo);
        }

        /// <summary>
        /// Returns true if the table is marked for preloading.
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static bool GetPreloadTableFlag(LocalizationTable table)
        {
            // TODO: We could just use the instance id so we dont need to load the whole table
            return Instance.GetPreloadTableFlagInternal(table);
        }

        /// <summary>
        /// Returns the <see cref="AssetTableCollection"/> that contains a table entry with the closest match to the provided text.
        /// Uses the Levenshtein distance method.
        /// </summary>
        /// <typeparam name="TTable"></typeparam>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public static (StringTableCollection collection, SharedTableData.SharedTableEntry entry, int matchDistance) FindSimilarKey(string keyName)
        {
            return Instance.FindSimilarKeyInternal(keyName);
        }

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
            return AddressableAssetSettingsDefaultObject.GetSettings(create && EnableAddressablesCreation);
        }

        internal virtual AddressableAssetEntry GetAssetEntry(Object asset) => GetAssetEntry(asset.GetInstanceID());

        internal virtual AddressableAssetEntry GetAssetEntry(int instanceId)
        {
            var settings = GetAddressableAssetSettings(false);
            if (settings == null)
                return null;

            var guid = GetAssetGuid(instanceId);
            return settings.FindAssetEntry(guid);
        }

        /// <summary>
        /// Returns the Addressables group with the matching name or creates a new one, if one could not be found.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="groupName"></param>
        /// <param name="create"></param>
        /// <param name="createUndo">Used to indicate if an Undo operation should be created.</param>
        /// <returns></returns>
        internal protected virtual AddressableAssetGroup GetGroup(AddressableAssetSettings settings, string groupName, bool create, bool createUndo)
        {
            var group = settings.FindGroup(groupName);
            if (group == null && create)
            {
                group = settings.CreateGroup(groupName, false, true, true, new List<AddressableAssetGroupSchema>() { ScriptableObject.CreateInstance<ContentUpdateGroupSchema>() }, typeof(BundledAssetGroupSchema));

                // Default to just the name of the group, no hashes.
                var schema = group.GetSchema<BundledAssetGroupSchema>();
                schema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.NoHash;

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
        internal protected virtual string FindUniqueAssetAddress(string address)
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
            if (locale == null)
                throw new ArgumentNullException(nameof(locale));

            if (!EditorUtility.IsPersistent(locale))
                throw new AssetNotPersistentException(locale);

            var aaSettings = GetAddressableAssetSettings(true);
            if (aaSettings == null)
                return;

            if (createUndo)
                Undo.RecordObject(aaSettings, "Add locale");

            var assetEntry = GetAssetEntry(locale);
            if (assetEntry == null)
            {
                var group = GetGroup(aaSettings, LocaleGroupName, true, createUndo);

                if (createUndo)
                    Undo.RecordObject(group, "Add locale");

                var guid = GetAssetGuid(locale);
                assetEntry = aaSettings.CreateOrMoveEntry(guid, group, true);
                assetEntry.address = locale.name;
            }

            // Clear the locales cache.
            m_ProjectLocales = null;

            if (!assetEntry.labels.Contains(LocalizationSettings.LocaleLabel))
            {
                aaSettings.AddLabel(LocalizationSettings.LocaleLabel);
                assetEntry.SetLabel(LocalizationSettings.LocaleLabel, true);
                EditorEvents.RaiseLocaleAdded(locale);
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

            var localeAssetEntry = GetAssetEntry(locale);
            if (localeAssetEntry == null)
                return;

            if (createUndo)
            {
                Undo.RecordObject(localeAssetEntry.parentGroup, "Remove locale");
            }

            // Clear the locale cache
            m_ProjectLocales = null;

            aaSettings.RemoveAssetEntry(localeAssetEntry.guid);
            EditorEvents.RaiseLocaleRemoved(locale);
        }

        /// <summary>
        /// <inheritdoc cref="GetLocales"/>
        /// </summary>
        /// <returns></returns>
        internal protected virtual ReadOnlyCollection<Locale> GetLocalesInternal()
        {
            if (m_ProjectLocales != null)
                return m_ProjectLocales;

            var foundLocales = new List<Locale>();

            var aaSettings = GetAddressableAssetSettings(true);
            if (aaSettings == null)
                return null;

            var foundAssets = new List<AddressableAssetEntry>();
            aaSettings.GetAllAssets(foundAssets, false, group => group != null, entry =>
            {
                return entry.labels.Contains(LocalizationSettings.LocaleLabel);
            });

            foreach (var localeAddressable in foundAssets)
            {
                if (!string.IsNullOrEmpty(localeAddressable.guid))
                {
                    var locale = AssetDatabase.LoadAssetAtPath<Locale>(AssetDatabase.GUIDToAssetPath(localeAddressable.guid));
                    if (locale != null && !(locale is PseudoLocale)) // Dont include Pseudo locales.
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

        void SortLocales()
        {
            if (m_ProjectLocales != null)
            {
                var localesList = m_ProjectLocales.ToList();
                localesList.Sort();
                m_ProjectLocales = localesList.AsReadOnly();
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
        protected virtual void SetPreloadTableInternal(LocalizationTable table, bool preload, bool createUndo = false)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table), "Can not set preload flag on a null table");

            var aaSettings = GetAddressableAssetSettings(true);
            if (aaSettings == null)
                return;

            var tableEntry = GetAssetEntry(table);
            if (tableEntry == null)
                throw new AddressableEntryNotFoundException(table);

            if (createUndo)
                Undo.RecordObjects(new Object[] { aaSettings, tableEntry.parentGroup }, "Set Preload flag");

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
        protected virtual bool GetPreloadTableFlagInternal(LocalizationTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table), "Can not get preload flag from a null table");

            var aaSettings = GetAddressableAssetSettings(true);
            if (aaSettings == null)
                return false;

            var tableEntry = GetAssetEntry(table);
            if (tableEntry == null)
                throw new AddressableEntryNotFoundException(table);

            return tableEntry.labels.Contains(LocalizationSettings.PreloadLabel);
        }

        /// <summary>
        ///  <inheritdoc cref=" FindSimilarKey"/>
        /// </summary>
        /// <param name="keyName"></param>
        /// <typeparam name="TTable"></typeparam>
        /// <returns></returns>
        protected virtual (StringTableCollection collection, SharedTableData.SharedTableEntry entry, int matchDistance) FindSimilarKeyInternal(string keyName)
        {
            // Check if we can find a matching key to the key name
            var collections = GetStringTableCollections();

            int currentMatchDistance = int.MaxValue;
            SharedTableData.SharedTableEntry currentEntry = null;
            StringTableCollection foundCollection = null;
            foreach (var tableCollection in collections)
            {
                var keys = tableCollection.SharedData;
                var foundKey = keys.FindSimilarKey(keyName, out int distance);
                if (foundKey != null && distance < currentMatchDistance)
                {
                    currentMatchDistance = distance;
                    currentEntry = foundKey;
                    foundCollection = tableCollection;
                }
            }
            return (foundCollection, currentEntry, currentMatchDistance);
        }

        void UndoRedoPerformed()
        {
            // Reset the locales as adding/removing a locale may have been undone.
            m_ProjectLocales = null;
        }

        internal virtual void CreateAsset(Object asset, string path)
        {
            AssetDatabase.CreateAsset(asset, path);
        }

        internal virtual string GetAssetGuid(int instanceId)
        {
            Debug.Assert(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(instanceId, out string guid, out long _), "Failed to extract the asset Guid");
            return guid;
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
