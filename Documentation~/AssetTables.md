# Asset Tables
An Asset Table is responsible for returning an Asset when requested via a **Key** (an identifying name or unique id).
An entry in an Asset Table contains a **Key Id** and the GUID of the referenced Asset that will be used to load it via the Addressables system.
An Asset Table entry can reference any Unity asset.

## Preloading

![Preloading can be configured from the Table Properties panel.](images/AssetTables_TableProperties.png)

By default an Asset Table and its contents are not preloaded, this means that the first time a request is made for an Asset, the table must be loaded and then the requested asset must be loaded, this will be performed asynchronously in the background however it will mean that the requested asset may not be immediately available. Assets can be preloaded so that they will be immediately available upon request by using the Preload Flag. This can be set by clicking the Table Properties button (Above the Locale name) and either enabling  **Preload All Tables** to mark all tables in this collection or **Preload Table** to only preload the selected table.

## Table Metadata
The [Metadata](Metadata.md) for a table can be shown by selecting the table properties via the button above the table column header.

**Shared** Metadata is stored in the **Key Database** for the table collection, this means that all Tables have access to this data.
Locale specific Metadata(The list below Shared) is stored in the table and is only accessible by the table with the data.

## Table Entry Metadata
The [Metadata](Metadata.md) for a table entry can be shown by selecting the table entry properties via the button in the table entry cell.

**Shared** Metadata is stored in the **Key Database Entry**  for the table collection, this means that all Tables have access to this data.
Locale specific Metadata(The list below Shared) is stored in the table entry and is only accessible by the table with the data.

![Editing Table Entry Metadata](images/AssetTables_EntryProperties.png)
