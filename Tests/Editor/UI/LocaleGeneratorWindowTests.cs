using System.Collections.ObjectModel;
using System.IO;
using NUnit.Framework;
using UnityEditor.Localization.UI;
using UnityEngine;
using UnityEngine.Localization;
using static UnityEditor.Localization.UI.LocaleGeneratorListView;

namespace UnityEditor.Localization.Tests.UI
{
    public class LocaleGeneratorWindowTests
    {
        LocaleGeneratorWindow m_Window;

        readonly string testPath = "Assets/" + nameof(LocaleGeneratorWindowTests);

        class ProjectWithNoLocales : LocalizationEditorSettings
        {
            internal override ReadOnlyCollection<Locale> GetLocalesInternal()
            {
                return new ReadOnlyCollection<Locale>(new Locale[0]);
            }
        }

        [SetUp]
        public void Setup()
        {
            LocalizationEditorSettings.Instance = new ProjectWithNoLocales();
            m_Window = EditorWindow.CreateInstance<LocaleGeneratorWindow>();
            m_Window.ShowUtility();

            if (Directory.Exists(testPath))
                Directory.Delete(testPath, true);

            AssetDatabase.CreateFolder("Assets", nameof(LocaleGeneratorWindowTests));
        }

        [TearDown]
        public void Teardown()
        {
            LocalizationEditorSettings.Instance = null;
            m_Window.Close();
            Object.DestroyImmediate(m_Window);
            AssetDatabase.DeleteAsset(testPath);
        }

        [Test]
        public void GeneratingLocalesDoesNotCauseCrash()
        {
            const int selectCount = 5;
            var rows = m_Window.m_ListView.GetRows();
            for (int i = 0; i < rows.Count; ++i)
            {
                var row = (LocaleTreeViewItem)rows[i];
                row.enabled = i < selectCount;
            }
            m_Window.m_ListView.SelectedCount = selectCount;
            LocaleGeneratorWindow.ExportSelectedLocales(testPath, m_Window.m_ListView.GetSelectedLocales());
        }
    }
}
