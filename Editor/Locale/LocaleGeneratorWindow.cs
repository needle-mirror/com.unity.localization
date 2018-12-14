using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Localization;

namespace UnityEditor.Localization
{
    class LocaleGeneratorWindow : EditorWindow
    {
        enum LocaleSource
        {
            CultureInfo,
            SystemLanguage
        }

        class Texts
        {
            public GUIContent baseClass = new GUIContent("Base Class", "The base class to generate the Locale assets with. Must inherit from Locale.");
            public GUIContent generateLocalesAndAddButton = new GUIContent("Generate and Add Locales", "Generates locale assets and assigns them to the Available Locales");
            public GUIContent generateLocalesButton = new GUIContent("Generate Locales");
            public GUIContent localeSource = new GUIContent("Locale Source", "Source data for generating the locales");
            public readonly string progressTitle = "Generating Locales";
            public readonly string saveDialog = "Save locales to folder";

            public GUIContent[] toolbarButtons =
            {
                new GUIContent("Select All", "Select all visible locales"),
                new GUIContent("Deselect All", "Deselect all visible locales")
            };
        }

        static Texts s_Texts;

        const float k_WindowFooterHeight = 150;

        LocaleSource m_LocaleSource;

        [SerializeField]
        SearchField m_SearchField;
        [SerializeField]
        LocaleGeneratorListView m_ListView;
        [SerializeField]
        int m_SelectedClass;

        Action<Locale> m_OnLocaleCreated;

        string[] m_LocaleTypesNames;
        Type[] m_LocaleTypes;

        [MenuItem("Window/Localization/Locale Generator")]
        public static void ShowWindow()
        {
            var window = (LocaleGeneratorWindow)GetWindow(typeof(LocaleGeneratorWindow));
            window.m_OnLocaleCreated = null;
            window.titleContent = new GUIContent("Locale Generator");
            window.minSize = new Vector2(500, 500);
            window.ShowUtility();
        }

        public static void ShowWindow(Action<Locale> onLocaleCreated)
        {
            var window = (LocaleGeneratorWindow)GetWindow(typeof(LocaleGeneratorWindow));
            window.m_OnLocaleCreated = onLocaleCreated;
            window.titleContent = new GUIContent("Locale Generator");
            window.minSize = new Vector2(500, 500);
            window.ShowUtility();
        }

        void OnEnable()
        {
            m_ListView = new LocaleGeneratorListView();
            m_ListView.Items = GenerateLocaleChoices(m_LocaleSource);
            m_SearchField = new SearchField();
            m_SearchField.downOrUpArrowKeyPressed += m_ListView.SetFocusAndEnsureSelectedItem;

            // Find Locale types
            var foundNames = new List<string>();
            var foundTypes = new List<Type>();
            foundNames.Add("Locale");
            foundTypes.Add(typeof(Locale));
            AssemblyScanner.FindSubclasses<Locale>(foundTypes, foundNames, true);
            m_LocaleTypesNames = foundNames.ToArray();
            m_LocaleTypes = foundTypes.ToArray();
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

            m_SelectedClass = EditorGUILayout.Popup(s_Texts.baseClass.text, m_SelectedClass, m_LocaleTypesNames);

            using (new EditorGUI.DisabledScope(m_ListView.SelectedCount == 0))
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(m_OnLocaleCreated != null ? s_Texts.generateLocalesAndAddButton : s_Texts.generateLocalesButton, GUILayout.Width(180)))
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

        void ExportSelectedLocales()
        {
            string path = EditorUtility.SaveFolderPanel(s_Texts.saveDialog, "Assets/", "");
            if (string.IsNullOrEmpty(path))
                return;

            try
            {
                // Generate the locale assets
                EditorUtility.DisplayProgressBar(s_Texts.progressTitle, "Creating Locale Objects", 0);
                var localeDict = new Dictionary<LocaleIdentifier, Locale>(); // Used for quick look up of parents
                var locales = new List<Locale>();
                var selectedIdentifiers = m_ListView.GetSelectedLocales();
                var localeType = m_LocaleTypes[m_SelectedClass];

                foreach (var selectedIdentifier in selectedIdentifiers)
                {
                    var locale = CreateInstance(localeType) as Locale;
                    if (locale == null)
                    {
                        Debug.LogError("Could not create a Locale of type " + localeType);
                        return;
                    }

                    locale.Identifier = selectedIdentifier;
                    locale.name = selectedIdentifier.CultureInfo.EnglishName;
                    locales.Add(locale);
                    localeDict[selectedIdentifier] = locale;
                }

                // Set up parents using available locales
                foreach (var locale in locales)
                {
                    CultureInfo localeParentCultureInfo = locale.Identifier.CultureInfo.Parent;
                    Locale foundParent = null;
                    while (localeParentCultureInfo != CultureInfo.InvariantCulture && foundParent == null)
                    {
                        localeDict.TryGetValue(localeParentCultureInfo.Name, out foundParent);
                        localeParentCultureInfo = localeParentCultureInfo.Parent;
                    }

                    locale.FallbackLocale = foundParent;
                }

                // Export the assets
                AssetDatabase.StartAssetEditing(); // Batch the assets into a single asset operation
                var relativePath = MakePathRelative(path);
                for (int i = 0; i < locales.Count; ++i)
                {
                    var locale = locales[i];
                    EditorUtility.DisplayProgressBar(s_Texts.progressTitle, "Creating Asset " + locale.name, i / (float)locales.Count);
                    var assetPath = Path.Combine(relativePath, string.Format("{0} ({1}).asset", locale.name, locale.Identifier.Code));
                    assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
                    AssetDatabase.CreateAsset(locale, assetPath);
                }

                AssetDatabase.StopAssetEditing();

                // Assign to AvailableLocales
                if (m_OnLocaleCreated != null)
                {
                    foreach (var locale in locales)
                    {
                        m_OnLocaleCreated(locale);
                    }
                }

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
                int length = Application.dataPath.Length - "Assets".Length;
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