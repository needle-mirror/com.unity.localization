#if PACKAGE_UGUI

using NUnit.Framework;
using UnityEditor.Localization.Plugins.UGUI;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Tables;
using UnityEngine.UI;

namespace UnityEditor.Localization.Tests
{
    public class LocalizeComponent_UGUITests : LocalizeComponentTests
    {
        [Test]
        public void Localize_UIText_AddsLocalizationComponentWithValidParameters_WhenTextMatchesKey()
        {
            var textComponent = m_Target.AddComponent<Text>();
            textComponent.text = kStringTableKey;

            var localizedComponent = LocalizeComponent_UGUI.SetupForLocalization(textComponent) as LocalizeStringBehaviour;
            Assert.NotNull(localizedComponent, $"Expected a {nameof(LocalizeStringBehaviour)} component to be returned.");

            Assert.AreEqual(StringTableKeyId, localizedComponent.StringReference.TableEntryReference.KeyId, "Expected the StringReference property to be setup with the correct key id.");
            Assert.AreEqual(sharedData.TableNameGuid, localizedComponent.StringReference.TableReference.TableNameGuid, "Expected the StringReference property to be setup with the correct table name guid.");

            // Check event
            Assert.AreEqual(1, localizedComponent.OnUpdateString.GetPersistentEventCount(), "Expected the UpdateString event to have 1 listener.");
            CheckEvent(localizedComponent.OnUpdateString, 0, "set_text", textComponent);
        }

        [Test]
        public void Localize_UIText_AddsLocalizationComponentWithDefaultParameters_WhenKeyDoesNotExist()
        {
            var textComponent = m_Target.AddComponent<Text>();
            textComponent.text = "Text that is not in a table";

            var localizedComponent = LocalizeComponent_UGUI.SetupForLocalization(textComponent) as LocalizeStringBehaviour;
            Assert.NotNull(localizedComponent, $"Expected a {nameof(LocalizeStringBehaviour)} component to be returned.");

            Assert.AreEqual(SharedTableData.EmptyId, localizedComponent.StringReference.TableEntryReference.KeyId, "Expected the StringReference property to be setup with the default key id.");
            Assert.AreEqual(TableReference.Type.Empty, localizedComponent.StringReference.TableReference.ReferenceType, "Expected the StringReference property to be empty.");

            // Check event
            Assert.AreEqual(1, localizedComponent.OnUpdateString.GetPersistentEventCount(), "Expected the UpdateString event to have 1 listener.");
            CheckEvent(localizedComponent.OnUpdateString, 0, "set_text", textComponent);
        }

        //[Test]
        //public void Localize_RawImage_AddsLocalizationComponentWithValidParameters()
        //{
        //    var imageComponent = m_Target.AddComponent<RawImage>();

        //    var localizedComponent = LocalizeComponent.SetupForLocalization(imageComponent) as LocalizeTexture2D;
        //    Assert.NotNull(localizedComponent, $"Expected a {nameof(LocalizeTexture2D)} component to be returned.");

        //    Assert.AreEqual(KeyDatabase.EmptyId, localizedComponent.AssetReference.KeyId, "Expected the AssetReference property to be setup with the default key id.");
        //    Assert.IsNull(localizedComponent.AssetReference.TableName, "Expected the AssetReference property to be setup with no table name.");

        //    // Check event
        //    Assert.AreEqual(1, localizedComponent.UpdateAsset.GetPersistentEventCount(), "Expected the UpdateAsset event to have 1 listener.");
        //    CheckEvent(localizedComponent.UpdateAsset, 0, "set_texture", imageComponent);
        //}
    }
}

#endif // PACKAGE_UGUI
