using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization
{
    class LocalizationAssetPostProcessor : AssetPostprocessor
    {
        #pragma warning disable CA1801 // CA1801 Review unused parameters
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            // We need to disable Addresables settings creation here.
            // When doing a full project import, there is no guarantee of OnPostprocessAllAssets order
            // and the Addressables may be called after Localization which would cause assets to be
            // re-added to groups that become invalid and a general corruption of Addressables data.
            LocalizationEditorSettings.EnableAddressablesCreation = false;

            try
            {
                foreach (var assetPath in importedAssets)
                {
                    if (assetPath.EndsWith("asset"))
                    {
                        var assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);

                        if (typeof(LocalizedTable).IsAssignableFrom(assetType))
                        {
                            var table = AssetDatabase.LoadAssetAtPath<LocalizedTable>(assetPath);
                            if (table != null)
                                LocalizationEditorSettings.AddOrUpdateTable(table, false);
                        }
                        else if (typeof(SharedTableData).IsAssignableFrom(assetType))
                        {
                            var sharedTableData = AssetDatabase.LoadAssetAtPath<SharedTableData>(assetPath);
                            if (sharedTableData != null)
                            {
                                Debug.Assert(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sharedTableData, out string sharedTableDataGuid, out long _), "Failed to extract SharedTableData Guid", sharedTableData);
                                var guid = Guid.Parse(sharedTableDataGuid);

                                if (sharedTableData.TableNameGuid != Guid.Empty)
                                    Debug.Assert(sharedTableData.TableNameGuid == guid, "SharedTableData Name Guid does not match the assets Guid. This may cause issues matching the correct TableName.", sharedTableData);
                                else
                                    sharedTableData.TableNameGuid = guid;
                            }
                        }
                        else if (typeof(Locale).IsAssignableFrom(assetType))
                        {
                            var locale = AssetDatabase.LoadAssetAtPath<Locale>(assetPath);
                            Debug.Assert(locale != null, "Failed to load Locale asset.");
                            LocalizationEditorSettings.AddLocale(locale, false);
                        }
                    }
                }
            }
            finally
            {
                LocalizationEditorSettings.EnableAddressablesCreation = true;
            }
        }

        #pragma warning restore CA1801
    }

    class LocalizationAssetModificationProcessor : AssetModificationProcessor
    {
        static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions _)
        {
            if (assetPath.EndsWith("asset"))
            {
                var assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);

                if (typeof(LocalizedTable).IsAssignableFrom(assetType))
                {
                    var table = AssetDatabase.LoadAssetAtPath<LocalizedTable>(assetPath);
                    LocalizationEditorSettings.RemoveTable(table, false);
                }
                else if (typeof(Locale).IsAssignableFrom(assetType))
                {
                    var locale = AssetDatabase.LoadAssetAtPath<Locale>(assetPath);
                    LocalizationEditorSettings.RemoveLocale(locale, false);
                }
            }
            return AssetDeleteResult.DidNotDelete;
        }
    }
}
