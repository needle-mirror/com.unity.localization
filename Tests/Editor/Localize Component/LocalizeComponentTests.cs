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

        protected SharedTableData sharedData { get; set; }

        protected const string kStringTableName = "MyText";
        protected const string kStringTableKey = "This is some text";
        protected uint StringTableKeyId;

        [SetUp]
        public virtual void Init()
        {
            Settings = new FakedLocalizationEditorSettings();
            LocalizationEditorSettings.Instance = Settings;
            sharedData = ScriptableObject.CreateInstance<SharedTableData>();
            sharedData.TableNameGuid = Guid.NewGuid();
            var entry = sharedData.AddKey(kStringTableKey);
            StringTableKeyId = entry.Id;
            m_Target = new GameObject("LocalizeComponent");

            var stringTable = ScriptableObject.CreateInstance<StringTable>();
            stringTable.SharedData = sharedData;
            stringTable.TableName = kStringTableName;
            stringTable.LocaleIdentifier = "en";
            stringTable.AddEntry(kStringTableKey, "");
            LocalizationEditorSettings.AddOrUpdateTable(stringTable, false);

            var collection = new AssetTableCollection()
            {
                SharedData = sharedData,
                Tables = new List<LocalizedTable> { stringTable },
                TableType = typeof(StringTable)
            };
            Settings.Collections.Add(collection);
        }

        [TearDown]
        public virtual void Teardown()
        {
            LocalizationEditorSettings.Instance = null;
            Object.DestroyImmediate(sharedData);
            Object.DestroyImmediate(m_Target);
        }

        protected static void CheckEvent(UnityEventBase evt, int eventIdx, string expectedMethodName, Object expectedTarget)
        {
            Assert.AreEqual(expectedMethodName, evt.GetPersistentMethodName(eventIdx), "Unexpected method name.");
            Assert.AreSame(expectedTarget, evt.GetPersistentTarget(eventIdx), "Unexpected target. It should be the component being localized.");
        }
    }
}
