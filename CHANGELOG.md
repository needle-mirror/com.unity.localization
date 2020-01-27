# Changelog
All notable changes to this package will be documented in this file.

## [0.6.0] - 2020-01-27
### Added
- Added PseudoLocale and Pseudo-localization methods(Accenter, CharacterSubstitutor, Encapsulator and Expander). This is used to generate Pseudo-localization using various methods during runtime(See docs for further info).
- Added support for loading multiple Locales from String and Asset Databases. By Default the **LocalizationSettings.SelectedLocale** will be used however it is now possible to provide a Locale as an argument. This means Localization data for multiple Locales can exist which allows for fallback Locales or just showing multiple languages at once.
- Added exception to LocalizedTable.TableName if the SharedTableData is null.
- Added **Locale Generator** button to **New Table** tab.
### Changed
- Renamed **KeyDatabase** to **SharedTableData**.
- Changed **Comment** metadata TimeStamp property type to DateTime.
- Disabled Addressables Settings asset creation during Localization OnPostprocessAllAssets. This could cause corruption of Addressables data due to OnPostprocessAllAssets execution order when importing a project for the first time.
- Updated to Addressables version *1.5.1*.
- Addressable groups now use the NoHash BundleNamingStyle by default.
### Fixed
- Fixed **NullReferenceException** sometimes being thrown by LocalizedString or LocalizedAsset during *ClearLoadingOperation*.
- Fixed **NullReferenceException** when adding SmartFormatter Source in String Database inspector.
- Fixed **ArgumentOutOfRangeException:** in StringTables when enabling a Missing Tables column.
- Fixed LocalizedReference field label overflowing when a selected key had multiple lines.
- Fixed LocalizedString property drawer adding too much vertical whitespace.
- Fixed items in Reorderable Lists ignoring the right border.
- Fixed LocalizedTableT entry Id not being updated when entry was moved or removed.
- Fixed GameViewLanguageMenu causing LocalizationSettings to Initialize when there was no Addressables/Locales in the project.
- Fixed Key name being unchangeable when its name was empty in the Asset Tables window.
- Fixed Search field border being too small in the Asset Tables Window.
- Fixed **New Table - Create** button being enabled when no locales were selected or existed in the project.
- Fixed **LocalizedString** and **LocalizedAsset** not automatically updating when the **TableReference** or **TableEntryReference** was changed and a **ChangeHandler** was being used.
- Fixed Addressable flags including **Preload** being removed during AddOrUpdateTable.
 
## [0.5.1] - 2019-11-22
- Updated to Addressables version *1.3.8*.
- Fixed **ArgumentOutOfRangeException:** in the Tables window when only 1 string or asset table existed.
- Exposed *CreateAssetTableCollection* in *LocalizationEditorSettings*.
- Implemented *IDictionary* to *LocalizedTableT* and Removed *TableEntries* property.
- Fixed LocalizedTableT not serializing changes made when using the Dictionary interface.
- Added *LocalizedTableT.CreateTableEntry*, this should be used when creating a *StringTableEntry* or *AssetTableEntry*.
- *ArgumentNullException* is now thrown when using a null ChangeHandler with *LocalizedString.RegisterChangeHandler* or *LocalizedAsset.RegisterChangeHandler*.
- Fixed *PluralLocalizationFormatter* using the Selected Locale when a CultureInfo provider was used.
- Current loading operations are now cleared when using *ClearChangeHandler* with *LocalizedAsset* and *LocalizedString*
- Added default constructor to FallbackLocale Metadata.
- Fixed error spamming when adding Metadata that does not have a default constructor.
- Removed missing image from LocalizationTablesWindow manual page.

## [0.5.0] - 2019-11-04
- Updated to Addressables version *1.2.4*.
- Removed type specific asset tables. The AssetTable class can now hold mixed types per entry.
- Removed Custom Locale support from Locale Generator Window. You should use Metadata to modify the Locale instead.
- Removed Preload behaviour from Localization Settings. Preload behaviour can now be controlled per table through the Asset Tables Window.
- Removed Addressable tests. These would cause issues when run in a project already using Addressables. They have been moved to an internal test suite.
- Removed *Key Database* field from *New Table* panel. Key databases should not be shared now as they act as the Table Name Guid.
- Added Metadata to Locale, LocalizedTable and TableEntry. Metadata can be used to add additional data that implements the IMetadata interface and is serializable.
- Added PreloadAssetTable Metadata. Add this to an AssetTable to configure the preload behaviour for individual assets contained in the table. By default the assets will not be preloaded if this is not present.
- Added PreloadAsset Metadata. Add this to Asset Table entries when using PreloadBehaviour.PreloadSelected to select the entry for preloading.
- Added Comment Metadata. This can be added to any Metadata to provide comments. In the future these comments will be extracted when exporting to external formats.
- Added SmartFormat for StringTables. This allows for advanced formatting of strings. Comes with support for named placeholders, lists, pluralization, gender, code reflection support and much more.
- Added Table Name Guid. Table Names can now be referenced by a Guid so that changes to the name do not break references.
- Fixed Locale Generator Window button **Generate Locales** staying disabled when using **Select All**.
- LocaleGeneratorListView now shows Locales that are already in the project.
- New Locales are now added to Addressables when created.
- Changed the name format for Tables for Addressables and asset creation to start with the name followed by the locale.
- Improved LocalizedString editor. It is now possible to modify the selected entry details in the Inspector without opening the Asset Tables Window.
- Moved Localization Table editor window option to *Window/Asset Management/Localization Tables*.

## [0.4.1] - 2019-07-09
- Updated to Addressables version '1.1.4-preview'.

## [0.4.0] - 2019-06-14
- Added AutoUpdate to LocalizedStringReference class. When enabled, the string will automatically be loaded any time a change is detected and the UpdateString event can be used to update the string. With AutoUpdate enabled calling GetLocalizedString is not necessary.
- Fixed StringTableEditor changes being lost when using inline editing.
- Fixed LocalizationBehaviour not updating the first time when LocalizationSettings are already initialized.
- Fixed LocalizationBehaviour not updating correctly if the asset or table has already been loaded.
- Improved drop down label in LocalizedStringReferencePropertyDrawer. Multi-line text is now correctly shown. (LOC-57)
- Added KeyDatabase.FindSimilarKey. Returns the closest match using a Levenshtein approximation.
- Improved key matching when using Localize context menu for Text components.
- Updated to Addressables version '0.8.6-preview'.

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
- Added option in **Asset Tables Windows** to add a missing table if a table does not exist for all available Locales.
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
- Removed stray file **README - External.md**

## [0.2.0] - 2018-12-11

### This is the first release of *Unity Package Localization*.
- Contains preview of asset and string localization support.
