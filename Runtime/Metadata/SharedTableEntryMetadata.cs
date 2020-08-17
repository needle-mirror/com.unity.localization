using System.Collections.Generic;
using UnityEngine.Localization.Tables;

namespace UnityEngine.Localization.Metadata
{
    /// <summary>
    /// Shared Metadata is data that is associated to one or more entries in a single <see cref="LocalizationTable"/>.
    /// While it is possible to share any metadata across multiple entries in a table, this will track the entries that
    /// are currently using it providing a quick way to get all the entries.
    /// </summary>
    public abstract class SharedTableEntryMetadata : IMetadata, ISerializationCallbackReceiver
    {
        [SerializeField]
        List<long> m_Entries = new List<long>();

        HashSet<long> m_EntriesLookup = new HashSet<long>();

        internal int Count => m_EntriesLookup.Count;

        internal bool IsRegistered(TableEntry entry)
        {
            return m_EntriesLookup.Contains(entry.Data.Id);
        }

        internal void Register(TableEntry entry)
        {
            m_EntriesLookup.Add(entry.Data.Id);
        }

        internal void Unregister(TableEntry entry)
        {
            m_EntriesLookup.Remove(entry.Data.Id);
        }

        /// <summary>
        /// Converts the internal hashset into a serializable list.
        /// </summary>
        public void OnBeforeSerialize()
        {
            if (m_Entries == null)
                m_Entries = new List<long>();
            else
                m_Entries.Clear();

            m_Entries.AddRange(m_EntriesLookup);
        }

        /// <summary>
        /// Converts the serializable list into a hashset.
        /// </summary>
        public void OnAfterDeserialize()
        {
            if (m_EntriesLookup == null)
                m_EntriesLookup = new HashSet<long>();
            else
                m_EntriesLookup.Clear();

            foreach (var e in m_Entries)
            {
                m_EntriesLookup.Add(e);
            }
        }
    }
}
