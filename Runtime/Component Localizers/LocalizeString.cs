using System;
using UnityEngine.Events;
using UnityEngine.ResourceManagement;

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
            get { return m_StringReference; }
            set { m_StringReference = value; }
        }

        public LocalizationUnityEvent UpdateString
        {
            get { return m_UpdateString; }
            set { m_UpdateString = value; }
        }

        public bool IsPlural
        {
            get { return m_IsPlural; }
            set
            {
                if(m_IsPlural == value)
                    return;

                m_IsPlural = value; 
                ForceUpdate();
            }
        }

        public int PluralValue
        {
            get { return m_PluralValue; }
            set
            {
                if (m_PluralValue == value)
                    return;

                m_PluralValue = value; 

                if(IsPlural)
                    ForceUpdate();
            }
        }

        protected override void OnLocaleChanged(Locale newLocale)
        {
            var stringOperation = m_IsPlural ? StringReference.GetLocalizedString(m_PluralValue) : StringReference.GetLocalizedString();
            stringOperation.Completed += StringLoaded;
        }

        protected virtual void StringLoaded(IAsyncOperation<string> stringOp)
        {
            if (stringOp.HasLoadedSuccessfully())
            {
                UpdateString.Invoke(stringOp.Result);
            }
        }
    }
}