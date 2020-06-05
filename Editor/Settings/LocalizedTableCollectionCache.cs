using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization
{
    class LocalizedTableCollectionCache
    {
        // Contains table and shared table data guids that map to the parent collection.
        Dictionary<string, LocalizedTableCollection> m_GuidToCollection;

        List<StringTableCollection> m_StringTableCollections;
        List<AssetTableCollection> m_AssetTableCollections;

        public List<AssetTableCollection> AssetTableCollections
        {
            get
            {
                if (m_AssetTableCollections == null)
                    m_AssetTableCollections = LoadTableCollections<AssetTableCollection>();
                return m_AssetTableCollections;
            }
        }

        public List<StringTableCollection> StringTableCollections
        {
            get
            {
                if (m_StringTableCollections == null)
                    m_StringTableCollections = LoadTableCollections<StringTableCollection>();
                return m_StringTableCollections;
            }
        }

        public Dictionary<string, LocalizedTableCollection> CollectionDependencies
        {
            get
            {
                if (m_GuidToCollection == null)
                {
                    m_GuidToCollection = new Dictionary<string, LocalizedTableCollection>();
                    AssetTableCollections.ForEach(CacheDependencies);
                    StringTableCollections.ForEach(CacheDependencies);
                }
                return m_GuidToCollection;
            }
        }

        public LocalizedTableCollectionCache()
        {
            LocalizationEditorSettings.EditorEvents.CollectionAdded += AddToCache;
            LocalizationEditorSettings.EditorEvents.CollectionRemoved += OnCollectionRemoved;
            LocalizationEditorSettings.EditorEvents.TableAddedToCollection += OnTableAddedToCollection;
            LocalizationEditorSettings.EditorEvents.TableRemovedFromCollection += OnTableRemovedFromCollection;
        }

        ~LocalizedTableCollectionCache()
        {
            LocalizationEditorSettings.EditorEvents.CollectionAdded -= AddToCache;
            LocalizationEditorSettings.EditorEvents.CollectionRemoved -= OnCollectionRemoved;
            LocalizationEditorSettings.EditorEvents.TableAddedToCollection -= OnTableAddedToCollection;
            LocalizationEditorSettings.EditorEvents.TableRemovedFromCollection -= OnTableRemovedFromCollection;
        }

        public StringTableCollection FindStringTableCollection(TableReference tableReference) => FindTableCollection(StringTableCollections, tableReference);
        public AssetTableCollection FindAssetTableCollection(TableReference tableReference) => FindTableCollection(AssetTableCollections, tableReference);

        public LocalizedTableCollection FindCollectionForSharedTableData(SharedTableData sharedTableData)
        {
            if (sharedTableData == null)
                throw new ArgumentNullException(nameof(sharedTableData));

            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sharedTableData, out var guid, out long _))
            {
                return FindCollectionFromDependencyGuid(guid);
            }
            return null;
        }

        public LocalizedTableCollection FindCollectionForTable(LocalizedTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(table, out var guid, out long _))
            {
                return FindCollectionFromDependencyGuid(guid);
            }
            return null;
        }

        public LocalizedTableCollection FindCollectionForInstanceId(int tableInstanceID)
        {
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(tableInstanceID, out var guid, out long _))
            {
                return FindCollectionFromDependencyGuid(guid);
            }
            return null;
        }

        public LocalizedTableCollection FindCollectionFromDependencyGuid(string tableGuid)
        {
            if (CollectionDependencies.TryGetValue(tableGuid, out var collection))
                return collection;
            return null;
        }

        public void FindLooseTablesUsingSharedTableData(SharedTableData sharedTableData, IList<LocalizedTable> foundTables)
        {
            if (sharedTableData == null)
                throw new ArgumentNullException(nameof(sharedTableData));
            if (foundTables == null)
                throw new ArgumentNullException(nameof(foundTables));

            // We need to find all tables that reference the shared table data that do not belong to a collection
            var foundAssets = AssetDatabase.FindAssets("t:LocalizedTable");
            foreach (var guid in foundAssets)
            {
                // Ignore tables we know about
                if (CollectionDependencies.ContainsKey(guid))
                    continue;

                // Filter by table type
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var loadedTable = AssetDatabase.LoadAssetAtPath<LocalizedTable>(path);
                if (loadedTable.SharedData == sharedTableData)
                    foundTables.Add(loadedTable);
            }
        }

        public void Clear()
        {
            m_GuidToCollection = null;
            m_StringTableCollections = null;
            m_AssetTableCollections = null;
        }

        void OnTableAddedToCollection(LocalizedTableCollection collection, LocalizedTable table)
        {
            if (m_GuidToCollection != null)
            {
                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(table, out var guid, out long _))
                {
                    Debug.LogError("Failed to extract table guid: " + table?.name, table);
                    return;
                }

                m_GuidToCollection[guid] = collection;
            }
        }

        void OnTableRemovedFromCollection(LocalizedTableCollection collection, LocalizedTable table)
        {
            if (m_GuidToCollection != null)
            {
                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(table, out var guid, out long _))
                {
                    Debug.LogError("Failed to extract table guid: " + table?.name, table);
                    return;
                }
                m_GuidToCollection.Remove(guid);
            }
        }

        void OnCollectionRemoved(LocalizedTableCollection collection)
        {
            if (collection is StringTableCollection stringTableCollection)
            {
                if (m_StringTableCollections != null && m_StringTableCollections.Contains(stringTableCollection))
                {
                    m_StringTableCollections.Remove(stringTableCollection);
                    m_GuidToCollection = null; // Clear cache
                }
            }
            else if (collection is AssetTableCollection assetTableCollection)
            {
                if (m_AssetTableCollections != null && m_AssetTableCollections.Contains(assetTableCollection))
                {
                    m_AssetTableCollections.Remove(assetTableCollection);
                    m_GuidToCollection = null; // Clear cache
                }
            }
            else
            {
                throw new System.Exception("Unhandled collection type: " + collection.GetType());
            }
        }

        protected virtual string[] FindAssets(string filter) => AssetDatabase.FindAssets(filter);

        protected virtual TCollection FindTableCollection<TCollection>(List<TCollection> list, TableReference tableReference) where TCollection : LocalizedTableCollection
        {
            Debug.Assert(list != null);

            string name = null;
            if (tableReference.ReferenceType == TableReference.Type.Guid)
            {
                var guid = TableReference.StringFromGuid(tableReference.TableCollectionNameGuid);
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var sharedTableData = AssetDatabase.LoadAssetAtPath<SharedTableData>(AssetDatabase.GUIDToAssetPath(guid));
                if (sharedTableData == null)
                {
                    Debug.LogError($"Could not load Shared Table Data at path '{path}' with guid '{guid}'.");
                    return null;
                }
                name = sharedTableData.TableCollectionName;
            }
            else
            {
                name = tableReference.TableCollectionName;
            }

            return list.FirstOrDefault(stc => stc.TableCollectionName == name);
        }

        protected virtual List<TCollection> LoadTableCollections<TCollection>() where TCollection : LocalizedTableCollection
        {
            var foundCollections = new List<TCollection>();
            var foundAssets = FindAssets($"t:{typeof(TCollection).Name}");
            foreach (var guid in foundAssets)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var collection = AssetDatabase.LoadAssetAtPath<TCollection>(path);
                if (collection == null)
                {
                    Debug.LogError($"Could not load collection as type {typeof(TCollection).Name} at path {path}.");
                    continue;
                }

                var validState = collection.IsValid;
                if (!validState.valid)
                {
                    Debug.LogWarning($"Collection {collection.name} is invalid and will be ignored because {validState.error}.");
                    continue;
                }
                foundCollections.Add(collection);
            }

            return foundCollections.OrderBy(col => col.TableCollectionName).ToList();
        }

        protected virtual void CacheDependencies(LocalizedTableCollection collection)
        {
            foreach (var table in collection.Tables)
            {
                var guid = LocalizationEditorSettings.Instance.GetAssetGuid(table.GetInstanceId());
                m_GuidToCollection[guid] = collection;
            }

            m_GuidToCollection[TableReference.StringFromGuid(collection.SharedData.TableCollectionNameGuid)] = collection;
        }

        protected virtual void AddToCache(LocalizedTableCollection collection)
        {
            var validState = collection.IsValid;
            if (!validState.valid)
            {
                Debug.LogWarning($"Collection {collection.name} is invalid and will be ignored because {validState.error}.");
                return;
            }

            if (collection is StringTableCollection stringTableCollection)
            {
                if (m_StringTableCollections != null && !m_StringTableCollections.Contains(stringTableCollection))
                {
                    m_StringTableCollections.Add(stringTableCollection);
                    if (m_GuidToCollection != null)
                    {
                        CacheDependencies(stringTableCollection);
                    }
                }
            }
            else if (collection is AssetTableCollection assetTableCollection)
            {
                if (m_AssetTableCollections != null && !m_AssetTableCollections.Contains(assetTableCollection))
                {
                    m_AssetTableCollections.Add(assetTableCollection);
                    if (m_GuidToCollection != null)
                    {
                        CacheDependencies(assetTableCollection);
                    }
                }
            }
            else
            {
                throw new System.Exception("Unhandled collection type: " + collection.GetType());
            }
        }
    }
}
