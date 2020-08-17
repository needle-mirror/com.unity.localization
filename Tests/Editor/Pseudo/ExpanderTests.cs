using System.Linq;
using NUnit.Framework;
using UnityEngine.Localization.Pseudo;

namespace UnityEditor.Localization.Tests.Pseudo
{
    public class ExpanderTests
    {
        Expander m_Method;

        public static string[] TestCases()
        {
            return new string[]
            {
                "",
                "Hello World",
                "This is some longer text that\ngoes over more than 1 line.",
                "Test with some symbols *&^%$£@",
                "A",
            };
        }

        [SetUp]
        public void Setup()
        {
            m_Method = new Expander();
        }

        [TestCaseSource("TestCases")]
        public void Length_IsIncreased(string input)
        {
            var message = Message.CreateMessage(input);
            m_Method.Transform(message);
            Assert.Greater(message.ToString().Length, input.Length, "Expected the string to be increased in length but it was not.");
            message.Release();
        }

        [TestCaseSource("TestCases")]
        public void Length_DoesNotChange_WhenExpandAmountIsNegative(string input)
        {
            m_Method.SetConstantExpansion(-1);
            var message = Message.CreateMessage(input);
            m_Method.Transform(message);
            Assert.AreEqual(input.Length, message.ToString().Length, "Expected the length to not change when using a negative value.");
            message.Release();
        }

        [TestCaseSource("TestCases")]
        public void PaddingIsAddedToStart_WhenPseudoCharactersLocation_Front(string input)
        {
            m_Method.Location = Expander.InsertLocation.Start;

            var message = Message.CreateMessage(input);
            m_Method.Transform(message);
            Assert.IsTrue(message.ToString().EndsWith(input), "Expected the result to end with the input string when adding padding at the start");
            message.Release();
        }

        [TestCaseSource("TestCases")]
        public void PaddingIsAddedToEnd_WhenPseudoCharactersLocation_Back(string input)
        {
            m_Method.Location = Expander.InsertLocation.End;

            var message = Message.CreateMessage(input);
            m_Method.Transform(message);
            Assert.IsTrue(message.ToString().StartsWith(input), "Expected the result to start with the input string when adding padding at the end");
            message.Release();
        }

        [Test]
        public void PaddingIsSplit_WhenPseudoCharactersLocation_Both()
        {
            const string testString = "TEST";
            m_Method.Location = Expander.InsertLocation.Both;

            var message = Message.CreateMessage(testString);
            m_Method.Transform(message);

            var result = message.ToString();
            Assert.IsFalse(result.StartsWith(testString), "Expected the result to not end with the input string when adding padding is at both ends.");
            Assert.IsFalse(result.EndsWith(testString), "Expected the result to not end with the input string when adding padding is at both ends.");
            Assert.IsTrue(result.Contains(testString), "Expected the result to contain the original string when adding padding at both ends.");
            message.Release();
        }

        [TestCaseSource("TestCases")]
        public void PaddingCharactersAreTheSame_WhenASingleCharacterIsUsed(string input)
        {
            const char character = 'X';
            m_Method.PaddingCharacters.Clear();
            m_Method.PaddingCharacters.Add(character);
            m_Method.MinimumStringLength = 10;

            var message = Message.CreateMessage(input);
            m_Method.Transform(message);
            var result = message.ToString();

            var insertedCharacters = result.Count(o => o == character);
            var expected = result.Length - input.Length;
            Assert.AreEqual(expected, insertedCharacters, "Expected all the inserted characters to be the same");
            message.Release();
        }

        [TestCaseSource("TestCases")]
        public void SameRandomCharactersAreUsedEachTime(string input)
        {
            var message1 = Message.CreateMessage(input);
            var message2 = Message.CreateMessage(input);

            m_Method.Transform(message1);
            m_Method.Transform(message2);
            Assert.AreEqual(message1.ToString(), message1.ToString(), "Expected the same pseudo random string to be generated each time.");
            message1.Release();
            message2.Release();
        }

        [TestCase(5, 1.0f)]
        [TestCase(9, 1.0f)]
        [TestCase(10, 2.0f)]
        [TestCase(15, 2.0f)]
        [TestCase(21, 3.0f)]
        [TestCase(50, 5.0f)]
        [TestCase(3000, 5.0f)]
        public void CorrectExpansionRuleIsUsed(int stringLength, float expectedExpansion)
        {
            var method = new Expander();
            method.ExpansionRules.Clear();
            method.ExpansionRules.AddRange(new[]
            {
                new Expander.ExpansionRule(0, 10, 1),
                new Expander.ExpansionRule(10, 20, 2),
                new Expander.ExpansionRule(20, 30, 3),
                new Expander.ExpansionRule(30, int.MaxValue, 5)
            });

            Assert.AreEqual(expectedExpansion, method.GetExpansionForLength(stringLength), "Wrong expansion value returned");
        }
    }
}
