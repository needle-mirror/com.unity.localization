# What's new in **1.5.0**

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the [Changelog](../changelog/CHANGELOG.html).

The main updates in this release include:

- Added content directory support for Addressables 4.0. Localization Addressable groups are now created from an Addressables **Group Template** that you assign per rule set, and new Analyze rules switch existing groups between content directories and AssetBundles. See [documentation](Addressables.md#group-template) for more details.
- Added support for UI Toolkit data bindings. Localized strings and assets can now be added as data bindings to UI Toolkit elements. See [documentation](UIToolkit.md) for more details.
- The Localization Tables window has had several UX improvements made. It is now also possible to add custom menu items to the tables window through `LocalizationImportMenuAttribute`, `LocalizationExportMenuAttribute` and `LocalizationEntryMenuAttribute`.
- Added `AsynchronousBehavior` to string and asset database. These can be used to set a global `WaitForCompletion` flag instead of having to set it on every `LocalizedAsset` and `LocalizedString`.
