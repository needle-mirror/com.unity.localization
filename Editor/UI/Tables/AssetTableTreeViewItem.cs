using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Tables;
using Object = UnityEngine.Object;

namespace UnityEditor.Localization.UI
{
    class AssetTableTreeViewItem : GenericAssetTableTreeViewItem<AssetTable>
    {
        static readonly Dictionary<string, Object> s_CachedAssets = new Dictionary<string, Object>();

        (ISelectable selected, Object value, AssetTable table)[] m_TableProperties;
        List<LocalizationTable> m_SortedTables;
        int m_StartIndex;

        SharedTableData m_SharedTableData;
        AssetTypeMetadata m_AssetTypeMetadata;
        AssetTableCollection m_AssetTableCollection;

        public Type AssetType => m_AssetTypeMetadata == null ? typeof(Object) : m_AssetTypeMetadata.Type;

        public override string displayName
        {
            get
            {
                DelayedInit();
                return base.displayName;
            }
            set => base.displayName = value;
        }

        static Object GetAssetFromCache(string guid)
        {
            if (s_CachedAssets.TryGetValue(guid, out var foundAsset))
                return foundAsset;

            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (asset != null)
            {
                s_CachedAssets[guid] = asset;
            }

            return asset;
        }

        public override void Initialize(LocalizationTableCollection collection, int startIdx, List<LocalizationTable> sortedTables)
        {
            m_SortedTables = sortedTables;
            m_StartIndex = startIdx;
            m_AssetTableCollection = (AssetTableCollection)collection;


            m_TableProperties = new(ISelectable, Object, AssetTable)[startIdx + sortedTables.Count];

            // Get the shared data
            m_SharedTableData = collection.SharedData;

            Debug.Assert(m_SharedTableData != null);
            for (int i = startIdx; i < m_TableProperties.Length; ++i)
            {
                m_TableProperties[i] = (null, null, sortedTables[i - startIdx] as AssetTable);
            }
            RefreshFields();
        }

        void DelayedInit()
        {
            if (m_TableProperties == null)
            {
                m_TableProperties = new(ISelectable, Object, AssetTable)[m_StartIndex + m_SortedTables.Count];

                // Get the shared data
                m_SharedTableData = m_AssetTableCollection.SharedData;

                Debug.Assert(m_SharedTableData != null);
                for (int i = m_StartIndex; i < m_TableProperties.Length; ++i)
                {
                    m_TableProperties[i] = (null, null, m_SortedTables[i - m_StartIndex] as AssetTable);
                }
                RefreshFields();
            }
        }

        public void RefreshFields()
        {
            for (int i = 0; i < m_TableProperties.Length; ++i)
            {
                var table = m_TableProperties[i].table;
                if (table == null)
                    continue;

                var entry = table.GetEntry(SharedEntry.Id);
                var guid = entry?.Guid;
                Object asset = null;
                if (!string.IsNullOrEmpty(guid))
                {
                    asset = GetAssetFromCache(guid);
                }
                m_TableProperties[i].value = asset;
            }
            UpdateType();
            UpdateSearchString();
        }

        void UpdateType()
        {
            m_AssetTypeMetadata = null;
            foreach (var md in m_SharedTableData.Metadata.MetadataEntries)
            {
                if (md is AssetTypeMetadata at)
                {
                    if (at.Contains(KeyId))
                    {
                        m_AssetTypeMetadata = at;
                        return;
                    }
                }
            }
        }

        public Object GetTableAsset(int colIdx)
        {
            DelayedInit();
            return m_TableProperties[colIdx].value;
        }

        public bool IsTableEntrySelected(int colIdx)
        {
            DelayedInit();
            ISelectable s = m_TableProperties[colIdx].selected;
            return s?.Selected ?? false;
        }

        public ISelectable Select(int colIdx, Locale locale)
        {
            DelayedInit();
            if (m_TableProperties[colIdx].selected == null)
            {
                var s = new TableEntrySelected(m_TableProperties[colIdx].table, KeyId, locale, MetadataType.AssetTableEntry | MetadataType.SharedAssetTableEntry);
                m_TableProperties[colIdx].selected = s;
            }
            return m_TableProperties[colIdx].selected;
        }

        public void SetAsset(Object asset, int colIdx)
        {
            DelayedInit();
            if (asset == null)
            {
                m_AssetTableCollection.RemoveAssetFromTable(m_TableProperties[colIdx].table, KeyId, true);
            }
            else
            {
                m_AssetTableCollection.AddAssetToTable(m_TableProperties[colIdx].table, KeyId, asset, true);
            }

            m_TableProperties[colIdx].value = asset;

            UpdateType();
            UpdateSearchString();
        }

        public override void OnDeleteKey()
        {
            DelayedInit();
            foreach (var tableProperties in m_TableProperties)
            {
                // If the column is selected then we need to disable it, so we are not trying to edit data that has been removed.
                if (tableProperties.selected != null)
                    tableProperties.selected.Selected = false;

                if (tableProperties.table != null && tableProperties.value != null)
                {
                    m_AssetTableCollection.RemoveAssetFromTable(tableProperties.table, KeyId, true);
                }
            }
        }

        void UpdateSearchString()
        {
            DelayedInit();
            using (StringBuilderPool.Get(out var sb))
            {
                sb.AppendLine(SharedEntry.Id.ToString());
                sb.AppendLine(SharedEntry.Key);
                foreach (var tableAsset in m_TableProperties)
                {
                    if (tableAsset.value != null)
                    {
                        sb.AppendLine(tableAsset.value.name);
                    }
                }

                displayName = sb.ToString();
            }
        }
    }
}
