using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using UnityEditor.Localization.UI;
using UnityEditor.Localization.UI.Toolkit;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace UnityEditor.Localization.Tests.UI
{
    public class LocalesProviderPropertyDrawerTests : WrapperWindowFixture
    {
        class Fixture : ScriptableObject
        {
            public LocalesProvider provider = new LocalesProvider();
        }

        class FakeLocaleProvider : LocalizationEditorSettings
        {
            public List<Locale> TestLocales { get; set; } = new List<Locale>();

            internal override ReadOnlyCollection<Locale> GetLocalesInternal() => TestLocales.AsReadOnly();

            internal override void AddLocaleInternal(Locale locale, bool createUndo)
            {
                TestLocales.Add(locale);
                EditorEvents.RaiseLocaleAdded(locale);
            }

            internal override void RemoveLocaleInternal(Locale locale, bool createUndo)
            {
                TestLocales.Remove(locale);
                EditorEvents.RaiseLocaleRemoved(locale);
            }

            public void DestroyLocales()
            {
                foreach (var locale in TestLocales)
                {
                    Object.DestroyImmediate(locale);
                }
            }
        }

        Fixture m_Instance;
        SerializedObject m_ScriptableObject;
        SerializedProperty m_Property;
        FakeLocaleProvider m_FakeLocaleProvider;
        LocalesProviderPropertyDrawer m_PropertyDrawer;

        [SetUp]
        public void Setup()
        {
            m_Instance = ScriptableObject.CreateInstance<Fixture>();
            m_ScriptableObject = new SerializedObject(m_Instance);
            m_Property = m_ScriptableObject.FindProperty("provider");
            Assert.That(m_ScriptableObject, Is.Not.Null);
            Assert.That(m_Property, Is.Not.Null);

            m_FakeLocaleProvider = new FakeLocaleProvider();
            LocalizationEditorSettings.Instance = m_FakeLocaleProvider;
            LocalizationEditorSettings.AddLocale(Locale.CreateLocale(SystemLanguage.English));
            LocalizationEditorSettings.AddLocale(Locale.CreateLocale(SystemLanguage.French));
            LocalizationEditorSettings.AddLocale(Locale.CreateLocale(SystemLanguage.Korean));

            m_PropertyDrawer = new LocalesProviderPropertyDrawer();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(m_Instance);
            LocalizationEditorSettings.Instance = null;
            m_FakeLocaleProvider.DestroyLocales();
        }

        void CheckListContainsProjectLocales(WrapperWindow wnd)
        {
            var listScrollView = wnd.rootVisualElement.Q<ReorderableList>().Q<ScrollView>();
            var locales = LocalizationEditorSettings.GetLocales().ToList();

            Assert.AreEqual(locales.Count, listScrollView.childCount, "Expected list size to match the number of project locales.");

            int localeCount = locales.Count;
            for (int i = 0; i < localeCount; ++i)
            {
                var item = listScrollView[i];
                var name = item.Q<TextField>("name");
                var code = item.Q<TextField>("code");

                Assert.NotNull(name, "Could not find name field.");
                Assert.NotNull(code, "Could not find code field.");

                var matchingLocale = locales.FirstOrDefault(l => l.name == name.value);
                Assert.NotNull(matchingLocale, $"Could not find a matching locale for {name.value}({code.value})");
                locales.Remove(matchingLocale);
            }

            Assert.That(locales, Is.Empty, "Expected all project locales to be in the ListView but they were not.");
        }

        [Ignore("Failing due to UI Toolkit changes.")]
        [UnityTest]
        public IEnumerator ListViewContainsAllProjectLocales()
        {
            WrapperWindow window = GetWindow((wnd) =>
            {
                CheckListContainsProjectLocales(wnd);
                return true;
            });

            var root = m_PropertyDrawer.CreatePropertyGUI(m_ScriptableObject.FindProperty("provider"));
            window.rootVisualElement.Add(root);

            yield return null;
            Assert.That(window.TestCompleted, Is.True);
        }

        [Ignore("Failing due to UI Toolkit changes.")]
        [UnityTest]
        public IEnumerator ListViewUpdatesWhenLocaleIsAdded()
        {
            bool localeAdded = false;
            WrapperWindow window = GetWindow((wnd) =>
            {
                // Add a new Locale
                if (!localeAdded)
                {
                    var newLocale = Locale.CreateLocale(SystemLanguage.Hebrew);
                    LocalizationEditorSettings.AddLocale(newLocale);
                    Assert.That(m_FakeLocaleProvider.TestLocales, Does.Contain(newLocale));
                    localeAdded = true;
                    return false;
                }
                else
                {
                    CheckListContainsProjectLocales(wnd);
                    return true;
                }
            });

            var root = m_PropertyDrawer.CreatePropertyGUI(m_ScriptableObject.FindProperty("provider"));
            window.rootVisualElement.Add(root);

            yield return null;
            Assert.That(window.TestCompleted, Is.True);
        }

        [Ignore("Failing due to UI Toolkit changes.")]
        [UnityTest]
        public IEnumerator ListViewUpdatesWhenLocaleIsRemoved()
        {
            WrapperWindow window = GetWindow((wnd) =>
            {
                // Remove a locale
                var localeToRemove = m_FakeLocaleProvider.TestLocales[0];
                LocalizationEditorSettings.RemoveLocale(localeToRemove);
                Assert.That(m_FakeLocaleProvider.TestLocales, Does.Not.Contains(localeToRemove));
                CheckListContainsProjectLocales(wnd);
                return true;
            });

            var root = m_PropertyDrawer.CreatePropertyGUI(m_ScriptableObject.FindProperty("provider"));
            window.rootVisualElement.Add(root);

            yield return null;
            Assert.That(window.TestCompleted, Is.True);
        }
    }
}
