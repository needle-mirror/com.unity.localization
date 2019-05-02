using UnityEngine.Localization;

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
                    var table = AssetDatabase.LoadAssetAtPath<LocalizedTable>(assetPath);
                    if (table != null)
                        LocalizationEditorSettings.AddOrUpdateTable(table);
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
                var table = AssetDatabase.LoadAssetAtPath<LocalizedTable>(assetPath);
                if (table != null)
                    LocalizationEditorSettings.RemoveTable(table);
            }
            return AssetDeleteResult.DidNotDelete;
        }
    }
}