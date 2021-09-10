using System.Collections;
using UnityEngine;
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
