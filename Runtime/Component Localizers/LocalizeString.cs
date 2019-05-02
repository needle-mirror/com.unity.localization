using System;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization.Components
{
    [AddComponentMenu("Localization/Generic/String")]
    public class LocalizeString : LocalizationBehaviour
    {
        [Serializable]
        public class LocalizationUnityEvent : UnityEvent<string> { };

        [SerializeField]
        LocalizedStringReference m_StringReference = new LocalizedStringReference();

        [SerializeField]
        LocalizationUnityEvent m_UpdateString = new LocalizationUnityEvent();

        [SerializeField]
        bool m_IsPlural;

        [SerializeField]
        int m_PluralValue = 1;
        
        public LocalizedStringReference StringReference
        {
            get => m_StringReference;
            set => m_StringReference = value;
        }

        public LocalizationUnityEvent UpdateString
        {
            get => m_UpdateString;
            set => m_UpdateString = value;
        }

        public bool IsPlural
        {
            get => m_IsPlural;
            set
            {
                if (m_IsPlural == value)
                    return;

                m_IsPlural = value;
                ForceUpdate();
            }
        }

        public int PluralValue
        {
            get => m_PluralValue;
            set
            {
                if (m_PluralValue == value)
                    return;

                m_PluralValue = value;

                if (IsPlural)
                    ForceUpdate();
            }
        }

        protected override void OnLocaleChanged(Locale newLocale)
        {
            var stringOperation = m_IsPlural ? StringReference.GetLocalizedString(m_PluralValue) : StringReference.GetLocalizedString();
            stringOperation.Completed += StringLoaded;
        }

        protected virtual void StringLoaded(AsyncOperationHandle<string> stringOp)
        {
            if (stringOp.Status != AsyncOperationStatus.Succeeded)
            {
                var error = "Failed to load string: " + m_StringReference;
                if (stringOp.OperationException != null)
                    error += "\n" + stringOp.OperationException;

                Debug.LogError(error, this);
                return;
            }

            UpdateString.Invoke(stringOp.Result);
        }
    }
}