//using NUnit.Framework;
//using UnityEngine;
//using UnityEngine.Localization.Metadata;

//namespace UnityEditor.Localization.Tests.Metadata
//{
//    public class MetadataCollectionTests
//    {
//        public class TestMetadata : ScriptableObject { }

//        MetadataCollection m_Collection;

//        [SetUp]
//        public void Setup()
//        {
//            m_Collection = new MetadataCollection();
//        }

//        static TestMetadata CreateTestMetadata()
//        {
//            var md = ScriptableObject.CreateInstance<TestMetadata>();
//            Undo.RegisterCreatedObjectUndo(md, "Create metadata");
//            return md;
//        }

//        TestMetadata AddTestMetadata()
//        {
//            var metadata = CreateTestMetadata();
//            m_Collection.AddMetadata(metadata);
//            return metadata;
//        }

//        [Test]
//        public void NewMetadataCollectionIsEmpty()
//        {
//            Assert.IsEmpty(m_Collection.Data, "Expected a newly created MetadataCollection to contain no data by default");
//        }

//        [Test]
//        public void NewMetadataCollectionHasData_IsFalse()
//        {
//            Assert.IsFalse(m_Collection.HasData, "Expected a newly created MetadataCollection to contain no data by default");
//        }

//        [Test]
//        public void GetMetadata_ReturnsMetadataAddedToCollection()
//        {
//            var metaadata = AddTestMetadata();
//            Assert.Contains(metaadata, m_Collection.Data, "Expected metadata to be added to the List.");
//            Assert.IsTrue(m_Collection.HasData, "Expected HasData to be true when collection contains data.");
//            Assert.IsNotNull(m_Collection.GetMetadata<TestMetadata>(), "Expected metadata to be returned when calling GetMetadata");
//        }

//        [Test]
//        public void RemoveMetadata_ReturnsMetadataFromCollection()
//        {
//            var metaadata = AddTestMetadata();
//            m_Collection.RemoveMetadata(metaadata);
//            Assert.IsEmpty(m_Collection.Data, "Expected a metadata to be removed.");
//        }
//    }
//}