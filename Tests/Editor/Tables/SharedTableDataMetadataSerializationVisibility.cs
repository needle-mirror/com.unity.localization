using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.Tests
{
    [Description("Ensure that SerializedProperties have visible children (LOC-85")]
    public class SharedTableDataMetadataSerializationVisibility
    {
        SharedTableData m_SharedTableData;
        SerializedObject m_SharedTableDataSerializedObject;
        SerializedProperty m_SharedEntryMetadataProperty;
        SerializedProperty m_CommentProperty;

        const string kCommentText = "Some Comment Text";
        const string kMetadataPath = "m_Entries.Array.data[0].m_Metadata";
        const string kCommentPath = "m_Items.Array.data[0]";

        [SetUp]
        public void Setup()
        {
            m_SharedTableData = ScriptableObject.CreateInstance<SharedTableData>();
            var key = m_SharedTableData.AddKey();
            key.Metadata.AddMetadata(new Comment { CommentText = kCommentText });

            m_SharedTableDataSerializedObject = new SerializedObject(m_SharedTableData);
            m_SharedEntryMetadataProperty = m_SharedTableDataSerializedObject.FindProperty(kMetadataPath);
            m_CommentProperty = m_SharedEntryMetadataProperty.FindPropertyRelative(kCommentPath);
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(m_SharedTableData);
        }

        [Test]
        public void EntryMetadata_SerializedPropertyHasVisibleChildren()
        {
            Assert.True(m_SharedEntryMetadataProperty.hasVisibleChildren);
        }

        [Test]
        public void EntryMetadataListItem_SerializedPropertyHasVisibleChildren()
        {
            Assert.True(m_CommentProperty.hasVisibleChildren);
        }
    }
}
