using System.Collections.Generic;

namespace UnityEngine.Localization
{
    public class StringTable : StringTableBase
    {
        [SerializeField]
        List<StringTableEntry> m_StringTableEntries = new List<StringTableEntry>();

        Dictionary<uint, StringTableEntry> m_StringDict;

        Dictionary<uint, StringTableEntry> StringDict
        {
            get
            {
                if (m_StringDict == null)
                {
                    m_StringDict = new Dictionary<uint, StringTableEntry>(m_StringTableEntries.Count);
                    foreach (var stringEntry in m_StringTableEntries)
                    {
                        m_StringDict[stringEntry.Id] = stringEntry;
                    }
                }

                return m_StringDict;
            }
        }

        public StringTableEntry AddEntry(uint keyId)
        {
            var entry = new StringTableEntry(keyId);
            m_StringTableEntries.Add(entry);

            if (m_StringDict != null)
                m_StringDict[keyId] = entry;

            return entry;
        }

        public StringTableEntry AddEntry(string key)
        {
            var newKey = Keys.GetId(key, true);
            return AddEntry(newKey);
        }

        public StringTableEntry GetEntry(uint keyId)
        {
            if (Keys == null)
            {
                Debug.LogError("StringTable does not have a KeyDatabase", this);
                return null;
            }

            return StringDict.TryGetValue(keyId, out StringTableEntry foundEntry) ? foundEntry : null;
        }

        public StringTableEntry GetEntry(string key)
        {
            if (Keys == null)
            {
                Debug.LogError("StringTable does not have a KeyDatabase", this);
                return null;
            }

            uint foundId = Keys.GetId(key);
            if (foundId == KeyDatabase.EmptyId)
                return null;

            return StringDict.TryGetValue(foundId, out StringTableEntry foundEntry) ? foundEntry : null;
        }

        /// <inheritdoc/>
        public override string GetLocalizedString(uint keyId)
        {
            var foundEntry = GetEntry(keyId);
            return foundEntry?.Translated;
        }

        /// <inheritdoc/>
        public override string GetLocalizedPluralString(uint keyId, int n)
        {
            var foundEntry = GetEntry(keyId);

            if (foundEntry == null || PluralHandler == null)
                return null;

            var pluralText = foundEntry.GetPlural(PluralHandler.Evaluate(n));
            return string.IsNullOrEmpty(pluralText) ? null : string.Format(pluralText, n);
        }
    }
}