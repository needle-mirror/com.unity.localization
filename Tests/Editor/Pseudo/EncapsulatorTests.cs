using NUnit.Framework;
using UnityEngine.Localization.Pseudo;

namespace UnityEditor.Localization.Tests.Pseudo
{
    public class EncapsulatorTests
    {
        Encapsulator m_Method;

        const string k_Start = "{";
        const string k_End = "}";

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
            m_Method = new Encapsulator() { Start = k_Start, End = k_End };
        }

        [TestCaseSource("TestCases")]
        public void StringIsEncapsulated(string input)
        {
            var message = Message.CreateMessage(input);
            m_Method.Transform(message);
            var result = message.ToString();
            Assert.IsTrue(result.StartsWith(k_Start), "Expected string to have the start string at the start.");
            Assert.IsTrue(result.EndsWith(k_End), "Expected the string to have the end string at the end.");

            int expectedLngth = k_End.Length + k_Start.Length + input.Length;
            Assert.AreEqual(expectedLngth, result.Length, "Expected the length of the string to be the sum of end string, start string and input.");
            message.Release();
        }
    }
}
