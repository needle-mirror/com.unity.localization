using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Metadata;
using UnityEngine.TestTools;

namespace UnityEditor.Localization.Tests.Metadata
{
    public class FallbackLocaleTests
    {
        static Locale CreateLocaleWithFallback(string name)
        {
            var locale = ScriptableObject.CreateInstance<Locale>();
            locale.name = name;
            var fallback = new FallbackLocale();
            locale.Metadata.AddMetadata(fallback);
            return locale;
        }

        [Test]
        public void CyclicFallback_ThrowsWarningAndIsNotAssigned_WhenLocalesRefereceEachOther()
        {
            var localeA = CreateLocaleWithFallback("A");
            var localeB = CreateLocaleWithFallback("B");
            var fallbackA = localeA.Metadata.GetMetadata<FallbackLocale>();
            var fallbackB = localeB.Metadata.GetMetadata<FallbackLocale>();

            LogAssert.Expect(LogType.Warning, new Regex("Cyclic fallback linking detected.*"));

            fallbackA.Locale = localeB;
            fallbackB.Locale = localeA; // Should throw an error

            Assert.IsNotNull(fallbackA.Locale, "Expected Fallback to not be null");
            Assert.IsNull(fallbackB.Locale, "Expected Fallback locale to be null.");

            Object.DestroyImmediate(localeA);
            Object.DestroyImmediate(localeB);
        }

        [Test]
        public void CyclicFallback_ThrowsWarningAndIsNotAssigned_WhenChainedLocalesReferenceFirstLocale()
        {
            const int numLocales = 10;
            var locales = new Locale[numLocales];

            Locale nextParent = null;
            for (int i = 0; i < numLocales; ++i)
            {
                locales[i] = CreateLocaleWithFallback(i.ToString());
                if (nextParent != null)
                {
                    var fb = locales[i].Metadata.GetMetadata<FallbackLocale>();
                    fb.Locale = nextParent;
                    Assert.IsNotNull(fb.Locale);
                }
                nextParent = locales[i];
            }

            LogAssert.Expect(LogType.Warning, new Regex("Cyclic fallback linking detected.*"));

            // link the front with the back to create a loop
            var fallback = locales[0].Metadata.GetMetadata<FallbackLocale>();
            fallback.Locale = locales[numLocales - 1];

            Assert.IsNull(fallback.Locale, "Expected Fallback locale to be null.");

            locales.ToList().ForEach(Object.DestroyImmediate);
        }

        [Test]
        public void CyclicFallback_ThrowsWarningAndIsNotAssigned_WhenLocaleRefereceItself()
        {
            var localeA = CreateLocaleWithFallback("A");
            var fallbackA = localeA.Metadata.GetMetadata<FallbackLocale>();

            LogAssert.Expect(LogType.Warning, new Regex("Cyclic fallback linking detected.*"));

            fallbackA.Locale = localeA;

            Assert.IsNull(fallbackA.Locale, "Expected Fallback locale to be null.");
            Object.DestroyImmediate(localeA);
        }
    }
}
