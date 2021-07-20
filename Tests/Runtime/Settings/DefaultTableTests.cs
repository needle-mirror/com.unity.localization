using NUnit.Framework;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization.Tests
{
    public class DefaultTableTests
    {
        LocalizationSettings     m_Settings;
        LocalizedAssetDatabase   m_TempAssetDatabase;
        LocalizedStringDatabase  m_TempStringDatabase;


        [SetUp]
        public void CreateTestLocalizationSettings()
        {
            LocalizationSettingsHelper.SaveCurrentSettings();

            m_Settings           = ScriptableObject.CreateInstance<LocalizationSettings>();
            m_TempAssetDatabase  = new LocalizedAssetDatabase();
            m_TempStringDatabase = new LocalizedStringDatabase();

            LocalizationSettings.Instance = m_Settings;
            LocalizationSettings.AssetDatabase  = m_TempAssetDatabase;
            LocalizationSettings.StringDatabase = m_TempStringDatabase;
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(m_Settings);
            LocalizationSettingsHelper.RestoreSettings();
        }

        [Test]
        public void CheckExceptionIsThrown_WhenCallingGetLocalizedStringAsync_WithDefaultTableReference_WhenIsEmptyOrNull()
        {
            var ex = Assert.Throws<System.Exception>(() => m_Settings.GetStringDatabase().GetLocalizedStringAsync("Test Entry 1"));
            Assert.That(ex.Message, Is.EqualTo($"Trying to get the DefaultTable however the {m_TempStringDatabase.GetType().Name} DefaultTable value has not been set. This can be configured in the Localization Settings."));
        }

        [Test]
        public void CheckExceptionIsThrown_WhenCallingGetLocalizedAssetAsync_WithDefaultTableReference_WhenIsEmptyOrNull()
        {
            var ex = Assert.Throws<System.Exception>(() => m_Settings.GetAssetDatabase().GetLocalizedAssetAsync<Texture>("Test Entry 1"));
            Assert.That(ex.Message, Is.EqualTo($"Trying to get the DefaultTable however the {m_TempAssetDatabase.GetType().Name} DefaultTable value has not been set. This can be configured in the Localization Settings."));
        }
    }
}
