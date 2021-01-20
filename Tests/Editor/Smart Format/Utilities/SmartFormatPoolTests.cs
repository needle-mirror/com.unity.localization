using System.Collections.Generic;
using NUnit.Framework;

namespace UnityEngine.Localization.SmartFormat.Tests.Utilities
{
    public class SmartFormatPoolTests
    {
        SmartFormatter m_SmartFormatter;

        [OneTimeSetUp]
        public void Setup()
        {
            ClearPools();
            m_SmartFormatter =  Smart.CreateDefaultSmartFormat();
        }

        static void ClearPools()
        {
            FormatCachePool.s_Pool.Clear();
            FormatDetailsPool.s_Pool.Clear();
            FormattingInfoPool.s_Pool.Clear();
            ParsingErrorsPool.s_Pool.Clear();
            SplitListPool.s_Pool.Clear();
            StringOutputPool.s_Pool.Clear();
            FormatItemPool.s_LiteralTextPool.Clear();
            FormatItemPool.s_FormatPool.Clear();
            FormatItemPool.s_PlaceholderPool.Clear();
            FormatItemPool.s_SelectorPool.Clear();
            StringBuilderPool.s_Pool.Clear();
        }

        static void NoActivePoolItems()
        {
            Assert.AreEqual(FormatCachePool.s_Pool.CountAll, FormatCachePool.s_Pool.CountInactive, $"{FormatCachePool.s_Pool.CountActive} instances were not returned to the FormatCachePool");
            Assert.AreEqual(FormatDetailsPool.s_Pool.CountAll, FormatDetailsPool.s_Pool.CountInactive, $"{FormatDetailsPool.s_Pool.CountActive} instances were not returned to the FormatDetailsPool");
            Assert.AreEqual(FormattingInfoPool.s_Pool.CountAll, FormattingInfoPool.s_Pool.CountInactive, $"{FormattingInfoPool.s_Pool.CountActive} instances were not returned to the FormattingInfoPool");
            Assert.AreEqual(ParsingErrorsPool.s_Pool.CountAll, ParsingErrorsPool.s_Pool.CountInactive, $"{ParsingErrorsPool.s_Pool.CountActive} instances were not returned to the ParsingErrorsPool");
            Assert.AreEqual(SplitListPool.s_Pool.CountAll, SplitListPool.s_Pool.CountInactive, $"{SplitListPool.s_Pool.CountActive} instances were not returned to the SplitListPool");
            Assert.AreEqual(StringOutputPool.s_Pool.CountAll, StringOutputPool.s_Pool.CountInactive, $"{StringOutputPool.s_Pool.CountActive} instances were not returned to the StringOutputPool");

            Assert.AreEqual(FormatItemPool.s_LiteralTextPool.CountAll, FormatItemPool.s_LiteralTextPool.CountInactive, $"{FormatItemPool.s_LiteralTextPool.CountActive} instances of LiteralText were not returned to the FormatItemPool");
            Assert.AreEqual(FormatItemPool.s_FormatPool.CountAll, FormatItemPool.s_FormatPool.CountInactive, $"{FormatItemPool.s_FormatPool.CountActive} instances of Format were not returned to the FormatItemPool");
            Assert.AreEqual(FormatItemPool.s_PlaceholderPool.CountAll, FormatItemPool.s_PlaceholderPool.CountInactive, $"{FormatItemPool.s_PlaceholderPool.CountActive} instances of Placeholder were not returned to the FormatItemPool");
            Assert.AreEqual(FormatItemPool.s_SelectorPool.CountAll, FormatItemPool.s_SelectorPool.CountInactive, $"{FormatItemPool.s_SelectorPool.CountActive} instances of Selector were not returned to the FormatItemPool");

            Assert.AreEqual(StringBuilderPool.s_Pool.CountAll, StringBuilderPool.s_Pool.CountInactive, $"{StringBuilderPool.s_Pool.CountActive} instances were not returned to the StringBuilderPool");
        }

        void FormatAndCheckPools(string format, string expected, params object[] args)
        {
            var result = m_SmartFormatter.Format(format, args);
            Assert.AreEqual(expected, result);
            NoActivePoolItems();
        }

        [Test]
        public void DefaultFormatSource_AllPooledItemsAreReturnedAfterUse()
        {
            FormatAndCheckPools("DefaultFormat Test {0}", "DefaultFormat Test Item", "Item");
        }

        [Test]
        public void ListSource_AllPooledItemsAreReturnedAfterUse()
        {
            FormatAndCheckPools("List Test {0:list:{}|, |, and }.", "List Test One, Two, and Three.", new List<string> { "One", "Two", "Three" });
        }

        [Test]
        public void ReflectionSource_AllPooledItemsAreReturnedAfterUse()
        {
            var color = Color.yellow;
            FormatAndCheckPools("Red: {r} Green: {g} Blue: {b}", "Red: 1 Green: 0.9215686 Blue: 0.01568628", color);
        }

        [Test]
        public void DictionarySource_AllPooledItemsAreReturnedAfterUse()
        {
            var dict = new Dictionary<string, string>();
            dict["A"] = "One";
            dict["B"] = "Two";
            dict["Item_3"] = "Three";
            FormatAndCheckPools("1:{A} 2:{B} 3:{Item_3}", "1:One 2:Two 3:Three", dict);
        }

        [Test]
        public void PluralFormatter__AllPooledItemsAreReturnedAfterUse()
        {
            FormatAndCheckPools("There {0:plural:is 1 item|are {} items}.", "There are 2 items.", 2);
        }

        [Test]
        public void ConditionalFormatter_AllPooledItemsAreReturnedAfterUse()
        {
            FormatAndCheckPools("Enabled? {0:Yes|No}", "Enabled? Yes", true);
            FormatAndCheckPools("{0:default|null or empty}", "null or empty", null);
        }

        [Test]
        public void ChooseFormatter_AllPooledItemsAreReturnedAfterUse()
        {
            FormatAndCheckPools("{0:choose(1|2|3):one|two|three|other}", "two", 2);
        }

        [Test]
        public void SubStringFormatter_AllPooledItemsAreReturnedAfterUse()
        {
            FormatAndCheckPools("{0:substr(5)}", "", "World");
        }
    }
}
