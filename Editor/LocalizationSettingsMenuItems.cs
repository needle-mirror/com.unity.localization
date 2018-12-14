using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace UnityEditor.Localization
{
    class LocalizationSettingsMenuItems
    {
        [MenuItem("Assets/Create/Localization/Localization Settings", false)]
        public static void CreateAssetWithMakeActiveDialog()
        {
            var ls = CreateLocalizationAsset();
            if (ls == null)
                return;

            if (EditorUtility.DisplayDialog("Active localization settings",
                "Do you wish to make this asset the active localization settings? The active localization settings will be included into any builds and preloaded at the start. This can be changed at 'Edit/Project Settings/Localization'",
                "Yes",
                "No"))
            {
                LocalizationPlayerSettings.ActiveLocalizationSettings = ls;
            }
        }

        public static LocalizationSettings CreateLocalizationAsset()
        {
            var path = EditorUtility.SaveFilePanelInProject("Save Localization Settings", "Localization Settings", "asset", "Please enter a filename to save the projects localization settings to.");

            if (string.IsNullOrEmpty(path))
                return null;

            var dependencies = new List<ScriptableObject>();
            var ls = LocalizationSettings.CreateDefault(dependencies);
            AssetDatabase.CreateAsset(ls, path);
            foreach (var scriptableObject in dependencies)
            {
                AssetDatabase.AddObjectToAsset(scriptableObject, ls);
            }
            AssetDatabase.SaveAssets();
            Selection.activeObject = ls;
            return ls;
        }
    }
}