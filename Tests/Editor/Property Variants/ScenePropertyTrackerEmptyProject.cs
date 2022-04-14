#if ENABLE_PROPERTY_VARIANTS

using NUnit.Framework;
using System.Collections;
using UnityEditor.Localization.PropertyVariants;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.TestTools;

namespace UnityEditor.Localization.Tests.PropertyVariants
{
    public class ScenePropertyTrackerEmptyProject
    {
        const string k_BackupPath = "ScenePropertyTrackerEmptyProject-settings-backup";

        [SetUp]
        public void Setup()
        {
            // Save the settings so we dont break the project the test is run in.
            var backupInstance = LocalizationEditorSettings.ActiveLocalizationSettings;
            if (backupInstance != null)
            {
                LocalizationEditorSettings.ActiveLocalizationSettings = null;
                var path = AssetDatabase.GetAssetPath(backupInstance);
                SessionState.SetString(k_BackupPath, path);
            }

            LocalizationSettings.Instance = null;
        }

        [TearDown]
        public void Teardown()
        {
            var backupPath = SessionState.GetString(k_BackupPath, null);
            if (!string.IsNullOrEmpty(backupPath))
            {
                var settings = AssetDatabase.LoadAssetAtPath<LocalizationSettings>(backupPath);
                Assert.NotNull(settings, "Failed to restore localization settings.");

                LocalizationEditorSettings.ActiveLocalizationSettings = settings;
                LocalizationSettings.Instance = settings;
                SessionState.EraseString(k_BackupPath);
            }
        }

        [Test]
        public void PostProcessModifications_DoesNotProduceErrors_InEditMode()
        {
            Assert.False(LocalizationSettings.HasSettings);

            ScenePropertyTracker.PostProcessModifications(new UnityEditor.UndoPropertyModification[0]);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator PostProcessModifications_DoesNotProduceErrors_InPlayMode()
        {
            yield return new EnterPlayMode();

            Assert.False(LocalizationSettings.HasSettings);

            ScenePropertyTracker.PostProcessModifications(new UnityEditor.UndoPropertyModification[0]);
            LogAssert.NoUnexpectedReceived();

            yield return new ExitPlayMode();
        }
    }
}
#endif
