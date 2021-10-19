# Scripting

This documentation describes the most common ways to interact with the Localization System via C# scripts.

You can download additional scripting examples from the package manager window, in the Localization package’s **Samples** section.

## LocalizedString

A `LocalizedString` accesses specific entries from within a [String Table](StringTables.md).

You can use the `LocalizedString` in a user script. It serializes and displays its own editor, which you can use to select the table and table entry, and edit the localized values inline, without opening the Tables window.

![The LocalizedString editor in Unity.](images/LocalizedString_Inspector.png)

The `LocalizedString` provides a `StringChanged` event. The script calls the event whenever the selected Locale changes. This makes your code simpler and more efficient, because the script only calls it when it needs to update the `LocalizedString`.

[!code-cs[localized-string-events](../DocCodeSamples.Tests/LocalizedStringSamples.cs#localized-string-events)]

## Dynamic Strings

Sometimes you might need to update a localized string, such as when using [Smart Strings](Smart/SmartStrings.md) or `String.Format` with arguments that have since changed. Calling `GetLocalizedString` with the arguments always updates the string. When you use the `StringChanged` event, you can use the `RefreshString` function to request an update, and the `Arguments` property to configure the arguments that format the string.

[!code-cs[localized-string-smart](../DocCodeSamples.Tests/LocalizedStringSamples.cs#localized-string-smart)]

## LocalizedAsset

LocalizedAsset is a generic class that accesses localized versions of Unity assets from within an [Asset Table](AssetTables.md).

You can use the `LocalizedAsset` in a user script. It serializes and displays its own editor, which you can use to select the table and table entry, and edit the localized values inline, without opening the Tables window.

![The LocalizedAsset editor in Unity.](images/LocalizedTexture_Inspector.png)

The `LocalizedString` provides an `AssetChanged` event. The script calls the event whenever a new localized asset is available, such as when the game language changes. This makes your code simpler and more efficient, because the script only calls it when it needs to update the `LocalizedAsset`.

To use a `LocalizedAsset`, you need to create a concrete version of the class and mark it as `Serializable`.

For example, to support localizing font assets, you could define the following class:

[!code-cs[localized-font](../DocCodeSamples.Tests/LocalizedFontSample.cs)]

In this example, you can now add `LocalizedFont` to a script, and `LocalizedString` will call the `AssetChanged` event when a localized Font asset is available.

The following Unity assets are already supported:

| **Type**          | **Class**             |
|-------------------|-----------------------|
| AudioClip         | `LocalizedAudioClip`  |
| GameObject/Prefab | `LocalizedGameObject` |
| Material          | `LocalizedMaterial`   |
| Sprite            | `LocalizedSprite`     |
| Texture           | `LocalizedTexture`    |

### TableReference

Use the [TableReference](xref:UnityEngine.Localization.Tables.TableReference) struct to reference an Asset Table or String Table. To reference a table, you can use either the table's name or its unique GUID. It is usually safer to use the GUID, because you might decide to change the table name in future, which would require you to manually update your references, but the GUID always stays the same. 

### TableEntryReference

Use the [TableEntryReference](xref:UnityEngine.Localization.Tables.TableEntryReference) struct to reference an entry in an Asset Table or String Table. To reference a table, you can use either the table entry's name or its unique Key ID, an unsigned long integer. It is usually safer to use the Key ID, because you might decide to change the table entry name in future, which would require you to manually update your references, but the Key ID always stays the same.

## Using AsyncOperationHandle

Unity does not hold all localized assets in memory ready for use. Instead, it loads them on demand when it needs them, and unloads them when it no longer needs them. Because of this, localized Assets might not be immediately available, and Unity might need to load them from disk or fetch them from a server. To facilitate this, Unity uses the [AsyncOperationHandle](https://docs.unity3d.com/Packages/com.unity.addressables@latest?subfolder=/manual/AddressableAssetsAsyncOperationHandle.html) as an interface to all requests.
When an Asset is not immediately available, the localization system returns an `AsyncOperationHandle`. When the operation has finished, the `AsyncOperationHandle` provides a `Completed` event to notify Unity. It calls this during `LateUpdate`. If the request has already completed (for example, when the requested data is already loaded from a previous request, or during preloading), you can check the `IsDone` property for immediate access via the `Result` property. Alternatively, the `Completed` event still occurs in `LateUpdate`, allowing for all code to follow the same path. You can also yield on an `AsyncOperationHandle` inside a coroutine.

To force an operation to complete on the main thread, call [WaitForCompletion](https://docs.unity3d.com/Packages/com.unity.addressables@latest?subfolder=/api/UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle.html#UnityEngine_ResourceManagement_AsyncOperations_AsyncOperationHandle_WaitForCompletion). See [Synchronous Workflow](https://docs.unity3d.com/Packages/com.unity.addressables@latest?subfolder=/manual/SynchronousAddressables.html) for further details.

## Make a basic Locale selection menu

This example demonstrates how to create a way for the person playing a game to select the language (defined in the Localization system by Locale) they want to use in the game.
To add a UI dropdown menu to the Scene, go to **GameObject > UI > Dropdown**, and attach the following script:

[!code-cs[locale-dropdown](../DocCodeSamples.Tests/LocaleDropdown.cs#example-code)]

You can see a more advanced version of this script in the Localization package samples.
