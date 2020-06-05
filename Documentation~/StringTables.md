# String Tables

A String Table returns a localized string when requested via a **Key** (an identifying name or unique id) for a specific Locale. String Tables work in a similar way to Asset Tables, but they contain all of the translated string data within them, and they do not require an additional loading step.
Each row on a String Table contains a **Key Id** and a string entry for a specified Locale. A string entry can be either a static string or a tokenized string for use with [Smart Strings](SmartStrings.md) or [String.Format](https://docs.microsoft.com/dotnet/api/system.string.format).

## String Table Collection

A String Table Collection is an **Editor only** asset. It provides an interface to one or more String Tables that all share the same Shared Table Data and Table Collection Name.

When you create a new String Table Collection, Unity creates a String Table Collection asset and a String Table asset for each Locale, and a single Shared Table Data asset that the String Tables reference. The Shared Table Data asset stores all key names and associated Id values, as well as Shared [Metadata](Metadata.md) (Metadata that all String Tables can access).

![Example String Table Collection assets for English, French and Japanese.](images/StringTableCollection_ProjectView.png)

![A String Table Collection references all tables that are part of it and can be accessed through the editor or via Editor scripts.](images/StringTableCollection_Overview.png)

You can use the String Table Collection Editor to configure the Editor and player settings, and to add and remove tables from the collection. Removing a table from a collection removes the table from the Localization system, but leaves its data intact so that you can add it back in the future. This allows for chosen String Tables to be enabled and disabled.

![String Table Collection inspector view.](images/StringTableCollection_Inspector.png)

Extensions are Editor only data that you can attach to a String Table. An extension can contain additional data that you wish to associate with the String Tables, or it can add functionality such as an importer or exporter for a particular file format or service. 

To create custom Extensions, you can inherit from *CollectionExtension* and add the *AssetTableCollectionExtension attribute*.

## Preloading

![Preloading can be configured from the Table Properties panel.](images/StringTables_TableProperties.png)

By default, the Localization system does not pre-load a String Table. This means that the first time Unity requests a String Table or its contents, the Localization table must first load the table., This happens asynchronously in the background, however, it means that the requested string might not be immediately available. 
You can configure the Localization system to pre-load String Tables, so that they are immediately available upon request. To do this, navigate to the Asset Tables window, click the Table Properties button (above the Locale name), and enable **Preload All Tables** to preload all tables in this collection or **Preload Table** to only preload the selected table.

## Smart Strings

You can use the **Smart** field to enable or disable [Smart Formatting](SmartStrings.md) for the table entry. Smart Formatting uses a modified version of the [SmartFormat](https://github.com/axuno/SmartFormat) library. SmartFormat is a lightweight templating library, with an emphasis on grammar. It makes it easy for a data-driven template to have proper pluralization, gender conjugation, lists, and conditional language logic. Named placeholders give a more intuitive and less error-prone way to introduce variables.

## Metadata

You can view [Metadata](Metadata.md) for tables and table entries. 
- To view Metadata for a table, select the button above the table column header.
- To view Metadata for a table entry, select the button in the table entry cell.

The Localization system stores **Shared** Metadata in the **Shared Table Data** for the table collection. This means that all Tables have access to this data. Locale-specific Metadata (the list below Shared) is stored in the table and is only accessible by the table with the data.

![Editing Table Entry Metadata](images/StringTables_EntryProperties.png)
