using NUnit.Framework;
using System;
using UnityEngine;
using UnityEngine.Localization;

namespace UnityEditor.Localization.Tests
{
    public class LocaleIdentifierVerification
    {
        [Test]
        public void SystemLanguageUnknownMapsToDefault()
        {
            var localeId = new LocaleIdentifier(SystemLanguage.Unknown);
            Assert.AreEqual(new LocaleIdentifier(), localeId);
        }

        [Test]
        public void DefaultCultureInfoIsNull()
        {
            Assert.IsNull(new LocaleIdentifier().CultureInfo, "Expected undefined to have no CultureInfo.");
        }

        [Test]
        public void GetCultureInfoReturnsNullWhenCodeIsNull()
        {
            LocaleIdentifier id = new LocaleIdentifier((string)null);
            Assert.IsNull(id.CultureInfo);
        }

        [Test]
        public void FromDefault_EqualsDefault()
        {
            LocaleIdentifier li1 = default;
            LocaleIdentifier li2 = default;
            Assert.AreEqual(li1, li2);
        }

        [Test]
        public void FromEmptyString_EqualsDefault()
        {
            LocaleIdentifier id = new LocaleIdentifier(string.Empty);
            Assert.AreEqual(new LocaleIdentifier(), id);
        }

        [Test]
        public void LocaleIdentifierComparision_IsNotCaseSensitive()
        {
            var locale = Locale.CreateLocale(SystemLanguage.ChineseSimplified);
            var id_zn_CN = new LocaleIdentifier("zh-Hans");

            Assert.IsTrue(locale.Identifier.Equals(id_zn_CN), "Expected LocaleIdentifier comparisons to be case-insensitive.");
        }
    }
}
