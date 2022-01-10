using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine.Localization.Pseudo;

namespace UnityEditor.Localization.Tests.Pseudo
{
    public class PreserveTagsTests
    {
        PreserveTags m_Method;

        [SetUp]
        public void Init()
        {
            m_Method = new PreserveTags();
        }

        [Test]
        public void RichTextTagsAreMarkedAsReadOnly()
        {
            var message = Message.CreateMessage("Hello <color=red>World</color>");
            m_Method.Transform(message);

            Assert.AreEqual(4, message.Fragments.Count, "Expected 3 fragments");
            Assert.AreEqual("Hello ", message.Fragments[0].ToString(), "Expected Fragment 0 to match");
            Assert.AreEqual(typeof(WritableMessageFragment), message.Fragments[0].GetType(), "Expected fragment 0 to be writable");

            Assert.AreEqual("<color=red>", message.Fragments[1].ToString(), "Expected Fragment 1 to match");
            Assert.AreEqual(typeof(ReadOnlyMessageFragment), message.Fragments[1].GetType(), "Expected fragment 1 to be readonly");

            Assert.AreEqual("World", message.Fragments[2].ToString(), "Expected Fragment 2 to match");
            Assert.AreEqual(typeof(WritableMessageFragment), message.Fragments[2].GetType(), "Expected fragment 2 to be writable");

            Assert.AreEqual("</color>", message.Fragments[3].ToString(), "Expected Fragment 3 to match");
            Assert.AreEqual(typeof(ReadOnlyMessageFragment), message.Fragments[3].GetType(), "Expected fragment 3 to be readonly");
            message.Release();
        }

        [Test]
        public void CustomTagsAreMarkedAsReadOnly()
        {
            m_Method.Opening = '{';
            m_Method.Closing = '}';

            var message = Message.CreateMessage("Some<xml> {Text}{here}");
            m_Method.Transform(message);

            Assert.AreEqual(3, message.Fragments.Count, "Expected 3 fragments");
            Assert.AreEqual("Some<xml> ", message.Fragments[0].ToString(), "Expected Fragment 0 to match");
            Assert.AreEqual(typeof(WritableMessageFragment), message.Fragments[0].GetType(), "Expected fragment 0 to be writable");

            Assert.AreEqual("{Text}", message.Fragments[1].ToString(), "Expected Fragment 1 to match");
            Assert.AreEqual(typeof(ReadOnlyMessageFragment), message.Fragments[1].GetType(), "Expected fragment 1 to be readonly");

            Assert.AreEqual("{here}", message.Fragments[2].ToString(), "Expected Fragment 2 to match");
            Assert.AreEqual(typeof(ReadOnlyMessageFragment), message.Fragments[2].GetType(), "Expected fragment 2 to be readonly");
            message.Release();
        }

        [TestCase("Hello World", "12345678912")]
        [TestCase("Hello <color=yellow> World</color> ", "123456<color=yellow>789123</color>4")]
        [TestCase("<color=yellow>Hello World</color>", "<color=yellow>12345678912</color>")]
        [TestCase("World</color>", "12345</color>")]
        [TestCase("<color>World", "<color>12345")]
        [TestCase("<<test>", "1<test>")]
        [TestCase("<<test>>", "1<test>2")]
        public void RichTextTagsArePreserved_WhenReplacingCharacters(string input, string expected)
        {
            // Replace each character with a wrapped number from 1-9
            var replaceMethod = new CharacterSubstitutor();
            replaceMethod.Method = CharacterSubstitutor.SubstitutionMethod.List;
            replaceMethod.ListMode = CharacterSubstitutor.ListSelectionMethod.LoopFromPrevious;
            replaceMethod.ReplacementList.Clear();
            replaceMethod.ReplacementList.AddRange(Enumerable.Range(1, 9).Select(v => v.ToString()[0]));

            var message = Message.CreateMessage(input);

            m_Method.Transform(message);
            replaceMethod.Transform(message);
            Assert.AreEqual(expected, message.ToString());
            message.Release();
        }

        [Test]
        [Description("OverflowException in pseudo locale when using multiple preserve tags (LOC-517)")]
        public static void TransformWithMultiplePreserveTags_DoesNotCorruptFragments()
        {
            const string input = "n ex[CONTROLS-Ffdsafdsa]amsf dsa";

            var preserve1 = new PreserveTags { Opening = '<', Closing = '>' };
            var preserve2 = new PreserveTags { Opening = '[', Closing = ']' };
            var preserve3 = new PreserveTags { Opening = '{', Closing = '}' };

            var message = Message.CreateMessage(input);

            preserve1.Transform(message);
            preserve2.Transform(message);
            preserve3.Transform(message);

            Assert.AreEqual(input, message.ToString());
        }
    }
}
