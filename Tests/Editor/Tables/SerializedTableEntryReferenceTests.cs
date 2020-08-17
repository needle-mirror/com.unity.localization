using NUnit.Framework;
using UnityEditor.Localization.UI;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.Tests
{
    public class SerializedTableEntryReferenceTests
    {
        class TestFixture : ScriptableObject
        {
            public TableEntryReference tableEntryReference;
        }

        TestFixture m_TestFixture;

        [SetUp]
        public void Setup()
        {
            m_TestFixture = ScriptableObject.CreateInstance<TestFixture>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(m_TestFixture);
        }

        void CheckSerializedTableEntryReferenceMatchesOriginalData()
        {
            var so = new SerializedObject(m_TestFixture);
            var property = so.FindProperty("tableEntryReference");

            SerializedTableEntryReference serializedTableEntryReference = new SerializedTableEntryReference(property);

            Assert.AreEqual(m_TestFixture.tableEntryReference.KeyId, serializedTableEntryReference.Reference.KeyId, "Expected the key id to match but it did not. The SerializedTableEntryReference should be able to recreate the TableEntryReference struct via the SerializedProperties.");
            Assert.AreEqual(m_TestFixture.tableEntryReference.Key, serializedTableEntryReference.Reference.Key, "Expected the key to match but it did not. The SerializedTableEntryReference should be able to recreate the TableEntryReference struct via the SerializedProperties.");
            Assert.AreEqual(m_TestFixture.tableEntryReference, serializedTableEntryReference.Reference, "Expected references to be equal but they were not. The SerializedTableEntryReference should be able to recreate the TableEntryReference struct via the SerializedProperties.");
        }

        [Test]
        public void TableEntryReference_UsingKeyId_IsRecreated()
        {
            m_TestFixture.tableEntryReference = 123;
            CheckSerializedTableEntryReferenceMatchesOriginalData();
        }

        [Test]
        public void TableEntryReference_UsingMaxLongKeyId_IsRecreated()
        {
            // Ensure the key is not truncated to an int.
            m_TestFixture.tableEntryReference = long.MaxValue;
            CheckSerializedTableEntryReferenceMatchesOriginalData();
        }

        [Test]
        public void TableEntryReference_UsingKeyName_IsRecreated()
        {
            m_TestFixture.tableEntryReference = "A string key";
            CheckSerializedTableEntryReferenceMatchesOriginalData();
        }

        [Test]
        public void TableEntryReference_WhenEmpty_IsRecreated()
        {
            CheckSerializedTableEntryReferenceMatchesOriginalData();
        }

        [Test]
        public void ChangesAreAppliedToAsset_Id()
        {
            var so = new SerializedObject(m_TestFixture);
            var property = so.FindProperty("tableEntryReference");
            var serializedTableEntryReference = new SerializedTableEntryReference(property);

            serializedTableEntryReference.Reference = 123;
            so.ApplyModifiedProperties();

            Assert.AreEqual(serializedTableEntryReference.Reference, m_TestFixture.tableEntryReference, "Expected Key Id to be applied to asset when changed through SerializedTableEntryReference.");
        }

        [Test]
        public void ChangesAreAppliedToAsset_KeyName()
        {
            var so = new SerializedObject(m_TestFixture);
            var property = so.FindProperty("tableEntryReference");
            var serializedTableEntryReference = new SerializedTableEntryReference(property);

            serializedTableEntryReference.Reference = "Key name";
            so.ApplyModifiedProperties();

            Assert.AreEqual(serializedTableEntryReference.Reference, m_TestFixture.tableEntryReference, "Expected Key Name to be applied to asset when changed through SerializedTableEntryReference.");
        }

        [Test]
        public void ChangesAreAppliedToAsset_EmptyId()
        {
            m_TestFixture.tableEntryReference = 123; // Make it a Key Id by default

            var so = new SerializedObject(m_TestFixture);
            var property = so.FindProperty("tableEntryReference");
            var serializedTableEntryReference = new SerializedTableEntryReference(property);

            // Now clear
            serializedTableEntryReference.Reference = SharedTableData.EmptyId;
            so.ApplyModifiedProperties();

            Assert.AreEqual(serializedTableEntryReference.Reference, m_TestFixture.tableEntryReference, "Expected reference to be Empty when changed through SerializedTableEntryReference.");
        }
    }
}
