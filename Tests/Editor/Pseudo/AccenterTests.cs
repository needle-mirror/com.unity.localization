using NUnit.Framework;
using UnityEngine.Localization.Pseudo;

namespace UnityEngine.Localization.Tests.Pseudo
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
            var result = m_Method.Transform(input);
            Assert.AreEqual(expected, result, "Expected the transformed string to match.");
        }
    }
}
