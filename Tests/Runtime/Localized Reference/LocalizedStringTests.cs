using NUnit.Framework;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Settings;

namespace UnityEngine.Localization.Tests
{
    public class LocalizedStringTests
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
            LocalizedString localizedString = new LocalizedString
            {
                CurrentLoadingOperation = Addressables.ResourceManager.CreateCompletedOperation(default(LocalizedStringDatabase.TableEntryResult), null)
            };

            Assert.DoesNotThrow(() => localizedString.ClearChangeHandler());
        }
    }
}
