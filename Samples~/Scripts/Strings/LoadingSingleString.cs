using UnityEngine;
using UnityEngine.Localization;

namespace UnityEditor.Localization.Samples
{
    /// <summary>
    /// This example shows how we can fetch and update a single string value.
    /// </summary>
    public class LoadingSingleString : MonoBehaviour
    {
        // A LocalizedStringReference provides a simple interface to retrieving translated strings.
        public LocalizedStringReference stringRef = new LocalizedStringReference() { TableName = "My String Table", Key = "Hello World" };

        // We will cache our translated string
        string m_TranslatedString;

        void OnEnable()
        {
            // During initialization we start a request for the string and subscribe to any locale change events so that we can update the string in the future.
            FetchString();
            LocalizationSettings.SelectedLocaleChanged += OnSelectedLocaleChanged;
        }

        void OnDisable()
        {
            LocalizationSettings.SelectedLocaleChanged -= OnSelectedLocaleChanged;
        }

        void OnSelectedLocaleChanged(Locale obj)
        {
            FetchString();
        }

        void FetchString()
        {
            // A string may not be immediately available such as during initialization of the localization system or when a table has not been loaded yet. 
            // The Completed event will be called when the string is ready.
            stringRef.GetLocalizedString().Completed += op => m_TranslatedString = op.Result;
        }

        void OnGUI()
        {
            GUILayout.Label(m_TranslatedString);
        }
    }
}