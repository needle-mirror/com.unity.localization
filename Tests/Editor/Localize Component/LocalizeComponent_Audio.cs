/*
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.Tests
{
    public class LocalizeComponent_Audio : LocalizeComponentTests
    {
        [Test]
        public void Localize_AudioSource_AddsLocalizationComponentWithValidParameters()
        {
            var audioComponent = m_Target.AddComponent<AudioSource>();

            var localizedComponent = LocalizeComponent.SetupForLocalization(audioComponent) as LocalizeAudioClip;
            Assert.NotNull(localizedComponent, $"Expected a {nameof(LocalizeAudioClip)} component to be returned.");

            Assert.AreEqual(KeyDatabase.EmptyId, localizedComponent.AssetReference.KeyId, "Expected the AssetReference property to be setup with the default key id.");
            Assert.IsNull(localizedComponent.AssetReference.TableName, "Expected the AssetReference property to be setup with no table name.");

            // Check event
            Assert.AreEqual(2, localizedComponent.UpdateAsset.GetPersistentEventCount(), "Expected the UpdateAsset event to have 2 listeners.");
            CheckEvent(localizedComponent.UpdateAsset, 0, "set_clip", audioComponent);
            CheckEvent(localizedComponent.UpdateAsset, 1, "Play", audioComponent);
        }
    }
}*/