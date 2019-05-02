using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace UnityEditor.Localization.Tests
{
    public class LocalizeComponentTests
    {
        GameObject m_Target;

        protected FakedLocalizationEditorSettings Settings { get; set; }

        protected KeyDatabase KeyDb { get; set; }

        const string kStringTableName = "MyText";
        const string kStringTableKey = "This is some text";
        uint StringTableKeyId;

        [SetUp]
        public void Init()
        {
            Settings = new FakedLocalizationEditorSettings();
            LocalizationEditorSettings.Instance = Settings;
            KeyDb = ScriptableObject.CreateInstance<KeyDatabase>();
            var entry = KeyDb.AddKey(kStringTableKey);
            StringTableKeyId = entry.Id;
            m_Target = new GameObject("LocalizeComponent");

            var stringTable = ScriptableObject.CreateInstance<StringTable>();
            stringTable.Keys = KeyDb;
            stringTable.TableName = kStringTableName;
            stringTable.LocaleIdentifier = "en";
            stringTable.AddEntry(kStringTableKey);
            LocalizationEditorSettings.AddOrUpdateTable(stringTable);
        }

        [TearDown]
        public void Teardown()
        {
            LocalizationEditorSettings.Instance = null;
            Object.DestroyImmediate(KeyDb);
            Object.DestroyImmediate(m_Target);
        }

        static void CheckEvent(UnityEventBase evt, int eventIdx, string expectedMethodName, Object expectedTarget)
        {
            Assert.AreEqual(expectedMethodName, evt.GetPersistentMethodName(eventIdx), "Unexpected method name.");
            Assert.AreSame(expectedTarget, evt.GetPersistentTarget(eventIdx), "Unexpected target. It should be the component being localized.");
        }

        [Test]
        public void Localize_UIText_AddsLocalizationComponentWithValidParameters_WhenTextMatchesKey()
        {
            var textComponent = m_Target.AddComponent<Text>();
            textComponent.text = kStringTableKey;

            var localizedComponent = LocalizeComponent.SetupForLocalization(textComponent) as LocalizeString;
            Assert.NotNull(localizedComponent, "Expected a LocalizeString component to be returned.");

            Assert.AreEqual(StringTableKeyId, localizedComponent.StringReference.KeyId, "Expected the StringReference property to be setup with the correct key id.");
            Assert.AreEqual(kStringTableName, localizedComponent.StringReference.TableName, "Expected the StringReference property to be setup with the correct table name.");

            // Check event
            Assert.AreEqual(1, localizedComponent.UpdateString.GetPersistentEventCount(), "Expected the UpdateString event to have 1 listener.");
            CheckEvent(localizedComponent.UpdateString, 0, "set_text", textComponent);
        }

        [Test]
        public void Localize_UIText_AddsLocalizationComponentWithDefaultParameters_WhenKeyDoesNotExist()
        {
            var textComponent = m_Target.AddComponent<Text>();
            textComponent.text = "Text that is not in a table";

            var localizedComponent = LocalizeComponent.SetupForLocalization(textComponent) as LocalizeString;
            Assert.NotNull(localizedComponent, "Expected a LocalizeString component to be returned.");

            Assert.AreEqual(KeyDatabase.EmptyId ,localizedComponent.StringReference.KeyId, "Expected the StringReference property to be setup with the default key id.");
            Assert.IsNull(localizedComponent.StringReference.TableName, "Expected the StringReference property to be setup with no table name.");

            // Check event
            Assert.AreEqual(1, localizedComponent.UpdateString.GetPersistentEventCount(), "Expected the UpdateString event to have 1 listener.");
            CheckEvent(localizedComponent.UpdateString, 0, "set_text", textComponent);
        }

        [Test]
        public void Localize_RawImage_AddsLocalizationComponentWithValidParameters()
        {
            var imageComponent = m_Target.AddComponent<RawImage>();

            var localizedComponent = LocalizeComponent.SetupForLocalization(imageComponent) as LocalizeTexture2D;
            Assert.NotNull(localizedComponent, "Expected a LocalizeTexture2D component to be returned.");

            Assert.AreEqual(KeyDatabase.EmptyId, localizedComponent.AssetReference.KeyId, "Expected the AssetReference property to be setup with the default key id.");
            Assert.IsNull(localizedComponent.AssetReference.TableName, "Expected the AssetReference property to be setup with no table name.");

            // Check event
            Assert.AreEqual(1, localizedComponent.UpdateAsset.GetPersistentEventCount(), "Expected the UpdateAsset event to have 1 listener.");
            CheckEvent(localizedComponent.UpdateAsset, 0, "set_texture", imageComponent);
        }

        [Test]
        public void Localize_AudioSource_AddsLocalizationComponentWithValidParameters()
        {
            var audioComponent = m_Target.AddComponent<AudioSource>();

            var localizedComponent = LocalizeComponent.SetupForLocalization(audioComponent) as LocalizeAudioClip;
            Assert.NotNull(localizedComponent, "Expected a LocalizeAudioClip component to be returned.");

            Assert.AreEqual(KeyDatabase.EmptyId, localizedComponent.AssetReference.KeyId, "Expected the AssetReference property to be setup with the default key id.");
            Assert.IsNull(localizedComponent.AssetReference.TableName, "Expected the AssetReference property to be setup with no table name.");

            // Check event
            Assert.AreEqual(2, localizedComponent.UpdateAsset.GetPersistentEventCount(), "Expected the UpdateAsset event to have 2 listeners.");
            CheckEvent(localizedComponent.UpdateAsset, 0, "set_clip", audioComponent);
            CheckEvent(localizedComponent.UpdateAsset, 1, "Play", audioComponent);
        }
    }
}