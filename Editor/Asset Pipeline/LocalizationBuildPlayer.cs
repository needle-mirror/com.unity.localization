using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace UnityEditor.Localization
{
    class LocalizationBuildPlayer : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        LocalizationSettings m_Settings;

        Object[] m_OriginalPreloadedAssets;

        bool m_RemoveFromPreloadedAssets;

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            m_RemoveFromPreloadedAssets = false;
            m_Settings = LocalizationEditorSettings.ActiveLocalizationSettings;
            if (m_Settings == null)
                return;

            // Add the localization settings to the preloaded assets.
            m_OriginalPreloadedAssets = PlayerSettings.GetPreloadedAssets();
            if (!m_OriginalPreloadedAssets.Contains(m_Settings))
            {
                var preloadedAssets = m_OriginalPreloadedAssets.ToList();
                preloadedAssets.Add(m_Settings);
                PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());

                // If we have to add the settings then we should also remove them.
                m_RemoveFromPreloadedAssets = true;
            }
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (m_Settings == null || !m_RemoveFromPreloadedAssets)
                return;

            // Revert back to original state
            PlayerSettings.SetPreloadedAssets(m_OriginalPreloadedAssets);
        }
    }
}
