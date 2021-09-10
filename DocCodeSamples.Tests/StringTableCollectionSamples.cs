using System.Text;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;

public class StringTableCollectionSamples
{
    [MenuItem("CONTEXT/StringTableCollection/Print CSV")]
    public static void CreateCSV(MenuCommand command)
    {
        var collection = command.context as StringTableCollection;

        var sb = new StringBuilder();

        // Header
        sb.Append("Key,");
        foreach (var table in collection.StringTables)
        {
            sb.Append(table.LocaleIdentifier);
            sb.Append(",");
        }
        sb.AppendLine();

        // Add each row
        foreach (var row in collection.GetRowEnumerator())
        {
            // Key column
            sb.Append(row.KeyEntry.Key);
            sb.Append(",");

            foreach (var tableEntry in row.TableEntries)
            {
                // The table entry will be null if no entry exists for this key
                sb.Append(tableEntry == null ? string.Empty : tableEntry.Value);
                sb.Append(",");
            }
            sb.AppendLine();
        }

        // Print the contents.
        Debug.Log(sb.ToString());
    }
}
