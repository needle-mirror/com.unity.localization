using UnityEngine;

namespace UnityEditor.Localization
{
    class EditorIcons
    {
        public static Texture Csv { get; private set; }
        public static Texture GameObjectLocalizer { get; private set; }
        public static Texture GoogleSheets { get; private set; }
        public static Texture Locale { get; private set; }
        public static Texture Metadata { get; private set; }
        public static Texture TableWindow { get; private set; }
        public static Texture WarningIcon { get; private set; }

        static EditorIcons()
        {
            Csv = GetTexture("CSV Extension/CSVExtension.png");
            GameObjectLocalizer = GetTexture("Game Object Localizer/GameObjectLocalizer.png");
            GoogleSheets = GetTexture("Google Sheets Provider/GoogleSheetsProvider.png");
            Locale = GetTexture("Locale/Locale.png");
            Metadata = GetTexture("Metadata/Metadata.png");
            TableWindow = GetTexture("Localization Tables Window/LocalizationTablesWindow.png");
            WarningIcon = EditorGUIUtility.IconContent("console.warnicon").image;
        }

        static Texture GetTexture(string path) => EditorGUIUtility.FindTexture("Packages/com.unity.localization/Editor/Icons/" + path);
    }
}
