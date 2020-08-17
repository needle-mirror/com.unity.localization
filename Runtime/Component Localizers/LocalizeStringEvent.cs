using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Localization.Tables;

namespace UnityEngine.Localization.Components
{
    /// <summary>
    /// Component that can be used to Localize a string.
    /// Allows for configuring optional string arguments and provides an update event that can be used to update the string.
    /// </summary>
    [AddComponentMenu("Localization/Localize String Event")]
    public class LocalizeStringEvent : MonoBehaviour
    {
        /// <summary>
        /// UnityEvent which can pass a string as an argument.
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
        /// Event that will be sent when the localized string is ready.
        /// </summary>
        public StringUnityEvent OnUpdateString
        {
            get => m_UpdateString;
            set => m_UpdateString = value;
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
