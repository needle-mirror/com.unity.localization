using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.Tests
{
    public class KeyDatabaseTests
    {
        KeyDatabase m_KeyDatabase;

        [SetUp]
        public void Init()
        {
            m_KeyDatabase = ScriptableObject.CreateInstance<KeyDatabase>();

            for (int i = 0; i < 100; ++i)
            {
                m_KeyDatabase.AddKey("My Key " + i);
            }
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(m_KeyDatabase);
        }


        [TestCase("Unique Key 1")]
        [TestCase("Unique Key 2")]
        [TestCase("Unique Key 3")]
        [TestCase("{0} {1} ####--++></?")]
        public void AddAndVerifyKeyIsAdded(string keyName)
        {
            Assert.IsFalse(m_KeyDatabase.Contains(keyName), "Expected the key to not already be in the database.");
            Assert.IsNotNull(m_KeyDatabase.AddKey(keyName), "Expected the key to added however it was not.");
            Assert.IsTrue(m_KeyDatabase.Contains(keyName), "Expected the key to be contained in the database.");
        }

        uint GetKeyIdAndVerifyItIsValid(string keyName)
        {
            var keyId = m_KeyDatabase.GetId(keyName);
            Assert.IsTrue(keyId != KeyDatabase.EmptyId, "Failed to find the id of the added key");
            return keyId;
        }

        [Test]
        public void All_KeyIds_AreUnique()
        {
            HashSet<uint> usedKeys = new HashSet<uint>();
            foreach (var entry in m_KeyDatabase.Entries)
            {
                Assert.IsFalse(usedKeys.Contains(entry.Id), "Expected all key ids to be unique, however this key has already been used: " + entry.Id);
                usedKeys.Add(entry.Id);
            }
        }

        [Test]
        public void All_Keys_AreUnique()
        {
            HashSet<string> usedKeys = new HashSet<string>();
            foreach (var entry in m_KeyDatabase.Entries)
            {
                Assert.IsFalse(usedKeys.Contains(entry.Key), "Expected all keys to be unique, however this key has already been used: " + entry.Key);
                usedKeys.Add(entry.Key);
            }
        }

        [Test]
        public void AddKey_AddingDuplicateKey_ReturnsNull()
        {
            const string keyName = "Duplicate Key";
            AddAndVerifyKeyIsAdded(keyName);
            Assert.IsNull(m_KeyDatabase.AddKey(keyName), "Expected the key to not be added a second time.");
        }

        [Test]
        public void AddKey_AllowsRemovedKeyToBeReAdded()
        {
            const string keyName = "Remove And Add Key";
            AddAndVerifyKeyIsAdded(keyName);

            m_KeyDatabase.RemoveKey(keyName);
            Assert.IsFalse(m_KeyDatabase.Contains(keyName), "Expected the key to not be contained when it has been removed from the database.");
            Assert.IsNotNull(m_KeyDatabase.AddKey(keyName), "Expected the key to added again after being removed, however it was not.");
        }

        [Test]
        public void KeyIsNotInDatabase_AfterRemovingKey_ByName()
        {
            const string keyName = "Remove Key By Name";
            AddAndVerifyKeyIsAdded(keyName);
            m_KeyDatabase.RemoveKey(keyName);
            Assert.IsFalse(m_KeyDatabase.Contains(keyName), "Expected the key to not be contained when it has been removed from the database.");
        }

        [Test]
        public void KeyIsNotInDatabase_AfterRemovingKey_ById()
        {
            const string keyName = "Remove Key By Name";
            AddAndVerifyKeyIsAdded(keyName);
            uint addedKeyId = GetKeyIdAndVerifyItIsValid(keyName);

            m_KeyDatabase.RemoveKey(addedKeyId);
            Assert.IsFalse(m_KeyDatabase.Contains(keyName), "Expected the key to not be contained when it has been removed from the database.");
        }

        [Test]
        public void RemovingInvalidKeyDoesNotChangeKeyCount_ByName()
        {
            var keyCount = m_KeyDatabase.Entries.Count;
            m_KeyDatabase.RemoveKey("Invalid Key");
            Assert.AreEqual(keyCount, m_KeyDatabase.Entries.Count, "Expected the key count to be the same.");
        }

        [Test]
        public void RemovingInvalidKeyDoesNotChangeKeyCount_ById()
        {
            var keyCount = m_KeyDatabase.Entries.Count;
            m_KeyDatabase.RemoveKey(1234);
            Assert.AreEqual(keyCount, m_KeyDatabase.Entries.Count, "Expected the key count to be the same.");
        }

        [TestCase("Start Name", "End Name")]
        [TestCase("12345", "11111")]
        [TestCase("Test Name {0}", "Test {0} Name {1}")]
        [TestCase("#MY_NAME#", "#MY_OTHER_NAME#")]
        [TestCase("#[][]<><>#", "#[0][0]<1><2>#")]
        public void KeyHasTheSameIdAfterRename_UsingKeyToRename(string originalName, string newName)
        {
            AddAndVerifyKeyIsAdded(originalName);
            uint keyId = GetKeyIdAndVerifyItIsValid(originalName);

            m_KeyDatabase.RenameKey(originalName, newName);
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
            uint keyId = GetKeyIdAndVerifyItIsValid(originalName);

            m_KeyDatabase.RenameKey(keyId, newName);
            Assert.AreEqual(keyId, GetKeyIdAndVerifyItIsValid(newName), "Expected renamed key to have the same id.");
        }
    }
}