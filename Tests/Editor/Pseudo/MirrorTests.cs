using NUnit.Framework;
using UnityEngine.Localization.Pseudo;

namespace UnityEditor.Localization.Tests.Pseudo
{
    public class MirrorTests
    {
        Mirror m_Method;

        [OneTimeSetUp]
        public void Init()
        {
            m_Method = new Mirror();
        }

        [TestCase("ABC", "CBA")]
        [TestCase("A", "A")]
        [TestCase("", "")]
        [TestCase("12", "21")]
        [TestCase("12345 67890ABC", "CBA09876 54321")]
        [TestCase("+-@:", ":@-+")]
        public void SingleLineTextIsReversed(string input, string expected)
        {
            var message = Message.CreateMessage(input);
            m_Method.Transform(message);

            Assert.AreEqual(expected, message.ToString(), "Expected the strings to match");
            message.Release();
        }

        [TestCase("This is some\nmultiple\nLines of text", "emos si sihT\nelpitlum\ntxet fo seniL")]
        [TestCase("\nThis has a new line\nat\nthe\nstart\nand\nend\n", "\nenil wen a sah sihT\nta\neht\ntrats\ndna\ndne\n")]
        [TestCase("One Line\n", "eniL enO\n")]
        [TestCase("\nStart of Line", "\neniL fo tratS")]
        public void MultiLineTextIsReversedPerLine(string input, string expected)
        {
            var message = Message.CreateMessage(input);
            m_Method.Transform(message);

            Assert.AreEqual(expected, message.ToString(), "Expected the strings to match");
            message.Release();
        }
    }
}
