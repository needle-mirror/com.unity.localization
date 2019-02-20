using NUnit.Framework;
using System.Collections.Generic;

namespace UnityEditor.Localization.UI.Tests
{
    [Category("Localization")]
    public class AssetTablesFieldTests
    {
        class EmptyProjectPlayerSettings : LocalizationPlayerSettings
        {
            protected override List<AssetTableCollection> GetAssetTablesInternal<TLocalizedTable>()
            {
                // An empty project with no asset tables
                return new List<AssetTableCollection>();
            }
        }

        [Test(Description ="Case: Exception when opening Asset Tables Window in an empty project(LOC-27)")]
        public void DoesNotThrowException_WhenNoTablesExistInProject()
        {
            LocalizationPlayerSettings.Instance = new EmptyProjectPlayerSettings();
            var assetTablesField = new AssetTablesField();
            LocalizationPlayerSettings.Instance = null;
        }
    }
}
