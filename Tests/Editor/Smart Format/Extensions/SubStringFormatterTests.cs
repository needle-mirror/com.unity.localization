using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine.Localization.SmartFormat.Core.Settings;
using UnityEngine.Localization.SmartFormat.Extensions;

namespace UnityEngine.Localization.SmartFormat.Tests.Extensions
{
    public class SubStringFormatterTests
    {
        readonly List<object> m_People;
        readonly SmartFormatter m_Smart;

        public SubStringFormatterTests()
        {
            m_Smart = Smart.CreateDefaultSmartFormat();
            m_Smart.Settings.FormatErrorAction = ErrorAction.ThrowError;
            m_Smart.Settings.ParseErrorAction = ErrorAction.ThrowError;

            if (m_Smart.FormatterExtensions.FirstOrDefault(fmt => fmt.Names.Contains("substr")) == null)
            {
                m_Smart.FormatterExtensions.Add(new SubStringFormatter());
            }

            m_People = new List<object>
            {new {Name = "Long John", City = "New York"}, new {Name = "Short Mary", City = "Massachusetts"}, };
        }

        [Test]
        public void LengthLongerThanString_ReturnEmptyString()
        {
            var formatter = m_Smart.GetFormatterExtension<SubStringFormatter>();
            var behavior = formatter.OutOfRangeBehavior;

            formatter.OutOfRangeBehavior = SubStringFormatter.SubStringOutOfRangeBehavior.ReturnEmptyString;
            Assert.AreEqual(string.Empty, m_Smart.Format("{Name:substr(0,999)}", m_People.First()));

            formatter.OutOfRangeBehavior = behavior;
        }

        [Test]
        public void LengthLongerThanString_ReturnStartIndexToEndOfString()
        {
            var formatter = m_Smart.GetFormatterExtension<SubStringFormatter>();
            var behavior = formatter.OutOfRangeBehavior;

            formatter.OutOfRangeBehavior = SubStringFormatter.SubStringOutOfRangeBehavior.ReturnStartIndexToEndOfString;
            Assert.AreEqual("Long John", m_Smart.Format("{Name:substr(0,999)}", m_People.First()));

            formatter.OutOfRangeBehavior = behavior;
        }

        [Test]
        public void LengthLongerThanString_ThrowException()
        {
            var formatter = m_Smart.GetFormatterExtension<SubStringFormatter>();
            var behavior = formatter.OutOfRangeBehavior;

            formatter.OutOfRangeBehavior = SubStringFormatter.SubStringOutOfRangeBehavior.ThrowException;
            Assert.Throws<SmartFormat.Core.Formatting.FormattingException>(() => m_Smart.Format("{Name:substr(0,999)}", m_People.First()));

            formatter.OutOfRangeBehavior = behavior;
        }

        [Test]
        public void NoParameters()
        {
            Assert.AreEqual("No parentheses: Long John", m_Smart.Format("No parentheses: {Name:substr}", m_People.First()));
            Assert.Throws<SmartFormat.Core.Formatting.FormattingException>(() => m_Smart.Format("No parameters: {Name:substr()}", m_People.First()));
            Assert.Throws<SmartFormat.Core.Formatting.FormattingException>(() => m_Smart.Format("Only delimiter: {Name:substr(,)}", m_People.First()));
        }

        [Test]
        public void StartPositionLongerThanString()
        {
            Assert.AreEqual(string.Empty, m_Smart.Format("{Name:substr(999)}", m_People.First()));
        }

        [Test]
        public void StartPositionAndLengthLongerThanString()
        {
            Assert.AreEqual(string.Empty, m_Smart.Format("{Name:substr(999,1)}", m_People.First()));
        }

        [Test]
        public void OnlyPositiveStartPosition()
        {
            Assert.AreEqual("John", m_Smart.Format("{Name:substr(5)}", m_People.First()));
        }

        [Test]
        public void StartPositionAndPositiveLength()
        {
            Assert.AreEqual("New", m_Smart.Format("{City:substr(0,3)}", m_People.First()));
        }

        [Test]
        public void OnlyNegativeStartPosition()
        {
            Assert.AreEqual("John", m_Smart.Format("{Name:substr(-4)}", m_People.First()));
        }

        [Test]
        public void NegativeStartPositionAndPositiveLength()
        {
            Assert.AreEqual("Jo", m_Smart.Format("{Name:substr(-4, 2)}", m_People.First()));
        }

        [Test]
        public void NegativeStartPositionAndNegativeLength()
        {
            Assert.AreEqual("Joh", m_Smart.Format("{Name:substr(-4, -1)}", m_People.First()));
        }

        [Test]
        public void DataItemIsNull()
        {
            Assert.AreEqual(new SubStringFormatter().NullDisplayString, m_Smart.Format("{Name:substr(0,3)}", new Dictionary<string, string> { { "Name", null } }));
        }
    }
}
