using System.Collections;
using UnityEngine;
using UnityEngine.Localization;

namespace UnityEditor.Localization.Samples
{
    /// <summary>
    /// This example shows how we can fetch multiple string values using a single operation.
    /// This example requires that your project contain a string table with the test values.
    /// </summary>
    public class LoadingMultipleStrings : MonoBehaviour
    {
        // A LocalizedStringReference provides a simple interface to retrieving translated strings and their tables.
        public LocalizedStringReference tableRef = new LocalizedStringReference() { TableName = "My String Table" };

        // We will cache our translated strings
        string m_TranslatedStringHello;
        string m_TranslatedStringGoodbye;
        string m_TranslatedStringThisIsATest;

        void OnEnable()
        {
            // During initialization we start a request for the string and subscribe to any locale change events so that we can update the string in the future.
            StartCoroutine(FetchString());
            LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
        }

        void OnDisable()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
        }

        void OnSelectedLocaleChanged(Locale obj)
        {
            StartCoroutine(FetchString());
        }

        IEnumerator FetchString()
        {
            // A string may not be immediately available such as during initialization of the localization system or when a table has not been loaded yet. 
            var loadingOperation = tableRef.GetLocalizedStringTable();
            yield return loadingOperation;

            if (loadingOperation.HasLoadedSuccessfully())
            {
                var stringTable = loadingOperation.Result;
                m_TranslatedStringThisIsATest = stringTable.GetLocalizedString("This is a test");
                m_TranslatedStringHello = stringTable.GetLocalizedString("Hello");
                m_TranslatedStringGoodbye = stringTable.GetLocalizedString("Goodbye");
            }
        }

        void OnGUI()
        {
            // We can check if the localization system is ready using the InitializationOperation.
            // Initialization involves loading locales and optionally preloading localized data for the current locale.
            if (!LocalizationSettings.InitializationOperation.IsDone)
            {
                GUILayout.Label("Initializing Localization");
                return;
            }

            GUILayout.Label(m_TranslatedStringThisIsATest);
            GUILayout.Label(m_TranslatedStringHello);
            GUILayout.Label(m_TranslatedStringGoodbye);
        }
    }
}