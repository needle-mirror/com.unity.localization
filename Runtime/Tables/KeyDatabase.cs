using System;
using System.Collections.Generic;
using UnityEngine.Localization.Metadata;

namespace UnityEngine.Localization.Tables
{
    /// <summary>
    /// The key database is responsible for holding localization keys and associating keys to unique ids.
    /// Each collection of tables will reference a single Key Database asset.
    /// </summary>
    public class KeyDatabase : ScriptableObject, ISerializationCallbackReceiver
    {
        /// <summary>
        /// A entry in the Key Database. Contains the unique id, the name of the key and optional Metadata.
        /// </summary>
        [Serializable]
        public class KeyDatabaseEntry
        {
            [SerializeField]
            uint m_Id;

            [SerializeField]
            string m_Key;

            [SerializeField]
            MetadataCollection m_Metadata = new MetadataCollection();

            /// <summary>
            /// Unique id(to this Key Database).
            /// </summary>
            public uint Id
            {
                get => m_Id;
                internal set => m_Id = value;
            }

            /// <summary>
            /// The name of the key, must also be unique.
            /// </summary>
            public string Key
            {
                get => m_Key;
                internal set => m_Key = value;
            }

            /// <summary>
            /// Optional Metadata for this key that is also shared between all tables that use this <see cref="KeyDatabase"/>.
            /// </summary>
            public MetadataCollection Metadata
            {
                get => m_Metadata;
                set => m_Metadata = value;
            }
        }

        /// <summary>
        /// Represents an empty or null Key Id.
        /// </summary>
        public const uint EmptyId = 0;

        [SerializeField, HideInInspector]
        uint m_NextAvailableId = 1;

        [SerializeField, HideInInspector]
        string m_TableName;

        [SerializeField, HideInInspector]
        string m_TableNameGuidString;

        [SerializeField, HideInInspector]
        List<KeyDatabaseEntry> m_Entries = new List<KeyDatabaseEntry>();

        [SerializeField]
        [MetadataType(MetadataType.KeyDatabase)]
        MetadataCollection m_Metadata;

        Guid m_TableNameGuid;

        // Used for fast lookup. Only generated when required.
        Dictionary<uint, KeyDatabaseEntry> m_IdDictionary = new Dictionary<uint, KeyDatabaseEntry>();
        Dictionary<string, KeyDatabaseEntry> m_KeyDictionary = new Dictionary<string, KeyDatabaseEntry>();

        /// <summary>
        /// All Key Database entries.
        /// </summary>
        public List<KeyDatabaseEntry> Entries
        {
            get => m_Entries;
            set => m_Entries = value;
        }

        /// <summary>
        /// The name of this asset table collection. 
        /// All <see cref="LocalizedTable"/> that use this KeyDatabase will have this name.
        /// </summary>
        public string TableName
        {
            get => m_TableName;
            set => m_TableName = value;
        }

        /// <summary>
        /// A unique Id that will never change. Comes from the KeyDatabase asset Guid.
        /// Provides a way to reference a table that will not be broken if the table name was to be changed.
        /// </summary>
        public Guid TableNameGuid
        {
            get => m_TableNameGuid;
            internal set => m_TableNameGuid = value;
        }

        /// <summary>
        /// Metadata that is shared between all tables.
        /// </summary>
        public MetadataCollection Metadata
        {
            get => m_Metadata;
            set => m_Metadata = value;
        }

        /// <summary>
        /// Get the key associated with the id.
        /// </summary>
        /// <param name="id">Id the key belongs to.</param>
        /// <returns>The found key or null if one can not be found.</returns>
        public string GetKey(uint id)
        {
            var foundPair = FindWithId(id);
            return foundPair?.Key;
        }

        /// <summary>
        /// Get the unique Id for the key name from the database.
        /// </summary>
        /// <param name="key">The key whose id is being requested.</param>
        /// <returns>The keys id value or <see cref="EmptyId"/> if one does not exist.</returns>
        public uint GetId(string key)
        {
            var foundPair = FindWithKey(key);
            return foundPair?.Id ?? 0;
        }

        /// <summary>
        /// Get the unique Id for the key name from the database, if one does not exist then a new entry is added.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="addNewKey"></param>
        /// <returns></returns>
        public uint GetId(string key, bool addNewKey)
        {
            var foundPair = FindWithKey(key);
            var foundId = EmptyId;
            if (foundPair != null)
            {
                foundId = foundPair.Id;
            }
            else if (addNewKey)
            {
                foundId = AddKeyInternal(key).Id;
            }

            return foundId;
        }

        /// <summary>
        /// Returns the Entry for the key id, this contains all data for the key.
        /// </summary>
        /// <param name="id">Id the key belongs to.</param>
        /// <returns>The found key entry or null if one can not be found.</returns>
        public KeyDatabaseEntry GetEntry(TableEntryReference tableEntryReference)
        {
            if (tableEntryReference.ReferenceType == TableEntryReference.Type.Name)
                return GetEntry(tableEntryReference.Key);
            return GetEntry(tableEntryReference.KeyId);
        }

        /// <summary>
        /// Returns the Entry for the key id, this contains all data for the key.
        /// </summary>
        /// <param name="id">Id the key belongs to.</param>
        /// <returns>The found key entry or null if one can not be found.</returns>
        public KeyDatabaseEntry GetEntry(uint id)
        {
            return FindWithId(id);
        }

        /// <summary>
        /// Returns the Entry for the key, this contains all data for the key.
        /// </summary>
        /// <param name="key">The name of the key.</param>
        /// <returns>The found key entry or null if one can not be found.</returns>
        public KeyDatabaseEntry GetEntry(string key)
        {
            return FindWithKey(key);
        }

        /// <summary>
        /// Is the Id value used by any entries in the database?
        /// </summary>
        /// <param name="id">Id to check.</param>
        /// <returns></returns>
        public bool Contains(uint id) => FindWithId(id) != null;

        /// <summary>
        /// Is the key value used by any entries in the database?
        /// </summary>
        /// <param name="key">Key to check.</param>
        /// <returns></returns>
        public bool Contains(string key) => FindWithKey(key) != null;

        /// <summary>
        /// Adds a new key to the database if one does not already exists with the same value. Duplicates are not allowed.
        /// </summary>
        /// <param name="key"></param>
        /// /// <returns>The new key or null if the key already exists.</returns>
        public KeyDatabaseEntry AddKey(string key) => !Contains(key) ? AddKeyInternal(key) : null;

        /// <summary>
        /// Adds a new key to the database with a default name.
        /// </summary>
        /// <returns></returns>
        public KeyDatabaseEntry AddKey()
        {
            const string newKeyName = "New Entry";

            var keyToTry = newKeyName;
            KeyDatabaseEntry entry = null;
            int counter = 1;
            while (entry == null)
            {
                if (Contains(keyToTry))
                {
                    keyToTry = $"{newKeyName} {counter++}";
                }
                else
                {
                    entry = AddKeyInternal(keyToTry);
                }
            }
            return entry;
        }

        /// <summary>
        /// Attempts to remove the key with provided id.
        /// </summary>
        /// <param name="id"></param>
        public void RemoveKey(uint id)
        {
            var foundEntry = FindWithId(id);
            if (foundEntry != null)
                RemoveKeyInternal(foundEntry);
        }

        /// <summary>
        /// Attempts to remove the key from the database.
        /// </summary>
        /// <param name="key">The key to be removed.</param>
        public void RemoveKey(string key)
        {
            var foundEntry = FindWithKey(key);
            if (foundEntry != null)
                RemoveKeyInternal(foundEntry);
        }

        /// <summary>
        /// Rename the key value for the provided id if it exists.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newValue"></param>
        public void RenameKey(uint id, string newValue)
        {
            var foundEntry = FindWithId(id);
            if (foundEntry != null)
                RenameKeyInternal(foundEntry, newValue);
        }

        /// <summary>
        /// Rename the key value if it exists.
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        public void RenameKey(string oldValue, string newValue)
        {
            var foundEntry = FindWithKey(oldValue);
            if (foundEntry != null)
                RenameKeyInternal(foundEntry, newValue);
        }

        /// <summary>
        /// Returns the KeyDatabaseEntry that is the most similar to the text.
        /// Uses the Levenshtein distance method.
        /// </summary>
        /// <param name="text">The text to match against.</param>
        /// <param name="distance">The number of edits needed to turn <paramref name="text"/> into the returned KeyDatabaseEntry, 0 being an exact match.</param>
        /// <returns>The KeyDatabaseEntry that is the most similar to the text or null if one could not be found.</returns>
        public KeyDatabaseEntry FindSimilarKey(string text, out int distance)
        {
            KeyDatabaseEntry foundEntry = null;
            distance = int.MaxValue;
            foreach (var entry in Entries)
            {
                var d = ComputeLevenshteinDistance(text.ToLower(), entry.Key.ToLower());
                if (d < distance)
                {
                    foundEntry = entry;
                    distance = d;
                }
            }

            return foundEntry;
        }

        /// <summary>
        /// Compute the distance between two strings.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>The number of edits needed to turn one string into another.</returns>
        static int ComputeLevenshteinDistance(string a, string b)
        {
            // Based on https://www.dotnetperls.com/levenshtein
            int n = a.Length;
            int m = b.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
                return m;

            if (m == 0)
                return n;

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++){}

            for (int j = 0; j <= m; d[0, j] = j++){}

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (b[j - 1] == a[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Mathf.Min(Mathf.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }

            // Step 7
            return d[n, m];
        }

        /// <summary>
        /// Returns a new unique id for this table.
        /// </summary>
        /// <returns></returns>
        protected virtual uint GenerateUniqueId() => m_NextAvailableId++;

        KeyDatabaseEntry AddKeyInternal(string key)
        {
            var newEntry = new KeyDatabaseEntry() { Id = GenerateUniqueId(), Key = key };
            Entries.Add(newEntry);

            if (m_IdDictionary.Count > 0)
                m_IdDictionary[newEntry.Id] = newEntry;
            if (m_KeyDictionary.Count > 0)
                m_KeyDictionary[key] = newEntry;

            return newEntry;
        }

        void RenameKeyInternal(KeyDatabaseEntry entry, string newValue)
        {
            if (m_KeyDictionary.Count > 0)
            {
                m_KeyDictionary.Remove(entry.Key);
                m_KeyDictionary[newValue] = entry;
            }

            entry.Key = newValue;
        }

        void RemoveKeyInternal(KeyDatabaseEntry entry)
        {
            if (m_KeyDictionary.Count > 0)
                m_KeyDictionary.Remove(entry.Key);

            if (m_IdDictionary.Count > 0)
                m_IdDictionary.Remove(entry.Id);

            Entries.Remove(entry);
        }

        KeyDatabaseEntry FindWithId(uint id)
        {
            if (id == EmptyId)
                return null;

            if (m_IdDictionary.Count == 0)
            {
                foreach (var keyAndIdPair in m_Entries)
                {
                    m_IdDictionary[keyAndIdPair.Id] = keyAndIdPair;
                }
            }

            m_IdDictionary.TryGetValue(id, out var foundPair);
            return foundPair;
        }

        KeyDatabaseEntry FindWithKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            if (m_KeyDictionary.Count == 0)
            {
                foreach (var keyAndIdPair in m_Entries)
                {
                    m_KeyDictionary[keyAndIdPair.Key] = keyAndIdPair;
                }
            }

            m_KeyDictionary.TryGetValue(key, out var foundPair);
            return foundPair;
        }

        /// <summary>
        /// Converts the Guid into a serializable string.
        /// </summary>
        public void OnBeforeSerialize()
        {
            m_TableNameGuidString = TableReference.StringFromGuid(m_TableNameGuid);
        }

        /// <summary>
        /// Converts the serializable string into a Guid.
        /// </summary>
        public void OnAfterDeserialize()
        {
            m_IdDictionary.Clear();
            m_KeyDictionary.Clear();

            m_TableNameGuid = string.IsNullOrEmpty(m_TableNameGuidString) ? Guid.Empty : Guid.Parse(m_TableNameGuidString);
        }
    }
}