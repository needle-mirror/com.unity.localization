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
            bool wasDirty = IsPlayerSettingsDirty();

            if (!m_OriginalPreloadedAssets.Contains(m_Settings))
            {
                var preloadedAssets = m_OriginalPreloadedAssets.ToList();
                preloadedAssets.Add(m_Settings);
                PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());

                // If we have to add the settings then we should also remove them.
                m_RemoveFromPreloadedAssets = true;

                // Clear the dirty flag so we dont flush the modified file (case 1254502)
                if (!wasDirty)
                    ClearPlayerSettingsDirtyFlag();
            }
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (m_Settings == null || !m_RemoveFromPreloadedAssets)
                return;

            bool wasDirty = IsPlayerSettingsDirty();

            // Revert back to original state
            PlayerSettings.SetPreloadedAssets(m_OriginalPreloadedAssets);

            // Clear the dirty flag so we dont flush the modified file (case 1254502)
            if (!wasDirty)
                ClearPlayerSettingsDirtyFlag();
        }

        static bool IsPlayerSettingsDirty()
        {
            var settings = Resources.FindObjectsOfTypeAll<PlayerSettings>();
            if (settings != null && settings.Length > 0)
                return EditorUtility.IsDirty(settings[0]);
            return false;
        }

        static void ClearPlayerSettingsDirtyFlag()
        {
            var settings = Resources.FindObjectsOfTypeAll<PlayerSettings>();
            if (settings != null && settings.Length > 0)
                EditorUtility.ClearDirty(settings[0]);
        }
    }
}
