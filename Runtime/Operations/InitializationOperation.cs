using UnityEngine.ResourceManagement;

namespace UnityEngine.Localization
{
    /// <summary>
    /// Performs all initialization work for the LocalizationSettings.
    /// </summary>
    public class InitializationOperation : AsyncOperationBase<LocalizationSettings>
    {
        int m_PreloadingOperations;
        LocalizationSettings m_Settings;

        /// <inheritdoc />
        public override void ResetStatus()
        {
            // Remove ourself from any preloading operations or we may have duplicate callbacks.
            if (m_PreloadingOperations > 0)
            {
                var assetOperation = m_Settings.GetAssetDatabase() as IPreloadRequired;
                if (assetOperation != null)
                    assetOperation.PreloadOperation.Completed -= PreloadOperationCompleted;

                var stringOperation = m_Settings.GetStringDatabase() as IPreloadRequired;

                if (stringOperation != null)
                    stringOperation.PreloadOperation.Completed -= PreloadOperationCompleted;
                
            }

            base.ResetStatus();
            m_PreloadingOperations = 0;
            m_Settings = null;
        }

        public virtual InitializationOperation Start(LocalizationSettings settings)
        {
            m_Settings = settings;

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
                    return this;
                }
                else
                {
                    m_Settings.InitializeSelectedLocale();
                }
            }

            PreLoadTables();
            return this;
        }

        private void PreloadOperationCompleted(IAsyncOperation obj)
        {
            m_PreloadingOperations--;

            if (obj.HasLoadedSuccessfully())
            {
                Status = obj.Status;
                m_Error = obj.OperationException;
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

        void FinishInitializing()
        {
            if (Status != AsyncOperationStatus.Failed)
                Status = AsyncOperationStatus.Succeeded;
            InvokeCompletionEvent();
        }
    }
}