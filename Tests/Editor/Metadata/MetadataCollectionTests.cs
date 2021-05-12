using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.Tests.Metadata
{
    [TestFixture(typeof(MetadataCollection))]
    [TestFixture(typeof(StringTable))]
    [TestFixture(typeof(AssetTable))]
    public class MetadataCollectionTests<T> where T : class, IMetadataCollection, new()
    {
        [Serializable]
        public class TestMetadata : IMetadata {}
        readonly bool k_IsScriptableObject = typeof(ScriptableObject).IsAssignableFrom(typeof(T));

        T m_Collection;

        [SetUp]
        public void Setup()
        {
            if (k_IsScriptableObject)
                m_Collection = ScriptableObject.CreateInstance(typeof(T)) as T;
            else
                m_Collection = new T();
        }

        [Test]
        public void NewMetadataCollectionIsEmpty()
        {
            Assert.IsEmpty(m_Collection.MetadataEntries, "Expected a newly created MetadataCollection to contain no data by default");
        }

        [Test]
        public void GetMetadata_ReturnsMetadataAddedToCollection()
        {
            var metaadata = new TestMetadata();
            m_Collection.AddMetadata(metaadata);

            Assert.IsTrue(m_Collection.MetadataEntries.Contains(metaadata), "Expected metadata to be added to the List.");
            Assert.AreEqual(metaadata, m_Collection.GetMetadata<TestMetadata>(), "Expected metadata to be returned when calling GetMetadata");
        }

        [Test]
        public void GetMetadatas_ReturnsMetadatasAddedToCollection()
        {
            var metaadata1 = new TestMetadata();
            var metaadata2 = new TestMetadata();

            m_Collection.AddMetadata(metaadata1);
            m_Collection.AddMetadata(metaadata2);

            var foundItems = m_Collection.GetMetadatas<TestMetadata>();

            var providedList = new List<TestMetadata>();
            m_Collection.GetMetadatas<TestMetadata>(providedList);

            Assert.AreEqual(foundItems, providedList, "Expected GetMetadatas to return the same list for both versions of the function.");
            Assert.AreEqual(2, foundItems.Count, "Expected found metadatas count to be 2, one for each metadata item.");
            Assert.IsTrue(foundItems.Contains(metaadata1), "Expected metadata 1 to be in the found metadatas List.");
            Assert.IsTrue(foundItems.Contains(metaadata2), "Expected metadata 1 to be in the found metadatas List.");
        }

        [Test]
        public void RemoveMetadata_RemovesMetadataFromCollection()
        {
            var metaadata = new TestMetadata();
            m_Collection.AddMetadata(metaadata);

            Assert.IsTrue(m_Collection.RemoveMetadata(metaadata), "Expected remove to return true when removing the metadata.");
            Assert.IsFalse(m_Collection.RemoveMetadata(metaadata), "Expected remove to return false when the metadata was already removed.");
            Assert.IsEmpty(m_Collection.MetadataEntries, "Expected a metadata to be removed.");
        }
    }
}
