using System.Collections.Generic;
using UnityEngine.ResourceManagement;

namespace UnityEngine.Localization
{
    /// <summary>
    /// Uses the AddressableAssets system, to store and retrieve all locale data.
    /// This allow for adding and removing locales in a build or for storing the locale data remotely.
    /// </summary>
    public class AddressableLocalesProvider : LocalesProvider, IPreloadRequired
    {
        List<Locale> m_Locales;
        IAsyncOperation<IList<Locale>> m_LoadOperation;

        public override List<Locale> Locales
        {
            get
            {
                if(m_LoadOperation == null)
                    Debug.LogError("Locales PreloadOperation has not been initialized.");
                return m_Locales;
            }
            set
            {
                m_Locales = value;
            }
        }

        /// <summary>
        /// The Locales loading operation. When set to isDone then all locales have been loaded. Can be Null if the operation has not started yet.
        /// </summary>
        public IAsyncOperation PreloadOperation
        {
            get
            {
                if (m_LoadOperation == null)
                {
                    Locales = new List<Locale>();
                    m_LoadOperation = AddressableAssets.Addressables.LoadAssets<Locale>(LocalizationSettings.LocaleLabel, LocaleLoaded);
                    m_LoadOperation.Retain();
                }

                return m_LoadOperation;
            }
        }

        void LocaleLoaded(IAsyncOperation<Locale> obj)
        {
            if (obj.Status == AsyncOperationStatus.Succeeded)
                AddLocale(obj.Result);
            else
            {
                Debug.LogError("Failed to load locale: " + obj.Context);
                if(obj.OperationException != null)
                    Debug.LogException(obj.OperationException);
            }
        }
    }
}