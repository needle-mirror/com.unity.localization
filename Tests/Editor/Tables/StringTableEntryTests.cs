using NUnit.Framework;

namespace UnityEditor.Localization.Tests
{
    public class StringTableEntryTests : StringTableTestsBase
    {
        public class MyClass
        {
            public float SomeFloatValue { get; } = 123.5f;
            public int SomeIntValue { get; } = 321;
        }

        MyClass m_ArgumetClass = new MyClass();

        [Test]
        public void SmartFormatEntry_ClearsCacheWhenValueIsChanged()
        {
            const string valueBefore = "This is a test {SomeFloatValue}";
            const string valueBeforeFormatted = "This is a test 123.5";
            const string valueAfter = "Value has now changed to {SomeIntValue}, an integer.";
            const string valueAfterFormatted = "Value has now changed to 321, an integer.";

            var entry = Table.AddEntry("Smart Format Cache Test", valueBefore);
            entry.IsSmart = true;

            var formattedBefore = entry.GetLocalizedString(m_ArgumetClass);
            Assert.AreEqual(valueBeforeFormatted, formattedBefore);

            // Now change the value
            entry.Value = valueAfter;

            var formattedAfter = entry.GetLocalizedString(m_ArgumetClass);
            Assert.AreEqual(valueAfterFormatted, formattedAfter);
        }
    }
}
