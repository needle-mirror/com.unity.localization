using UnityEngine;
using NUnit.Framework;
using UnityEngine.Localization;

namespace UnityEditor.Localization.Tests
{
    [Category("Localization")]
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
    }
}
