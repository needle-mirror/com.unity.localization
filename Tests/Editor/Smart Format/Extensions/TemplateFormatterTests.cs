using NUnit.Framework;
using UnityEngine.Localization.SmartFormat.Core.Formatting;
using UnityEngine.Localization.SmartFormat.Core.Settings;
using UnityEngine.Localization.SmartFormat.Extensions;

namespace UnityEngine.Localization.SmartFormat.Tests.Extensions
{
    public class TemplateFormatterTests
    {
        SmartFormatter m_Smart;

        public TemplateFormatterTests()
        {
            m_Smart = Smart.CreateDefaultSmartFormat();
        }

        [OneTimeSetUp]
        public void SetupSmart()
        {
            RegisterTemplates(m_Smart);
        }

        private void RegisterTemplates(SmartFormatter smart)
        {
            var templates = new TemplateFormatter() { Formatter = smart };
            smart.AddExtensions(templates);

            templates.Register("firstLast", "{First} {Last}");
            templates.Register("lastFirst", "{Last}, {First}");
            templates.Register("FIRST", "{First.ToUpper}");
            templates.Register("last", "{Last.ToLower}");

            if (smart.Settings.CaseSensitivity == CaseSensitivityType.CaseSensitive)
            {
                templates.Register("LAST", "{Last.ToUpper}");
            }

            templates.Register("NESTED", "{:template:FIRST} {:template:last}");
        }

        private void TestWithScottRippey(string format, string expected)
        {
            var person = new
            {
                First = "Scott",
                Last = "Rippey",
            };

            var actual = m_Smart.Format(format, person);
            Assert.AreEqual(expected, actual);
        }

        private void TestWithMultipleUsers(string format, string expected)
        {
            var people = new[]
            {
                new { First = "Jim", Last = "Halpert" },
                new { First = "Pam", Last = "Beasley" },
                new { First = "Dwight", Last = "Schrute" },
            };

            var actual = m_Smart.Format(format, (object)people);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [TestCase("{First} {Last}", "Scott Rippey")]
        public void Sanity_test(string format, string expected) { TestWithScottRippey(format, expected); }

        [Test]
        [TestCase("{:template(firstLast)}", "Scott Rippey")]
        [TestCase("{:template:firstLast}", "Scott Rippey")]
        [TestCase("{:template():firstLast}", "Scott Rippey")]
        [TestCase("{:t(firstLast)}", "Scott Rippey")]
        [TestCase("{:t:firstLast}", "Scott Rippey")]
        [TestCase("{:t(firstLast):IGNORED}", "Scott Rippey")]
        [TestCase("{:t:firstLast}", "Scott Rippey")]
        public void Template_can_be_called_with_options_or_with_formatString(string format, string expected) { TestWithScottRippey(format, expected); }

        [Test]
        [TestCase("{:template:lastFirst}", "Rippey, Scott")]
        [TestCase("{:template:FIRST}", "SCOTT")]
        [TestCase("{:template:last}", "rippey")]
        [TestCase("{:template:LAST}", "RIPPEY")]
        public void Simple_templates_work_as_expected(string format, string expected) { TestWithScottRippey(format, expected); }

        [Test]
        [TestCase("{:template:FIRST} {:template:last}", "SCOTT rippey")]
        [TestCase("{:template:firstLast} | {:template:lastFirst}", "Scott Rippey | Rippey, Scott")]
        public void Multiple_templates_can_be_used(string format, string expected) { TestWithScottRippey(format, expected); }

        [Test]
        [TestCase("{:template:NESTED}", "SCOTT rippey")]
        public void Templates_can_be_nested(string format, string expected) { TestWithScottRippey(format, expected); }

        [Test]
        [TestCase("{:{:template:FIRST} {:template:last}|, }", "JIM halpert, PAM beasley, DWIGHT schrute")]
        [TestCase("{:{:template:NESTED}|, }", "JIM halpert, PAM beasley, DWIGHT schrute")]
        public void Templates_can_be_reused(string format, string expected) { TestWithMultipleUsers(format, expected); }


        [Test]
        [TestCase("{:template:AAAA}")]
        [TestCase("{:template:9999}")]
        [TestCase("{:template:FIRST_}")]
        [TestCase("{:template:_last}")]
        [TestCase("{:template:}")]
        [TestCase("{:template()}")]
        [TestCase("{:template(AAAA):}")]
        [TestCase("{:template():AAAA}")]
        public void Templates_must_be_defined(string format)
        {
            m_Smart.Settings.FormatErrorAction = ErrorAction.ThrowError;
            Assert.Throws<FormattingException>(() => m_Smart.Format(format, 5));
        }

        [Test]
        [TestCase("{:template:first}")]
        [TestCase("{:template:firstlast}")]
        [TestCase("{:template:LaSt}")]
        public void Templates_are_case_sensitive(string format)
        {
            m_Smart.Settings.FormatErrorAction = ErrorAction.ThrowError;
            Assert.Throws<FormattingException>(() => m_Smart.Format(format, 5));
        }

        [Test]
        [TestCase("{:template:first}", "SCOTT")]
        [TestCase("{:template:FIRST}", "SCOTT")]
        [TestCase("{:template:last}", "rippey")]
        [TestCase("{:template:LAST}", "rippey")]
        [TestCase("{:template:nested}", "SCOTT rippey")]
        [TestCase("{:template:NESTED}", "SCOTT rippey")]
        [TestCase("{:template:NeStEd}", "SCOTT rippey")]
        [TestCase("{:template:fIrStLaSt}", "Scott Rippey")]
        public void Templates_can_be_case_insensitive_and_overwrite_each_other(string format, string expected)
        {
            this.m_Smart = Smart.CreateDefaultSmartFormat();
            this.m_Smart.Settings.CaseSensitivity = CaseSensitivityType.CaseInsensitive;
            RegisterTemplates(this.m_Smart);
            TestWithScottRippey(format, expected);

            // Reset:
            this.SetupSmart();
        }
    }
}
