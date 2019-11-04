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
        private KeyDatabase m_Keys;

        /// <summary>
        /// The type of <see cref="LocalizedTable"/> implementation for this collection.
        /// By default this will either by <see cref="StringTable"/> or <see cref="AssetTable"/>.
        /// </summary>
        public Type TableType { get; set; }

        /// <summary>
        /// The name all tables in this collection share.
        /// </summary>
        public string TableName { get => Keys.TableName; }

        /// <summary>
        /// A list of <see cref="AddressableAssetEntry"/>, each one represents a single table in this collection.
        /// </summary>
        public List<AddressableAssetEntry> TableEntries { get; set; } = new List<AddressableAssetEntry>();

        /// <summary>
        /// The Key database that is shared by all the tables in this collection.
        /// </summary>
        public KeyDatabase Keys
        {
            get
            {
                if (m_Keys == null)
                {
                    // Just load 1 table so we can get the Key db.
                    var tableAsset = AssetDatabase.LoadAssetAtPath<LocalizedTable>(TableEntries[0].AssetPath);
                    if (tableAsset != null)
                        m_Keys = tableAsset.Keys;
                }
                return m_Keys;
            }
            set => m_Keys = value;
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
            Keys = null;
            foreach (var addressableEntry in TableEntries)
            {
                var tableAsset = AssetDatabase.LoadAssetAtPath<LocalizedTable>(addressableEntry.AssetPath);
                if (tableAsset == null)
                    continue;

                var keyDb = tableAsset.Keys;

                if (Keys != null && Keys != keyDb)
                {
                    Debug.LogError($"Table '{addressableEntry.AssetPath}' does not use the same KeyDatabase as the other tables with the same name and type. Tables must share the same KeyDatabase. This table will be ignored.", tableAsset);
                    continue;
                }
                Keys = keyDb;
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
