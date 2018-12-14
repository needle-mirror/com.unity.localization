using System.IO;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Localization.UI
{
    public static class Resources
    {
        const string k_TemplateRoot = "Packages/com.unity.localization/Editor/UI/Templates";
        const string k_StyleRoot = "Packages/com.unity.localization/Editor/UI/Styles";

        public static string GetStyleSheetPath(string filename)
        {
            return string.Format("{0}/{1}.uss", k_StyleRoot, filename);
        }

        static string TemplatePath(string filename)
        {
            return string.Format("{0}/{1}.uxml", k_TemplateRoot, filename);
        }

        public static VisualElement GetTemplate(string templateFilename)
        {
            var path = TemplatePath(templateFilename);
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);

            if (asset == null)
                throw new FileNotFoundException("Failed to load UI Template at path " + path);

            return asset.CloneTree(null);
        }
    }
}