using System;
using NUnit.Framework;
using UnityEditor.Localization.UI;
using UnityEngine;
using UnityEngine.Localization.Tables;
using Object = UnityEngine.Object;

namespace UnityEditor.Localization.Tests
{
    public class SerializedTableReferenceTests
    {
        class TestFixture : ScriptableObject
        {
            public TableReference tableReference;
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

        void CheckSerializedTableReferenceMatchesOriginalData()
        {
            var so = new SerializedObject(m_TestFixture);
            var property = so.FindProperty("tableReference");

            SerializedTableReference serializedTableReference = new SerializedTableReference(property);

            Assert.AreEqual(m_TestFixture.tableReference.TableCollectionName, serializedTableReference.Reference.TableCollectionName, "Expected the Table Collection Name to match but it did not. The SerializedTableReference should be able to recreate the TableEntryReference struct via the SerializedProperties.");
            Assert.AreEqual(m_TestFixture.tableReference.TableCollectionNameGuid, serializedTableReference.Reference.TableCollectionNameGuid, "Expected the Table Collection Name GUID to match but it did not. The SerializedTableReference should be able to recreate the TableEntryReference struct via the SerializedProperties.");
            Assert.AreEqual(m_TestFixture.tableReference, serializedTableReference.Reference, "Expected references to be equal but they were not. The SerializedTableReference should be able to recreate the TableEntryReference struct via the SerializedProperties.");
        }

        [Test]
        public void TableReference_UsingTableCollectionNameGuid_IsRecreated()
        {
            m_TestFixture.tableReference = Guid.NewGuid();
            CheckSerializedTableReferenceMatchesOriginalData();
        }

        [Test]
        public void TableReference_UsingTableCollectionName_IsRecreated()
        {
            m_TestFixture.tableReference = "table collection name";
            CheckSerializedTableReferenceMatchesOriginalData();
        }

        [Test]
        public void TableReference_WhenEmpty_IsRecreated()
        {
            m_TestFixture.tableReference = string.Empty;
            CheckSerializedTableReferenceMatchesOriginalData();
        }

        [Test]
        public void ChangesAreAppliedToAsset_Guid()
        {
            var so = new SerializedObject(m_TestFixture);
            var property = so.FindProperty("tableReference");
            var serializedTableEntryReference = new SerializedTableReference(property);

            serializedTableEntryReference.Reference = Guid.NewGuid();
            so.ApplyModifiedProperties();

            Assert.AreEqual(serializedTableEntryReference.Reference, m_TestFixture.tableReference, "Expected Guid to be applied to asset when changed through SerializedTableReference.");
        }

        [Test]
        public void ChangesAreAppliedToAsset_TableCollectionName()
        {
            var so = new SerializedObject(m_TestFixture);
            var property = so.FindProperty("tableReference");
            var serializedTableEntryReference = new SerializedTableReference(property);

            serializedTableEntryReference.Reference = "table collection name";
            so.ApplyModifiedProperties();

            Assert.AreEqual(serializedTableEntryReference.Reference, m_TestFixture.tableReference, "Expected Table Collection Name to be applied to asset when changed through SerializedTableReference.");
        }

        [Test]
        public void ChangesAreAppliedToAsset_EmptyId()
        {
            m_TestFixture.tableReference = "table collection name"; // Make it a string name by default

            var so = new SerializedObject(m_TestFixture);
            var property = so.FindProperty("tableReference");
            var serializedTableEntryReference = new SerializedTableReference(property);

            // Now clear
            serializedTableEntryReference.Reference = Guid.Empty;
            so.ApplyModifiedProperties();

            Assert.AreEqual(serializedTableEntryReference.Reference, m_TestFixture.tableReference, "Expected reference to be Empty when changed through SerializedTableReference.");
        }
    }
}
