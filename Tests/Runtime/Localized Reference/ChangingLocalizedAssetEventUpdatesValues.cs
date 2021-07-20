using NUnit.Framework;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization.Tests
{
    public class ChangingLocalizedAssetEventUpdatesValues
    {
        // We don't want to do any loading, we just want to know if a call is made into the database
        class FixtureAssetDatabase : LocalizedAssetDatabase
        {
            public TableReference? LastTableReference { get; set; }
            public TableEntryReference? LastTableEntryReference { get; set; }

            public override AsyncOperationHandle<TObject> GetLocalizedAssetAsync<TObject>(TableReference tableReference, TableEntryReference tableEntryReference, Locale locale, FallbackBehavior fallbackBehavior = FallbackBehavior.UseProjectSettings)
            {
                LastTableReference = tableReference;
                LastTableEntryReference = tableEntryReference;
                return AddressablesInterface.ResourceManager.CreateCompletedOperation(default(TObject), null);
            }
        }

        GameObject m_GameObject;
        LocalizeTextureEvent m_LocalizeTextureEvent;
        FixtureAssetDatabase m_FixtureAssetDatabase;

        const string k_DefaultTableCollectionName = "Default Asset Table";
        const string k_DefaultEntryName = "Default Asset Table Entry";

        void CheckEntryWasRequested(string expectedTableCollectionName, string expectedEntryName)
        {
            Assert.IsNotNull(m_FixtureAssetDatabase.LastTableReference, "Expected LocalizeAsset to call into LocalizedStringDatabase with a valid TableReference but it did not.");
            Assert.IsNotNull(m_FixtureAssetDatabase.LastTableEntryReference, "Expected LocalizeAsset to call into LocalizedStringDatabase with a valid TableEntryReference but it did not.");

            Assert.AreEqual(expectedTableCollectionName, m_FixtureAssetDatabase.LastTableReference.Value.TableCollectionName, "Expected table collection name to match.");
            Assert.AreEqual(expectedEntryName, m_FixtureAssetDatabase.LastTableEntryReference.Value.Key, "Expected entry key name to match.");
        }

        void ClearLastGetTableEntryValues()
        {
            m_FixtureAssetDatabase.LastTableReference = null;
            m_FixtureAssetDatabase.LastTableEntryReference = null;
        }

        [SetUp]
        public void Setup()
        {
            LocalizationSettingsHelper.SaveCurrentSettings();

            LocalizationSettings.Instance = ScriptableObject.CreateInstance<LocalizationSettings>();
            m_FixtureAssetDatabase = new FixtureAssetDatabase();
            LocalizationSettings.AssetDatabase = m_FixtureAssetDatabase;

            m_GameObject = new GameObject(nameof(ChangingLocalizedAssetEventUpdatesValues));
            m_LocalizeTextureEvent = m_GameObject.AddComponent<LocalizeTextureEvent>();

            // Setup with a default value so we do not start with an empty StringReference.
            m_LocalizeTextureEvent.AssetReference.SetReference(k_DefaultTableCollectionName, k_DefaultEntryName);
            ClearLastGetTableEntryValues();
        }

        [TearDown]
        public void Teardown()
        {
            // Delete GameObject first as it will call into LocalizationSettings during cleanup.
            Object.DestroyImmediate(m_GameObject);
            Object.DestroyImmediate(LocalizationSettings.Instance);
            LocalizationSettingsHelper.RestoreSettings();
        }

        [Test]
        public void AssetReference_SetReference_TriggersUpdate()
        {
            const string tableName = "SetReference Table Collection Name";
            const string entryName =  "SetReference Entry Name";

            m_LocalizeTextureEvent.AssetReference.SetReference(tableName, entryName);
            CheckEntryWasRequested(tableName, entryName);
        }

        [Test]
        public void AssetReference_SettingTableReference_TriggersUpdate()
        {
            const string tableName = "SettingTableReference Table Collection Name";
            m_LocalizeTextureEvent.AssetReference.TableReference = tableName;
            CheckEntryWasRequested(tableName, k_DefaultEntryName);
        }

        [Test]
        public void AssetReference_SettingTableEntryReference_TriggersUpdate()
        {
            const string entryName = "SettingTableEntryReference Entry Name";
            m_LocalizeTextureEvent.AssetReference.TableEntryReference = entryName;
            CheckEntryWasRequested(k_DefaultTableCollectionName, entryName);
        }

        [Test]
        public void SettingAssetReference_TriggersUpdate()
        {
            const string tableName = "SettingAssetReference Table Collection Name";
            const string entryName =  "SettingAssetReference Entry Name";

            var newAssetReference = new LocalizedTexture(){ TableReference = tableName, TableEntryReference = entryName };

            m_LocalizeTextureEvent.AssetReference = newAssetReference;
            CheckEntryWasRequested(tableName, entryName);
        }
    }
}
