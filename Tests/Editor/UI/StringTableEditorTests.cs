using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization;

namespace UnityEditor.Localization.Tests.UI
{
    public class StringTableEditorTests
    {
        StringTable m_Table;
        StringTableEntry m_TableEntry;
        int m_DirtyCount;

        [SetUp]
        public void Setup()
        {
            m_Table = ScriptableObject.CreateInstance<StringTable>();
            m_Table.Keys = ScriptableObject.CreateInstance<KeyDatabase>();
            m_TableEntry = m_Table.AddEntry("Test Entry");
            m_DirtyCount = EditorUtility.GetDirtyCount(m_Table);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(m_Table.Keys);
            Object.DestroyImmediate(m_Table);
        }

        [Test]
        public void TranslatedTextCallback_SetsTableDirty()
        {
            StringTableEditor.TranslatedTextChangedCallback(m_Table, m_TableEntry, "New Value");
            var newDirtyCount = EditorUtility.GetDirtyCount(m_Table);
            Assert.Greater(newDirtyCount, m_DirtyCount, "Expected the table to be set dirty when the translated text was changed.");
        }

        [Test]
        public void TranslatedPluralTextChangedCallback_SetsTableDirty()
        {
            StringTableEditor.TranslatedPluralTextChangedCallback(m_Table, m_TableEntry, 0, "New Value");
            var newDirtyCount = EditorUtility.GetDirtyCount(m_Table);
            Assert.Greater(newDirtyCount, m_DirtyCount, "Expected the table to be set dirty when the translated plural text was changed.");
        }
    }
}
