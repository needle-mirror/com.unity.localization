#if !UNITY_2019_2_OR_NEWER || PACKAGE_UGUI

using NUnit.Framework;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace UnityEditor.Localization.Tests
{
    public class LocalizeComponent_UGUI : LocalizeComponentTests
    {
        [Test]
        public void Localize_UIText_AddsLocalizationComponentWithValidParameters_WhenTextMatchesKey()
        {
            var textComponent = m_Target.AddComponent<Text>();
            textComponent.text = kStringTableKey;

            var localizedComponent = LocalizeComponent.SetupForLocalization(textComponent) as LocalizeString;
            Assert.NotNull(localizedComponent, $"Expected a {nameof(LocalizeString)} component to be returned.");

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
            Assert.NotNull(localizedComponent, $"Expected a {nameof(LocalizeString)} component to be returned.");

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
            Assert.NotNull(localizedComponent, $"Expected a {nameof(LocalizeTexture2D)} component to be returned.");

            Assert.AreEqual(KeyDatabase.EmptyId, localizedComponent.AssetReference.KeyId, "Expected the AssetReference property to be setup with the default key id.");
            Assert.IsNull(localizedComponent.AssetReference.TableName, "Expected the AssetReference property to be setup with no table name.");

            // Check event
            Assert.AreEqual(1, localizedComponent.UpdateAsset.GetPersistentEventCount(), "Expected the UpdateAsset event to have 1 listener.");
            CheckEvent(localizedComponent.UpdateAsset, 0, "set_texture", imageComponent);
        }
    }
}

#endif // !UNITY_2019_2_OR_NEWER || PACKAGE_UGUI