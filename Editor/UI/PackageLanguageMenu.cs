#if PACKAGE_DEVICE_SIMULATOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.DeviceSimulator;
using UnityEngine.Localization.Settings;
using UnityEditor.UIElements;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEditor.Localization.UI
{
    /// <summary>
    /// Provides a popup menu in the Device Simulator window for changing the <see cref="LocalizationSettings.SelectedLocale"/>.
    /// </summary>
    [InitializeOnLoad]
    class PackageLanguageMenu : PopupField<Locale>, IDeviceSimulatorExtension
    {
        static List<Locale> s_Locales = new List<Locale>();
        const string k_Title = "Localization";

        public string extensionTitle { get { return k_Title; } }

        public PackageLanguageMenu() :
            base(GetChoices(), 0)
        {
            focusable = false;
            labelElement.style.minWidth = 60;
            labelElement.style.maxWidth = 60;
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
        }

        void PlayModeStateChanged(PlayModeStateChange obj)
        {
            if (obj == PlayModeStateChange.EnteredPlayMode && LocalizationEditorSettings.ShowLocaleMenuInGameView)
            {
                var selectedLocaleOperation = LocalizationSettings.SelectedLocaleAsync;
                if (selectedLocaleOperation.IsDone)
                    OnLanguageChanged(selectedLocaleOperation.Result);
                else
                    selectedLocaleOperation.Completed += OnLanguageChanged;

                SetEnabled(true);
            }
            else
            {
                if (LocalizationSettings.HasSettings)
                    LocalizationSettings.SelectedLocaleChanged -= OnLanguageChanged;

                SetEnabled(false);
            }
        }

        void OnLanguageChanged(AsyncOperationHandle<Locale> operation)
        {
            OnLanguageChanged(operation.Result);
        }

        void OnLanguageChanged(Locale locale)
        {
            SetValueWithoutNotify(locale);
        }

        static List<Locale> GetChoices()
        {
            s_Locales.Clear();
            s_Locales.AddRange(LocalizationEditorSettings.GetLocales());
            return s_Locales;
        }

        public void OnExtendDeviceSimulator(VisualElement visualElement)
        {
            var toolbar = new VisualElement();
            var menu = new PackageLanguageMenu();
            menu.RegisterValueChangedCallback((evt) => LocalizationSettings.SelectedLocale = evt.newValue);
            menu.SetEnabled(false);
            toolbar.Add(menu);

            var toggle = visualElement.Q<Toggle>();
            var label = new Label { text = "Locale" };
            toggle?.Q<VisualElement>("unity-checkmark")?.parent?.Add(label);
            visualElement.Add(toolbar);
        }
    }
}

#endif
