using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization.Metadata;

namespace UnityEditor.Localization.Tests.Metadata
{
    public class AssetTypeMetadataTests
    {
        AssetTypeMetadata m_AssetTypeMetadata;

        [SetUp]
        public void Setup()
        {
            m_AssetTypeMetadata = new AssetTypeMetadata();
        }

        readonly Type k_TextureType = typeof(Texture2D);
        readonly string k_TextureTypeString = typeof(Texture2D).AssemblyQualifiedName;

        [Test]
        public void TypeString_IsSetBeforeSerialization()
        {
            Assert.IsNull(m_AssetTypeMetadata.m_TypeString, "Expected type string to be empty by default");
            m_AssetTypeMetadata.Type = k_TextureType;
            m_AssetTypeMetadata.OnBeforeSerialize();

            Assert.AreEqual(k_TextureTypeString, m_AssetTypeMetadata.m_TypeString, "Expected type string to be set before serialization.");
        }

        [Test]
        public void Type_IsSetAfterSerialization()
        {
            Assert.IsNull(m_AssetTypeMetadata.Type, "Expected Type to be null by default");
            m_AssetTypeMetadata.m_TypeString = k_TextureTypeString;
            m_AssetTypeMetadata.OnAfterDeserialize();

            Assert.AreEqual(k_TextureType, m_AssetTypeMetadata.Type, "Expected type to be set after serialization.");
        }
    }
}
