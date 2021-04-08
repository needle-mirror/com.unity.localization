using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.Localization.SmartFormat.Core.Extensions;
using UnityEngine.Localization.SmartFormat.Core.Formatting;
using UnityEngine.Localization.SmartFormat.Core.Output;
using UnityEngine.Localization.SmartFormat.Core.Settings;
using UnityEngine.Localization.SmartFormat.Tests.TestUtils;
using UnityEngine.Localization.SmartFormat.Utilities;

namespace UnityEngine.Localization.SmartFormat.Tests.Core
{
    public class FormatterTests
    {
        private object[] errorArgs = new object[] { new FormatDelegate(format => { throw new Exception("ERROR!"); }) };

        [Test]
        public void Formatter_Throws_Exceptions()
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.FormatErrorAction = ErrorAction.ThrowError;

            Assert.Throws<FormattingException>(() => formatter.Test("--{0}--", errorArgs, "--ERROR!--ERROR!--"));
        }

        [Test]
        public void Formatter_Outputs_Exceptions()
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.FormatErrorAction = ErrorAction.OutputErrorInResult;

            formatter.Test("--{0}--{0:ZZZZ}--", errorArgs, "--ERROR!--ERROR!--");
        }

        [Test]
        public void Formatter_Ignores_Exceptions()
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.FormatErrorAction = ErrorAction.Ignore;

            formatter.Test("--{0}--{0:ZZZZ}--", errorArgs, "------");
        }

        [Test]
        public void Formatter_Maintains_Tokens()
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.FormatErrorAction = ErrorAction.MaintainTokens;

            formatter.Test("--{0}--{0:ZZZZ}--", errorArgs, "--{0}--{0:ZZZZ}--");
        }

        [Test]
        public void Formatter_Maintains_Object_Tokens()
        {
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.FormatErrorAction = ErrorAction.MaintainTokens;
            formatter.Test("--{Object.Thing}--", errorArgs, "--{Object.Thing}--");
        }

        [Test]
        public void Formatter_AlignNull()
        {
            string name = null;
            var obj = new { name = name };
            var str2 = Smart.Format("Name: {name,-10}| Column 2", obj);
            Assert.That(str2, Is.EqualTo("Name:           | Column 2"));
        }

        [Test]
        public void Formatter_NotifyFormattingError()
        {
            var obj = new { Name = "some name" };
            var badPlaceholder = new List<string>();

            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.FormatErrorAction = ErrorAction.Ignore;
            formatter.OnFormattingFailure += (o, args) => badPlaceholder.Add(args.Placeholder);
            var res = formatter.Format("{NoName} {Name} {OtherMissing}", obj);
            Assert.That(badPlaceholder.Count == 2 && badPlaceholder[0] == "{NoName}" && badPlaceholder[1] == "{OtherMissing}");
        }

        [Test]
        public void LeadingBackslashMustNotEscapeBraces()
        {
            var smart = Smart.CreateDefaultSmartFormat();
            smart.Settings.ConvertCharacterStringLiterals = false;

            var expected = "\\Hello";
            var actual = smart.Format("\\{Test}", new { Test = "Hello" });
            Assert.AreEqual(expected, actual);

            smart.Settings.ConvertCharacterStringLiterals = true;

            expected = @"\Hello";
            actual = smart.Format(@"\\{Test}", new { Test = "Hello" }); // double backslash means escaping the backslash
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void NullAndBoxedNullBehaveTheSame()
        {
            // see issue https://github.com/scottrippey/SmartFormat.NET/issues/101
            var smart = Smart.CreateDefaultSmartFormat();
            object boxedNull = null;
            Assert.AreEqual(smart.Format("{0}", null), smart.Format("{0}", boxedNull));
        }

        class ExampleSourceFormatter : ISource, IFormatter
        {
            public string[] Names { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
            {
                throw new NotImplementedException();
            }

            public bool TryEvaluateSelector(ISelectorInfo selectorInfo)
            {
                throw new NotImplementedException();
            }

            public bool TryEvaluateAllLiterals(IFormattingInfo formattingInfo)
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void GetSourceExtension_ReturnsNullWhenNotFound()
        {
            var smart = Smart.CreateDefaultSmartFormat();
            Assert.Null(smart.GetSourceExtension<ExampleSourceFormatter>());
        }

        [Test]
        public void GetFormatterExtension_ReturnsNullWhenNotFound()
        {
            var smart = Smart.CreateDefaultSmartFormat();
            Assert.Null(smart.GetFormatterExtension<ExampleSourceFormatter>());
        }

        [Test]
        public void SmartFormatter_FormatDetails()
        {
            var args = new object[] {new Dictionary<string, string> {{"Greeting", "Hello"}} };
            var format = "{Greeting}";
            var output = new StringOutput();
            var formatter = Smart.CreateDefaultSmartFormat();
            formatter.Settings.CaseSensitivity = CaseSensitivityType.CaseInsensitive;
            formatter.Settings.ConvertCharacterStringLiterals = true;
            formatter.Settings.FormatErrorAction = ErrorAction.OutputErrorInResult;
            formatter.Settings.ParseErrorAction = ErrorAction.OutputErrorInResult;
            var formatParsed = formatter.Parser.ParseFormat(format, new List<string>  {string.Empty});
            var formatDetails = FormatDetailsPool.Get(formatter, formatParsed, args, null, null, output);

            Assert.AreEqual(args, formatDetails.OriginalArgs);
            Assert.AreEqual(format, formatDetails.OriginalFormat.RawText);
            Assert.AreEqual(formatter.Settings, formatDetails.Settings);
            Assert.IsTrue(formatDetails.FormatCache == null);
        }
    }
}
