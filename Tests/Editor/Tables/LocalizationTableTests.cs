using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization.Tables;
using Object = UnityEngine.Object;

namespace UnityEditor.Localization.Tests
{
    [TestFixture(typeof(StringTable), typeof(StringTableEntry))]
    [TestFixture(typeof(AssetTable), typeof(AssetTableEntry))]
    public class LocalizationTableTests<TTable, TEntry>
        where TEntry : TableEntry
        where TTable : DetailedLocalizationTable<TEntry>
    {
        TTable m_Table;
        SharedTableData m_SharedTableData;

        [SetUp]
        public void Init()
        {
            m_Table = ScriptableObject.CreateInstance<TTable>();
            m_Table.name = nameof(TTable);
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
        public void TableCollectionName_ThrowsExceptionWhenSharedTableDataIsNull()
        {
            m_Table.SharedData = null;
            Assert.Throws<NullReferenceException>(() => Assert.NotNull(m_Table.TableCollectionName));
        }

        [Test]
        public void TableCollectionName_MatchesSharedTableData()
        {
            const string tableName = "My Table 1";
            m_SharedTableData.TableCollectionName = tableName;
            Assert.AreEqual(tableName, m_Table.TableCollectionName, $"Expected table collection name to match the {nameof(SharedTableData)}.");
        }

        [Test]
        public void AddEntry_ThrowsExceptionWhenSharedTableDataIsNull()
        {
            m_Table.SharedData = null;
            Assert.Throws<NullReferenceException>(() => m_Table.AddEntry("test key", ""));
        }

        [Test]
        public void AddEntry_ThrowsExceptionWhenKeyIdZeroIsUsed()
        {
            Assert.Throws<ArgumentException>(() => m_Table.AddEntry(0, ""));
        }

        [Test]
        public void AddEntry_CreatesNewEntryIfItDoesNotExist_UsingKeyName()
        {
            int entryCountBefore = m_Table.Count;
            Assert.NotNull(m_Table.AddEntry("entry name", "entry value"), "Expected AddEntry to return the new entry but it did not.");
            Assert.Greater(m_Table.Count, entryCountBefore, "Expected table entry count to be greater after adding a new entry but it was not.");
        }

        [Test]
        public void AddEntry_CreatesNewEntryIfItDoesNotExist_UsingKey()
        {
            const long entryKey = 123;
            const string entryValue = "My Entry Value";

            int entryCountBefore = m_Table.Count;
            var entry = m_Table.AddEntry(entryKey, entryValue);

            Assert.NotNull(entry, "Expected AddEntry to return the new entry but it did not.");
            Assert.AreEqual(entryKey, entry.Data.Id, "Expected new entry Id to match the one passed into AddEntry");
            Assert.AreEqual(entryValue, entry.Data.Localized, "Expected new entry value to match the one passed into AddEntry.");
            Assert.Greater(m_Table.Count, entryCountBefore, "Expected table entry count to be greater after adding a new entry but it was not.");
        }

        [Test]
        public void AddEntry_CreatesNewEntryInSharedTableDataIfItDoesNotExist_UsingKeyName()
        {
            const string entryName = "My Test Entry";
            m_Table.AddEntry(entryName, "My Entry Value");
            Assert.NotNull(m_SharedTableData.GetEntry(entryName), "Expected entry to be created in Shared Table Data but it was not.");
        }

        [Test]
        public void AddEntry_DoesNotCreateNewEntryInSharedTableDataIfItDoesNotExist_UsingKey()
        {
            const long entryKey = 1112;
            m_Table.AddEntry(entryKey, "entry value");
            Assert.IsNull(m_SharedTableData.GetEntry(entryKey), "Expected entry to NOT created in Shared Table Data when using a Key.");
        }

        [Test]
        public void AddEntry_UsesExistingSharedTableId_UsingKeyName()
        {
            const string entryName = "My Test Entry";

            // Add several entries including the one we care about
            m_SharedTableData.AddKey("Some Value 1");
            m_SharedTableData.AddKey("Some Value 2");
            var sharedEntry = m_SharedTableData.AddKey(entryName);
            m_SharedTableData.AddKey("Some Value 3");
            m_SharedTableData.AddKey("Some Value 4");

            var newEntry = m_Table.AddEntry(entryName, "entry value");
            Assert.AreEqual(sharedEntry.Id, newEntry.Data.Id, "Expected the new entry to use the correct key id from the Shared Table Data.");
        }

        [Test]
        public void AddEntry_ReplacesOldEntry_UsingKeyName()
        {
            const string entryName = "My Test Entry";
            const string entryNewValue = "My New Value";
            var entry = m_Table.AddEntry(entryName, "original value");

            int entryCount = m_Table.Count;
            var updatedEntry = m_Table.AddEntry(entryName, entryNewValue);
            Assert.AreSame(entry, updatedEntry, "Expected the same entry to be used.");
            Assert.AreEqual(entryNewValue, updatedEntry.Data.Localized, "Expected value to be updated.");
            Assert.AreEqual(entryCount, m_Table.Count, "Expected the entry count to remain the same.");
        }

        [Test]
        public void AddEntry_ReplacesOldEntry_UsingKey()
        {
            const long entryKey = 1111;
            const string entryNewValue = "My New Value";
            var entry = m_Table.AddEntry(entryKey, "original value");

            int entryCount = m_Table.Count;
            var updatedEntry = m_Table.AddEntry(entryKey, entryNewValue);
            Assert.AreSame(entry, updatedEntry, "Expected the same entry to be used.");
            Assert.AreEqual(entryCount, m_Table.Count, "Expected the entry count to remain the same.");
        }

        [Test]
        public void RemoveEntry_RemovesExistingEntry_UsingKey()
        {
            const long entryKey = 1111;
            var entry = m_Table.AddEntry(entryKey, "entry value");

            int beforeCount = m_Table.Count;
            Assert.IsTrue(m_Table.RemoveEntry(entryKey), "Expected RemoveEntry to return true");
            Assert.Less(m_Table.Count, beforeCount, "Expected the entry count to be less but it was not.");
            Assert.IsNull(m_Table.GetEntry(entryKey), "Expected the entry to be removed");
            Assert.AreEqual(SharedTableData.EmptyId, entry.Data.Id, "Expected the entry id to be updated when removed");
        }

        [Test]
        public void RemoveEntry_RemovesExistingEntry_UsingKeyName()
        {
            const string entryKeyName = "My Entry";
            var entry = m_Table.AddEntry(entryKeyName, "entry value");

            int beforeCount = m_Table.Count;
            Assert.IsTrue(m_Table.RemoveEntry(entryKeyName), "Expected RemoveEntry to return true");
            Assert.Less(m_Table.Count, beforeCount, "Expected the entry count to be less but it was not.");
            Assert.IsNull(m_Table.GetEntry(entryKeyName), "Expected the entry to be removed");
            Assert.AreEqual(SharedTableData.EmptyId, entry.Data.Id, "Expected the entry id to be updated when removed");
        }

        [Test]
        public void Remove_RemovesExistingEntry_UsingKey()
        {
            const long entryKey = 1111;
            var entry = m_Table.AddEntry(entryKey, "entry value");

            int beforeCount = m_Table.Count;
            Assert.IsTrue(m_Table.Remove(entryKey), "Expected Remove to return true");
            Assert.Less(m_Table.Count, beforeCount, "Expected the entry count to be less but it was not.");
            Assert.IsNull(m_Table.GetEntry(entryKey), "Expected the entry to be removed");
            Assert.AreEqual(SharedTableData.EmptyId, entry.Data.Id, "Expected the entry id to be updated when removed");
        }

        [Test]
        public void SettingEntryWithArrayAccessor_ThrowsExceptionWithEntryFromAnotherTable_UsingKey()
        {
            var otherTable = ScriptableObject.CreateInstance<TTable>();
            otherTable.SharedData = ScriptableObject.CreateInstance<SharedTableData>();
            var entry = otherTable.AddEntry("My Entry", "My Entry Value");
            Assert.Throws<ArgumentException>(() => m_Table[123] = entry);

            Object.DestroyImmediate(otherTable.SharedData);
            Object.DestroyImmediate(otherTable);
        }

        [Test]
        public void SettingEntryWithArrayAccessor_ThrowsExceptionWithEntryFromAnotherTable_UsingKeyName()
        {
            var otherTable = ScriptableObject.CreateInstance<TTable>();
            otherTable.SharedData = ScriptableObject.CreateInstance<SharedTableData>();
            var entry = otherTable.AddEntry("My Entry", "My Entry Value");
            Assert.Throws<ArgumentException>(() => m_Table["Entry Name"] = entry);

            Object.DestroyImmediate(otherTable.SharedData);
            Object.DestroyImmediate(otherTable);
        }

        [Test]
        public void Add_ThrowsExceptionWithEntryFromAnotherTable_UsingKey()
        {
            var otherTable = ScriptableObject.CreateInstance<TTable>();
            otherTable.SharedData = ScriptableObject.CreateInstance<SharedTableData>();
            var entry = otherTable.AddEntry("My Entry", "My Entry Value");
            Assert.Throws<ArgumentException>(() => m_Table.Add(123, entry));

            Object.DestroyImmediate(otherTable.SharedData);
            Object.DestroyImmediate(otherTable);
        }

        [Test]
        public void Add_ThrowsExceptionWithEntryFromAnotherTable_UsingKeyValuePair()
        {
            var otherTable = ScriptableObject.CreateInstance<TTable>();
            otherTable.SharedData = ScriptableObject.CreateInstance<SharedTableData>();
            var entry = otherTable.AddEntry("My Entry", "My Entry Value");
            Assert.Throws<ArgumentException>(() => m_Table.Add(new KeyValuePair<long, TEntry>(123, entry)));

            Object.DestroyImmediate(otherTable.SharedData);
            Object.DestroyImmediate(otherTable);
        }

        [Test]
        public void SettingEntryWithArrayAccessor_MovesEntry_UsingKey()
        {
            const long originalEntryId = 123;
            const long newEntryId = 321;

            var entry = m_Table.AddEntry(originalEntryId, "entry value");

            m_Table[newEntryId] = entry;
            Assert.AreEqual(newEntryId, entry.Data.Id, "Expected entry id to be updated when it was moved.");
            Assert.IsNull(m_Table.GetEntry(originalEntryId), "Expected no entry to exist for the original id value yet one does.");
            Assert.AreSame(entry, m_Table.GetEntry(newEntryId), "Expected entry to be returned for the new Key Id but it was not.");
        }

        [Test]
        public void SettingEntryWithArrayAccessor_MovesEntry_UsingKeyName()
        {
            const string originalEntryName = "Original Entry Name";
            const string newEntryName = "New Entry Name";

            var entry = m_Table.AddEntry(originalEntryName, "entry value");

            m_Table[newEntryName] = entry;
            Assert.IsNull(m_Table.GetEntry(originalEntryName), "Expected no entry to exist for the original id value yet one does.");
            Assert.AreSame(entry, m_Table.GetEntry(newEntryName), "Expected entry to be returned for the new Key Id but it was not.");

            var newId = m_SharedTableData.GetId(newEntryName);
            Assert.AreEqual(newId, entry.Data.Id, "Expected entry id value to be updated when entry was moved.");
        }

        [Test]
        public void SettingEntryWithArrayAccessor_AcceptsEntryCreatedUsingCreateTableEntry_UsingKey()
        {
            const long entryKey = 123;
            var newEntry = m_Table.CreateTableEntry();
            m_Table[entryKey] = newEntry;

            Assert.AreEqual(entryKey, newEntry.Data.Id, "Expected Key Id to be set.");
            Assert.AreSame(newEntry, m_Table.GetEntry(entryKey), "Expected entry to be found.");
        }

        [Test]
        public void SettingEntryWithArrayAccessor_AcceptsEntryCreatedUsingCreateTableEntry_UsingKeyName()
        {
            const string entryKeyName = "My Key Entry";
            var newEntry = m_Table.CreateTableEntry();
            m_Table[entryKeyName] = newEntry;

            Assert.AreSame(newEntry, m_Table.GetEntry(entryKeyName), "Expected entry to be found.");
        }

        [Test]
        public void ContainsKey_ReturnsTrueIfTheKeyExistsInTheTable()
        {
            const long entryKey = 123456;
            m_Table.AddEntry(entryKey, "entry value");
            Assert.IsTrue(m_Table.ContainsKey(entryKey), "Expected ContainsKey to be true when the value exists in the table.");
        }

        [Test]
        public void ContainsKey_ReturnsFalseIfTheKeyDoesNotExistInTheTable()
        {
            const long entryKey = 123456;
            Assert.IsFalse(m_Table.ContainsKey(entryKey), "Expected ContainsKey to be false when the value does not exist in the table.");
        }

        TEntry[] CreateMissingEntries()
        {
            var entry1 = m_Table.AddEntry("Missing Entry 1", "Missing Entry 1 Value");
            var entry2 = m_Table.AddEntry("Missing Entry 2", "Missing Entry 2 Value");
            var entry3 = m_Table.AddEntry("Missing Entry 3", "Missing Entry 3 Value");

            // Now remove the entries from the shared table data
            m_SharedTableData.RemoveKey(entry1.KeyId);
            m_SharedTableData.RemoveKey(entry2.KeyId);
            m_SharedTableData.RemoveKey(entry3.KeyId);

            return new TEntry[] { entry1, entry2, entry3 };
        }

        [Test]
        public void CheckForMissingSharedTableDataEntries_FindsMissingEntries()
        {
            var missingEntries = CreateMissingEntries();

            // Copy the ids as they are changed if the item is removed from the table.
            var missingEntryIds = missingEntries.Select(e => e.KeyId).ToArray();

            var foundEntries = m_Table.CheckForMissingSharedTableDataEntries(MissingEntryAction.Nothing);

            Assert.AreEqual(missingEntries.Length, foundEntries.Count);
            for (int i = 0; i < foundEntries.Count; ++i)
            {
                Assert.Contains(foundEntries[i], missingEntries);
                Assert.AreEqual(missingEntryIds[i], missingEntries[i].KeyId, "Expected entry Id value to not change.");
                Assert.IsFalse(m_SharedTableData.Contains(missingEntryIds[i]), "Expected missing entry to not be added to shared table data when MissingEntryAction.Nothing is used.");
                Assert.IsTrue(m_Table.ContainsKey(missingEntryIds[i]), "Expected missing entry to still be part of table when MissingEntryAction.Nothing is used.");
            }
        }

        [Test]
        public void CheckForMissingSharedTableDataEntries_MissingEntryAction_AddEntriesToSharedData()
        {
            var missingEntries = CreateMissingEntries();

            // Copy the ids as they are changed if the item is removed from the table.
            var missingEntryIds = missingEntries.Select(e => e.KeyId).ToArray();

            var foundEntries = m_Table.CheckForMissingSharedTableDataEntries(MissingEntryAction.AddEntriesToSharedData);

            Assert.AreEqual(missingEntries.Length, foundEntries.Count);
            for (int i = 0; i < foundEntries.Count; ++i)
            {
                Assert.Contains(foundEntries[i], missingEntries);
                Assert.AreEqual(missingEntryIds[i], missingEntries[i].KeyId, "Expected entry Id value to not change.");
                Assert.IsTrue(m_SharedTableData.Contains(missingEntryIds[i]), "Expected missing entry to be added to shared table data when MissingEntryAction.AddEntriesToSharedData is used.");
                Assert.IsTrue(m_Table.ContainsKey(missingEntryIds[i]), "Expected missing entry to still be part of table when MissingEntryAction.AddEntriesToSharedData is used.");
            }
        }

        [Test]
        public void CheckForMissingSharedTableDataEntries_MissingEntryAction_RemoveEntriesFromTable()
        {
            var missingEntries = CreateMissingEntries();

            // Copy the ids as they are changed if the item is removed from the table.
            var missingEntryIds = missingEntries.Select(e => e.KeyId).ToArray();

            var foundEntries = m_Table.CheckForMissingSharedTableDataEntries(MissingEntryAction.RemoveEntriesFromTable);

            Assert.AreEqual(missingEntries.Length, foundEntries.Count);
            for (int i = 0; i < foundEntries.Count; ++i)
            {
                Assert.Contains(foundEntries[i], missingEntries);
                Assert.AreEqual(SharedTableData.EmptyId, missingEntries[i].KeyId, "Expected entry Id value to be set to 0 when removed from table.");
                Assert.IsFalse(m_SharedTableData.Contains(missingEntryIds[i]), "Expected missing entry to be added to shared table data when MissingEntryAction.RemoveEntriesFromTable is used.");
                Assert.IsFalse(m_Table.ContainsKey(missingEntryIds[i]), "Expected missing entry to still be removed from the table when MissingEntryAction.RemoveEntriesFromTable is used.");
            }
        }
    }
}
