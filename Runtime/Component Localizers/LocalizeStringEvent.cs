using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Localization.Tables;

namespace UnityEngine.Localization.Components
{
    /// <summary>
    /// Component that can be used to Localize a string.
    /// Provides an update event <see cref="UpdateString(string)"/> that can be used to automatically update the string
    /// when the <see cref="Settings.LocalizationSettings.SelectedLocale"/> or <see cref="StringReference"/> changes.
    /// Allows for configuring optional arguments that will be used by **Smart Format** or <c>String.Format</c>.
    /// </summary>
    /// <example>
    /// This example shows how a Localized String Event can be dynamically updated with a different localized string or new formatting data.
    /// <code source="../../DocCodeSamples.Tests/LocalizeStringEventExample.cs"/>
    /// </example>
    /// <example>
    /// ![](../manual/images/scripting/LocalizeStringEventExample_Inspector.png)
    /// </example>
    /// <example>
    /// Example of String Table Contents
    ///
    /// ![Example of String Table Contents](../manual/images/scripting/LocalizeStringEventExample_StringTable.png)
    /// </example>
    /// <example>
    /// Example results in Game
    ///
    /// ![Example results in Game](../manual/images/scripting/LocalizeStringEventExample_GameView.gif)
    /// </example>
    [AddComponentMenu("Localization/Localize String Event")]
    public class LocalizeStringEvent : MonoBehaviour
    {
        /// <summary>
        /// [UnityEvent](https://docs.unity3d.com/ScriptReference/Events.UnityEvent.html) which contains the Localized String as an argument.
        /// </summary>
        [Serializable]
        public class StringUnityEvent : UnityEvent<string> {};

        [SerializeField]
        LocalizedString m_StringReference = new LocalizedString();

        [SerializeField]
        List<Object> m_FormatArguments = new List<Object>();

        [SerializeField]
        StringUnityEvent m_UpdateString = new StringUnityEvent();

        /// <summary>
        /// References the <see cref="StringTable"/> and <see cref="StringTableEntry"/> of the localized string.
        /// </summary>
        public LocalizedString StringReference
        {
            get => m_StringReference;
            set
            {
                // Unsubscribe from the old string reference.
                ClearChangeHandler();

                m_StringReference = value;

                if (enabled)
                    RegisterChangeHandler();
            }
        }

        /// <summary>
        /// Event that will be sent when the localized string is available.
        /// </summary>
        public StringUnityEvent OnUpdateString
        {
            get => m_UpdateString;
            set => m_UpdateString = value;
        }

        /// <summary>
        /// Forces the string to be regenerated, such as when the string formatting argument values have changed.
        /// </summary>
        public void RefreshString()
        {
            StringReference.RefreshString();
        }

        /// <summary>
        /// Starts listening for changes to <see cref="StringReference"/>.
        /// </summary>
        protected virtual void OnEnable() => RegisterChangeHandler();

        /// <summary>
        /// Stops listening for changes to <see cref="StringReference"/>.
        /// </summary>
        protected virtual void OnDisable() => ClearChangeHandler();

        /// <summary>
        /// Invokes the <see cref="OnUpdateString"/> event.
        /// </summary>
        /// <param name="value"></param>
        protected virtual void UpdateString(string value) => OnUpdateString.Invoke(value);

        void RegisterChangeHandler()
        {
            if (m_FormatArguments.Count > 0)
            {
                StringReference.Arguments = m_FormatArguments.ToArray();
            }

            StringReference.StringChanged += UpdateString;
        }

        void ClearChangeHandler()
        {
            StringReference.StringChanged -= UpdateString;
        }
    }
}
