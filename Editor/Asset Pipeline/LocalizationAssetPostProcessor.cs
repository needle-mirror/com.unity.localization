using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization
{
    class LocalizationAssetPostProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
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
                    else if (typeof(KeyDatabase).IsAssignableFrom(assetType))
                    {
                        var keyDatabase = AssetDatabase.LoadAssetAtPath<KeyDatabase>(assetPath);
                        if (keyDatabase != null)
                        {
                            Debug.Assert(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(keyDatabase, out string keyDbGuid, out long _), "Failed to extract KeyDatabase Guid", keyDatabase);
                            var guid = Guid.Parse(keyDbGuid);

                            if (keyDatabase.TableNameGuid != Guid.Empty)
                                Debug.Assert(keyDatabase.TableNameGuid == guid, "Key Database Name Guid does not match the assets Guid. This may cause issues matching the correct TableName.", keyDatabase);
                            else
                                keyDatabase.TableNameGuid = guid;
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
    }

    class LocalizationAssetModificationProcessor : AssetModificationProcessor
    {
        static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions removeAssetOptions)
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