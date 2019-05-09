# Changelog
All notable changes to this package will be documented in this file.

## [0.3.2] - 2019-05-09
- Added support for ugui package in 2019.2.0a15. ugui support is optional and will be enabled automatically if the ugui package is in the project.
- Removed support for 2018.3. The ugui support requires optional assemblies which are not supported in 2018.3.
- AssemblyScannerCache now uses UnityEditor.TypeCache in 2019.2 which should provide improved performance in Editor.
- Added an error when adding non-persistent assets to asset tables.
- Fixed StringTableEditor changes being lost due to not setting the table dirty.

## [0.3.1] - 2019-05-02
- Added KeyDatabase. All string keys are now contained within this and mapped to a unique(to the database) id value which is used by the table.
- Assets are now grouped in Addressables by the locale. This allows you to serve them on-demand for a particular language, instead of bundling them with the game release. Shared assets are stored in a separate group.
- Assets are now removed from Addressables when they are not referenced by any Asset Tables.
- Fixed UIElements issues in 2019.1+.
- Merged LocalizationPlayerSettings into LocalizationEditorSettings.
- Removed AddressableAssetTableT.GetAsset. GetAssetAsync should be used instead.
- Added option in `Asset Tables Windows` to add a missing table if a table does not exist for all available Locales.
- Fixed UIElements compilation issues in 2019.1+
- Updated to Addressables version '0.7.5-preview'.
- Removed support for .Net 3.5, this is due to a limitation in Addressables.

## [0.2.5] - 2019-03-01
- Updated to Addressables version '0.6.7-preview'.

## [0.2.3] - 2019-02-20
- Fixed adding Locales and Tables to Addressables not working when the Addressables settings asset did not exist(LOC-24).
- Fixed exception when changing a table name in the Table Editor window.
- Fixed AssetTablesField exception when no Asset Tables exist in the project.
- Updated to Addressables version '0.5.3-preview'.

## [0.2.2] - 2018-12-17
- Fixed NullRefException when opening the Asset Tables window and no Asset Tables exist in the project

## [0.2.1] - 2018-12-14
- Removed stray file `README - External.md`

## [0.2.0] - 2018-12-11

### This is the first release of *Unity Package Localization*.
- Contains preview of asset and string localization support.
