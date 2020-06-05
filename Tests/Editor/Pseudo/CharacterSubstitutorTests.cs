using System.Linq;
using NUnit.Framework;
using UnityEngine.Localization.Pseudo;

namespace UnityEngine.Localization.Tests.Pseudo
{
    public class CharacterSubstitutorTests
    {
        [TestCase("KARL", "karl")]
        [TestCase("HeLLo WorLd123", "hello world123")]
        [TestCase("lower?case#'-+", "lower?case#'-+")]
        public void Method_ToLower_CharactersAreLowercase(string input, string expected)
        {
            var method = new CharacterSubstitutor();
            method.Method = CharacterSubstitutor.SubstitutionMethod.ToLower;
            var result = method.Transform(input);
            Assert.AreEqual(expected, result, "Expected the transformed string to match.");
        }

        [TestCase("karl", "KARL")]
        [TestCase("hello world123", "HELLO WORLD123")]
        [TestCase("lower?case#'-+", "LOWER?CASE#'-+")]
        public void Method_ToUpper_CharactersAreUppercase(string input, string expected)
        {
            var method = new CharacterSubstitutor();
            method.Method = CharacterSubstitutor.SubstitutionMethod.ToUpper;
            var result = method.Transform(input);
            Assert.AreEqual(expected, result, "Expected the transformed string to match.");
        }

        [TestCase("Some text")]
        [TestCase("dfdsfdsfsdgf43tr43t43523")]
        [TestCase("_")]
        [TestCase("@:~{_+_D\"DDKWDKAWSNDKA54463576/-")]
        public void Method_List_CharactersAreAllReplacedWhenUsingSingleReplacementChar(string input)
        {
            const char replacementChar = '_';
            var method = new CharacterSubstitutor();
            method.Method = CharacterSubstitutor.SubstitutionMethod.List;
            method.ReplacementList.Add(replacementChar);
            var result = method.Transform(input);
            var count = result.Count(o => o == replacementChar);
            Assert.AreEqual(input.Length, count, "Expected all characters to be replaced with the same character when replacement chars only has a single value: " + result);
        }

        [TestCase("Some text", "ABCABCABC")]
        [TestCase("ABC", "ABC")]
        [TestCase("abc", "ABC")]
        [TestCase("_", "A")]
        [TestCase("1234", "ABCA")]
        public void Method_List_CharactersAreAllReplaced_WhenUsingLoopFromStart(string input, string expected)
        {
            var method = new CharacterSubstitutor();
            method.Method = CharacterSubstitutor.SubstitutionMethod.List;
            method.ListMode = CharacterSubstitutor.ListSelectionMethod.LoopFromStart;
            method.ReplacementList.Clear();
            method.ReplacementList.AddRange(new[] { 'A', 'B', 'C' });
            var result = method.Transform(input);
            Assert.AreEqual(expected, result, "Expected the transformed string to match.");
        }

        [Test]
        public void Method_List_CharactersAreAllReplaced_WhenUsingLoopFromPrevious()
        {
            var method = new CharacterSubstitutor();
            method.Method = CharacterSubstitutor.SubstitutionMethod.List;
            method.ListMode = CharacterSubstitutor.ListSelectionMethod.LoopFromPrevious;
            method.ReplacementList.Clear();
            method.ReplacementList.AddRange(new[] { 'A', 'B', 'C', 'D', 'E' });

            const string input1 = "aaa";
            const string input2 = "bbbb";
            const string expected1 = "ABC";
            const string expected2 = "DEAB";

            var result1 = method.Transform(input1);
            var result2 = method.Transform(input2);

            Assert.AreEqual(expected1, result1, "Expected the transformed string to match.");
            Assert.AreEqual(expected2, result2, "Expected the transformed string to match.");
        }

        [TestCase("KARL", "LRAK")]
        [TestCase("LRAK", "KARL")]
        [TestCase("FXX", "FYY")]
        [TestCase("{FXXK}", "[FYYL]")]
        [TestCase("", "")]
        [TestCase("123P~", "123P~")]
        public void Method_Map_CharactersAreReplaced(string input, string expected)
        {
            var method = new CharacterSubstitutor();
            method.Method = CharacterSubstitutor.SubstitutionMethod.Map;
            method.ReplacementMap['L'] = 'K';
            method.ReplacementMap['R'] = 'A';
            method.ReplacementMap['A'] = 'R';
            method.ReplacementMap['K'] = 'L';
            method.ReplacementMap['X'] = 'Y';
            method.ReplacementMap['{'] = '[';
            method.ReplacementMap['}'] = ']';

            var result = method.Transform(input);
            Assert.AreEqual(expected, result, "Expected the transformed string to match.");
        }
    }
}
