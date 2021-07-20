#if UNITY_ANDROID || PACKAGE_DOCS_GENERATION
using System;
using System.IO;
using System.Text;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEditor.Localization.Platform.Utility;
using UnityEngine.Localization.Platform.Android;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace UnityEditor.Localization.Platform.Android
{
    public static class Player
    {
        const string k_InfoFile = "strings.xml";
        const string k_LegacyIconName = "app_icon.png";
        const string k_RoundIconName = "app_icon_round.png";
        const string k_AdaptiveIconBackgroundName = "ic_launcher_background.png";
        const string k_AdaptiveIconForegroundName = "ic_launcher_foreground.png";
        const string k_AdaptiveIcon_AppIconInfo = "app_icon.xml";
        const string k_AdaptiveIcon_AppRoundIconInfo = "app_icon_round.xml";

        static PackageManager.PackageInfo s_PackageInfo;
        static PackageManager.PackageInfo LocalizationPackageInfo
        {
            get
            {
                if (s_PackageInfo == null)
                {
                    s_PackageInfo = PackageManager.PackageInfo.FindForAssembly(typeof(LocalizationSettings).Assembly);
                }
                return s_PackageInfo;
            }
        }

        /// <summary>
        /// Updates the Gradle project 'strings.xml' file with localized values using the <see cref="AppInfo"/> from <see cref="LocalizationSettings.Metadata"/>.
        /// </summary>
        /// <param name="projectDirectory">The root project directory to be updated. This is where the Android player was built to.</param>
        public static void AddLocalizationToAndroidGradleProject(string projectDirectory)
        {
            var appInfo = LocalizationSettings.Metadata.GetMetadata<AppInfo>();
            var roundIconInfo = LocalizationSettings.Metadata.GetMetadata<RoundIconsInfo>();
            var legacyIconInfo = LocalizationSettings.Metadata.GetMetadata<LegacyIconsInfo>();
            var adaptiveIconInfo = LocalizationSettings.Metadata.GetMetadata<AdaptiveIconsInfo>();

            if (appInfo == null && roundIconInfo == null && legacyIconInfo == null && adaptiveIconInfo == null)
                return;

            AddLocalizationToAndroidGradleProject(projectDirectory, appInfo, roundIconInfo, legacyIconInfo, adaptiveIconInfo);
        }

        /// <summary>
        /// Updates the Android Gradle project file with localized values using <see cref="AppInfo"/>.
        /// </summary>
        /// <param name="projectDirectory">The root project directory to be updated. This is where the Android player was built to.</param>
        /// <param name="appInfo">Contains the localized values for the App.</param>
        /// <param name="roundIconsInfo">Contains the localized values for Android Round Icon. Refer Android documentation for more details : https://developer.android.com/about/versions/nougat/android-7.1.html#circular-icons</param>
        /// <param name="legacyIconsInfo">Contains the localized values for Android Legacy Icon.</param>
        /// <param name="adaptiveIconsInfo">Contains the localized values for Android Adaptive Icon. . Refer Android documentation for more details : https://developer.android.com/guide/practices/ui_guidelines/icon_design_adaptive</param>
        public static void AddLocalizationToAndroidGradleProject(string projectDirectory, AppInfo appInfo, RoundIconsInfo roundIconsInfo = null, LegacyIconsInfo legacyIconsInfo = null, AdaptiveIconsInfo adaptiveIconsInfo = null)
        {
            if (appInfo == null)
                throw new ArgumentNullException(nameof(appInfo));

            var project = new GradleProjectSettings();
            foreach (var locale in LocalizationEditorSettings.GetLocales())
            {
                var localeIdentifier = locale.Identifier.Code.Replace("-", "-r");
                GenerateLocalizedXmlFile("App Name", Path.Combine(Directory.CreateDirectory(Path.Combine(project.GetResFolderPath(projectDirectory), "values-b+" + localeIdentifier)).FullName, k_InfoFile), locale, appInfo);

                //Generate icons
                var folderNames = new List<string>
                {
                    $"mipmap-{localeIdentifier}-hdpi",
                    $"mipmap-{localeIdentifier}-ldpi",
                    $"mipmap-{localeIdentifier}-mdpi",
                    $"mipmap-{localeIdentifier}-xhdpi",
                    $"mipmap-{localeIdentifier}-xxhdpi",
                    $"mipmap-{localeIdentifier}-xxxhdpi",
                    $"mipmap-{localeIdentifier}-anydpi-v26"
                };


                if (roundIconsInfo != null || legacyIconsInfo != null || adaptiveIconsInfo != null)
                    GenerateIconDirectory(folderNames, project.GetResFolderPath(projectDirectory), locale);

                if (roundIconsInfo != null)
                    GenerateRoundIcons(folderNames, project.GetResFolderPath(projectDirectory), locale, roundIconsInfo);

                if (legacyIconsInfo != null)
                    GenerateLegacyIcons(folderNames, project.GetResFolderPath(projectDirectory), locale, legacyIconsInfo);

                if (adaptiveIconsInfo != null)
                    GenerateAdaptiveIcons(folderNames, project.GetResFolderPath(projectDirectory), locale, adaptiveIconsInfo);
            }

            var androidManifest = new AndroidManifest(project.GetManifestPath(projectDirectory));
            androidManifest.SetAtrribute("label", project.LabelName);

            if (adaptiveIconsInfo != null || legacyIconsInfo != null || roundIconsInfo != null)
            {
                androidManifest.SetAtrribute("icon", project.IconLabelName);
                androidManifest.SetAtrribute("roundIcon", project.RoundIconLabelName);
            }

            androidManifest.SaveIfModified();
        }

        static void GenerateIconDirectory(List<string> folderNames, string path, Locale locale)
        {
            foreach (var name in folderNames)
            {
                if (!Directory.Exists(Path.Combine(path, name)))
                    Directory.CreateDirectory(Path.Combine(path, name));
            }
        }

        static void GenerateRoundIcons(List<string> folderNames, string path, Locale locale, RoundIconsInfo iconInfo)
        {
            // We get the folder path of the respective density qualifier by combining the android `res' folder path and
            // the localized folder name from the folderNames list, for exmaple "mipmap-ar-hdpi" which is icon folder for arabic Locale
            // The k_RoundIconName variable contains the round icon name which will be used as file name for saving the respective LocalizedTexture to a ".png" file.
            GenerateLocalizedIcon(Path.Combine(Path.Combine(path, folderNames[0]), k_RoundIconName), locale, iconInfo.RoundHdpi);
            GenerateLocalizedIcon(Path.Combine(Path.Combine(path, folderNames[1]), k_RoundIconName), locale, iconInfo.RoundIdpi);
            GenerateLocalizedIcon(Path.Combine(Path.Combine(path, folderNames[2]), k_RoundIconName), locale, iconInfo.RoundMdpi);
            GenerateLocalizedIcon(Path.Combine(Path.Combine(path, folderNames[3]), k_RoundIconName), locale, iconInfo.RoundXhdpi);
            GenerateLocalizedIcon(Path.Combine(Path.Combine(path, folderNames[4]), k_RoundIconName), locale, iconInfo.RoundXXHdpi);
            GenerateLocalizedIcon(Path.Combine(Path.Combine(path, folderNames[5]), k_RoundIconName), locale, iconInfo.RoundXXXHdpi);
        }

        static void GenerateLegacyIcons(List<string> folderNames, string path, Locale locale, LegacyIconsInfo iconInfo)
        {
            // We get the folder path of the respective density qualifier by combining the android `res' folder path and
            // the localized folder name from the folderNames list, for exmaple "mipmap-ar-hdpi" which is icon folder for arabic Locale
            // The k_LegacyIconName variable contains the round icon name which will be used as file name for saving the respective LocalizedTexture to a ".png" file.
            GenerateLocalizedIcon(Path.Combine(Path.Combine(path, folderNames[0]), k_LegacyIconName), locale, iconInfo.LegacyHdpi);
            GenerateLocalizedIcon(Path.Combine(Path.Combine(path, folderNames[1]), k_LegacyIconName), locale, iconInfo.LegacyIdpi);
            GenerateLocalizedIcon(Path.Combine(Path.Combine(path, folderNames[2]), k_LegacyIconName), locale, iconInfo.LegacyMdpi);
            GenerateLocalizedIcon(Path.Combine(Path.Combine(path, folderNames[3]), k_LegacyIconName), locale, iconInfo.LegacyXhdpi);
            GenerateLocalizedIcon(Path.Combine(Path.Combine(path, folderNames[4]), k_LegacyIconName), locale, iconInfo.LegacyXXHdpi);
            GenerateLocalizedIcon(Path.Combine(Path.Combine(path, folderNames[5]), k_LegacyIconName), locale, iconInfo.LegacyXXXHdpi);
        }

        static void GenerateAdaptiveIcons(List<string> folderNames, string path, Locale locale, AdaptiveIconsInfo iconInfo)
        {
            //Adaptive Background Icons
            // We get the folder path of the respective density qualifier by combining the android `res' folder path and
            // the localized folder name from the folderNames list, for exmaple "mipmap-ar-hdpi" which is icon folder for arabic Locale
            // The k_AdaptiveIconBackgroundName variable contains the name for background icon which we use it for our Adaptive icon,
            // which will be used as file name for saving the respective LocalizedTexture to a ".png" file.
            GenerateLocalizedIcon(Path.Combine(Path.Combine(path, folderNames[0]), k_AdaptiveIconBackgroundName), locale, iconInfo.AdaptiveHdpi.Background);
            GenerateLocalizedIcon(Path.Combine(Path.Combine(path, folderNames[1]), k_AdaptiveIconBackgroundName), locale, iconInfo.AdaptiveIdpi.Background);
            GenerateLocalizedIcon(Path.Combine(Path.Combine(path, folderNames[2]), k_AdaptiveIconBackgroundName), locale, iconInfo.AdaptiveMdpi.Background);
            GenerateLocalizedIcon(Path.Combine(Path.Combine(path, folderNames[3]), k_AdaptiveIconBackgroundName), locale, iconInfo.AdaptiveXhdpi.Background);
            GenerateLocalizedIcon(Path.Combine(Path.Combine(path, folderNames[4]), k_AdaptiveIconBackgroundName), locale, iconInfo.AdaptiveXXHdpi.Background);
            GenerateLocalizedIcon(Path.Combine(Path.Combine(path, folderNames[5]), k_AdaptiveIconBackgroundName), locale, iconInfo.AdaptiveXXXHdpi.Background);

            //Adaptive Foreground Icons
            // We get the folder path of the respective density qualifier by combining the android `res' folder path and
            // the localized folder name from the folderNames list, for exmaple "mipmap-ar-hdpi" which is icon folder for arabic Locale
            // The k_AdaptiveIconForegroundName variable contains the name for foreground icon which we use it for our Adaptive icon.
            // which will be used as file name for saving the respective LocalizedTexture to a ".png" file.
            GenerateLocalizedIcon(Path.Combine(Path.Combine(path, folderNames[0]), k_AdaptiveIconForegroundName), locale, iconInfo.AdaptiveHdpi.Foreground);
            GenerateLocalizedIcon(Path.Combine(Path.Combine(path, folderNames[1]), k_AdaptiveIconForegroundName), locale, iconInfo.AdaptiveIdpi.Foreground);
            GenerateLocalizedIcon(Path.Combine(Path.Combine(path, folderNames[2]), k_AdaptiveIconForegroundName), locale, iconInfo.AdaptiveMdpi.Foreground);
            GenerateLocalizedIcon(Path.Combine(Path.Combine(path, folderNames[3]), k_AdaptiveIconForegroundName), locale, iconInfo.AdaptiveXhdpi.Foreground);
            GenerateLocalizedIcon(Path.Combine(Path.Combine(path, folderNames[4]), k_AdaptiveIconForegroundName), locale, iconInfo.AdaptiveXXHdpi.Foreground);
            GenerateLocalizedIcon(Path.Combine(Path.Combine(path, folderNames[5]), k_AdaptiveIconForegroundName), locale, iconInfo.AdaptiveXXXHdpi.Foreground);

            WriteAdaptiveIconsXML(Path.Combine(Path.Combine(path, folderNames[6]), k_AdaptiveIcon_AppIconInfo));
            WriteAdaptiveIconsXML(Path.Combine(Path.Combine(path, folderNames[6]), k_AdaptiveIcon_AppRoundIconInfo));
        }

        static void GenerateLocalizedIcon(string filePath, Locale locale, LocalizedTexture localizedTexture)
        {
            if (localizedTexture.IsEmpty)
                return;

            var tableCollection = LocalizationEditorSettings.GetAssetTableCollection(localizedTexture.TableReference);
            var table = tableCollection?.GetTable(locale.Identifier) as AssetTable;
            var entry = table?.GetEntryFromReference(localizedTexture.TableEntryReference);

            if (entry == null || string.IsNullOrWhiteSpace(entry.LocalizedValue))
            {
                var fallBack = FallbackLocaleHelper.GetLocaleFallback(locale);
                if (fallBack != null)
                {
                    GenerateLocalizedIcon(filePath, fallBack, localizedTexture);
                    return;
                }

                Debug.LogWarning($"Could not find a localized Icons for {locale} from {localizedTexture}");
                return;
            }
            var iconPath = AssetDatabase.GUIDToAssetPath(entry.Guid);
            if (string.IsNullOrEmpty(iconPath))
                return;

            if (File.Exists(filePath))
                File.Delete(filePath);

            var texture = AssetDatabase.LoadAssetAtPath(iconPath, typeof(Texture2D)) as Texture2D;
            byte[] bytes = texture.EncodeToPNG();

            //Saving the Icon as ".png" file to gradle res folder
            File.WriteAllBytes(filePath, bytes);
        }

        static void WriteAdaptiveIconsXML(string filePath)
        {
            using (var stream = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                stream.WriteLine(
                    $@"<?xml version=""1.0"" encoding=""utf-8""?>" +
                    "<!--" +
                    "\n" +
                    $"\tThis file was auto-generated by {LocalizationPackageInfo.name}\n" +
                    $"\tVersion {LocalizationPackageInfo.version}\n" +
                    $"\tChanges to this file may cause incorrect behavior and will be lost if the project is rebuilt.\n" +
                    $"-->" +
                    "\n" +
                    $@"<adaptive-icon xmlns:android=""http://schemas.android.com/apk/res/android"">
                            <background android:drawable=""@mipmap/ic_launcher_background""/>
                            <foreground android:drawable=""@mipmap/ic_launcher_foreground""/>
                        </adaptive-icon> ");
            }
        }

        static void GenerateLocalizedXmlFile(string valueName, string filePath, Locale locale, AppInfo appinfo)
        {
            var localizedString = appinfo.DisplayName;
            if (localizedString.IsEmpty)
                return;

            var tableCollection = LocalizationEditorSettings.GetStringTableCollection(localizedString.TableReference);
            var table = tableCollection?.GetTable(locale.Identifier) as StringTable;
            var entry = table?.GetEntryFromReference(localizedString.TableEntryReference);

            if (entry == null || string.IsNullOrWhiteSpace(entry.LocalizedValue))
            {
                // Use fallback?
                var fallBack = FallbackLocaleHelper.GetLocaleFallback(locale);
                if (fallBack != null)
                {
                    GenerateLocalizedXmlFile(valueName, filePath, fallBack, appinfo);
                    return;
                }

                Debug.LogWarning($"{valueName}: Could not find a localized value for {locale} from {localizedString}");
                return;
            }
            Debug.Assert(!entry.IsSmart, $"Localized App Values ({valueName}) do not support Smart Strings - {localizedString}");
            Debug.Assert(!entry.LocalizedValue.Contains("'"), $"Localized App Value ({valueName}) does not support Single Quote. \nEntry contains invalid character: {localizedString}\n{entry.LocalizedValue}");

            using (var stream = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                stream.WriteLine(
                    $@"<?xml version=""1.0"" encoding=""utf-8""?>" +
                    "<!--" +
                    "\n" +
                    $"\t{k_InfoFile}\n" +
                    $"\tThis file was auto-generated by {LocalizationPackageInfo.name}\n" +
                    $"\tVersion {LocalizationPackageInfo.version}\n" +
                    $"\tChanges to this file may cause incorrect behavior and will be lost if the project is rebuilt.\n" +
                    $"-->" +
                    "\n" +
                    $@"<resources>
                       <string name=""app_name""> {entry.LocalizedValue} </string>
                       </resources>");
            }
        }
    }
}
#endif
