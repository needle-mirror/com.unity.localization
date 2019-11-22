using System;
using System.Collections.Generic;
using UnityEngine.Localization.Metadata;

namespace UnityEngine.Localization.Tables
{
    /// <summary>
    /// Abstract base class for all tables.
    /// Contains common functionality.
    /// </summary>
    public abstract class LocalizedTable : ScriptableObject, IMetadataCollection
    {
        [SerializeField]
        LocaleIdentifier m_LocaleId;

        [SerializeField, HideInInspector]
        KeyDatabase m_KeyDatabase;

        [SerializeField]
        MetadataCollection m_Metadata = new MetadataCollection();

        [SerializeField]
        List<TableEntryData> m_TableData = new List<TableEntryData>();

        /// <summary>
        /// The locale this asset table supports.
        /// </summary>
        public LocaleIdentifier LocaleIdentifier
        {
            get => m_LocaleId;
            set => m_LocaleId = value;
        }

        /// <summary>
        /// The name of this asset table collection.
        /// </summary>
        public string TableName
        {
            get => Keys.TableName;
            set => Keys.TableName = value;
        }

        /// <summary>
        /// Database of all keys used by this Table.
        /// </summary>
        public KeyDatabase Keys
        {
            get => m_KeyDatabase;
            set => m_KeyDatabase = value;
        }

        /// <summary>
        /// All entries stored within the table.
        /// </summary>
        internal List<TableEntryData> TableData
        {
            get => m_TableData;
            set => m_TableData = value;
        }

        /// <summary>
        /// Table Metadata.
        /// </summary>
        public IList<IMetadata> Entries
        {
            get => m_Metadata.Entries;
        }

        /// <summary>
        /// Returns the first Metadata item from <see cref="Entries"/> of type TObject.
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <returns></returns>
        public TObject GetMetadata<TObject>() where TObject : IMetadata
        {
            return m_Metadata.GetMetadata<TObject>();
        }

        /// <summary>
        /// Populates the list with all Metadata from <see cref="Entries"/> that is of type TObject.
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="foundItems"></param>
        public void GetMetadatas<TObject>(IList<TObject> foundItems) where TObject : IMetadata
        {
            m_Metadata.GetMetadatas(foundItems);
        }

        /// <summary>
        /// Returns all Metadata from <see cref="Entries"/> that is of type TObject.
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <returns></returns>
        public IList<TObject> GetMetadatas<TObject>() where TObject : IMetadata
        {
            return m_Metadata.GetMetadatas<TObject>();
        }

        /// <summary>
        /// Add an entry to <see cref="Entries"/>.
        /// </summary>
        /// <param name="md"></param>
        public void AddMetadata(IMetadata md)
        {
            m_Metadata.AddMetadata(md);
        }

        /// <summary>
        /// Remove an entry from <see cref="Entries"/>.
        /// </summary>
        /// <param name="md"></param>
        /// <returns></returns>
        public bool RemoveMetadata(IMetadata md)
        {
            return m_Metadata.RemoveMetadata(md);
        }

        /// <summary>
        /// Checks if the Metadata is contained within <see cref="Entries"/>.
        /// </summary>
        /// <param name="md"></param>
        /// <returns></returns>
        public bool Contains(IMetadata md)
        {
            return m_Metadata.Contains(md);
        }

        /// <summary>
        /// Returns the key with the matching name from the <see cref="KeyDatabase"/>, if one exists.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>The found key or null if one could not be found.</returns>
        /// <exception cref="Exception">Thrown if the <see cref="KeyDatabase"/> is null.</exception>
        protected uint FindKeyId(string key)
        {
            if (Keys == null)
                throw new NullReferenceException($"Can not find Key Id for \"{key}\". The Table \"{TableName} does not have a Key Database.");
            return Keys.GetId(key, true);
        }

        /// <summary>
        /// Returns a string representation of the table in the format "{TableName}({LocaleIdentifier})".
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{TableName}({LocaleIdentifier})";
    }
}