using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization.Components;

namespace UnityEditor.Localization.Tests
{
    [TestFixture(typeof(LocalizeTextureBehaviour))]
    [TestFixture(typeof(LocalizeAudioClipBehaviour))]
    public class LocalizeComponentSerializationTests<TComponent> where TComponent : Component
    {
        TComponent m_Component;
        SerializedObject m_SerializedObject;

        [OneTimeSetUp]
        public void Setup()
        {
            var go = new GameObject(nameof(LocalizeComponentSerializationTests<TComponent>));
            m_Component = go.AddComponent<TComponent>();
            m_SerializedObject = new SerializedObject(m_Component);
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(m_Component.gameObject);
        }

        [TestCase("m_UpdateAsset")]
        [TestCase("m_LocalizedAssetReference")]
        public void PropertyIsSerialized(string propertyName)
        {
            Assert.NotNull(m_SerializedObject.FindProperty(propertyName), $"Expected property {propertyName} to be serialized but it could not be found.");
        }
    }
}
