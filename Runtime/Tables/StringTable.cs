using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Settings;
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
        /// The raw localized value without any formatting applied.
        /// </summary>
        public string Value
        {
            get => Data.Localized;
            set => Data.Localized = value;
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
        /// Returns the raw localized value without any formatting applied.
        /// </summary>
        /// <returns></returns>
        public string GetLocalizedString()
        {
            return Data.Localized;
        }

        /// <summary>
        /// Returns the localized text after formatting has been applied.
        /// Formatting will use SmartFormat is <see cref="IsSmart"/> is true else it will default to String.Format.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public string GetLocalizedString(params object[] args)
        {
            string translatedText = null;

            if (IsSmart)
            {
                translatedText = LocalizationSettings.StringDatabase.SmartFormatter.FormatWithCache(ref m_FormatCache, Data.Localized, args);
            }
            else if (!string.IsNullOrEmpty(Data.Localized))
            {
                translatedText = args == null ? Data.Localized : string.Format(Data.Localized, args);
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
        /// Creates a new, empty StringTableEntry.
        /// </summary>
        /// <returns></returns>
        public override StringTableEntry CreateTableEntry()
        {
            return new StringTableEntry() { Table = this, Data = new TableEntryData() };
        }
    }
}
