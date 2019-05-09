using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization;
using ModificationEvent = UnityEditor.Localization.LocalizationEditorSettings.ModificationEvent;

namespace UnityEditor.Localization.Tests
{
    public class ModificationEventTests : AddressableAssetTestBase
    {
        class EventData
        {
            public ModificationEvent eventType;
            public object context;
        }
        List<EventData> m_CapturedEvents = new List<EventData>();
        EventData m_LastEvent;

        public KeyDatabase KeyDb { get; set; }

        protected override void OnInit()
        {
            KeyDb = ScriptableObject.CreateInstance<KeyDatabase>();
            CreateAsset(KeyDb, "General Use Key Db");
            LocalizationEditorSettings.OnModification += LocalizationEditorSettingsOnOnModification;
        }

        protected override void OnCleanup()
        {
            LocalizationEditorSettings.OnModification -= LocalizationEditorSettingsOnOnModification;
        }

        private void LocalizationEditorSettingsOnOnModification(ModificationEvent evt, object obj)
        {
            m_LastEvent = new EventData() {eventType = evt, context = obj};
            m_CapturedEvents.Add(m_LastEvent);
        }

        void ClearCapturedEvents()
        {
            m_CapturedEvents.Clear();
            m_LastEvent = null;
        }

        [SetUp]
        public void Setup()
        {
            ClearCapturedEvents();
        }

        void VerifyEventWasSent(ModificationEvent expectedEvent, object expectedContext, string message)
        {
            Assert.AreEqual(1, m_CapturedEvents.Count, "Expected 1 event to have been sent during modification." + message);
            Assert.IsNotNull(m_LastEvent, "Failed to capture the last event." + message);
            Assert.AreEqual(expectedEvent, m_LastEvent.eventType, "Incorrect event type was sent." + message);
            Assert.AreSame(expectedContext, m_LastEvent.context, "Unexpected context object sent." + message);
        }

        void VerifyNoEventWasSent(string message)
        {
            Assert.IsEmpty(m_CapturedEvents, "Expected 0 events to be sent." + message );
            Assert.IsNull(m_LastEvent, "Expected no events to be sent." + message);
        }

        Locale CreateAndAddLocaleAsset(string assetName)
        {
            var locale = Locale.CreateLocale(SystemLanguage.Danish);
            CreateAsset(locale, assetName);
            LocalizationEditorSettings.AddLocale(locale);
            return locale;
        }

        LocalizedTable CreateAndAddTableAsset(Type tableType, string assetName)
        {
            var assetPath = Path.Combine(k_TestConfigFolder, $"{assetName}_{tableType}.asset");
            var createdTable = LocalizationEditorSettings.CreateAssetTable(Locale.CreateLocale(SystemLanguage.English), KeyDb, assetName, tableType, assetPath);
            return createdTable;
        }

        [Test]
        public void AddLocale_SendsEvent()
        {
            var locale = CreateAndAddLocaleAsset(nameof(AddLocale_SendsEvent));
            VerifyEventWasSent(ModificationEvent.LocaleAdded, locale, "Expected LocaleAdded event when calling LocalizationEditorSettings.AddLocale");
            ClearCapturedEvents();

            LocalizationEditorSettings.RemoveLocale(locale);
            VerifyEventWasSent(ModificationEvent.LocaleRemoved, locale, "Expected LocaleRemoved event when calling LocalizationEditorSettings.RemoveLocale");
        }

        [Test]
        public void AddLocale_DoesNotSendEvents_WhenLocaleIsAlreadyAdded()
        {
            var locale = CreateAndAddLocaleAsset(nameof(AddLocale_DoesNotSendEvents_WhenLocaleIsAlreadyAdded));
            ClearCapturedEvents();

            LocalizationEditorSettings.AddLocale(locale);
            VerifyNoEventWasSent("Expected no events to be sent when the Locale has already been added.");
        }

        [Test]
        public void RemoveLocale_SendsEvent()
        {
            var locale = CreateAndAddLocaleAsset(nameof(RemoveLocale_SendsEvent));
            ClearCapturedEvents();

            LocalizationEditorSettings.RemoveLocale(locale);
            VerifyEventWasSent(ModificationEvent.LocaleRemoved, locale, "Expected LocaleRemoved event when calling LocalizationEditorSettings.RemoveLocale");
        }

        [Test]
        public void RemoveLocale_DoesNotSendEvents_WhenLocaleIsNotAdded()
        {
            var locale = CreateAndAddLocaleAsset(nameof(RemoveLocale_DoesNotSendEvents_WhenLocaleIsNotAdded));
            LocalizationEditorSettings.RemoveLocale(locale);
            ClearCapturedEvents();

            LocalizationEditorSettings.RemoveLocale(locale);
            VerifyNoEventWasSent("Expected no events to be sent when the Locale has already been removed.");
        }

        [TestCaseSource(nameof(AllTableTypes))]
        public void AddTable_SendsEvent(Type tableType)
        {
            var createdTable = CreateAndAddTableAsset(tableType, nameof(AddTable_SendsEvent));
            VerifyEventWasSent(ModificationEvent.TableAdded, createdTable, "Expected TableAdded event to be sent when using CreateAssetTable.");
        }

        [TestCaseSource(nameof(AllTableTypes))]
        public void RemoveTable_SendsEvent(Type tableType)
        {
            var createdTable = CreateAndAddTableAsset(tableType, nameof(AddTable_SendsEvent));
            ClearCapturedEvents();

            LocalizationEditorSettings.RemoveTable(createdTable);
            VerifyEventWasSent(ModificationEvent.TableRemoved, createdTable, "Expected TableRemoved event to be sent when using RemoveTable.");
        }

        [TestCaseSource(nameof(AllTableTypes))]
        public void GetAssetTables_DoesNotIncludeRemovedTable_DuringTableRemovedCall(Type tableType)
        {
            var createdTable = CreateAndAddTableAsset(tableType, nameof(GetAssetTables_DoesNotIncludeRemovedTable_DuringTableRemovedCall));
            ClearCapturedEvents();
            bool onModificationCalled = false;

            LocalizationEditorSettings.OnModification += (evt, obj) =>
            {
                var tables = LocalizationEditorSettings.GetAllAssetTables();
                Assert.False(tables.Contains(createdTable));
                onModificationCalled = true;
            };

            LocalizationEditorSettings.RemoveTable(createdTable);
            Assert.True(onModificationCalled, "Expected OnModification to be called but it was not, the test was not run.");
        }
    }
}
