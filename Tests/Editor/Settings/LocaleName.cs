using NUnit.Framework;
using UnityEditor;

namespace UnityEngine.Localization.Tests
{
    public class LocaleName
    {
        const string kPath = "Assets/LocaleNameTest.asset";

        Locale m_Locale;

        [SetUp]
        public void Setup()
        {
            m_Locale = Locale.CreateLocale("my locale");
            AssetDatabase.CreateAsset(m_Locale, kPath);
        }

        [TearDown]
        public void Teardown()
        {
            Assert.True(AssetDatabase.DeleteAsset(kPath), "Failed to delete asset");
        }

        [Description("[Localization] Locale Asset's Name is not saved when renaming it through the Inspector(LOC-144)")]
        [Test]
        public void LocaleNameChangesAreSaved()
        {
            const string newName = "New Locale Name";
            m_Locale.LocaleName = newName;
            Assert.AreEqual(newName, m_Locale.LocaleName);
        }
    }
}
