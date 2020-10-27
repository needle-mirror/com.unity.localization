using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor.Localization.Plugins.CSV;
using UnityEditor.Localization.Plugins.CSV.Columns;
using UnityEngine;

namespace UnityEditor.Localization.Samples.CSV
{
    public static class CsvExamples
    {
        /// <summary>
        /// This example show how to export a collection with default settings.
        /// </summary>
        public static void SimpleExport()
        {
            var collection = LocalizationEditorSettings.GetStringTableCollection("My Strings");
            using (var stream = new StreamWriter("My Strings CSV.csv", false, Encoding.UTF8))
            {
                Csv.Export(stream, collection);
            }
        }

        static List<CsvColumns> CreateCustomColumnMapping()
        {
            var columnMappings = new List<CsvColumns>();

            // We must have 1 KeyIdColumns so that we can map the table entries correctly
            columnMappings.Add(new KeyIdColumns
            {
                IncludeId = true, // Include the Id column field.
                IncludeSharedComments = true, // Include Shared comments.
            });

            // Export English with no comments
            columnMappings.Add(new LocaleColumns
            {
                LocaleIdentifier = "en",
                IncludeComments = false
            });

            // Export Japanese with comments
            columnMappings.Add(new LocaleColumns
            {
                LocaleIdentifier = SystemLanguage.Japanese,
                IncludeComments = true
            });

            return columnMappings;
        }

        /// <summary>
        /// This example shows how to configure the data you wish to export in CSV.
        /// </summary>
        public static void ExportCustomColumns()
        {
            var collection = LocalizationEditorSettings.GetStringTableCollection("My Strings");

            // Use custom column mappings to control what data gets exported
            var columns = CreateCustomColumnMapping();

            // Now export
            using (var stream = new StreamWriter("My Strings CSV.csv", false, Encoding.UTF8))
            {
                Csv.Export(stream, collection, columns);
            }
        }

        /// <summary>
        /// This example show how to import a collection with default settings.
        /// </summary>
        public static void SimpleImport()
        {
            var collection = LocalizationEditorSettings.GetStringTableCollection("My Strings");
            using (var stream = new StreamReader("My Strings CSV.csv"))
            {
                Csv.ImportInto(stream, collection);
            }
        }

        public static void ImportCustomColumns()
        {
            var collection = LocalizationEditorSettings.GetStringTableCollection("My Strings");

            // Use custom column mappings to control what data gets imported.
            var columns = CreateCustomColumnMapping();

            using (var stream = new StreamReader("My Strings CSV.csv"))
            {
                Csv.ImportInto(stream, collection, columns);
            }
        }

        /// <summary>
        /// This example shows how we can import every String Table Collection that contains a CSV Extension.
        /// </summary>
        [MenuItem("Localization Samples/CSV/Import All CSV Files")]
        public static void ImportAllExtensions()
        {
            // Get every String Table Collection
            var stringTableCollections = LocalizationEditorSettings.GetStringTableCollections();

            foreach (var collection in stringTableCollections)
            {
                // Its possible a String Table Collection may have more than one extension.
                foreach (var extension in collection.Extensions)
                {
                    if (extension is CsvExtension csvExtension)
                    {
                        if (!string.IsNullOrEmpty(csvExtension.File) && File.Exists(csvExtension.File))
                        {
                            using (var stream = new StreamReader(csvExtension.File))
                            {
                                Csv.ImportInto(stream, collection, csvExtension.Columns);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This example shows how we can export every String Table Collection that contains a CSV Extension.
        /// </summary>
        [MenuItem("Localization Samples/CSV/Export All CSV Files(With Extensions)")]
        public static void PullAllExtensions()
        {
            // Get every String Table Collection
            var stringTableCollections = LocalizationEditorSettings.GetStringTableCollections();

            foreach (var collection in stringTableCollections)
            {
                // Its possible a String Table Collection may have more than one extension.
                foreach (var extension in collection.Extensions)
                {
                    if (extension is CsvExtension csvExtension)
                    {
                        if (!string.IsNullOrEmpty(csvExtension.File))
                        {
                            using (var stream = new StreamWriter(csvExtension.File, false, Encoding.UTF8))
                            {
                                Csv.Export(stream, collection, csvExtension.Columns);
                            }
                        }
                    }
                }
            }
        }
    }
}
