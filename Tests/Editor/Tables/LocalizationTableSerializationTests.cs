using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.Tests
{
    public class LocalizationTableSerializationTests
    {
        StringTable sourceTable;
        StringTable targetTable;

        readonly List<(long key, string value)> testEntries = new List<(long key, string value)>
        {
            (1, "Entry 1"),
            (2, "Test"),
            (5, "Some Value"),
            (6, ""),
            (100, "Hello World")
        };

        [SetUp]
        public void Init()
        {
            sourceTable = ScriptableObject.CreateInstance<StringTable>();

            // Add the test entries to the table
            testEntries.ForEach(e => sourceTable.AddEntry(e.key, e.value));
            Assert.Greater(sourceTable.Count, 0, "Expected the source table to contain some entries but it does not.");
        }

        [TearDown]
        public void Cleanup()
        {
            Object.DestroyImmediate(sourceTable);
            Object.DestroyImmediate(targetTable);
        }

        void SerializeSourceTableIntoTargetTable()
        {
            // Serialize the table to json and then create a new table with the serialized data.
            var json = JsonUtility.ToJson(sourceTable);
            targetTable = ScriptableObject.CreateInstance<StringTable>();
            JsonUtility.FromJsonOverwrite(json, targetTable);
        }

        [Test]
        public void StringTable_AddEntryChanges_AreSerialized()
        {
            SerializeSourceTableIntoTargetTable();

            foreach (var testEntry in testEntries)
            {
                var foundEntry = targetTable.GetEntry(testEntry.key);
                Assert.NotNull(foundEntry, $"Expected the target table to contain the entry `{testEntry.key}` but it does not.");
                Assert.AreEqual(testEntry.value, foundEntry.Value, message: $"Expected the target table entry `{testEntry.key}` value to match the source table.");
            }
        }

        [Test]
        public void StringTable_ClearEntries_RemovesAllEntries()
        {
            sourceTable.Clear();
            SerializeSourceTableIntoTargetTable();

            Assert.IsEmpty(targetTable, "Expected the target table to have no entries.");
        }

        [Test]
        public void StringTable_ChangesToDictionary_AddEntry_AreSerialized()
        {
            const string k_Value = nameof(StringTable_ChangesToDictionary_AddEntry_AreSerialized);
            const long k_Key = 12345;

            // Add a value through the dictionary interface
            var entry = sourceTable.CreateTableEntry();
            entry.Value = k_Value;
            sourceTable.Add(k_Key, entry);

            SerializeSourceTableIntoTargetTable();

            var foundEntry = targetTable.GetEntry(k_Key);
            Assert.NotNull(foundEntry, $"Expected the target table to have an entry with the key `{k_Key}`");
            Assert.AreEqual(foundEntry.Value, k_Value, "Expected the target table entry to match the source table but it does not.");
        }

        [Test]
        public void StringTable_ChangesToDictionary_RemoveEntry_AreSerialized()
        {
            const long k_Key = 5;

            Assert.IsTrue(sourceTable.ContainsKey(k_Key), "Expected source table to contain the key but it did not.");
            sourceTable.Remove(k_Key);

            SerializeSourceTableIntoTargetTable();

            Assert.IsFalse(targetTable.ContainsKey(k_Key), "Expected target table to not contain the removed key.");
        }
    }
}
