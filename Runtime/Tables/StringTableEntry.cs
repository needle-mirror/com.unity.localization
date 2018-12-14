using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Localization
{
    [Serializable]
    public class StringTableEntry
    {
        #if UNITY_EDITOR || INCLUDE_COMMENTS
        /// <summary>
        /// Contains data for string table fields that is not needed for runtime use.
        /// </summary>
        [Serializable]
        public struct StringTableComments
        {
            [Multiline]
            public string[] translatorComments;

            [Tooltip("Comments given by the programmer, directed at the translator.")]
            [Multiline]
            public string[] extractedComments;

            [Tooltip("References where this text is used.")]
            [Multiline]
            public string[] referenceComments;
        }

        [SerializeField]
        StringTableComments m_Comments;

        /// <summary>
        /// Optional comment data for the entry.
        /// By default this will be removed from a build unless <see cref="INCLUDE_COMMENTS"/> is defined in the build.
        /// </summary>
        public StringTableComments Comments
        {
            get { return m_Comments; }
            set { m_Comments = value; }
        }
        #endif

        // TODO: Hash128 the key
        [Multiline]
        [SerializeField] string m_Id;

        [Tooltip("Translated text. Item 0 should be used only except when supporting plurals.")]
        [Multiline]
        [SerializeField] List<string> m_Translated;

        public StringTableEntry(string id)
        {
            Id = id;
        }

        /// <summary>
        /// The Key for this table entry. Must be unique for each table.
        /// The key can be the original untranslated string, for example "My name is {0}" or an Id value such as "PLAYER_NAME".
        /// </summary>
        public string Id
        {
            get { return m_Id; }
            set { m_Id = value; }
        }

        /// <summary>
        /// The translated text to use. When using plurals this is item 0 from the plurals list.
        /// </summary>
        public string Translated
        {
            get
            {
                if (m_Translated == null || m_Translated.Count == 0)
                    return null;
                return m_Translated[0];
            }
            set
            {
                if (m_Translated == null || m_Translated.Count == 0)
                    m_Translated = new List<string>(){ value };
                else
                    m_Translated[0] = value;
            }
        }

        /// <summary>
        /// Returns all translated versions of the strings. Each item represents a plural version.
        /// See <see cref="PluralForm"/> on how to determine which item to use.
        /// </summary>
        public List<string> TranslatedPlurals
        {
            get { return m_Translated; }
            set { m_Translated = value; }
        }

        /// <summary>
        /// Return the translated string for the given index. 
        /// </summary>
        /// <param name="n">Plural index. See <see cref="PluralForm"/></param>
        /// <returns></returns>
        public string GetPlural(int n)
        {
            if(m_Translated != null && n < m_Translated.Count)
                return m_Translated[n];
            return null;
        }

        /// <summary>
        /// Sets the plural at n, if an entry does not exist then one is created.
        /// </summary>
        /// <param name="n">TranslatedPlurals index.</param>
        /// <param name="val">Value to add/replace at n.</param>
        public void SetPlural(int n, string val)
        {
            if (m_Translated == null)
                m_Translated = new List<string>();

            if(n >= m_Translated.Count)
            {
                // Resize the list
                m_Translated.AddRange(Enumerable.Repeat(string.Empty, n + 1 - m_Translated.Count));
            }

            m_Translated[n] = val;
        }
    }
}