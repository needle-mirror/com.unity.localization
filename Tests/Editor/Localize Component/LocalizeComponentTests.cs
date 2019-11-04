using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Tables;
using Object = UnityEngine.Object;

namespace UnityEditor.Localization.Tests
{
    public abstract class LocalizeComponentTests
    {
        protected GameObject m_Target;

        protected FakedLocalizationEditorSettings Settings { get; set; }

        protected KeyDatabase KeyDb { get; set; }

        protected const string kStringTableName = "MyText";
        protected const string kStringTableKey = "This is some text";
        protected uint StringTableKeyId;

        [SetUp]
        public virtual void Init()
        {
            Settings = new FakedLocalizationEditorSettings();
            LocalizationEditorSettings.Instance = Settings;
            KeyDb = ScriptableObject.CreateInstance<KeyDatabase>();
            KeyDb.TableNameGuid = Guid.NewGuid();
            var entry = KeyDb.AddKey(kStringTableKey);
            StringTableKeyId = entry.Id;
            m_Target = new GameObject("LocalizeComponent");

            var stringTable = ScriptableObject.CreateInstance<StringTable>();
            stringTable.Keys = KeyDb;
            stringTable.TableName = kStringTableName;
            stringTable.LocaleIdentifier = "en";
            stringTable.AddEntry(kStringTableKey, "");
            LocalizationEditorSettings.AddOrUpdateTable(stringTable, false);

            var collection = new AssetTableCollection()
            {
                Keys = KeyDb,
                Tables = new List<LocalizedTable>{ stringTable },
                TableType = typeof(StringTable)
            };
            Settings.Collections.Add(collection);
        }

        [TearDown]
        public virtual void Teardown()
        {
            LocalizationEditorSettings.Instance = null;
            Object.DestroyImmediate(KeyDb);
            Object.DestroyImmediate(m_Target);
        }

        protected static void CheckEvent(UnityEventBase evt, int eventIdx, string expectedMethodName, Object expectedTarget)
        {
            Assert.AreEqual(expectedMethodName, evt.GetPersistentMethodName(eventIdx), "Unexpected method name.");
            Assert.AreSame(expectedTarget, evt.GetPersistentTarget(eventIdx), "Unexpected target. It should be the component being localized.");
        }
    }
}