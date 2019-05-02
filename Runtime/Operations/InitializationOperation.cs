using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization
{
    /// <summary>
    /// Performs all initialization work for the LocalizationSettings.
    /// </summary>
    class InitializationOperation : AsyncOperationBase<LocalizationSettings>
    {
        int m_PreloadingOperations;
        LocalizationSettings m_Settings;
        string m_Error;

        public InitializationOperation(LocalizationSettings settings)
        {
            m_Settings = settings;
        }

        protected override void Execute()
        {
            // First time initialization requires loading locales and selecting the startup locale without sending a locale changed event.
            if (m_Settings.GetSelectedLocale() == null)
            {
                // Load Locales
                var locales = m_Settings.GetAvailableLocales() as IPreloadRequired;
                if (locales != null && !locales.PreloadOperation.IsDone)
                {
                    locales.PreloadOperation.Completed += (async) =>
                    {
                        m_Settings.InitializeSelectedLocale();
                        PreLoadTables();
                    };
                    return;
                }

                m_Settings.InitializeSelectedLocale();
            }

            PreLoadTables();
        }

        void PreloadOperationCompleted(AsyncOperationHandle asyncOperation)
        {
            m_PreloadingOperations--;

            if (asyncOperation.Status != AsyncOperationStatus.Succeeded)
            {
                m_Error = "Failed to preload: " + asyncOperation.DebugName;
                Debug.LogError(m_Error, m_Settings);
                if (asyncOperation.OperationException != null)
                {
                    Debug.LogException(asyncOperation.OperationException);
                    m_Error += "\n" + asyncOperation.OperationException;
                }
            }

            Debug.Assert(m_PreloadingOperations >= 0);
            if (m_PreloadingOperations == 0)
                FinishInitializing();
        }

        void PreLoadTables()
        {
            if (m_Settings.PreloadBehavior == PreloadBehavior.OnDemand)
            {
                FinishInitializing();
                return;
            }

            Debug.Assert(m_PreloadingOperations == 0);
            m_PreloadingOperations = 0;
            var assetOperation = m_Settings.GetAssetDatabase() as IPreloadRequired;
            if (assetOperation != null)
            {
                Debug.Log("Localization: Preloading Asset Tables(" + Time.timeSinceLevelLoad + ")");
                if (!assetOperation.PreloadOperation.IsDone)
                {
                    assetOperation.PreloadOperation.Completed += (async) =>
                    {
                        Debug.Log("Localization: Finished Preloading Asset Tables(" + Time.timeSinceLevelLoad + ")");
                        PreloadOperationCompleted(async);
                    };
                    m_PreloadingOperations++;
                }
            }

            var stringOperation = m_Settings.GetStringDatabase() as IPreloadRequired;
            if (stringOperation != null)
            {
                Debug.Log("Localization: Preloading String Tables(" + Time.timeSinceLevelLoad + ")");
                if (!stringOperation.PreloadOperation.IsDone)
                {
                    stringOperation.PreloadOperation.Completed += (async) =>
                    {
                        Debug.Log("Localization: Finished Preloading String Tables(" + Time.timeSinceLevelLoad + ")");
                        PreloadOperationCompleted(async);
                    };
                    m_PreloadingOperations++;
                }
            }

            if (m_PreloadingOperations == 0)
                FinishInitializing();
        }

        void FinishInitializing() => Complete(m_Settings, string.IsNullOrEmpty(m_Error), m_Error);
    }
}