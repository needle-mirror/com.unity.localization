using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Settings;

namespace UnityEditor.Localization.UI
{
    class LocaleGeneratorWindow : EditorWindow
    {
        internal enum LocaleSource
        {
            CultureInfo,
            SystemLanguage
        }

        class Texts
        {
            public GUIContent generateLocalesButton = new GUIContent("Generate Locales");
            public GUIContent localeSource = new GUIContent("Locale Source", "Source data for generating the locales");
            public const string progressTitle = "Generating Locales";
            public const string saveDialog = "Save locales to folder";

            public GUIContent[] toolbarButtons =
            {
                new GUIContent("Select All", "Select all visible locales"),
                new GUIContent("Deselect All", "Deselect all visible locales")
            };
        }

        static Texts s_Texts;

        const float k_WindowFooterHeight = 150;

        internal LocaleSource m_LocaleSource;

        [SerializeField]
        internal SearchField m_SearchField;
        [SerializeField]
        internal LocaleGeneratorListView m_ListView;

        //[MenuItem("Window/Localization/Locale Generator")]
        public static void ShowWindow()
        {
            var window = (LocaleGeneratorWindow)GetWindow(typeof(LocaleGeneratorWindow));
            window.titleContent = new GUIContent("Locale Generator", EditorIcons.LocalizationSettings.image);
            window.minSize = new Vector2(500, 500);
            window.ShowUtility();
        }

        void OnEnable()
        {
            m_ListView = new LocaleGeneratorListView();
            m_ListView.Items = GenerateLocaleChoices(m_LocaleSource);
            m_SearchField = new SearchField();
            m_SearchField.downOrUpArrowKeyPressed += m_ListView.SetFocusAndEnsureSelectedItem;
        }

        void OnGUI()
        {
            if (s_Texts == null)
                s_Texts = new Texts();

            EditorGUI.BeginChangeCheck();
            var newSource = (LocaleSource)EditorGUILayout.EnumPopup(s_Texts.localeSource, m_LocaleSource);
            if (EditorGUI.EndChangeCheck() && m_LocaleSource != newSource)
            {
                m_LocaleSource = newSource;
                m_ListView.Items = GenerateLocaleChoices(m_LocaleSource);
            }

            DrawLocaleList();

            using (new EditorGUI.DisabledScope(m_ListView.SelectedCount == 0))
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(s_Texts.generateLocalesButton, GUILayout.Width(180)))
                {
                    ExportSelectedLocales();
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        void DrawLocaleList()
        {
            m_ListView.searchString = m_SearchField.OnToolbarGUI(m_ListView.searchString);
            var rect = EditorGUILayout.GetControlRect(false, position.height - k_WindowFooterHeight);
            m_ListView.OnGUI(rect);

            var selection = GUILayout.Toolbar(-1, s_Texts.toolbarButtons, EditorStyles.toolbarButton);
            if (selection >= 0)
            {
                m_ListView.SelectLocales(selection == 0);
            }
        }

        internal void ExportSelectedLocales()
        {
            var path = EditorUtility.SaveFolderPanel(Texts.saveDialog, "Assets/", "");
            if (!string.IsNullOrEmpty(path))
                ExportSelectedLocales(path);
        }

        internal void ExportSelectedLocales(string path)
        {
            try
            {
                // Generate the locale assets
                EditorUtility.DisplayProgressBar(Texts.progressTitle, "Creating Locale Objects", 0);
                var localeDict = new Dictionary<LocaleIdentifier, Locale>(); // Used for quick look up of parents
                var locales = new List<Locale>();
                var selectedIdentifiers = m_ListView.GetSelectedLocales();

                foreach (var selectedIdentifier in selectedIdentifiers)
                {
                    var locale = CreateInstance<Locale>();
                    locale.Identifier = selectedIdentifier;
                    locale.name = selectedIdentifier.CultureInfo.EnglishName;
                    locales.Add(locale);
                    localeDict[selectedIdentifier] = locale;
                }

                // When checking for fallbacks we also need to take into account the existing locales
                var allLocales = new List<Locale>(locales);
                allLocales.AddRange(LocalizationEditorSettings.GetLocales());

                // Set up fallbacks
                foreach (var locale in allLocales)
                {
                    var localeParentCultureInfo = locale.Identifier.CultureInfo.Parent;
                    Locale foundParent = null;
                    while (localeParentCultureInfo != CultureInfo.InvariantCulture && foundParent == null)
                    {
                        localeDict.TryGetValue(localeParentCultureInfo.Name, out foundParent);
                        localeParentCultureInfo = localeParentCultureInfo.Parent;
                    }

                    if (foundParent != null)
                    {
                        locale.Metadata.AddMetadata(new FallbackLocale(foundParent));
                        EditorUtility.SetDirty(locale);
                    }
                }

                // Export the assets
                AssetDatabase.StartAssetEditing(); // Batch the assets into a single asset operation
                var relativePath = MakePathRelative(path);
                for (int i = 0; i < locales.Count; ++i)
                {
                    var locale = locales[i];
                    EditorUtility.DisplayProgressBar(Texts.progressTitle, "Creating Asset " + locale.name, i / (float)locales.Count);
                    var assetPath = Path.Combine(relativePath, $"{locale.name} ({locale.Identifier.Code}).asset");
                    assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
                    AssetDatabase.CreateAsset(locale, assetPath);
                }

                AssetDatabase.StopAssetEditing();

                Close();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        static string MakePathRelative(string path)
        {
            if (path.Contains(Application.dataPath))
            {
                var length = Application.dataPath.Length - "Assets".Length;
                return path.Substring(length, path.Length - length);
            }

            return path;
        }

        static List<LocaleIdentifier> GenerateLocaleChoices(LocaleSource source)
        {
            var locales = new List<LocaleIdentifier>();

            if (source == LocaleSource.CultureInfo)
            {
                var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

                for (int i = 0; i < cultures.Length; ++i)
                {
                    var cultureInfo = cultures[i];

                    if (cultureInfo.LCID == CultureInfo.InvariantCulture.LCID)
                        continue;

                    // Ignore legacy cultures
                    if (cultureInfo.EnglishName.Contains("Legacy"))
                        continue;

                    locales.Add(new LocaleIdentifier(cultureInfo));
                }
            }
            else
            {
                for (int i = 0; i < (int)SystemLanguage.Unknown; ++i)
                {
                    locales.Add(new LocaleIdentifier((SystemLanguage)i));
                }
            }

            return locales;
        }
    }
}