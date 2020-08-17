# Scripting

The common ways to interact with the Localization System using scripts are covered here.

## Localized Reference

A Localized Reference allows for referencing an entry inside of a Table. Internally it contains a TableReference and a TableEntryReference.

### TableReference
A TableReference refers to a Table, tables can be referenced by either by using their **Name** or by a name **Guid**. By using a name Guid it is safe to rename the table without losing all references. The name Guid is taken from the **KeyDatabase** asset Guid.
The TableReference provides a simple way to reference a table either by the name of the Guid, it also features inspector and serialization support.

### TableEntryReference
A TableEntryReference refers to a specific entry within a Table. It is possible to reference a table entry by either using the **Name** or **Key** of the entry or by using the **Key Id**, an unsigned integer. The TableEntryReference provides a simple way to reference a table entry by the name or Id, it also features inspector and serialization support.

### LocalizedString
The LocalizedString is a Localized Reference that can be used to reference a [String Table](StringTables.md) entry.
The LocalizedString editor allows for selecting String Table Entries, Creating new Table entries, Creating new String Tables and new Locales.

![The LocalizedString editor.](images/LocalizedString_Inspector.png)

![String Table Entries can be selected through the inspector.](images/LocalizedStringReference_Select.png)

The following simple example shows how a LocalizedString can use used to Localize some GUI text.

```
using UnityEngine;
using UnityEngine.Localization;

public class LocalizedStringExample : MonoBehaviour
{
    public LocalizedString myString;

    void OnGUI()
    {
        var localizedText = myString.GetLocalizedString();
        if (localizedText.IsDone)
        {
            GUILayout.Label(localizedText.Result);
        }
    }
}
```

The LocalizedString also provides a **ChangeHandler**, this will be called whenever the selected Locale is changed allowing for more efficient code that is only called when an update is required.

```
using UnityEngine;
using UnityEngine.Localization;

public class LocalizedStringExample : MonoBehaviour
{
    public LocalizedString myString;

    string localizedText;

    /// <summary>
    /// Register a ChangeHandler. This will be called whenever we need to update our string.
    /// </summary>
    void OnEnable()
    {
        myString.RegisterChangeHandler(UpdateString);
    }

    private void OnDisable()
    {
        myString.ClearChangeHandler();
    }

    void UpdateString(string s)
    {
        localizedText = s;
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField(localizedText);
    }
}
}
```

### Dynamic Strings

At times it may be necessary to update a localized string, such as when using [Smart Strings](SmartStrings.md) or *String.Format* with arguments that have since changed.
Calling **GetLocalizedString** with the arguments will always update the string.
When using a **ChangeHandler**, the **RefreshString** function can be used to request an update and the **Arguments** property can be used to configure the arguments used to format the string.

```
using UnityEngine;
using UnityEngine.Localization;

/// <summary>
/// This example expects a Smart String with a named placeholder of `TimeNow`, such as "The time now is {TimeNow}".
/// </summary>
public class LocalizedStringExample : MonoBehaviour
{
    public LocalizedString myString;

    string localizedText;

    public float TimeNow => Time.time;

    /// <summary>
    /// Register a ChangeHandler. This will be called whenever we need to update our string.
    /// </summary>
    void OnEnable()
    {
        myString.Arguments.Add(this); // Add our new argument
        myString.RegisterChangeHandler(UpdateString);
    }

    private void OnDisable()
    {
        myString.ClearChangeHandler();
    }

    void UpdateString(string s)
    {
        localizedText = s;
    }

    void OnGUI()
    {
        // This will either call UpdateString immediately (if the table is loaded) or when the table is available.
        myString.RefreshString();
        GUILayout.Label(localizedText);
    }
}
```

## Using AsyncOperationHandle

The localization system is designed so that Unity does not need to hold all localized Assets in memory ready for use, but can instead load them on demand when it needs them, and unload them when it no longer needs them. Because of this, localized Assets might not be immediately available, and Unity might need to load them from disk or fetch them from a server. To facilitate this, Unity uses the [AsyncOperationHandle](https://docs.unity3d.com/Packages/com.unity.addressables@latest?subfolder=/manual/AddressableAssetsAsyncOperationHandle.html) as an interface to all requests.

When an Asset is not immediately available, the localization system returns an *AsyncOperationHandle*. The *AsyncOperationHandle* provides a Completed event that notifies Unity when the operation has finished. It calls this during *LateUpdate*. If the request has already been completed, such as when the requested data is already loaded from a previous request or during preloading, then the *IsDone* property can be used to check for immediate access through the *Result* property, alternative the Completed event still occurs in the LateUpdate, allowing for all code to follow the same path. You can also yield on an *AsyncOperationHandle* inside of a coroutine.

## A Simple Locale Selection Menu

Players need a way to select the Locale they want to use in the game. To add a UI Dropdown menu to the Scene, go to **GameObject > UI > Dropdown**, and attach the following script:

```
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class LocaleDropdown : MonoBehaviour
{
    public Dropdown dropdown;

    IEnumerator Start()
    {
        // Wait for the localization system to initialize, loading Locales, preloading etc.
        yield return LocalizationSettings.InitializationOperation;

        // Generate list of available Locales
        var options = new List<Dropdown.OptionData>();
        int selected = 0;
        for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; ++i)
        {
            var locale = LocalizationSettings.AvailableLocales.Locales[i];
            if (LocalizationSettings.SelectedLocale == locale)
                selected = i;
            options.Add(new Dropdown.OptionData(locale.name));
        }
        dropdown.options = options;

        dropdown.value = selected;
        dropdown.onValueChanged.AddListener(LocaleSelected);
    }

    static void LocaleSelected(int index)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
    }
}
```
