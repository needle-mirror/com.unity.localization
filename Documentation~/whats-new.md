# What's new in **1.0.0**

This section contains information about new features, improvements, and issues fixed.

For a complete list of changes made, refer to the [Changelog](../changelog/CHANGELOG.html).

The main updates in this release include:

## Added

- Added a new Localized Asset and Localized String picker which uses **Unity Search**. This has improved performance and supports multiple localization filters and custom table view columns. This is also available in the Unity Search window but does not currently do anything when an entry is selected. The legacy picker can be reenabled through the preferences window.
- Added additional Persistent Variable types for: byte, sbyte, short, ushort, long, ulong, uint and double.
- Added support for localizing the User Tracking Description (NSUserTrackingUsageDescription) on iOS.

## Fixed

- Improved the detection of the Android system language.
- The SystemLocale selector now detects the iOS app preferred language.

## Deprecated

- `LocalizationEditorSettings.FindSimilarKey` is now deprecated and will be removed in the future. Unity Search now supports searching. See [TableEntrySearchData](xref:UnityEditor.Localization.Search.TableEntrySearchData) for further details.
