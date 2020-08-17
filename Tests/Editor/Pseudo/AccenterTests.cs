using NUnit.Framework;
using UnityEngine.Localization.Pseudo;

namespace UnityEditor.Localization.Tests.Pseudo
{
    public class AccenterTests
    {
        Accenter m_Method;

        [OneTimeSetUp]
        public void Setup()
        {
            m_Method = new Accenter();
        }

        [TestCase("", "")]
        [TestCase("ABC", "ÅƁÇ")]
        [TestCase("Hello World", "Ĥéļļö Ŵöŕļð")]
        [TestCase("I have 123 Apples!", "Î ĥåṽé ①②③ Åþþļéš¡")]
        [TestCase("SoME SyMbOlS ~{}-=@£$%^&*", "ŠöṀÉ ŠýṀƀÖļŠ ˞{}‐≂՞£€‰˄⅋⁎")]
        public void CharactersAreAccented(string input, string expected)
        {
            var message = Message.CreateMessage(input);
            m_Method.Transform(message);

            Assert.AreEqual(expected, message.ToString(), "Expected the transformed string to match.");
            message.Release();
        }
    }
}
