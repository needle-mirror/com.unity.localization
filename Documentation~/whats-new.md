# What's new in **1.4.2**

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the [Changelog](../changelog/CHANGELOG.html).

The main updates in this release include:

## Added

- Added `ITableProvider` and `ITablePostProcessor`. These let you provide tables from custom localizations and to apply changes to a table when it first loads. (LOC-673)
- Added support for referencing sub-assets. Previously we would only address the main asset and extract the sub-asset by matching the type. This was unpredictable when the main asset was the same type or contained multiple sub-assets with the same type. We now address the sub-asset by name so that we can ensure we get it correctly regardless of the type or number of other sub-assets. (LOC-793)
- Added support for tracking a [MeshFilter](https://docs.unity3d.com/ScriptReference/MeshFilter.html) when using **Localized Property Variants**. (LOC-719)
- Added **Preload Behavior** to **Localization Settings**. This allows further control over which assets to preload, such as loading fallback assets as well as the selected locale. (LOC-762)
