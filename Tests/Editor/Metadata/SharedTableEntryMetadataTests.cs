using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.Tests.Metadata
{
    [System.Serializable]
    class TestTag : SharedTableEntryMetadata {}

    [TestFixture(typeof(StringTable), typeof(StringTableEntry))]
    [TestFixture(typeof(AssetTable), typeof(AssetTableEntry))]
    public class SharedTableEntryMetadataTests<TTable, TEntry>
        where TEntry : TableEntry
        where TTable : DetailedLocalizationTable<TEntry>
    {
        TTable m_Table;
        SharedTableData m_SharedTableData;

        [SetUp]
        public void Init()
        {
            m_Table = ScriptableObject.CreateInstance<TTable>();
            m_Table.name = $"SharedTableEntryMetadataTests - {nameof(TTable)}";
            m_SharedTableData = ScriptableObject.CreateInstance<SharedTableData>();
            m_SharedTableData.name = "Shared " + m_Table.name;
            m_Table.SharedData = m_SharedTableData;
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(m_Table);
            Object.DestroyImmediate(m_SharedTableData);
        }

        [Test]
        public void AddingTagToEntry_AddsTagToTableAndEntryMetdata()
        {
            Assert.That(m_Table.MetadataEntries, Has.Count.EqualTo(0), "Expected no table metadata to exist when just created.");
            Assert.That(m_Table.SharedData.Metadata.MetadataEntries, Has.Count.EqualTo(0), "Expected no shared metadata to exist when just created.");

            var entry = m_Table.AddEntry("Entry 1", string.Empty);
            Assert.False(entry.HasTagMetadata<TestTag>(), "Expected HasTagMetadata to return false when no tag has been added.");

            entry.AddTagMetadata<TestTag>();

            Assert.That(m_Table.MetadataEntries, Has.Count.EqualTo(1), "Expected table to contain the tag metadata but it did not.");
            Assert.That(m_Table.SharedData.Metadata.MetadataEntries, Has.Count.EqualTo(0), "Expected shared metadata to not be modified when adding a tag.");
            Assert.True(entry.HasTagMetadata<TestTag>(), "Expected HasTagMetadata to return true for the new tag but it did not.");

            var tag = m_Table.MetadataEntries[0] as TestTag;
            Assert.NotNull(tag);

            Assert.True(tag.IsRegistered(entry), "Expected entry id to be registered with the tag");
            Assert.AreSame(tag, entry.MetadataEntries[0], "Expected the entry metadata to contain the tag.");
        }

        [Test]
        public void AddingTagToEntry_AppendsToExistingTag()
        {
            var entry1 = m_Table.AddEntry("Entry 1", string.Empty);
            var entry2 = m_Table.AddEntry("Entry 2", string.Empty);
            entry1.AddTagMetadata<TestTag>();
            entry2.AddTagMetadata<TestTag>();

            Assert.That(m_Table.MetadataEntries, Has.Count.EqualTo(1), "Expected table to contain 1 tag metadata but it did not.");
            Assert.That(m_Table.SharedData.Metadata.MetadataEntries, Has.Count.EqualTo(0), "Expected shared metadata to not be modified when adding a tag.");

            Assert.True(entry1.HasTagMetadata<TestTag>(), "Expected HasTagMetadata to return true for entry 1.");
            Assert.True(entry2.HasTagMetadata<TestTag>(), "Expected HasTagMetadata to return true for entry 2.");

            var tag = m_Table.MetadataEntries[0] as TestTag;
            Assert.NotNull(tag);

            Assert.True(tag.IsRegistered(entry1), "Expected entry 1 id to be registered with the tag");
            Assert.True(tag.IsRegistered(entry2), "Expected entry 2 id to be registered with the tag");

            Assert.AreSame(tag, entry1.MetadataEntries[0], "Expected the entry 1 metadata to contain the same tag.");
            Assert.AreSame(tag, entry2.MetadataEntries[0], "Expected the entry 2 metadata to contain the same tag.");
        }

        [Test]
        public void RemovingTagFromEntry_RemovesTagFromTableAndEntry_WhenNoOtherEntriesUseTheTag()
        {
            var entry = m_Table.AddEntry("Entry 2", string.Empty);
            entry.AddTagMetadata<TestTag>();

            var tag = m_Table.MetadataEntries[0] as TestTag;
            Assert.NotNull(tag);
            Assert.True(tag.IsRegistered(entry), "Expected entry id to be registered with the tag");

            entry.RemoveTagMetadata<TestTag>();

            Assert.That(m_Table.MetadataEntries, Has.Count.EqualTo(0), "Expected no table metadata to exist when tag was removed.");
            Assert.That(entry.MetadataEntries, Has.Count.EqualTo(0), "Expected entry metadata to exist when tag was removed.");
            Assert.False(tag.IsRegistered(entry), "Expected IsRegistered to be false when tag was removed");
            Assert.False(entry.HasTagMetadata<TestTag>(), "Expected HasTagMetadata to return false when tag was removed.");
        }

        [Test]
        public void RemovingTagFromEntry_PreservesTag_WhenOtherEntriesUseIt()
        {
            var entry1 = m_Table.AddEntry("Entry 1", string.Empty);
            var entry2 = m_Table.AddEntry("Entry 2", string.Empty);
            entry1.AddTagMetadata<TestTag>();
            entry2.AddTagMetadata<TestTag>();

            entry2.RemoveTagMetadata<TestTag>();

            Assert.That(m_Table.MetadataEntries, Has.Count.EqualTo(1), "Expected table to contain 1 tag metadata but it did not.");
            Assert.That(m_Table.SharedData.Metadata.MetadataEntries, Has.Count.EqualTo(0), "Expected shared metadata to not be modified when adding a tag.");

            Assert.True(entry1.HasTagMetadata<TestTag>(), "Expected HasTagMetadata to return true for entry 1.");
            Assert.False(entry2.HasTagMetadata<TestTag>(), "Expected HasTagMetadata to return false for entry 2.");

            var tag = m_Table.MetadataEntries[0] as TestTag;
            Assert.NotNull(tag);

            Assert.True(tag.IsRegistered(entry1), "Expected entry 1 id to be registered with the tag");
            Assert.False(tag.IsRegistered(entry2), "Expected entry 2 id to not be registered with the tag");
        }

        [Test]
        public void RemovingTagFromEntry_RemovesAllTags_WhenNoOtherEntriesUseTheTag()
        {
            var entry = m_Table.AddEntry("Entry 2", string.Empty);
            entry.AddTagMetadata<TestTag>();

            // Add some errant tags
            m_Table.MetadataEntries.Add(new TestTag());
            m_Table.MetadataEntries.Add(new TestTag());

            var tag = m_Table.MetadataEntries[0] as TestTag;
            Assert.NotNull(tag);
            Assert.True(tag.IsRegistered(entry), "Expected entry id to be registered with the tag");

            entry.RemoveTagMetadata<TestTag>();

            Assert.That(m_Table.MetadataEntries, Has.Count.EqualTo(0), "Expected no table metadata to exist when tag was removed.");
            Assert.That(entry.MetadataEntries, Has.Count.EqualTo(0), "Expected entry metadata to exist when tag was removed.");
            Assert.False(tag.IsRegistered(entry), "Expected IsRegistered to be false when tag was removed");
            Assert.False(entry.HasTagMetadata<TestTag>(), "Expected HasTagMetadata to return false when tag was removed.");
        }
    }
}
