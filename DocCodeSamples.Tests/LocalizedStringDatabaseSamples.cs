using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

#region preload-example

public class PreloadingSample : MonoBehaviour
{
    IEnumerator Start()
    {
        // Tables that are not marked as Preload can be manually preloaded.
        var preloadOperation = LocalizationSettings.StringDatabase.PreloadTables(new TableReference[] { "UI Text", "Game Text" });
        yield return preloadOperation;

        // Get some text from the table, this will be immediately available now the table has been preloaded
        var uiText = LocalizationSettings.StringDatabase.GetTableEntryAsync("UI Text", "Start_Game").Result;
        Debug.Log(uiText);
    }
}

#endregion

#region release-example

public class ReleaseSample : MonoBehaviour
{
    AsyncOperationHandle<StringTable> m_Table;

    IEnumerator Start()
    {
        TableReference tableReference = "My Game Text";
        m_Table = LocalizationSettings.StringDatabase.GetTableAsync(tableReference);

        yield return m_Table;

        // To prevent a table from being released we can acquire a reference to it.
        // Now we will always keep this table, even if the Selected Locale is changed.
        Addressables.ResourceManager.Acquire(m_Table);

        // We can tell the Localization system to release references to the table.
        LocalizationSettings.StringDatabase.ReleaseTable(tableReference);
    }

    private void OnDisable()
    {
        // To release the table we call Release.
        Addressables.Release(m_Table);
    }
}

#endregion
