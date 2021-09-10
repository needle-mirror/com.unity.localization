using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization
{
    /// <summary>
    /// Provides methods for managing multiple <see cref="StringTable"/> in the Editor.
    /// </summary>
    public class StringTableCollection : LocalizationTableCollection
    {
        /// <inheritdoc/>
        protected internal override Type TableType => typeof(StringTable);

        /// <inheritdoc/>
        protected internal override Type RequiredExtensionAttribute => typeof(StringTableCollectionExtensionAttribute);

        /// <inheritdoc/>
        protected internal override string DefaultGroupName => "String Table";

        /// <summary>
        /// A helper property which is the contents of <see cref="LocalizationTableCollection.Tables"/> loaded and cast to <see cref="StringTable"/>.
        /// </summary>
        public virtual ReadOnlyCollection<StringTable> StringTables => new ReadOnlyCollection<StringTable>(Tables.Select(t => t.asset as StringTable).ToList().AsReadOnly());

        /// <summary>
        /// Returns a string that contains all the unique characters that are used for all localized values in the tables that belong to the supplied <see cref="LocaleIdentifier"/>'s.
        /// This will also include Smart String entries but will only consider the <see cref="UnityEngine.Localization.SmartFormat.Core.Parsing.LiteralText"/> values,
        /// it will not consider <see cref="UnityEngine.Localization.SmartFormat.Core.Parsing.Placeholder"/> values.
        /// </summary>
        /// <param name="localeIdentifiers">The tables to be included.</param>
        /// <returns>All distinct characters or an empty string if no tables or entries.</returns>.
        public string GenerateCharacterSet(params LocaleIdentifier[] localeIdentifiers)
        {
            if (localeIdentifiers == null || localeIdentifiers.Length == 0)
                throw new ArgumentException(nameof(localeIdentifiers), "Must provide at least 1 LocaleIdentifier");

            var characters = ExtractLiteralCharacters(localeIdentifiers);
            var distinct = characters.Distinct().OrderBy(c => c);
            return string.Concat(distinct);
        }

        internal IEnumerable<char> ExtractLiteralCharacters(params LocaleIdentifier[] localeIdentifiers)
        {
            IEnumerable<char> e = "";
            foreach (var id in localeIdentifiers)
            {
                // Create an enumerator for all the tables and entries.
                var table = GetTable(id) as StringTable;
                if (table != null)
                    e = e.Concat(table.CollectLiteralCharacters());
            }
            return e;
        }

        /// <summary>
        /// Returns an enumerator that can be used to step through each key and its localized values, such as in a foreach loop.
        /// Internally <see cref="SharedTableData"/> and <see cref="StringTable"/>'s are separate assets with their own internal list of values.
        /// This means that when iterating through each key a lookup must be made in each table in order to retrieve the localized value,
        /// this can become slow when dealing with a large number of tables and entries.
        /// GetRowEnumerator improves this process by first sorting the multiple internal lists and then stepping through each conceptual row at a time.
        /// It handles missing keys and table entries and provides a more efficient and faster way to iterate through the tables.
        /// </summary>
        /// <example>
        /// This example shows how a StringTableCollection could be exported as CSV.
        /// <code source="../../DocCodeSamples.Tests/TableCollectionSamples.cs" region="row-enumerator"/>
        /// </example>
        /// <returns></returns>
        public IEnumerable<Row<StringTableEntry>> GetRowEnumerator() => GetRowEnumerator<StringTable, StringTableEntry>(StringTables);

        /// <summary>
        /// <inheritdoc cref="GetRowEnumerator"/>
        /// </summary>
        /// <param name="tables"></param>
        /// <returns></returns>
        public static IEnumerable<Row<StringTableEntry>> GetRowEnumerator(params StringTable[] tables) => GetRowEnumerator<StringTable, StringTableEntry>(tables);

        ///<inheritdoc/>
        public override void RemoveEntry(TableEntryReference entryReference)
        {
            var entry = SharedData.GetEntryFromReference(entryReference);
            if (entry == null)
                return;

            foreach (var table in StringTables)
                table.RemoveEntry(entry.Id);
            SharedData.RemoveKey(entry.Key);

            LocalizationEditorSettings.EditorEvents.RaiseTableEntryRemoved(this, entry);
        }
    }
}
