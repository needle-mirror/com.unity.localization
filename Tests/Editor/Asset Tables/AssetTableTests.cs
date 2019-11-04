using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization.Tables;
using UnityEngine.TestTools;
/*
namespace UnityEditor.Localization.Tests
{
    public class AssetTableTests
    {
        Texture2DAssetTable m_Texture2DAssetTable;
        KeyDatabase m_KeyDatabase;

        [SetUp]
        public void Init()
        {
            m_Texture2DAssetTable = ScriptableObject.CreateInstance<Texture2DAssetTable>();
            m_KeyDatabase = ScriptableObject.CreateInstance<KeyDatabase>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(m_Texture2DAssetTable);
            Object.DestroyImmediate(m_KeyDatabase);
        }

        [Test]
        public void NoKeyDatabase_AddAsset_ThrowsError()
        {
            m_Texture2DAssetTable.AddAsset("test key", "");
            LogAssert.Expect(LogType.Error, new Regex("The Asset Table does not have a Key Database"));
        }

        [Test]
        public void GetGuidFromKey_ReturnsGuidAssociatedWithId()
        {
            const string keyName = "AddAssetKeyId";
            const string guid = "90df10d9626db44918b3e2f35e4deaaa";
            var keyEntry = m_KeyDatabase.AddKey(keyName);
            m_Texture2DAssetTable.Keys = m_KeyDatabase;

            m_Texture2DAssetTable.AddAsset(keyName, guid);

            Assert.AreEqual(guid, m_Texture2DAssetTable.GetGuidFromKey(keyEntry.Id), "Expected the guid returned to be the same one used during AddAsset.");
        }

        [Test]
        public void GetGuidFromKey_ReturnsEmptyStringWhenNoEntryExists()
        {
            Assert.AreEqual(string.Empty, m_Texture2DAssetTable.GetGuidFromKey(123), "Expected no guid to be returned when the entry is not in the table.");
        }

        public static IEnumerable SupportedAssetTypeCases()
        {
            yield return new TestCaseData(typeof(AudioClipAssetTable)).Returns(typeof(AudioClip));
            yield return new TestCaseData(typeof(SpriteAssetTable)).Returns(typeof(Sprite));
            yield return new TestCaseData(typeof(Texture2DAssetTable)).Returns(typeof(Texture2D));
        }

        [TestCaseSource(nameof(SupportedAssetTypeCases))]
        public System.Type SupportedAssetType_ReturnsCorrectAssetType(System.Type tableType)
        {
            var table = ScriptableObject.CreateInstance(tableType) as AddressableAssetTable;
            var assetType = table.SupportedAssetType;
            Object.DestroyImmediate(table);
            return assetType;
        }
    }
}*/