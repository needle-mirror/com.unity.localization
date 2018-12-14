using System.Collections.Generic;

namespace UnityEngine.Localization
{
    public abstract class LocalizedTable : ScriptableObject
    {
        [SerializeField]
        LocaleIdentifier m_LocaleId;

        [SerializeField]
        string m_TableName = "Default";

        /// <summary>
        /// The locale this asset table supports.
        /// </summary>
        public LocaleIdentifier LocaleIdentifier
        {
            get { return m_LocaleId; }
            set { m_LocaleId = value; }
        }

        /// <summary>
        /// The name of this asset table. Must be unique per locale.
        /// </summary>
        public string TableName
        {
            get { return m_TableName; }
            set { m_TableName = value; }
        }

        /// <summary>
        /// Add a new key to the table. Must be unique or it will be ignored.
        /// </summary>
        /// <param name="key"></param>
        public abstract void AddKey(string key);

        /// <summary>
        /// Remove the key if it exists.
        /// </summary>
        /// <param name="key"></param>
        public abstract void RemoveKey(string key);

        /// <summary>
        /// Replace the old key value with the newKey value.
        /// </summary>
        public abstract void ReplaceKey(string key, string newKey);

        /// <summary>
        /// Populate the HashSet with all keys in this table.
        /// Allows us to collate the keys from multiple locale for the same table name.
        /// </summary>
        /// <param name="keySet"></param>
        public abstract void GetKeys(HashSet<string> keySet);
    }
}