using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Localization.Tables;

namespace UnityEngine.Localization.Metadata
{
    /// <summary>
    /// Shared Metadata is data that is associated to all tables that use the same <see cref="SharedTableData"/>, also known as an AssetTableCollection.
    /// Multiple table entries can be included in the SharedTableCollectionMetadata and The tables that use each table entry are recorded.
    /// </summary>
    [Serializable]
    public abstract class SharedTableCollectionMetadata : IMetadata, ISerializationCallbackReceiver
    {
        [Serializable]
        class Item
        {
            [SerializeField]
            uint m_KeyId;

            [SerializeField]
            List<string> m_TableCodes = new List<string>();

            public uint KeyId
            {
                get => m_KeyId;
                set => m_KeyId = value;
            }

            public List<string> Tables
            {
                get => m_TableCodes;
                set => m_TableCodes = value;
            }
        }

        [SerializeField, HideInInspector]
        List<Item> m_Entries = new List<Item>();

        Dictionary<uint, HashSet<string>> m_EntriesLookup = new Dictionary<uint, HashSet<string>>();

        /// <summary>
        /// Are any table entries currently associated to this Metadata?
        /// </summary>
        public bool IsEmpty => m_EntriesLookup.Count == 0;

        /// <summary>
        /// Is the table entry associated to this Metadata?
        /// </summary>
        /// <param name="keyId"></param>
        /// <returns></returns>
        public bool Contains(uint keyId)
        {
            return m_EntriesLookup.ContainsKey(keyId);
        }

        /// <summary>
        /// Is the table entry and culture code associated to this Metadata?
        /// </summary>
        /// <param name="keyId"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public bool Contains(uint keyId, string code)
        {
            return m_EntriesLookup.TryGetValue(keyId, out var codes) && codes.Contains(code);
        }

        /// <summary>
        /// Add the table entry for a specific table to the shared Metadata.
        /// </summary>
        /// <param name="keyId">The Id of the table entry.</param>
        /// <param name="code">The table culture code.</param>
        public void AddEntry(uint keyId, string code)
        {
            m_EntriesLookup.TryGetValue(keyId, out var item);
            if (item == null)
            {
                item = new HashSet<string>();
                m_EntriesLookup[keyId] = item;
            }

            item.Add(code);
        }

        /// <summary>
        /// Remove the table entry for a specific table from the shared Metadata.
        /// </summary>
        /// <param name="keyId"></param>
        /// <param name="code"></param>
        public void RemoveEntry(uint keyId, string code)
        {
            if (m_EntriesLookup.TryGetValue(keyId, out var item))
            {
                item.Remove(code);

                if (item.Count == 0)
                    m_EntriesLookup.Remove(keyId);
            }
        }

        /// <summary>
        /// Converts the internal dictionary into a serializable list.
        /// </summary>
        public virtual void OnBeforeSerialize()
        {
            m_Entries.Clear();
            foreach (var entry in m_EntriesLookup)
            {
                m_Entries.Add(new Item(){ KeyId = entry.Key, Tables = entry.Value.ToList()});
            }
        }

        /// <summary>
        /// Converts the serializable list into a dictionary.
        /// </summary>
        public virtual void OnAfterDeserialize()
        {
            m_EntriesLookup = new Dictionary<uint, HashSet<string>>();
            foreach (var entry in m_Entries)
            {
                m_EntriesLookup[entry.KeyId] = new HashSet<string>(entry.Tables);
            }
        }
    }
}
