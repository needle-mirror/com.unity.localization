#if PACKAGE_UGUI

using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

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

    void OnDisable()
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

    void OnDisable()
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

#region get-localized-string

public class LocalizedStringGetExample : MonoBehaviour
{
    public Text myText;

    public LocalizedString myString = new LocalizedString
    {
        TableReference = "My String Table",
        TableEntryReference = "My Game Text"
    };

    void OnEnable()
    {
        LocalizationSettings.SelectedLocaleChanged += LocaleChanged;
        LoadString();
    }

    void OnDisable()
    {
        LocalizationSettings.SelectedLocaleChanged -= LocaleChanged;
    }

    void LocaleChanged(Locale locale)
    {
        LoadString();
    }

    void LoadString()
    {
        var operation = myString.GetLocalizedStringAsync();
        UpdateString(operation);
    }

    void UpdateString(AsyncOperationHandle<string> value)
    {
        if (!value.IsDone)
        {
            // Defer the callback until the operation is finished
            value.Completed += UpdateString;
            return;
        }

        myText.text = value.Result;
    }
}
#endregion

public class LocalizedStringConstructor
{
    #region localized-string-constructor

    public LocalizedString usingNames = new LocalizedString("My String Table", "My Game Text");

    public LocalizedString usingTableNameAndEntryId = new LocalizedString("My String Table", 4324324541);

    public LocalizedString usingTableGuidAndEntryId = new LocalizedString(new System.Guid("6e79ded14bc9e0a4d9bf2b8aac246bfe"), 323453434);

    public LocalizedString usingTableGuidAndEntryName = new LocalizedString(new System.Guid("6e79ded14bc9e0a4d9bf2b8aac246bfe"), "Start Game");
    #endregion

    #region localized-string-constructor-editor

    public LocalizedString GenerateLocalizedStringInEditor()
    {
        // The main advantage to using a table Guid and entry Id is that references will not be lost when changes are made to the Table name or Entry name.
        var collection = LocalizationEditorSettings.GetStringTableCollection("My String Table");
        var entry = collection.SharedData.GetEntry("Start Game");
        return new LocalizedString(collection.SharedData.TableCollectionNameGuid, entry.Id);
    }

    #endregion

    void SetTableReferenceExamples()
    {
        #region localized-string-table-reference

        var localizedString = new LocalizedString();

        // Table Reference can be set with the name of the table
        localizedString.TableReference = "My String Table";

        // Or the Table Collection Guid
        localizedString.TableReference = new System.Guid("6e79ded14bc9e0a4d9bf2b8aac246bfe");
        #endregion
    }

    void SetTableEntryReferenceExamples()
    {
        #region localized-string-table-entry-reference

        var localizedString = new LocalizedString();

        // Table Entry Reference can be set with the name of the Key
        localizedString.TableEntryReference = "Exit Game";

        // Or the Key Id
        localizedString.TableEntryReference = 342423423;
        #endregion
    }

    void SetReferenceExamples()
    {
        #region localized-string-set-reference

        var localizedString = new LocalizedString();

        // Setting with names
        localizedString.SetReference("UI Text", "Options");

        // Setting with the table name and Key Id
        localizedString.SetReference("UI Text", 3432444324);

        // Setting with the Table Name Guid and Key name
        localizedString.SetReference(new System.Guid("6e79ded14bc9e0a4d9bf2b8aac246bfe"), "Options");

        // Setting with the Table Name Guid and the Key Id
        localizedString.SetReference(new System.Guid("6e79ded14bc9e0a4d9bf2b8aac246bfe"), 3432444324);
        #endregion
    }
}

#endif
