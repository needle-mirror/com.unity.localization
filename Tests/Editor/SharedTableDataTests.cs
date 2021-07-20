using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.Tests
{
    public class SharedTableDataTests
    {
        SharedTableData m_SharedTableData;

        [SetUp]
        public void Init()
        {
            m_SharedTableData = ScriptableObject.CreateInstance<SharedTableData>();

            for (int i = 0; i < 100; ++i)
            {
                m_SharedTableData.AddKey("My Key " + i);
            }
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(m_SharedTableData);
        }

        [TestCase("Unique Key 1")]
        [TestCase("Unique Key 2")]
        [TestCase("Unique Key 3")]
        [TestCase("{0} {1} ####--++></?")]
        public void AddAndVerifyKeyIsAdded(string keyName)
        {
            Assert.IsFalse(m_SharedTableData.Contains(keyName), "Expected the key to not already be in the shared table data.");
            Assert.IsNotNull(m_SharedTableData.AddKey(keyName), "Expected the key to added however it was not.");
            Assert.IsTrue(m_SharedTableData.Contains(keyName), "Expected the key to be contained in the  shared table data.");
        }

        long GetKeyIdAndVerifyItIsValid(string keyName)
        {
            var keyId = m_SharedTableData.GetId(keyName);
            Assert.IsTrue(keyId != SharedTableData.EmptyId, "Failed to find the id of the added key");
            return keyId;
        }

        [Test]
        public void All_KeyIds_AreUnique()
        {
            HashSet<long> usedKeys = new HashSet<long>();
            foreach (var entry in m_SharedTableData.Entries)
            {
                Assert.IsFalse(usedKeys.Contains(entry.Id), "Expected all key ids to be unique, however this key has already been used: " + entry.Id);
                usedKeys.Add(entry.Id);
            }
        }

        [Test]
        public void All_Keys_AreUnique()
        {
            HashSet<string> usedKeys = new HashSet<string>();
            foreach (var entry in m_SharedTableData.Entries)
            {
                Assert.IsFalse(usedKeys.Contains(entry.Key), "Expected all keys to be unique, however this key has already been used: " + entry.Key);
                usedKeys.Add(entry.Key);
            }
        }

        [Test]
        public void AddKey_AllowsRemovedKeyToBeReAdded()
        {
            const string keyName = "Remove And Add Key";
            AddAndVerifyKeyIsAdded(keyName);

            m_SharedTableData.RemoveKey(keyName);
            Assert.IsFalse(m_SharedTableData.Contains(keyName), "Expected the key to not be contained when it has been removed from the shared table data.");
            Assert.IsNotNull(m_SharedTableData.AddKey(keyName), "Expected the key to added again after being removed, however it was not.");
        }

        [Test]
        public void AddKey_GeneratesUniqueId_WhenNextIdValueHasAlreadyBeenUsed()
        {
            const int customId = 1234;

            // Create an entry with a custom Id
            Assert.Null(m_SharedTableData.GetEntry(customId), "Expected no Key to already exist with the Id");
            var entry = m_SharedTableData.AddKey("My Custom Entry", customId);
            Assert.NotNull(entry);

            // Start generating id values using the custom id as the next id.
            m_SharedTableData.KeyGenerator = new SequentialIDGenerator(customId);

            var currentEntryCount = m_SharedTableData.Entries.Count;
            var newEntry = m_SharedTableData.AddKey();
            Assert.AreNotEqual(customId, newEntry.Id, "Expected new entry to not use the custom Id");
            Assert.Greater(m_SharedTableData.Entries.Count, currentEntryCount, "Expected entry count to have increased");
        }

        [Test]
        public void AddKey_GeneratesUniqueKey_WhenNewEntryKeyHasAlreadyBeenUsed()
        {
            string expectedKey = $"{SharedTableData.NewEntryKey} 3";

            // Use some of the new Entry names
            m_SharedTableData.AddKey(SharedTableData.NewEntryKey);
            m_SharedTableData.AddKey($"{SharedTableData.NewEntryKey} 1");
            m_SharedTableData.AddKey($"{SharedTableData.NewEntryKey} 2");
            m_SharedTableData.AddKey($"{SharedTableData.NewEntryKey} 4");

            var entry = m_SharedTableData.AddKey();
            Assert.AreEqual(expectedKey, entry.Key);
        }

        [Test]
        public void KeyIsNotInSharedTableData_AfterRemovingKey_ByName()
        {
            const string keyName = "Remove Key By Name";
            AddAndVerifyKeyIsAdded(keyName);
            m_SharedTableData.RemoveKey(keyName);
            Assert.IsFalse(m_SharedTableData.Contains(keyName), "Expected the key to not be contained when it has been removed from the shared table data.");
        }

        [Test]
        public void KeyIsNotInSharedTableData_AfterRemovingKey_ById()
        {
            const string keyName = "Remove Key By Name";
            AddAndVerifyKeyIsAdded(keyName);
            var addedKeyId = GetKeyIdAndVerifyItIsValid(keyName);

            m_SharedTableData.RemoveKey(addedKeyId);
            Assert.IsFalse(m_SharedTableData.Contains(keyName), "Expected the key to not be contained when it has been removed from the shared table data.");
        }

        [Test]
        public void RemovingInvalidKeyDoesNotChangeKeyCount_ByName()
        {
            var keyCount = m_SharedTableData.Entries.Count;
            m_SharedTableData.RemoveKey("Invalid Key");
            Assert.AreEqual(keyCount, m_SharedTableData.Entries.Count, "Expected the key count to be the same.");
        }

        [Test]
        public void RemovingInvalidKeyDoesNotChangeKeyCount_ById()
        {
            var keyCount = m_SharedTableData.Entries.Count;
            m_SharedTableData.RemoveKey(1234);
            Assert.AreEqual(keyCount, m_SharedTableData.Entries.Count, "Expected the key count to be the same.");
        }

        [TestCase("Start Name", "End Name")]
        [TestCase("12345", "11111")]
        [TestCase("Test Name {0}", "Test {0} Name {1}")]
        [TestCase("#MY_NAME#", "#MY_OTHER_NAME#")]
        [TestCase("#[][]<><>#", "#[0][0]<1><2>#")]
        public void KeyHasTheSameIdAfterRename_UsingKeyToRename(string originalName, string newName)
        {
            AddAndVerifyKeyIsAdded(originalName);
            var keyId = GetKeyIdAndVerifyItIsValid(originalName);

            m_SharedTableData.RenameKey(originalName, newName);
            Assert.AreEqual(keyId, GetKeyIdAndVerifyItIsValid(newName), "Expected renamed key to have the same id.");
        }

        [TestCase("Start Name", "End Name")]
        [TestCase("12345", "11111")]
        [TestCase("Test Name {0}", "Test {0} Name {1}")]
        [TestCase("#MY_NAME#", "#MY_OTHER_NAME#")]
        [TestCase("#[][]<><>#", "#[0][0]<1><2>#")]
        public void KeyHasTheSameIdAfterRename_UsingKeyIdToRename(string originalName, string newName)
        {
            AddAndVerifyKeyIsAdded(originalName);
            var keyId = GetKeyIdAndVerifyItIsValid(originalName);

            m_SharedTableData.RenameKey(keyId, newName);
            Assert.AreEqual(keyId, GetKeyIdAndVerifyItIsValid(newName), "Expected renamed key to have the same id.");
        }

        [Test]
        public void RemapChangesKeyIdWhenNewKeyIsNotUsed()
        {
            var newEntry = m_SharedTableData.AddKey();

            // Now remap to a negative version which should not be in use.
            var id = newEntry.Id;
            var newKey = -newEntry.Id;

            Assert.IsTrue(m_SharedTableData.RemapId(id, newKey), "Expected Remap to return true.");
            Assert.AreEqual(newKey, newEntry.Id, "Expected entry key to be changed to the new key.");
            Assert.IsNull(m_SharedTableData.GetEntry(id), "Expected no entry to exist for old key.");
            Assert.IsNotNull(m_SharedTableData.GetEntry(newKey), "Expected an entry to exist for the new key.");
        }

        [Test]
        public void RemapDoesNotChangeKeyIdWhenNewKeyIsInUse()
        {
            var newEntry1 = m_SharedTableData.AddKey();
            var newEntry2 = m_SharedTableData.AddKey();

            // Now remap to an already used id
            var id = newEntry1.Id;
            var newKey = newEntry2.Id;

            Assert.IsFalse(m_SharedTableData.RemapId(id, newKey), "Expected Remap to return false when new key is already in use.");
            Assert.AreEqual(id, newEntry1.Id, "Expected entry key to have not been changed.");
            Assert.AreSame(newEntry1, m_SharedTableData.GetEntry(id), "Expected same entry to still exist for old key.");
            Assert.AreSame(newEntry2, m_SharedTableData.GetEntry(newKey), "Expected same entry to still exist for new key.");
        }
    }
}
