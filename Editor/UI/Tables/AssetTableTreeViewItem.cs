using System;
using System.Collections.Generic;
using System.Text;
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

        SharedTableData m_SharedTableData;
        AssetTypeMetadata m_AssetTypeMetadata;

        public Type AssetType => m_AssetTypeMetadata == null ? typeof(Object) : m_AssetTypeMetadata.Type;

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

        public override void Initialize(List<LocalizedTable> tables, int startIdx)
        {
            m_TableProperties = new(ISelectable, Object, AssetTable)[startIdx + tables.Count];

            // Get the shared data
            if (tables.Count > 0)
                m_SharedTableData = tables[0].SharedData;

            Debug.Assert(m_SharedTableData != null);
            for (int i = startIdx; i < m_TableProperties.Length; ++i)
            {
                m_TableProperties[i] = (null, null, tables[i - startIdx] as AssetTable);
            }
            RefreshFields();
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
            foreach (var md in  m_SharedTableData.Metadata.MetadataEntries)
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
            return m_TableProperties[colIdx].value;
        }

        public bool IsTableEntrySelected(int colIdx)
        {
            ISelectable s = m_TableProperties[colIdx].selected;
            return s?.Selected ?? false;
        }

        public ISelectable Select(int colIdx, Locale locale)
        {
            if (m_TableProperties[colIdx].selected == null)
            {
                var s = new TableEntrySelected(m_TableProperties[colIdx].table, KeyId, locale, MetadataType.AssetTableEntry);
                m_TableProperties[colIdx].selected = s;
            }
            return m_TableProperties[colIdx].selected;
        }

        public void SetAsset(Object asset, int colIdx)
        {
            var group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Set asset");
            if (asset == null)
            {
                LocalizationEditorSettings.RemoveAssetFromTable(m_TableProperties[colIdx].table, KeyId, m_TableProperties[colIdx].value, true);
            }
            else
            {
                LocalizationEditorSettings.AddAssetToTable(m_TableProperties[colIdx].table, KeyId, asset, true);
            }

            m_TableProperties[colIdx].value = asset;

            UpdateType();
            UpdateSearchString();
            Undo.CollapseUndoOperations(group);
        }

        public override void OnDeleteKey()
        {
            foreach (var tableProperties in m_TableProperties)
            {
                if (tableProperties.table != null && tableProperties.value != null)
                {
                    LocalizationEditorSettings.RemoveAssetFromTable(tableProperties.table, KeyId, tableProperties.value);
                }
            }
        }

        void UpdateSearchString()
        {
            var sb = new StringBuilder();
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
