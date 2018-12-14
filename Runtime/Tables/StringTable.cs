using System.Collections.Generic;

namespace UnityEngine.Localization
{
    public class StringTable : StringTableBase
    {
        [SerializeField]
        List<StringTableEntry> m_StringTableEntries = new List<StringTableEntry>();

        Dictionary<string, StringTableEntry> m_StringDict;

        Dictionary<string, StringTableEntry> StringDict
        {
            get
            {
                if (m_StringDict == null)
                {
                    m_StringDict = new Dictionary<string, StringTableEntry>(m_StringTableEntries.Count);
                    foreach (var stringEntry in m_StringTableEntries)
                    {
                        m_StringDict[stringEntry.Id] = stringEntry;
                    }
                }

                return m_StringDict;
            }
        }

        public StringTableEntry GetEntry(string key)
        {
            StringTableEntry foundEntry;
            return StringDict.TryGetValue(key, out foundEntry) ? foundEntry : null;
        }

        /// <inheritdoc/>
        public override string GetLocalizedString(string key)
        {
            StringTableEntry foundEntry;
            return StringDict.TryGetValue(key, out foundEntry) ? foundEntry.Translated : null;
        }

        /// <inheritdoc/>
        public override string GetLocalizedPluralString(string key, int n)
        {
            StringTableEntry foundEntry;
            if (StringDict.TryGetValue(key, out foundEntry))
            {
                return string.Format(foundEntry.GetPlural(PluralHandler.Evaluate(n)), n);
            }
            return null;
        }

        /// <inheritdoc/>
        public override void AddKey(string key)
        {
            if((m_StringDict != null && m_StringDict.ContainsKey(key)) || m_StringTableEntries.Exists(te => te.Id == key))
            {
                Debug.LogWarningFormat("Can not add duplicate key '{0}' to table {1}.", key, TableName);
            }
            else
            {
                var ste = new StringTableEntry(key);
                if (m_StringDict != null)
                    m_StringDict[key] = ste;
                m_StringTableEntries.Add(ste);
            }
        }

        /// <inheritdoc/>
        public override void RemoveKey(string key)
        {
            if (m_StringDict != null)
                m_StringDict.Remove(key);

            for(int i = 0; i < m_StringTableEntries.Count; ++i)
            {
                if(m_StringTableEntries[i].Id == key)
                {
                    m_StringTableEntries.RemoveAt(i);
                    return;
                }
            }
        }

        /// <inheritdoc/>
        public override void ReplaceKey(string key, string newKey)
        {
            if (m_StringDict != null)
            {
                StringTableEntry foundEntry;
                if(m_StringDict.TryGetValue(key, out foundEntry))
                {
                    foundEntry.Id = newKey;
                    m_StringDict.Remove(key);
                    m_StringDict[newKey] = foundEntry;
                }
            }
            else
            {
                StringTableEntry foundEntry = m_StringTableEntries.Find(ste => ste.Id == key);
                if(foundEntry != null)
                    foundEntry.Id = newKey;
            }
        }

        /// <inheritdoc/>
        public override void GetKeys(HashSet<string> keySet)
        {
            foreach(var ste in m_StringTableEntries)
            {
                keySet.Add(ste.Id);
            }
        }
    }
}