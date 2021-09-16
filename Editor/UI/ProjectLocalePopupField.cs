using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

namespace UnityEditor.Localization.UI
{
    /// <summary>
    /// Field to use to select a <see cref="Locale"/> from the project.
    /// </summary>
    public class ProjectLocalePopupField : PopupField<Locale>
    {
        static List<Locale> s_Locales = new List<Locale>();

        public new class UxmlFactory : UxmlFactory<ProjectLocalePopupField, UxmlTraits> {}

        public new class UxmlTraits : PopupField<Locale>.UxmlTraits {}

        /// <summary>
        /// Creates a new instance of the field.
        /// </summary>
        public ProjectLocalePopupField() :
            base(GetChoices(), 0, LocaleLabel, LocaleLabel)
        {
            formatListItemCallback = LocaleLabel;
            formatSelectedValueCallback = LocaleLabel;

            LocalizationEditorSettings.EditorEvents.LocaleAdded += LocaleAddedToProject;
            LocalizationEditorSettings.EditorEvents.LocaleRemoved += LocaleRemovedFromProject;

            EditorApplication.playModeStateChanged += PlayModeStateChanged;

            if (!LocalizationSettings.HasSettings)
                return;

            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;

            this.RegisterValueChangedCallback(evt => LocalizationSettings.SelectedLocale = evt.newValue);

            if (LocalizationSettings.SelectedLocaleAsync.IsDone)
            {
                if (LocalizationSettings.SelectedLocaleAsync.Result != null)
                    SetValueWithoutNotify(LocalizationSettings.SelectedLocaleAsync.Result);
            }
            else
            {
                LocalizationSettings.SelectedLocaleAsync.Completed += op =>
                {
                    if (op.Result != null)
                        SetValueWithoutNotify(op.Result);
                };
            }
        }

        ~ProjectLocalePopupField()
        {
            LocalizationEditorSettings.EditorEvents.LocaleAdded -= LocaleAddedToProject;
            LocalizationEditorSettings.EditorEvents.LocaleRemoved -= LocaleRemovedFromProject;
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
        }

        void PlayModeStateChanged(PlayModeStateChange obj)
        {
            if (obj == PlayModeStateChange.EnteredEditMode)
            {
                SetValueWithoutNotify(LocalizationSettings.SelectedLocale);
            }
            else if (obj == PlayModeStateChange.EnteredPlayMode)
            {
                LocalizationSettings.InitializationOperation.Completed += op =>
                {
                    SetValueWithoutNotify(LocalizationSettings.SelectedLocale);
                };
            }
        }

        static string LocaleLabel(Locale locale)
        {
            if (locale == null)
                return "None";
            return locale.ToString();
        }

        static List<Locale> GetChoices()
        {
            s_Locales.Clear();
            s_Locales.Add(null);
            s_Locales.AddRange(LocalizationEditorSettings.GetLocales());
            s_Locales.AddRange(LocalizationEditorSettings.GetPseudoLocales());
            return s_Locales;
        }

        void LocaleRemovedFromProject(Locale locale)
        {
            if (value == locale)
                value = null;
            GetChoices();
        }

        void LocaleAddedToProject(Locale locale)
        {
            GetChoices();
        }

        void OnLanguageChanged(Locale locale)
        {
            if (!GetChoices().Contains(locale))
                return;

            SetValueWithoutNotify(locale);
        }

        /// <summary>
        /// Supports pressing up and down to change selected Locale.
        /// </summary>
        /// <param name="evt"></param>
        protected override void ExecuteDefaultActionAtTarget(EventBase evt)
        {
            base.ExecuteDefaultActionAtTarget(evt);

            if (evt == null)
                return;

            // Allow users to press up and down arrows to quickly change locale.
            KeyDownEvent kde = (evt as KeyDownEvent);
            if (kde != null)
            {
                if (kde.keyCode == KeyCode.UpArrow)
                {
                    var newIndex = index - 1;
                    if (newIndex < 0)
                        newIndex = s_Locales.Count - 1;
                    index = newIndex;

                    // Indicate the event was used.
                    // Prevents the OS from making the alert sound when an input was not handled.
                    evt.StopPropagation();
                }
                else if (kde.keyCode == KeyCode.DownArrow)
                {
                    var newIndex = index + 1;
                    if (newIndex >= s_Locales.Count)
                        newIndex = 0;
                    index = newIndex;

                    // Indicate the event was used.
                    // Prevents the OS from making the alert sound when an input was not handled.
                    evt.StopPropagation();
                }
            }
        }
    }
}
