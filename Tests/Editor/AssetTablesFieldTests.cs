using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Localization.UI;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.Tests.UI
{
    public class AssetTablesFieldTests
    {
        class EmptyProjectPlayerSettings : LocalizationEditorSettings
        {
            protected override List<AddressableAssetEntry> GetAssetTablesInternal(Type tableType)
            {
                // An empty project with no asset tables
                return new List<AddressableAssetEntry>();
            }
        }

        [Test(Description = "Case: Exception when opening Asset Tables Window in an empty project(LOC-27)")]
        public void DoesNotThrowException_WhenNoTablesExistInProject()
        {
            LocalizationEditorSettings.Instance = new EmptyProjectPlayerSettings();
            Assert.IsEmpty(LocalizationEditorSettings.GetAssetTables<LocalizedTable>(), "Expected no tables");
            Assert.DoesNotThrow(() => new ProjectTablesPopup());
            LocalizationEditorSettings.Instance = null;
        }
    }
}
