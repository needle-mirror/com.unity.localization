using System.IO;
using NUnit.Framework;
using UnityEditor.Localization.UI;
using UnityEngine;
using static UnityEditor.Localization.UI.LocaleGeneratorListView;

namespace UnityEditor.Localization.Tests.UI
{
    public class LocaleGeneratorWindowTests
    {
        LocaleGeneratorWindow m_Window;

        readonly string testPath = "Assets/" + nameof(LocaleGeneratorWindowTests);

        [SetUp]
        public void Setup()
        {
            m_Window = EditorWindow.CreateInstance<LocaleGeneratorWindow>();
            m_Window.ShowUtility();
            Assert.IsFalse(Directory.Exists(testPath), "Test directory already exists.");
            AssetDatabase.CreateFolder("Assets", nameof(LocaleGeneratorWindowTests));
        }

        [TearDown]
        public void Teardown()
        {
            m_Window.Close();
            Object.DestroyImmediate(m_Window);
            Directory.Delete(testPath, true);
            AssetDatabase.Refresh();
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
            m_Window.ExportSelectedLocales(testPath);
        }
    }
}
