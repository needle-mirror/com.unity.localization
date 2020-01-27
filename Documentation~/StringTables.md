# String Tables
A String Table is responsible for returning a localized string when requested via a **Key** (an identifying name or unique id).
String Tables work in a similar way to Asset Tables, but they contain all of the translated string data within them, and they do not require an additional loading step.


Each row on a String Table contains a **Key Id** and a string entry for a specified Locale. A string entry can be either a static string or a tokenized string for use with [Smart Strings](SmartStrings.md) or *String.Format*.



## Preloading

![Preloading can be configured from the Table Properties panel.](images/StringTables_TableProperties.png)

By default a String Table is not preloaded, this means that the first time a request is made for a String Table or its contents, the table must be loaded, this will be performed asynchronously in the background however it will mean that the requested string may not be immediately available. String Tables can be preloaded so that they will be immediately available upon request by using the Preload Flag. This can be set by clicking the Table Properties button (Above the Locale name) and either enabling  **Preload All Tables** to mark all tables in this collection or **Preload Table** to only preload the selected table.

## Smart Strings

The **Smart** field can be used to enable or disable [Smart Formatting](SmartStrings.md) for the table entry. Smart Formatting utilizes a modified version of the [SmartFormat](https://github.com/axuno/SmartFormat) library, *SmartFormat is a lightweight templating library, with an emphasis on grammar.
It makes it easy for a data-driven template to have proper pluralization, gender conjugation, lists, and conditional language logic. Named placeholder give a more intuitive and less error-prone way to introduce variables.*.

## Table Metadata
The [Metadata](Metadata.md) for a table can be shown by selecting the table properties via the button above the table column header.

**Shared** Metadata is stored in the **Key Database** for the table collection, this means that all Tables have access to this data.
Locale specific Metadata(The list below Shared) is stored in the table and is only accessible by the table with the data.

## Table Entry Metadata
The [Metadata](Metadata.md) for a table entry can be shown by selecting the table entry properties via the button in the table entry cell.

**Shared** Metadata is stored in the **Key Database Entry**  for the table collection, this means that all Tables have access to this data.
Locale specific Metadata(The list below Shared) is stored in the table entry and is only accessible by the table with the data.

![Editing Table Entry Metadata](images/StringTables_EntryProperties.png)
