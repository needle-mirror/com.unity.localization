using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization;

namespace UnityEditor.Localization.Tests
{
    public class LocaleIdentifierVerification
    {
        [Test]
        public void SystemLanguageUnknownMapsToUndefined()
        {
            var localeId = new LocaleIdentifier(SystemLanguage.Unknown);
            Assert.AreEqual(LocaleIdentifier.Undefined, localeId);
        }

        [Test]
        public void UndefinedCultureInfoIsNull()
        {
            Assert.IsNull(LocaleIdentifier.Undefined.CultureInfo, "Expected undefined to have no CultureInfo.");
        }

        [Test]
        public void GetCultureInfoReturnsNullWhenCodeIsNull()
        {
            LocaleIdentifier id = new LocaleIdentifier((string)null);
            Assert.IsNull(id.CultureInfo);
        }
    }
}
