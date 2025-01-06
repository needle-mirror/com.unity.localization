using UnityEngine;
using UnityEngine.Localization.Components;

#region update-string
public class MyCustomLocalizedStringEvent : LocalizeStringEvent
{
    protected override void UpdateString(string translatedValue)
    {
        Debug.Log("UpdateString called with " + translatedValue);
    }
}

#endregion

#region refresh-string
public class RefreshStringExample : MonoBehaviour
{
    // Set via inspector
    public LocalizeStringEvent localizeStringEvent;

    // Set via code
    [HideInInspector]
    public int someValue;

    void Start()
    {
        // Assuming the localized string is in the format "The value is {someValue}"
        localizeStringEvent.StringReference.Arguments = new object[] { this };

        // Add a listener to the event
        localizeStringEvent.OnUpdateString.AddListener(OnStringChanged);
    }

    void OnStringChanged(string s)
    {
        Debug.Log($"String changed to `{s}`");
    }

    public void SetValue(int value)
    {
        someValue = value;

        // The localized string does not know the value has changed so we call RefreshString to force an update.
        localizeStringEvent.RefreshString();
    }
}

#endregion
