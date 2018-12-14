using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace UnityEditor.Localization
{
    public static class LocalizationPlayerSettings
    {
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
            get
            {
                LocalizationSettings settings;
                EditorBuildSettings.TryGetConfigObject(LocalizationSettings.ConfigName, out settings);
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

        public static List<AssetTableCollection> GetAssetTables<TLocalizedTable>() where TLocalizedTable : LocalizedTable
        {
            var foundTables = new List<AssetTableCollection>();
            
            // Find all Table assets and their associated editors
            var tableAssets = AssetDatabase.FindAssets("t:" + typeof(TLocalizedTable).Name);

            // Collate by type and table name
            var typeLookup = new Dictionary<Type, Dictionary<string, AssetTableCollection>>();
            foreach (var taGuid in tableAssets)
            {
                string path = AssetDatabase.GUIDToAssetPath(taGuid);
                var localizedTable = AssetDatabase.LoadAssetAtPath<TLocalizedTable>(path);
                Debug.Assert(localizedTable != null);

                Dictionary<string, AssetTableCollection> nameLookup;
                AssetTableCollection tableCollection;
                if (typeLookup.TryGetValue(localizedTable.GetType(), out nameLookup))
                {
                    if (!nameLookup.TryGetValue(localizedTable.TableName, out tableCollection))
                    {
                        tableCollection = new AssetTableCollection() { TableType = localizedTable.GetType() };
                        foundTables.Add(tableCollection);
                        nameLookup[localizedTable.TableName] = tableCollection;
                    }
                }
                else
                {
                    tableCollection = new AssetTableCollection() { TableType = localizedTable.GetType() };
                    foundTables.Add(tableCollection);

                    nameLookup = new Dictionary<string, AssetTableCollection>();
                    nameLookup[localizedTable.TableName] = tableCollection;
                    typeLookup[localizedTable.GetType()] = nameLookup;
                }

                // Add to table
                tableCollection.Tables.Add(localizedTable);
            }
            return foundTables;
        }
    }

    /// <summary>
    /// Asset tables are collated by their type and table name
    /// </summary>
    public class AssetTableCollection
    {
        LocalizedTableEditor m_Editor;

        public Type TableType { get; set; }

        public Type AssetType
        {
            get
            {
                var assetTable = Tables[0] as LocalizedAssetTable;
                return assetTable != null ? assetTable.SupportedAssetType : null;
            }
        }

        public LocalizedTableEditor TableEditor
        {
            get
            {
                if (m_Editor == null)
                {
                    Editor editor = null;
                    Editor.CreateCachedEditor(Tables.ToArray(), null, ref editor);
                    Debug.Assert(editor != null);
                    m_Editor = editor as LocalizedTableEditor;
                }
                return m_Editor;
            }
        }

        public string TableName { get { return Tables[0].TableName; } }

        public List<LocalizedTable> Tables { get; set; }

        public AssetTableCollection()
        {
            Tables = new List<LocalizedTable>();
        }

        public HashSet<string> GetKeys()
        {
            var keys = new HashSet<string>();
            foreach(var tbl in Tables)
            {
                tbl.GetKeys(keys);
            }
            return keys;
        }

        public override string ToString()
        {
            return TableName + "("+ TableType.Name  + ")";
        }
    }
}
