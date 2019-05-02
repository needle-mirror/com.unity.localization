using System.IO;

#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif

namespace UnityEditor.Localization.UI
{
    static class Resources
    {
        const string k_TemplateRoot = "Packages/com.unity.localization/Editor/UI/Templates";
        const string k_StyleRoot = "Packages/com.unity.localization/Editor/UI/Styles";

        public static string GetStyleSheetPath(string filename) => $"{k_StyleRoot}/{filename}.uss";

        static string TemplatePath(string filename) => $"{k_TemplateRoot}/{filename}.uxml";

        public static VisualElement GetTemplate(string templateFilename)
        {
            var path = TemplatePath(templateFilename);
            var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);

            if (asset == null)
                throw new FileNotFoundException("Failed to load UI Template at path " + path);

            #if UNITY_2019_1_OR_NEWER
            return asset.CloneTree();
            #else
            return asset.CloneTree(null);
            #endif
        }
    }
}
