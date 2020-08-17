using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using UnityEditor.Localization.UI;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.TestTools;

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

            internal protected override ReadOnlyCollection<Locale> GetLocalesInternal() => TestLocales.AsReadOnly();

            protected override void AddLocaleInternal(Locale locale, bool createUndo)
            {
                TestLocales.Add(locale);
                EditorEvents.RaiseLocaleAdded(locale);
            }

            protected override void RemoveLocaleInternal(Locale locale, bool createUndo)
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

        void CheckListContainsProjectLocales()
        {
            var items = m_PropertyDrawer.LocalesList.list;
            var locales = LocalizationEditorSettings.GetLocales().ToList();
            foreach (SerializedLocaleItem serializedLocale in items)
            {
                Assert.That(serializedLocale, Is.Not.Null);

                // Check the locale exists in the project
                Assert.That(locales, Does.Contain(serializedLocale.Reference), "Expected the Locale in the ListView to be in the project but it was not.");

                // Remove the locale, it should only be in the list once and we want to know whats leftover.
                locales.Remove(serializedLocale.Reference);
            }

            Assert.That(locales, Is.Empty, "Expected all project locales to be in the ListView but they were not.");
        }

        [UnityTest]
        public IEnumerator ListViewContainsAllProjectLocales()
        {
            WrapperWindow window = GetWindow((wnd) =>
            {
                // Perform an update
                var height = m_PropertyDrawer.GetPropertyHeight(m_Property, GUIContent.none);
                m_PropertyDrawer.OnGUI(new Rect(wnd.position.x, wnd.position.y, wnd.position.width, height), m_Property, GUIContent.none);

                CheckListContainsProjectLocales();

                return true;
            });

            yield return null;
            Assert.That(window.TestCompleted, Is.True);
        }

        [UnityTest]
        public IEnumerator ListViewUpdatesWhenLocaleIsAdded()
        {
            WrapperWindow window = GetWindow((wnd) =>
            {
                // Perform an update
                var height = m_PropertyDrawer.GetPropertyHeight(m_Property, GUIContent.none);
                m_PropertyDrawer.OnGUI(new Rect(wnd.position.x, wnd.position.y, wnd.position.width, height), m_Property, GUIContent.none);

                // Add a new Locale
                var newLocale = Locale.CreateLocale(SystemLanguage.Hebrew);
                LocalizationEditorSettings.AddLocale(newLocale);

                // Update again, it should not contain the new element
                height = m_PropertyDrawer.GetPropertyHeight(m_Property, GUIContent.none);
                m_PropertyDrawer.OnGUI(new Rect(wnd.position.x, wnd.position.y, wnd.position.width, height), m_Property, GUIContent.none);

                Assert.That(m_FakeLocaleProvider.TestLocales, Does.Contain(newLocale));
                CheckListContainsProjectLocales();

                return true;
            });

            yield return null;
            Assert.That(window.TestCompleted, Is.True);
        }

        [UnityTest]
        public IEnumerator ListViewUpdatesWhenLocaleIsRemoved()
        {
            WrapperWindow window = GetWindow((wnd) =>
            {
                // Perform an update
                var height = m_PropertyDrawer.GetPropertyHeight(m_Property, GUIContent.none);
                m_PropertyDrawer.OnGUI(new Rect(0, 0, 1000, height), m_Property, GUIContent.none);

                // Remove a locale
                var localeToRemove = m_FakeLocaleProvider.TestLocales[0];
                LocalizationEditorSettings.RemoveLocale(localeToRemove);
                Assert.That(m_FakeLocaleProvider.TestLocales, Does.Not.Contains(localeToRemove));

                // Update again, it should not contain the new element
                height = m_PropertyDrawer.GetPropertyHeight(m_Property, GUIContent.none);
                m_PropertyDrawer.OnGUI(new Rect(0, 0, 1000, height), m_Property, GUIContent.none);
                CheckListContainsProjectLocales();

                return true;
            });

            yield return null;
            Assert.That(window.TestCompleted, Is.True);
        }
    }
}
