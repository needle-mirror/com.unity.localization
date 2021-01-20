using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat;
using UnityEngine.Localization.SmartFormat.Core.Formatting;

namespace UnityEngine.Localization.Tables
{
    /// <summary>
    /// A <see cref="StringTable"/> entry.
    /// </summary>
    public class StringTableEntry : TableEntry
    {
        FormatCache m_FormatCache;

        /// <summary>
        /// Used when <see cref="IsSmart"/> is true and <see cref="GetLocalizedString"/> is called.
        /// Contains information about the format including any <see cref="IGlobalVariable"/> that were used.
        /// </summary>
        public FormatCache FormatCache
        {
            get => m_FormatCache;
            set => m_FormatCache = value;
        }

        /// <summary>
        /// The raw localized value without any formatting applied.
        /// </summary>
        public string Value
        {
            get => Data.Localized;
            set
            {
                Data.Localized = value;
                FormatCache = null;
            }
        }

        /// <summary>
        /// Is the entry marked with the <see cref="SmartFormatTag"/>?
        /// Entries that are smart will use <see cref="SmartFormat"/> to format the localized text.
        /// </summary>
        public bool IsSmart
        {
            get
            {
                // TODO: Cache value
                return Data.Metadata.GetMetadata<SmartFormatTag>() != null;
            }
            set
            {
                if (value)
                {
                    FormatCache = null;
                    AddTagMetadata<SmartFormatTag>();
                }
                else
                {
                    RemoveTagMetadata<SmartFormatTag>();
                }
            }
        }

        internal StringTableEntry()
        {
        }

        /// <summary>
        /// Attempts to remove the entry from the <see cref="StringTable"/> that it belongs to.
        /// If <see cref="TableEntry.Table"/> is null then a warning will be produced.
        /// </summary>
        public void RemoveFromTable()
        {
            var stringTable = Table as StringTable;
            if (stringTable == null)
            {
                Debug.LogWarning($"Failed to remove {nameof(StringTableEntry)} with id {KeyId} and value `{Value}` as it does not belong to a table.");
            }
            else
            {
                stringTable.Remove(KeyId);
            }
        }

        /// <summary>
        /// Returns the localized text after formatting has been applied.
        /// This will use SmartFormat if <see cref="IsSmart"/> is true else it will return the raw unformatted value.
        /// </summary>
        /// <returns></returns>
        public string GetLocalizedString() => GetLocalizedString(null, null);

        /// <summary>
        /// Returns the localized text after formatting has been applied.
        /// Formatting will use SmartFormat if <see cref="IsSmart"/> is true else it will default to String.Format.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public string GetLocalizedString(params object[] args) => GetLocalizedString(null, args);

        /// <summary>
        /// Returns the localized text after formatting has been applied.
        /// Formatting will use SmartFormat is <see cref="IsSmart"/> is true else it will default to String.Format.
        /// </summary>
        /// <param name="formatProvider">Custom format provider used with String.Format and smart strings.</param>
        /// <param name="args"></param>
        /// <returns></returns>
        public string GetLocalizedString(IFormatProvider formatProvider, params object[] args)
        {
            string translatedText = null;

            if (IsSmart)
            {
                #if UNITY_EDITOR
                if (!LocalizationSettings.Instance.IsPlaying)
                {
                    FormatCache formatCache = null;
                    translatedText = LocalizationSettings.StringDatabase.SmartFormatter.FormatWithCache(ref formatCache, Data.Localized, formatProvider, args);
                }
                else
                #endif
                translatedText = LocalizationSettings.StringDatabase.SmartFormatter.FormatWithCache(ref m_FormatCache, Data.Localized, formatProvider, args);
            }
            else if (!string.IsNullOrEmpty(Data.Localized))
            {
                if (args != null)
                    translatedText = formatProvider == null ? string.Format(Data.Localized, args) : string.Format(formatProvider, Data.Localized, args);
                else
                    translatedText = Data.Localized;
            }

            return translatedText;
        }
    }

    /// <summary>
    /// A table that stores localized strings for a specific <see cref="Locale"/>.
    /// </summary>
    public class StringTable : DetailedLocalizationTable<StringTableEntry>
    {
        /// <summary>
        /// Returns the unique characters used by all entries in this table.
        /// This will also include Smart String entries but will only consider the <see cref="UnityEngine.Localization.SmartFormat.Core.Parsing.LiteralText"/> values,
        /// it will not consider <see cref="UnityEngine.Localization.SmartFormat.Core.Parsing.Placeholder"/> values.
        /// </summary>
        public string GenerateCharacterSet()
        {
            var literals = CollectLiteralCharacters();

            // Sort the output so the results are more deterministic.
            var sorted = literals.Distinct().OrderBy(c => c);

            return string.Concat(sorted);
        }

        internal IEnumerable<char> CollectLiteralCharacters()
        {
            IEnumerable<char> e = "";
            var smart = new SmartFormatterLiteralCharacterExtractor(LocalizationSettings.StringDatabase?.SmartFormatter);
            foreach (var entry in Values)
            {
                if (entry.IsSmart)
                {
                    e = e.Concat(smart.ExtractLiteralsCharacters(entry.LocalizedValue));
                }
                else
                {
                    e = e.Concat(entry.LocalizedValue.AsEnumerable());
                }
            }
            return e;
        }

        /// <summary>
        /// Creates a new, empty StringTableEntry.
        /// </summary>
        /// <returns></returns>
        public override StringTableEntry CreateTableEntry()
        {
            return new StringTableEntry() { Table = this, Data = new TableEntryData() };
        }
    }
}
