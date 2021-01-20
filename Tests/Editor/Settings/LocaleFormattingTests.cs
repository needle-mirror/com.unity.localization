using System;
using NUnit.Framework;

namespace UnityEngine.Localization.Tests
{
    public class LocaleFormattingTests
    {
        [TestCase("en", false, "fr", "$123.45 1,234,567.35 12/29/2020 12:00:00 AM")]
        [TestCase("en", true, "fr", "123,45 € 1 234 567,35 29/12/2020 00:00:00")]
        [TestCase("ja", true, "", "¤123.45 1,234,567.35 12/29/2020 00:00:00")]
        [TestCase("ja", false, "", "￥123 1,234,567.35 2020/12/29 0:00:00")]
        [TestCase("custom", true, "en", "$123.45 1,234,567.35 12/29/2020 12:00:00 AM")]
        [TestCase("custom", true, "hu", "123,45 Ft 1 234 567,35 2020. 12. 29. 0:00:00")]
        [TestCase("en", true, "unknown", "$123.45 1,234,567.35 12/29/2020 12:00:00 AM")]
        public void ExpectedFormatterIsUsed(string code, bool useCustom, string formatCode, string expectedOutput)
        {
            var locale = Locale.CreateLocale(code);
            locale.CustomFormatterCode = formatCode;
            locale.UseCustomFormatter = useCustom;

            var formattedText = string.Format(locale.Formatter, "{0:C} {1:N} {2}", 123.45f, 1234567.345, new DateTime(2020, 12, 29));
            Assert.AreEqual(expectedOutput, formattedText);
            Object.DestroyImmediate(locale);
        }
    }
}
