using NUnit.Framework;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Settings;

namespace UnityEngine.Localization.Tests
{
    public class LocalizedAssetTests
    {
        [SetUp]
        public void Setup()
        {
            // We need localization settings to prevent ValidateSettingsExist throwing an error.
            Assert.NotNull(LocalizationSettings.Instance);
        }

        [Test]
        public void ClearChangeHandler_DoesNotThrowException_WhenCompletedWasNotUsed()
        {
            var localizedTexture = new LocalizedTexture
            {
                CurrentLoadingOperation = Addressables.ResourceManager.CreateCompletedOperation((Texture)Texture2D.whiteTexture, null)
            };

            Assert.DoesNotThrow(() => localizedTexture.ClearChangeHandler());
        }
    }
}
