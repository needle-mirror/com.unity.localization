using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor.Localization.UI;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization
{
    /// <summary>
    /// Editor interface to a collection of tables which all share the same <see cref="SharedTableData"/>.
    /// </summary>
    public abstract class LocalizationTableCollection : ScriptableObject, ISerializationCallbackReceiver
    {
        /// <summary>
        /// Represents a single Key and its localized values when using GetRowEnumerator.
        /// </summary>
        /// <typeparam name="TEntry"></typeparam>
        public class Row<TEntry> where TEntry : TableEntry
        {
            /// <summary>
            /// The <see cref="LocaleIdentifier"/> for each table value in <see cref="TableEntries"/>.
            /// The order of the tables is guaranteed not to change.
            /// </summary>
            public LocaleIdentifier[] TableEntriesReference { get; internal set; }

            /// <summary>
            /// The Key for the current row.
            /// </summary>
            public SharedTableData.SharedTableEntry KeyEntry { get; internal set; }

            /// <summary>
            /// The entries taken from all the tables for the current <see cref="KeyEntry"/>.
            /// The value may be null, such as when the table does not have a value for the current key.
            /// </summary>
            public TEntry[] TableEntries { get; internal set; }
        }

        [SerializeField]
        LazyLoadReference<SharedTableData> m_SharedTableData;

        [SerializeField]
        List<LazyLoadReference<LocalizationTable>> m_Tables = new List<LazyLoadReference<LocalizationTable>>();

        [SerializeReference]
        List<CollectionExtension> m_Extensions = new List<CollectionExtension>();

        [SerializeField]
        string m_Group;

        ReadOnlyCollection<LazyLoadReference<LocalizationTable>> m_ReadOnlyTables;
        ReadOnlyCollection<CollectionExtension> m_ReadOnlyExtensions;

        internal protected abstract Type TableType { get; }

        internal protected abstract Type RequiredExtensionAttribute { get; }

        /// <summary>
        /// The Addressables group that the tables will be added to for this collection.
        /// </summary>
        protected abstract string DefaultAddressablesGroupName { get; }

        internal protected abstract string DefaultGroupName { get; }

        internal (bool valid, string error) IsValid
        {
            get
            {
                if (SharedData != null)
                    return (true, null);
                return (false, "SharedTableData is null");
            }
        }

        /// <summary>
        /// All tables that are part of this collection.
        /// Tables are stored as LazyLoadReferences so that they are only loaded when required and not when the collection is loaded.
        /// </summary>
        public ReadOnlyCollection<LazyLoadReference<LocalizationTable>> Tables
        {
            get
            {
                if (m_ReadOnlyTables == null)
                {
                    RemoveBrokenTables();
                    m_ReadOnlyTables = m_Tables.AsReadOnly();
                }
                return m_ReadOnlyTables;
            }
        }

        /// <summary>
        /// Extensions attached to the collection. Extensions can be used for attaching additional data or functionality to a collection.
        /// </summary>
        public ReadOnlyCollection<CollectionExtension> Extensions
        {
            get
            {
                if (m_ReadOnlyExtensions == null)
                    m_ReadOnlyExtensions = m_Extensions.AsReadOnly();
                return m_ReadOnlyExtensions;
            }
        }

        /// <summary>
        /// The name of this collection of Tables.
        /// </summary>
        public string TableCollectionName { get => SharedData.TableCollectionName; set => SharedData.TableCollectionName = value; }

        /// <summary>
        /// Reference that can be used to refer to this table collection.
        /// </summary>
        public TableReference TableCollectionNameReference => SharedData.TableCollectionNameGuid;

        /// <summary>
        /// The <see cref="SharedTableData"/> that is used by all tables in this collection.
        /// </summary>
        public virtual SharedTableData SharedData { get => m_SharedTableData.asset; internal set => m_SharedTableData.asset = value; }

        /// <summary>
        /// Collections can be added to groups which will be used when showing the list of collections in the Localization Window.
        /// For example all collections with the Group "UI" would be shown under a UI menu in the `Selected Table Collection` field.
        /// </summary>
        public string Group { get => m_Group; set => m_Group = value; }

        protected virtual void OnEnable()
        {
            if (string.IsNullOrEmpty(m_Group))
            {
                // Default group if one is not set
                m_Group = DefaultGroupName;
            }
        }

        /// <summary>
        /// Changes the table collection name.
        /// This will change <see cref="SharedTableData.TableCollectionName"/> and update the Addressables data for all tables within the collection.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="createUndo"></param>
        public void SetTableCollectionName(string name, bool createUndo = false)
        {
            if (name == TableCollectionName)
                return;

            var undoGroup = Undo.GetCurrentGroup();
            if (createUndo)
                Undo.RecordObject(SharedData, "Change Table Collection Name");

            EditorUtility.SetDirty(SharedData);

            SharedData.TableCollectionName = name;
            RefreshAddressables(createUndo);

            if (createUndo)
                Undo.CollapseUndoOperations(undoGroup);
        }

        /// <summary>
        /// Sets the preload flag for all tables in this collection.
        /// </summary>
        /// <param name="preload">Should the tables be preloaded? True for preloading or false to load on demand.</param>
        /// <param name="createUndo">Create an undo point?</param>
        public virtual void SetPreloadTableFlag(bool preload, bool createUndo = false)
        {
            RemoveBrokenTables();
            foreach (var table in m_Tables)
            {
                LocalizationEditorSettings.SetPreloadTableFlag(table.asset, preload, createUndo);
            }
        }

        /// <summary>
        /// Are the tables in the collection set to preload?
        /// </summary>
        /// <returns>True if <b>all</b> tables are set to preload else false.</returns>
        public virtual bool IsPreloadTableFlagSet()
        {
            RemoveBrokenTables();
            return m_Tables.TrueForAll(tbl => LocalizationEditorSettings.GetPreloadTableFlag(tbl.asset));
        }

        /// <summary>
        /// Adds the table to the collection and updates Addressable assets.
        /// The table will not be added if it is not using the same <see cref="SharedTableData"/> as the collection.
        /// </summary>
        /// <param name="table">The table to add to the collection.</param>
        /// <param name="createUndo">Should an Undo operation be created?</param>
        /// <param name="postEvent">Should the <see cref="LocalizationEditorEvents.TableAddedToCollection"/> event be sent after the table was added?</param>
        public virtual void AddTable(LocalizationTable table, bool createUndo = false, bool postEvent = true)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            if (!EditorUtility.IsPersistent(table))
                throw new AssetNotPersistentException(table);

            if (table.SharedData != SharedData)
                throw new Exception($"Can not add table {table}, it has different Shared Data. Table uses {table.SharedData.name} but collection uses {SharedData.name}");

            if (!CanAddTable(table))
                return;

            if (createUndo)
                Undo.RecordObject(this, "Add table to collection");

            EditorUtility.SetDirty(this);

            AddTableToAddressables(table, createUndo);
            m_Tables.Add(new LazyLoadReference<LocalizationTable> { asset = table });

            if (postEvent)
                LocalizationEditorSettings.EditorEvents.RaiseTableAddedToCollection(this, table);
        }

        /// <summary>
        /// Creates a table in the collection.
        /// </summary>
        /// <param name="localeIdentifier"></param>
        public virtual LocalizationTable AddNewTable(LocaleIdentifier localeIdentifier)
        {
            var defaultDirectory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(this));
            var relativePath = PathHelper.MakePathRelative(defaultDirectory);
            var tableName = AddressHelper.GetTableAddress(TableCollectionName, localeIdentifier);
            var path = Path.Combine(relativePath, tableName + ".asset");
            return AddNewTable(localeIdentifier, path);
        }

        /// <summary>
        /// Creates a table in the collection.
        /// </summary>
        /// <param name="localeIdentifier"></param>
        /// <param name="path"></param>
        public virtual LocalizationTable AddNewTable(LocaleIdentifier localeIdentifier, string path)
        {
            if (ContainsTable(localeIdentifier))
                throw new Exception("Can not add new table. The same LocaleIdentifier is already in use.");

            var table = CreateInstance(TableType) as LocalizationTable;
            table.LocaleIdentifier = localeIdentifier;
            table.SharedData = SharedData;

            LocalizationEditorSettings.Instance.CreateAsset(table, path);
            AddTable(table);
            return table;
        }

        /// <summary>
        /// Removes the table from the collection and updates Addressables assets.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="createUndo"></param>
        /// <param name="postEvent">Should the <see cref="LocalizationEditorEvents.TableRemovedFromCollection"/> event be sent after the table has been removed?</param>
        public virtual void RemoveTable(LocalizationTable table, bool createUndo = false, bool postEvent = true)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            // We use the instance id so as not to force the tables to be loaded.
            var tableInstanceID = table.GetInstanceID();
            var index = m_Tables.FindIndex(t => t.GetInstanceId() == tableInstanceID);

            if (index == -1)
                return;

            if (createUndo)
                Undo.RecordObject(this, "Remove table from collection");

            EditorUtility.SetDirty(this);

            RemoveTableFromAddressables(table, false);
            m_Tables.RemoveAt(index);
            if (postEvent)
                LocalizationEditorSettings.EditorEvents.RaiseTableRemovedFromCollection(this, table);
        }

        /// <summary>
        /// Returns the table with the matching <see cref="LocaleIdentifier"/>.
        /// </summary>
        /// <param name="localeIdentifier">The locale identifier that the table must have.</param>
        /// <returns>The table with the matching <see cref="LocaleIdentifier"/> or null if one does not exist in the collection.</returns>
        public virtual LocalizationTable GetTable(LocaleIdentifier localeIdentifier)
        {
            foreach (var tbl in m_Tables)
            {
                if (tbl.asset?.LocaleIdentifier == localeIdentifier)
                    return tbl.asset;
            }
            return null;
        }

        /// <summary>
        /// Forces Addressables data to be updated for this collection.
        /// This will ensure that <see cref="SharedData"/> and <see cref="Tables"/> are both part of Addressables and correctly labeled.
        /// </summary>
        /// <param name="createUndo"></param>
        public void RefreshAddressables(bool createUndo = false)
        {
            RemoveBrokenTables();
            AddSharedTableDataToAddressables();
            foreach (var table in m_Tables)
            {
                AddTableToAddressables(table.asset, createUndo);
            }
        }

        /// <summary>
        /// Checks if a table with the same instance Id exists in the collection.
        /// This check should be fast as a table does not need to be loaded to have its instance Id compared.
        /// </summary>
        /// <param name="table">The table to look for.</param>
        /// <returns></returns>
        public bool ContainsTable(LocalizationTable table)
        {
            // We use the instance id so as not to force the tables to be loaded.
            var tableInstanceID = table.GetInstanceID();
            return m_Tables.Any(t => t.GetInstanceId() == tableInstanceID);
        }

        /// <summary>
        /// Checks if a table with the same <see cref="LocaleIdentifier"/> exists in the collection.
        /// </summary>
        /// <param name="localeIdentifier">The Id to match against.</param>
        /// <returns></returns>
        public bool ContainsTable(LocaleIdentifier localeIdentifier) => GetTable(localeIdentifier) != null;

        /// <summary>
        /// Attaches the provided extension to the collection.
        /// </summary>
        /// <param name="extension">The extension to add to the collection.</param>
        /// <exception cref="ArgumentException">Thrown if the extension does not have correct attribute.</exception>
        public void AddExtension(CollectionExtension extension)
        {
            if (extension == null)
                throw new ArgumentNullException(nameof(extension));

            if (!Attribute.IsDefined(extension.GetType(), RequiredExtensionAttribute))
                throw new ArgumentException($"Can not add extension. It requires the Attribute {RequiredExtensionAttribute}.", nameof(extension));
            m_Extensions.Add(extension);
        }

        /// <summary>
        /// Removes the extension from <see cref="Extensions"/>.
        /// </summary>
        /// <param name="extension"></param>
        public void RemoveExtension(CollectionExtension extension) => m_Extensions.Remove(extension);

        protected static IEnumerable<Row<TEntry>> GetRowEnumerator<TTable, TEntry>(IEnumerable<TTable> tables)
            where TTable : DetailedLocalizationTable<TEntry>
            where TEntry : TableEntry
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));

            SharedTableData sharedTableData = null;

            // Prepare the tables - Sort the keys and table entries
            var sortedTableEntries = new List<IOrderedEnumerable<TEntry>>();
            foreach (var table in tables)
            {
                if (sharedTableData == null)
                {
                    sharedTableData = table.SharedData;
                }
                else if (sharedTableData != table.SharedData)
                {
                    throw new Exception("All tables must share the same SharedData.");
                }

                if (table != null)
                {
                    var s = table.Values.OrderBy(e => e.KeyId);
                    sortedTableEntries.Add(s);
                }
            }

            var sortedKeyEntries = sharedTableData.Entries.OrderBy(e => e.Id);

            var currentTableRowIterator = sortedTableEntries.Select(o =>
            {
                var itr = o.GetEnumerator();
                itr.MoveNext();
                return itr;
            }).ToArray();

            var currentRow = new Row<TEntry>
            {
                TableEntriesReference = tables.Select(t => t.LocaleIdentifier).ToArray(),
                TableEntries = new TEntry[sortedTableEntries.Count]
            };

            using (StringBuilderPool.Get(out var warningMessage))
            {
                // Extract the table row values for this key.
                // If the table has a key value then add it to currentTableRow otherwise use null.
                foreach (var keyRow in sortedKeyEntries)
                {
                    currentRow.KeyEntry = keyRow;

                    // Extract the string table entries for this row
                    for (int i = 0; i < currentRow.TableEntries.Length; ++i)
                    {
                        var tableRowItr = currentTableRowIterator[i];

                        // Skip any table entries that may not not exist in Shared Data
                        while (tableRowItr != null && tableRowItr.Current?.KeyId < keyRow.Id)
                        {
                            warningMessage.AppendLine($"{tableRowItr.Current.Table.name} - {tableRowItr.Current.KeyId} - {tableRowItr.Current.Data.Localized}");
                            if (!tableRowItr.MoveNext())
                            {
                                currentTableRowIterator[i] = null;
                                break;
                            }
                        }

                        if (tableRowItr?.Current?.KeyId == keyRow.Id)
                        {
                            currentRow.TableEntries[i] = tableRowItr.Current;
                            if (!tableRowItr.MoveNext())
                            {
                                currentTableRowIterator[i] = null;
                            }
                        }
                        else
                        {
                            currentRow.TableEntries[i] = null;
                        }
                    }

                    yield return currentRow;
                }

                // Any warning messages?
                if (warningMessage.Length > 0)
                {
                    warningMessage.Insert(0, "Found entries in Tables that were missing a Shared Table Data Entry. These entries were ignored:\n");
                    Debug.LogWarning(warningMessage.ToString(), sharedTableData);
                }
            }
        }

        /// <summary>
        /// Can this table be added to the collection?
        /// Checks if the table is already in the collection or if a table with the same <see cref="LocaleIdentifier"/> is in the collection.
        /// </summary>
        /// <param name="table">The table to add.</param>
        /// <returns></returns>
        protected virtual bool CanAddTable(LocalizationTable table)
        {
            // Don't add the same table or if a table with the same locale id exists.
            if (m_Tables.Any(tbl => tbl.asset == table || tbl.asset?.LocaleIdentifier == table.LocaleIdentifier))
                return false;
            return table.GetType().IsAssignableFrom(TableType);
        }

        /// <summary>
        /// Adds <see cref="SharedData"/> to Addressables.
        /// </summary>
        /// <param name="sharedTableData"></param>
        /// <param name="createUndo"></param>
        internal protected virtual void AddSharedTableDataToAddressables()
        {
            var aaSettings = LocalizationEditorSettings.Instance.GetAddressableAssetSettings(true);
            if (aaSettings == null)
                return;

            // Add the shared table data
            var sharedDataGuid = LocalizationEditorSettings.Instance.GetAssetGuid(SharedData);
            var sharedDataEntry = aaSettings.FindAssetEntry(sharedDataGuid);
            if (sharedDataEntry == null)
            {
                // Add to the shared assets group
                var sharedGroup = LocalizationEditorSettings.Instance.GetGroup(aaSettings, LocalizationEditorSettings.SharedAssetGroupName, true, false);
                sharedDataEntry = aaSettings.CreateOrMoveEntry(sharedDataGuid, sharedGroup);
                sharedDataEntry.address = SharedData.name;
            }
        }

        /// <summary>
        /// Add the table to the Addressable assets system.
        /// </summary>
        /// <param name="table">The table to add.</param>
        /// <param name="createUndo">Should an Undo operation be recorded?</param>
        protected virtual void AddTableToAddressables(LocalizationTable table, bool createUndo)
        {
            var aaSettings = LocalizationEditorSettings.Instance.GetAddressableAssetSettings(true);
            if (aaSettings == null)
                return;

            if (createUndo)
                Undo.RecordObject(aaSettings, "Update table");

            // Has the asset already been added?
            var tableEntry = LocalizationEditorSettings.Instance.GetAssetEntry(table);
            var tableAdded = tableEntry == null;
            if (tableEntry == null)
            {
                var groupName = DefaultAddressablesGroupName;
                var group = LocalizationEditorSettings.Instance.GetGroup(aaSettings, groupName, true, createUndo);

                if (createUndo)
                    Undo.RecordObject(group, "Update table");

                var tableGuid = LocalizationEditorSettings.Instance.GetAssetGuid(table);
                tableEntry = aaSettings.CreateOrMoveEntry(tableGuid, group, true);
            }
            else if (createUndo)
            {
                Undo.RecordObject(tableEntry.parentGroup, "Update table");
            }

            tableEntry.address = AddressHelper.GetTableAddress(table.TableCollectionName, table.LocaleIdentifier);
            tableEntry.labels.RemoveWhere(AddressHelper.IsLocaleLabel); // Locale may have changed so clear the old ones.

            // Label the locale
            var localeLabel = AddressHelper.FormatAssetLabel(table.LocaleIdentifier);
            aaSettings.AddLabel(localeLabel);
            tableEntry.SetLabel(localeLabel, true);
        }

        /// <summary>
        /// Remove the table from the Addressables system.
        /// </summary>
        /// <param name="table">The table to remove.</param>
        /// <param name="createUndo">Should an Undo operation be recorded?</param>
        protected virtual void RemoveTableFromAddressables(LocalizationTable table, bool createUndo)
        {
            if (table == null)
                return;

            var settings = LocalizationEditorSettings.Instance.GetAddressableAssetSettings(false);
            if (settings == null)
                return;

            var tableEntry = LocalizationEditorSettings.Instance.GetAssetEntry(table);
            if (tableEntry == null)
                return;

            if (createUndo)
            {
                Undo.RecordObjects(new UnityEngine.Object[] { settings, tableEntry.parentGroup }, "Remove table");
            }

            settings.RemoveAssetEntry(tableEntry.guid);
        }

        /// <summary>
        /// Called when the asset is created or imported into a project(via OnPostprocessAllAssets).
        /// </summary>
        internal protected virtual void ImportCollectionIntoProject()
        {
            RefreshAddressables();

            var missingLocales = new List<LocaleIdentifier>();
            foreach (var table in Tables)
            {
                var locale = LocalizationEditorSettings.GetLocale(table.asset.LocaleIdentifier.Code);
                if (locale == null)
                {
                    missingLocales.Add(new LocaleIdentifier(table.asset.LocaleIdentifier.Code));
                }
            }

            if (missingLocales.Count > 0)
            {
                var defaultDirectory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(this));
                var relativePath = PathHelper.MakePathRelative(defaultDirectory);

                LocaleGeneratorWindow.ExportSelectedLocales(relativePath, missingLocales);

                var sb = new StringBuilder();
                sb.AppendLine($"The following missing Locales have been added to the project because they are used by the Collection {TableCollectionName}:");
                missingLocales.ForEach(l => sb.AppendLine(l.ToString()));
                Debug.Log(sb.ToString());
            }
        }

        /// <summary>
        /// Called to remove the asset from a project, such as when it is about to be deleted.
        /// </summary>
        internal protected virtual void RemoveCollectionFromProject()
        {
            foreach (var tbl in m_Tables)
            {
                if (tbl.asset != null)
                    RemoveTableFromAddressables(tbl.asset, false);
            }
            LocalizationEditorSettings.EditorEvents.RaiseCollectionRemoved(this);
        }

        void RemoveBrokenTables()
        {
            // We cant do this in OnBeforeSerialize or OnAfterDeserialize as it uses ForceLoadFromInstanceID and this is not allowed to be called during serialization.
            int brokenCount = 0;
            for (int i = 0; i < m_Tables.Count; ++i)
            {
                if (m_Tables[i].isBroken)
                {
                    m_Tables.RemoveAt(i);
                    --i;
                    ++brokenCount;
                }
            }

            if (brokenCount > 0)
            {
                Debug.LogWarning($"{brokenCount} Broken table reference was found and removed for {TableCollectionName} collection. References to this table or its assets may have not been cleaned up.", this);
            }
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            m_ReadOnlyTables = null;
            m_ReadOnlyExtensions = null;
        }

        public override string ToString() => $"{TableCollectionName}({TableType.Name})";
    }
}
