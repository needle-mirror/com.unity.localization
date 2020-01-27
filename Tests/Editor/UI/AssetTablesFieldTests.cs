using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.Localization.UI;
/*
namespace UnityEditor.Localization.Tests.UI
{
    public class AssetTablesFieldTests
    {
        class EmptyProjectPlayerSettings : LocalizationEditorSettings
        {
            protected override List<AssetTableCollection> GetAssetTablesCollectionInternal<TLocalizedTable>()
            {
                // An empty project with no asset tables
                return new List<AssetTableCollection>();
            }
        }

        [Test(Description ="Case: Exception when opening Asset Tables Window in an empty project(LOC-27)")]
        public void DoesNotThrowException_WhenNoTablesExistInProject()
        {
            LocalizationEditorSettings.Instance = new EmptyProjectPlayerSettings();
            Assert.DoesNotThrow(() => new AssetTablesField());
            LocalizationEditorSettings.Instance = null;
        }
    }
}
*/
