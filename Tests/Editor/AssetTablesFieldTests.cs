//using NUnit.Framework;
//using UnityEditor.Localization.UI;
//using UnityEngine.Localization.Tables;

//namespace UnityEditor.Localization.Tests.UI
//{
//    public class AssetTablesFieldTests
//    {
//        class EmptyProjectPlayerSettings : LocalizationEditorSettings
//        {
//            public EmptyProjectPlayerSettings()
//            {
//                AssetTableCollectionCache = new EmptyAssetTableCollectionCache();
//            }
//        }

//        [Test(Description = "Case: Exception when opening Asset Tables Window in an empty project(LOC-27)")]
//        public void DoesNotThrowException_WhenNoTablesExistInProject()
//        {
//            LocalizationEditorSettings.Instance = new EmptyProjectPlayerSettings();
//            Assert.IsEmpty(LocalizationEditorSettings.GetAssetTables<LocalizationTable>(), "Expected no tables");
//            Assert.DoesNotThrow(() => new ProjectTablesPopup());
//            LocalizationEditorSettings.Instance = null;
//        }
//    }
//}
