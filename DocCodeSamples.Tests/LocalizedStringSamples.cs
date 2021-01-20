using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;

#region localized-string-events

public class LocalizedStringWithEvents : MonoBehaviour
{
    public LocalizedString myString;

    string localizedText;

    /// <summary>
    /// Register a ChangeHandler. This is called whenever the string needs to be updated.
    /// </summary>
    void OnEnable()
    {
        myString.StringChanged += UpdateString;
    }

    private void OnDisable()
    {
        myString.StringChanged -= UpdateString;
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
#endregion

#region localized-string-smart

/// <summary>
/// This example expects a Smart String with a named placeholder of `TimeNow`, such as "The time now is {TimeNow}".
/// </summary>
public class LocalizedStringSmart : MonoBehaviour
{
    public LocalizedString myString;

    string localizedText;

    public float TimeNow => Time.time;

    /// <summary>
    /// Register a ChangeHandler. This is called whenever we need to update our string.
    /// </summary>
    void OnEnable()
    {
        myString.Arguments = new[] { this };
        myString.StringChanged += UpdateString;
    }

    private void OnDisable()
    {
        myString.StringChanged -= UpdateString;
    }

    void UpdateString(string s)
    {
        localizedText = s;
    }

    void OnGUI()
    {
        // This calls UpdateString immediately (if the table is loaded) or when the table is available.
        myString.RefreshString();
        GUILayout.Label(localizedText);
    }
}
#endregion
