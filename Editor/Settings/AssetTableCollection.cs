using System;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization
{
    /// <summary>
    /// Contains information on all the tables associated with a table name and type.
    /// </summary>
    public class AssetTableCollection : IEquatable<AssetTableCollection>
    {
        List<LocalizedTable> m_Tables;
        private SharedTableData m_SharedTableData;

        /// <summary>
        /// The type of <see cref="LocalizedTable"/> implementation for this collection.
        /// By default this will either by <see cref="StringTable"/> or <see cref="AssetTable"/>.
        /// </summary>
        public Type TableType { get; set; }

        /// <summary>
        /// The name all tables in this collection share.
        /// </summary>
        public string TableName { get => SharedData.TableName; }

        /// <summary>
        /// A list of <see cref="AddressableAssetEntry"/>, each one represents a single table in this collection.
        /// </summary>
        public List<AddressableAssetEntry> TableEntries { get; set; } = new List<AddressableAssetEntry>();

        /// <summary>
        /// The data that is shared across all the tables in this collection.
        /// </summary>
        public SharedTableData SharedData
        {
            get
            {
                if (m_SharedTableData == null)
                {
                    // Just load 1 table so we can get the shared data.
                    var tableAsset = AssetDatabase.LoadAssetAtPath<LocalizedTable>(TableEntries[0].AssetPath);
                    if (tableAsset != null)
                        m_SharedTableData = tableAsset.SharedData;
                    Debug.Assert(m_SharedTableData != null, "Shared table Data should not be null.", TableEntries[0].MainAsset);
                }
                return m_SharedTableData;
            }
            set => m_SharedTableData = value;
        }

        /// <summary>
        /// The Table assets for this collection. Calling get will force all the tables to be loaded.
        /// </summary>
        public List<LocalizedTable> Tables
        {
            get
            {
                if (m_Tables == null)
                {
                    LoadTables();
                }

                return m_Tables;
            }

            internal set => m_Tables = value;
        }

        void LoadTables()
        {
            Tables = new List<LocalizedTable>();
            m_SharedTableData = null;
            foreach (var addressableEntry in TableEntries)
            {
                var tableAsset = AssetDatabase.LoadAssetAtPath<LocalizedTable>(addressableEntry.AssetPath);
                if (tableAsset == null)
                    continue;

                var sharedTableData = tableAsset.SharedData;

                if (m_SharedTableData != null && m_SharedTableData != sharedTableData)
                {
                    Debug.LogError($"Table '{addressableEntry.AssetPath}' does not use the same {nameof(SharedTableData)} as the other tables with the same name and type. Tables must share the same {nameof(SharedTableData)}. This table will be ignored.", tableAsset);
                    continue;
                }
                m_SharedTableData = sharedTableData;
                Tables.Add(tableAsset);
            }
        }

        /// <summary>
        /// Returns <see cref="TableName"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => TableName;

        /// <summary>
        /// Compare another <see cref="AssetTableCollection"/> to this one.
        /// Compares the <see cref="TableEntries"/> field.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(AssetTableCollection other)
        {
            if (other == null)
                return false;
            return TableEntries == other.TableEntries;
        }
    }
}
