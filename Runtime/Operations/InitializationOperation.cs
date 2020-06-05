using System.Collections.Generic;
using UnityEngine.Localization.Settings;
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
        float m_Progress;
        List<IInitialize> m_PostInitializers;

        const float k_LocalesProgress = 0.2f;
        const float k_DatabaseProgress = 0.4f;

        // TODO: More detailed progress. Take the progress form the sub operations.
        protected override float Progress => m_Progress;

        protected override string DebugName => "Localization Settings Initialization";

        public void Init(LocalizationSettings settings)
        {
            // Collect the post initializers?
            if (m_Settings != settings)
            {
                m_PostInitializers = settings.GetInitializers();
            }

            m_Settings = settings;
            m_Error = null;
            m_PreloadingOperations = 0;
            m_Progress = 0;
        }

        protected override void Execute()
        {
            LoadLocales();
        }

        void LoadLocales()
        {
            var localeOp = m_Settings.GetSelectedLocaleAsync();
            if (!localeOp.IsDone)
            {
                localeOp.Completed += async => PreLoadTables();
            }
            else
            {
                PreLoadTables();
            }
        }

        void PreloadOperationCompleted(AsyncOperationHandle asyncOperation)
        {
            m_Progress += k_DatabaseProgress;
            m_PreloadingOperations--;

            if (asyncOperation.Status != AsyncOperationStatus.Succeeded)
            {
                m_Error = "Failed to preload: " + asyncOperation.DebugName;
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
            Debug.Assert(m_PreloadingOperations == 0);
            m_Progress = k_LocalesProgress;
            m_PreloadingOperations = 0;

            if (m_Settings.GetAssetDatabase() is IPreloadRequired assetOperation && !assetOperation.PreloadOperation.IsDone)
            {
                assetOperation.PreloadOperation.Completed += PreloadOperationCompleted;
                m_PreloadingOperations++;
            }
            else
            {
                m_Progress += k_DatabaseProgress;
            }

            if (m_Settings.GetStringDatabase() is IPreloadRequired stringOperation && !stringOperation.PreloadOperation.IsDone)
            {
                stringOperation.PreloadOperation.Completed += PreloadOperationCompleted;
                m_PreloadingOperations++;
            }
            else
            {
                m_Progress += k_DatabaseProgress;
            }

            if (m_PreloadingOperations == 0)
                FinishInitializing();
        }

        void PostInitializeExtensions()
        {
            if (m_PostInitializers == null)
                return;

            foreach (var init in m_PostInitializers)
            {
                init.PostInitialization(m_Settings);
            }
        }

        void FinishInitializing()
        {
            PostInitializeExtensions();
            Complete(m_Settings, string.IsNullOrEmpty(m_Error), m_Error);
        }
    }
}
