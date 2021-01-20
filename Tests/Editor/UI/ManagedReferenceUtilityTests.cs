using System;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Localization.UI;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Metadata;
using Object = UnityEngine.Object;

namespace UnityEditor.Localization.Tests.UI
{
    public class ManagedReferenceUtilityTests
    {
        [DisplayName("Custom Name")]
        [Serializable]
        public class MetadataWithDisplayName : IMetadata {}

        [DisplayName(null)]
        [Serializable]
        public class MetadataWithNullDisplayName : IMetadata {}

        [DisplayName("")]
        [Serializable]
        public class MetadataWithEmptyDisplayName : IMetadata {}

        [Serializable]
        public class SerializeRefClass {}

        public class Fixture : ScriptableObject
        {
            [SerializeReference]
            public IMetadata metadataWithCustomName = new MetadataWithDisplayName();

            [SerializeReference]
            public IMetadata metadataWithNullDisplayName = new MetadataWithEmptyDisplayName();

            [SerializeReference]
            public IMetadata metadataWithEmptyDisplayName = new MetadataWithEmptyDisplayName();

            [SerializeReference]
            public SerializeRefClass normalSerializeReferenceClass = new SerializeRefClass();
        }

        Fixture m_Fixture;
        SerializedObject m_SerializedObject;

        [OneTimeSetUp]
        public void Setup()
        {
            m_Fixture = ScriptableObject.CreateInstance<Fixture>();
            m_SerializedObject = new SerializedObject(m_Fixture);
        }

        string GetManagedReferenceFullTypenameForProperty(string propertyName)
        {
            var prop = m_SerializedObject.FindProperty(propertyName);
            Assert.NotNull(prop, $"Could not find Fixture property {propertyName}.");
            return prop.managedReferenceFullTypename;
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(m_Fixture);
        }

        [TestCase("metadataWithCustomName", "Custom Name")]
        [TestCase("metadataWithNullDisplayName", "Metadata With Empty Display Name")]
        [TestCase("metadataWithEmptyDisplayName", "Metadata With Empty Display Name")]
        [TestCase("normalSerializeReferenceClass", "Serialize Ref Class")]
        public void GetDisplayName_ReturnsExpectedName(string fixtureProperty, string expected)
        {
            var typeName = GetManagedReferenceFullTypenameForProperty(fixtureProperty);
            var guiContent = ManagedReferenceUtility.GetDisplayName(typeName);
            Assert.AreEqual(expected, guiContent.text, $"Unexpected Display Name for managed reference type {typeName}");
        }

        [Test]
        public void GetDisplayName_ReurnsEmptyWhenNameIsNullOrEmpty()
        {
            Assert.AreEqual(ManagedReferenceUtility.Empty, ManagedReferenceUtility.GetDisplayName((string)null));
            Assert.AreEqual(ManagedReferenceUtility.Empty, ManagedReferenceUtility.GetDisplayName(string.Empty));
        }

        [TestCase(typeof(MetadataWithDisplayName), "metadataWithCustomName")]
        [TestCase(typeof(MetadataWithEmptyDisplayName), "metadataWithNullDisplayName")]
        [TestCase(typeof(MetadataWithEmptyDisplayName), "metadataWithEmptyDisplayName")]
        [TestCase(typeof(SerializeRefClass), "normalSerializeReferenceClass")]
        public void GetType_FindsCorrectTypeFromManagedReferenceTypeNameString(Type expectedType, string fixtureProperty)
        {
            var typeName = GetManagedReferenceFullTypenameForProperty(fixtureProperty);
            var type = ManagedReferenceUtility.GetType(typeName);
            Assert.AreEqual(expectedType, type);
        }

        [Test]
        public void GetType_ThrowsWhenNameIsNullOrEmpty()
        {
            Assert.Throws<ArgumentException>(() => ManagedReferenceUtility.GetType(null));
            Assert.Throws<ArgumentException>(() => ManagedReferenceUtility.GetType(string.Empty));
        }
    }
}
