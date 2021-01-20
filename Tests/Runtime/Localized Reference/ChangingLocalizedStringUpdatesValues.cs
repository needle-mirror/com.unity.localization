using NUnit.Framework;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization.Tests
{
    public class ChangingLocalizedStringUpdatesValues
    {
        // We don't want to do any loading, we just want to know if a call is made into the database
        class FixtureStringDatabase : LocalizedStringDatabase
        {
            public TableReference? LastTableReference { get; set; }
            public TableEntryReference? LastTableEntryReference { get; set; }

            public override AsyncOperationHandle<TableEntryResult> GetTableEntryAsync(TableReference tableReference, TableEntryReference tableEntryReference, Locale locale, FallbackBehavior fallbackBehavior)
            {
                LastTableReference = tableReference;
                LastTableEntryReference = tableEntryReference;
                return AddressablesInterface.ResourceManager.CreateCompletedOperation(new TableEntryResult(), null);
            }
        }

        GameObject m_GameObject;
        LocalizeStringEvent m_LocalizeStringEvent;
        FixtureStringDatabase m_FixtureStringDatabase;
        Locale m_Selected;

        const string kDefaultTableCollectionName = "Default String Table";
        const string kDefaultEntryName = "Default String Table Entry";

        void CheckEntryWasRequested(string expectedTableCollectionName, string expectedEntryName)
        {
            Assert.IsNotNull(m_FixtureStringDatabase.LastTableReference, "Expected LocalizeString to call into LocalizedStringDatabase with a valid TableReference but it did not.");
            Assert.IsNotNull(m_FixtureStringDatabase.LastTableEntryReference, "Expected LocalizeString to call into LocalizedStringDatabase with a valid TableEntryReference but it did not.");

            Assert.AreEqual(expectedTableCollectionName, m_FixtureStringDatabase.LastTableReference.Value.TableCollectionName, "Expected table collection name to match.");
            Assert.AreEqual(expectedEntryName, m_FixtureStringDatabase.LastTableEntryReference.Value.Key, "Expected entry key name to match.");
        }

        void ClearLastGetTableEntryValues()
        {
            m_FixtureStringDatabase.LastTableReference = null;
            m_FixtureStringDatabase.LastTableEntryReference = null;
        }

        [SetUp]
        public void Setup()
        {
            LocalizationSettingsHelper.SaveCurrentSettings();

            LocalizationSettings.Instance = ScriptableObject.CreateInstance<LocalizationSettings>();

            var localeProvider = new TestLocaleProvider();
            m_Selected = Locale.CreateLocale("en");
            localeProvider.AddLocale(m_Selected);
            LocalizationSettings.Instance.SetAvailableLocales(localeProvider);

            m_FixtureStringDatabase = new FixtureStringDatabase();
            LocalizationSettings.StringDatabase = m_FixtureStringDatabase;

            m_FixtureStringDatabase.NoTranslationFoundMessage = "No translation found for '{key}'";
            m_GameObject = new GameObject(nameof(ChangingLocalizedStringUpdatesValues));
            m_LocalizeStringEvent = m_GameObject.AddComponent<LocalizeStringEvent>();

            // Setup with a default value so we do not start with an empty StringReference.
            m_LocalizeStringEvent.StringReference.SetReference(kDefaultTableCollectionName, kDefaultEntryName);
            ClearLastGetTableEntryValues();
        }

        [TearDown]
        public void Teardown()
        {
            // Delete GameObject first as it will call into LocalizationSettings during cleanup.
            Object.DestroyImmediate(m_GameObject);
            Object.DestroyImmediate(LocalizationSettings.Instance);
            Object.DestroyImmediate(m_Selected);
            LocalizationSettingsHelper.RestoreSettings();
        }

        [Test]
        public void LocalizedString_SetReference_TriggersUpdate()
        {
            const string tableName = "SetReference Table Collection Name";
            const string entryName =  "SetReference Entry Name";

            m_LocalizeStringEvent.StringReference.SetReference(tableName, entryName);
            CheckEntryWasRequested(tableName, entryName);
        }

        [Test]
        public void LocalizedString_SettingTableReference_TriggersUpdate()
        {
            const string tableName = "SettingTableReference Table Collection Name";
            m_LocalizeStringEvent.StringReference.TableReference = tableName;
            CheckEntryWasRequested(tableName, kDefaultEntryName);
        }

        [Test]
        public void LocalizedString_SettingTableEntryReference_TriggersUpdate()
        {
            const string entryName = "SettingTableEntryReference Entry Name";
            m_LocalizeStringEvent.StringReference.TableEntryReference = entryName;
            CheckEntryWasRequested(kDefaultTableCollectionName, entryName);
        }

        [Test]
        public void SettingStringReference_TriggersUpdate()
        {
            const string tableName = "SettingStringReference Table Collection Name";
            const string entryName =  "SettingStringReference Entry Name";

            var newStringReference = new LocalizedString(){ TableReference = tableName, TableEntryReference = entryName };

            m_LocalizeStringEvent.StringReference = newStringReference;
            CheckEntryWasRequested(tableName, entryName);
        }
    }
}
