using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine.Localization.SmartFormat.Core.Formatting;
using UnityEngine.Localization.SmartFormat.Core.Settings;
using UnityEngine.Localization.SmartFormat.Extensions;

namespace UnityEngine.Localization.SmartFormat.Tests.Extensions
{
    public class IsMatchFormatterTests
    {
        private Dictionary<string, object> m_Variable = new Dictionary<string, object>() { {"theKey", "Some123Content"}};
        private SmartFormatter m_Formatter;

        public IsMatchFormatterTests()
        {
            m_Formatter = Smart.CreateDefaultSmartFormat();
            var ismatch = new IsMatchFormatter();
            ismatch.RegexOptions = RegexOptions.CultureInvariant;
            m_Formatter.AddExtensions(ismatch);
            m_Formatter.Settings.FormatErrorAction = ErrorAction.ThrowError;
        }

        [TestCase("{theKey:ismatch(^.+123.+$):Okay - {}|No match content}", RegexOptions.None, "Okay - Some123Content")]
        [TestCase("{theKey:ismatch(^.+123.+$):Fixed content if match|No match content}", RegexOptions.None, "Fixed content if match")]
        [TestCase("{theKey:ismatch(^.+999.+$):{}|No match content}", RegexOptions.None, "No match content")]
        [TestCase("{theKey:ismatch(^.+123.+$):|Only content with no match}", RegexOptions.None, "")]
        [TestCase("{theKey:ismatch(^.+999.+$):|Only content with no match}", RegexOptions.None, "Only content with no match")]
        [TestCase("{theKey:ismatch(^SOME123.+$):Okay - {}|No match content}", RegexOptions.IgnoreCase, "Okay - Some123Content")]
        [TestCase("{theKey:ismatch(^SOME123.+$):Okay - {}|No match content}", RegexOptions.None, "No match content")]
        public void Test_Formats_And_CaseSensitivity(string format, RegexOptions options, string expected)
        {
            ((IsMatchFormatter)m_Formatter.FormatterExtensions.First(fex =>
                fex.GetType() == typeof(IsMatchFormatter))).RegexOptions = options;

            Assert.AreEqual(expected, m_Formatter.Format(format, m_Variable));
        }

        [Test]
        public void Test_FormatException()
        {
            // less than 2 format options throw exception
            Assert.Throws<FormattingException>(() =>
                m_Formatter.Format("{theKey:ismatch(^.+123.+$):Dummy content}", m_Variable));
        }

        [Test]
        public void Test_List()
        {
            var myList = new List<int> {100, 200, 300};
            Assert.AreEqual("100.00, 200.00 and 'no match'",
                m_Formatter.Format(CultureInfo.InvariantCulture,
                    "{0:list:{:ismatch(^100|200|999$):{:0.00}|'no match'}|, | and }", myList));

            Assert.AreEqual("'match', 'match' and 'no match'",
                m_Formatter.Format(CultureInfo.InvariantCulture,
                    "{0:list:{:ismatch(^100|200|999$):'match'|'no match'}|, | and }", myList));
        }
    }
}
