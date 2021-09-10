using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor.Localization.Plugins.CSV.Columns;
using UnityEditor.Localization.Reporting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.Plugins.CSV
{
    static class MenuItems
    {
        const string kPrefFile = "Localization-csv-file";

        public static string PreviousDirectory
        {
            get
            {
                var file = EditorPrefs.GetString(kPrefFile, "Assets/");
                return Path.GetDirectoryName(file);
            }
        }

        [MenuItem("CONTEXT/StringTableCollection/Export/CSV")]
        public static void ExportCollection(MenuCommand command)
        {
            var collection = command.context as StringTableCollection;
            Assert.IsTrue(collection != null, "Expected StringTableCollection");
            Export(ColumnMapping.CreateDefaultMapping(false), collection);
        }

        [MenuItem("CONTEXT/StringTableCollection/Export/CSV(With Comments)")]
        public static void ExportCollectionWithComments(MenuCommand command)
        {
            var collection = command.context as StringTableCollection;
            Assert.IsTrue(collection != null, "Expected StringTableCollection");
            Export(ColumnMapping.CreateDefaultMapping(true), collection);
        }

        [MenuItem("CONTEXT/StringTable/Export/CSV")]
        static void ExportStringTable(MenuCommand command)
        {
            var table = command.context as StringTable;
            Assert.IsTrue(table != null, "Expected StringTable");
            ExportTable(table, false);
        }

        [MenuItem("CONTEXT/StringTable/Export/CSV(With Comments)")]
        static void ExportStringTableWithComments(MenuCommand command)
        {
            var table = command.context as StringTable;
            Assert.IsTrue(table != null, "Expected StringTable");
            ExportTable(table, true);
        }

        static void ExportTable(StringTable table, bool includeComments)
        {
            var collection = LocalizationEditorSettings.GetCollectionFromTable(table) as StringTableCollection;
            if (collection == null)
            {
                Debug.LogError("String Table must belong to a StringTableCollection.");
                return;
            }

            var cellMappings = new CsvColumns[] { new KeyIdColumns { IncludeSharedComments = includeComments }, new LocaleColumns { LocaleIdentifier = table.LocaleIdentifier, IncludeComments = includeComments } };
            Export(cellMappings, collection);
        }

        static void Export(IList<CsvColumns> cellMappings, StringTableCollection collection)
        {
            var path = EditorUtility.SaveFilePanel($"Export {collection.TableCollectionName} to CSV", PreviousDirectory, collection.TableCollectionName, "csv");
            if (string.IsNullOrEmpty(path))
                return;

            EditorPrefs.SetString(kPrefFile, path);

            // Use FileShare.ReadWrite to avoid IOException: Sharing violation (LOC-348)
            using (var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
            {
                var stream = new StreamWriter(fs, Encoding.UTF8);
                var reporter = TaskReporter.CreateDefaultReporter();
                reporter.Start("Exporting " + path, string.Empty);
                Csv.Export(stream, collection, cellMappings, reporter);
            }

            EditorUtility.RevealInFinder(path);
        }

        [MenuItem("CONTEXT/StringTableCollection/Import/CSV")]
        public static void ImportCollection(MenuCommand command)
        {
            var collection = command.context as StringTableCollection;
            Assert.IsTrue(collection != null, "Expected StringTableCollection");

            var path = EditorUtility.OpenFilePanel($"Import CSV into {collection.TableCollectionName}", PreviousDirectory, "csv");
            if (string.IsNullOrEmpty(path))
                return;

            EditorPrefs.SetString(kPrefFile, path);

            // Use FileShare.ReadWrite to avoid IOException: Sharing violation (LOC-348)
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var stream = new StreamReader(fs);
                var reporter = TaskReporter.CreateDefaultReporter();
                reporter.Start("Importing " + path, string.Empty);
                Csv.ImportInto(stream, collection, true, reporter);
            }
        }

        [MenuItem("CONTEXT/StringTable/Import/CSV File")]
        static void ImportStringTable(MenuCommand command)
        {
            var table = command.context as StringTable;
            Assert.IsTrue(table != null, "Expected StringTable");

            var path = EditorUtility.OpenFilePanel($"Import CSV into {table.TableData}({table.LocaleIdentifier})", PreviousDirectory, "csv");
            if (string.IsNullOrEmpty(path))
                return;

            EditorPrefs.SetString(kPrefFile, path);

            var collection = LocalizationEditorSettings.GetCollectionFromTable(table) as StringTableCollection;
            if (collection == null)
            {
                Debug.LogError("String Table must belong to a StringTableCollection.");
                return;
            }

            var cellMappings = new CsvColumns[] { new KeyIdColumns(), new LocaleColumns { LocaleIdentifier = table.LocaleIdentifier } };

            // Use FileShare.ReadWrite to avoid IOException: Sharing violation (LOC-348)
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var stream = new StreamReader(fs);
                var reporter = TaskReporter.CreateDefaultReporter();
                reporter.Start("Importing " + path, string.Empty);
                Csv.ImportInto(stream, collection, cellMappings, true, reporter);
            }
        }
    }
}
