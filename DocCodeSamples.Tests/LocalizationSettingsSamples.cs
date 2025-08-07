using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

#region asynchronous

public class InitializationOperationExampleAsync : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return LocalizationSettings.InitializationOperation;

        Debug.Log("Initialization Completed");
    }
}
#endregion

#region asynchronous-event

public class InitializationOperationExampleAsyncEvent : MonoBehaviour
{
    void Start()
    {
        var init = LocalizationSettings.InitializationOperation;
        init.Completed += a => Debug.Log("Initialization Completed");
    }
}
#endregion

#region synchronous

public class InitializationOperationExampleSync : MonoBehaviour
{
    void Start()
    {
        // Force initialization to complete synchronously.
        LocalizationSettings.InitializationOperation.WaitForCompletion();
    }
}
#endregion

#region selected-locale-changed

public class SelectedLocaleChangedExample : MonoBehaviour
{
    Locale currentLocale;

    void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
    }

    void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
    }

    IEnumerable Start()
    {
        // Get the initial selected locale value
        var selectedLocale = LocalizationSettings.SelectedLocaleAsync;
        yield return selectedLocale;
        currentLocale = selectedLocale.Result;
    }

    void OnSelectedLocaleChanged(Locale locale)
    {
        currentLocale = locale;
    }

    void OnGUI()
    {
        if (currentLocale != null)
            GUILayout.Label("The current locale is " + currentLocale.LocaleName);
    }
}
#endregion

#region force-update

public class ForceUpdateExample : MonoBehaviour
{
    public LocalizedString myString = new LocalizedString("MyTable", "MyEntry");

    void OnEnable()
    {
        myString.StringChanged += UpdateText;
    }

    void OnDisable()
    {
        myString.StringChanged -= UpdateText;
    }

    void UpdateText(string text)
    {
        Debug.Log("The value is " + text);
    }

    // This method is called to change the value in the string table.
    [ContextMenu("Change Source Value")]
    public void ChangeSourceValue()
    {
        // Get the table and entry, and update the value.
        var tableEntry = LocalizationSettings.StringDatabase.GetTableEntry(myString.TableReference, myString.TableEntryReference);
        tableEntry.Entry.Value = "New Value";

        // Force a refresh to update everything, including any uses of the changed entry.
        LocalizationSettings.Instance.ForceRefresh();
    }
}
#endregion
