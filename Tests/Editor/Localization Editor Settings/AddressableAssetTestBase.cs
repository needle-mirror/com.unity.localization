using System.IO;
using NUnit.Framework;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.Localization;
using System.Collections.Generic;
using System;
using Object = UnityEngine.Object;

namespace UnityEditor.Localization.Tests
{
    public class AddressableAssetTestBase
    {
        protected const string k_TestConfigName = "AddressableAssetSettings.Localization.Tests";
        protected const string k_TestConfigFolder = "Assets/AddressableAssetsData_LocalizationTests";
        protected FakedAddressableLocalizationEditorSettings m_LocalizationEditorSettings;
        protected AddressableAssetSettings m_AddressableSettings;

        protected virtual bool PersistSettings { get { return true; } }

        [OneTimeSetUp]
        public void Init()
        {
            if (Directory.Exists(k_TestConfigFolder))
                AssetDatabase.DeleteAsset(k_TestConfigFolder);
            if (!Directory.Exists(k_TestConfigFolder))
            {
                Directory.CreateDirectory(k_TestConfigFolder);
                AssetDatabase.Refresh();
            }

            m_AddressableSettings = AddressableAssetSettings.Create(k_TestConfigFolder, k_TestConfigName, true, PersistSettings);
            m_LocalizationEditorSettings = new FakedAddressableLocalizationEditorSettings();
            m_LocalizationEditorSettings.TestAddressableAssetSettings = m_AddressableSettings;
            LocalizationEditorSettings.Instance = m_LocalizationEditorSettings;
            OnInit();
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            OnCleanup();
            if (Directory.Exists(k_TestConfigFolder))
                AssetDatabase.DeleteAsset(k_TestConfigFolder);
            LocalizationEditorSettings.Instance = null;
        }

        protected static List<SystemLanguage> GenerateSampleLanguages()
        {
            return new List<SystemLanguage>()
            {
                SystemLanguage.English,
                SystemLanguage.French,
                SystemLanguage.Arabic,
                SystemLanguage.Japanese,
                SystemLanguage.Chinese,
            };
        }

        public static List<Type> AllTableTypes()
        {
            return new List<Type>()
            {
                typeof(AudioClipAssetTable),
                typeof(SpriteAssetTable),
                typeof(StringTable),
                typeof(Texture2DAssetTable)
            };
        }

        protected AddressableAssetEntry FindAddressableAssetEntry(Object asset)
        {
            Assert.IsNotNull(asset);

            string guid;
            long localId;
            Assert.IsTrue(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out guid, out localId), "Could not get FileIdentifier: " + asset);
            return m_AddressableSettings.FindAssetEntry(guid);
        }

        protected void VerifyAssetIsInAddressables(Object asset, string message = "")
        {
            Assert.IsNotNull(FindAddressableAssetEntry(asset), "Expected the asset to be added to Addressables but it was not. " + message);
        }

        protected void VerifyAssetIsNotInAddressables(Object asset, string message = "")
        {
            Assert.IsNull(FindAddressableAssetEntry(asset), "Expected the asset to not be in Addressables but it was. " + message);
        }

        protected static void CreateAsset(Object asset, string name)
        {
            var path = Path.Combine(k_TestConfigFolder, name +  ".asset");
            AssetDatabase.CreateAsset(asset, path);
        }

        protected static void DeleteAsset(Object asset)
        {
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(asset));
        }

        protected static Texture2D CreateTestTexture(string name)
        {
            var bytes = Texture2D.whiteTexture.EncodeToPNG();
            var path = Path.Combine(k_TestConfigFolder, name + ".png");
            File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        protected virtual void OnInit() { }
        protected virtual void OnCleanup() { }
    }
}
