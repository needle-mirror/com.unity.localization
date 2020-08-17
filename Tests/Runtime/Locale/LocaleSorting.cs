using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.Localization.Pseudo;

namespace UnityEngine.Localization.Tests
{
    public class LocaleSorting
    {
        Locale m_Arabic;
        Locale m_Basque;
        Locale m_Catalan;
        Locale m_French;
        PseudoLocale m_Pseudo;

        [SetUp]
        public void Setup()
        {
            m_Arabic = Locale.CreateLocale(SystemLanguage.Arabic);
            m_Basque = Locale.CreateLocale(SystemLanguage.Basque);
            m_Catalan = Locale.CreateLocale(SystemLanguage.Catalan);
            m_French = Locale.CreateLocale(SystemLanguage.French);
            m_Pseudo = PseudoLocale.CreatePseudoLocale();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(m_Arabic);
            Object.DestroyImmediate(m_Basque);
            Object.DestroyImmediate(m_Catalan);
            Object.DestroyImmediate(m_French);
            Object.DestroyImmediate(m_Pseudo);
        }

        [Test]
        public void LocalesWithDifferentSortOrder_AreSortedBySortOrder()
        {
            var localesList = new List<Locale> { m_Arabic, m_Basque, m_Catalan, m_French, m_Pseudo };

            // Expected order
            m_Basque.SortOrder = 0;
            m_Catalan.SortOrder = 1;
            m_Arabic.SortOrder = 2;
            m_Pseudo.SortOrder = 3;
            m_French.SortOrder = 4;

            localesList.Sort();

            Assert.AreSame(m_Basque, localesList[0], "Expected item at index 0 to match");
            Assert.AreSame(m_Catalan, localesList[1], "Expected item at index 1 to match");
            Assert.AreSame(m_Arabic, localesList[2], "Expected item at index 2 to match");
            Assert.AreSame(m_Pseudo, localesList[3], "Expected item at index 3 to match");
            Assert.AreSame(m_French, localesList[4], "Expected item at index 4 to match");
        }

        [Test]
        public void LocalesWithMatchingSortOrder_AreSortedByName()
        {
            var localesList = new List<Locale> { m_Basque, m_Arabic, m_French, m_Catalan };

            // Expected order
            m_Arabic.SortOrder = 1;
            m_Basque.SortOrder = 1;
            m_Catalan.SortOrder = 1;
            m_French.SortOrder = 1;

            localesList.Sort();

            Assert.AreSame(m_Arabic, localesList[0], "Expected item at index 0 to match");
            Assert.AreSame(m_Basque, localesList[1], "Expected item at index 1 to match");
            Assert.AreSame(m_Catalan, localesList[2], "Expected item at index 2 to match");
            Assert.AreSame(m_French, localesList[3], "Expected item at index 3 to match");
        }

        [Test]
        public void PseudoLocalesAreSortedAfterLocalesWhenSortOrderIsTheSame()
        {
            var localesList = new List<Locale> { m_Pseudo, m_Basque, m_Arabic, m_French, m_Catalan };

            // Expected order
            m_Arabic.SortOrder = 1;
            m_Basque.SortOrder = 1;
            m_Catalan.SortOrder = 1;
            m_French.SortOrder = 1;
            m_Pseudo.SortOrder = 1;

            localesList.Sort();

            Assert.AreSame(m_Arabic, localesList[0], "Expected item at index 0 to match");
            Assert.AreSame(m_Basque, localesList[1], "Expected item at index 1 to match");
            Assert.AreSame(m_Catalan, localesList[2], "Expected item at index 2 to match");
            Assert.AreSame(m_French, localesList[3], "Expected item at index 3 to match");
            Assert.AreSame(m_Pseudo, localesList[4], "Expected item at index 4 to match");
        }

        [Test]
        public void SortingBySortOrderAndName()
        {
            var localesList = new List<Locale> { m_Basque, m_Arabic, m_French, m_Catalan, m_Pseudo };

            // Expected order
            m_Catalan.SortOrder = 0;
            m_Basque.SortOrder = 1;
            m_French.SortOrder = 1;
            m_Arabic.SortOrder = 2;
            m_Pseudo.SortOrder = 2;

            localesList.Sort();

            Assert.AreSame(m_Catalan, localesList[0], "Expected item at index 0 to match");
            Assert.AreSame(m_Basque, localesList[1], "Expected item at index 1 to match");
            Assert.AreSame(m_French, localesList[2], "Expected item at index 2 to match");
            Assert.AreSame(m_Arabic, localesList[3], "Expected item at index 3 to match");
            Assert.AreSame(m_Pseudo, localesList[4], "Expected item at index 4 to match");
        }
    }
}
