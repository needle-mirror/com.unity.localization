using System.Collections.Generic;
using System;

namespace UnityEngine.Localization
{
    /// <summary>
    /// The key database is responsible for holding localization keys and associating keys to unique ids.
    /// </summary>
    public class KeyDatabase : ScriptableObject, ISerializationCallbackReceiver
    {
        [Serializable]
        public class KeyDatabaseEntry
        {
            [SerializeField]
            uint m_Id;

            [SerializeField]
            string m_Key;

            public uint Id
            {
                get => m_Id;
                set => m_Id = value;
            }

            public string Key
            {
                get => m_Key;
                set => m_Key = value;
            }
        }

        public const uint EmptyId = 0;

        [SerializeField, HideInInspector]
        uint m_NextAvailableId = 1;

        [SerializeField, HideInInspector]
        List<KeyDatabaseEntry> m_Entries = new List<KeyDatabaseEntry>();

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
        /// Get the unique Id for the key from the database.
        /// </summary>
        /// <param name="key">The key whose id is being requested.</param>
        /// <returns>The keys id value or <see cref="EmptyId"/> if one does not exist.</returns>
        public uint GetId(string key)
        {
            var foundPair = FindWithKey(key);
            return foundPair?.Id ?? 0;
        }

        /// <summary>
        /// 
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

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            m_IdDictionary.Clear();
            m_KeyDictionary.Clear();
        }
    }
}