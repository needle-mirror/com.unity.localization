﻿using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;

namespace UnityEditor.Localization.UI
{
    /// <summary>
    /// Provides a popup menu in the GameView for changing the <see cref="LocalizationSettings.SelectedLocale"/>.
    /// </summary>
    [InitializeOnLoad]
    class GameViewLanguageMenu : PopupField<Locale>
    {
        static List<(VisualElement toolbar, GameViewLanguageMenu menu)> s_GameViews = new List<(VisualElement toolbar, GameViewLanguageMenu menu)>();
        static List<Locale> s_Locales = new List<Locale>();

        static GameViewLanguageMenu()
        {
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
            if (Application.isPlaying && LocalizationSettings.HasSettings && LocalizationEditorSettings.ShowLocaleMenuInGameView)
                Show();
        }

        public GameViewLanguageMenu() : 
            base(GetChoices(), 0)
        {
            focusable = false;
            labelElement.style.minWidth = 60;
            labelElement.style.maxWidth = 60;
        }

        /// <summary>
        /// Enabled the menu in all GameViews.
        /// </summary>
        public static void Show()
        {
            LocalizationSettings.SelectedLocaleChanged += OnLanguageChanged;

            var initOp = LocalizationSettings.InitializationOperation;
            if (initOp.IsDone)
            {
                AddToolbarsToGameViews();
            }
            else
            {
                initOp.Completed += (hnd) => AddToolbarsToGameViews();
            }
        }

        /// <summary>
        /// Removes the menu from all GameViews.
        /// </summary>
        public static void Hide()
        {
            if (LocalizationSettings.HasSettings)
                LocalizationSettings.SelectedLocaleChanged -= OnLanguageChanged;
            ClearViews();
        }

        static void PlayModeStateChanged(PlayModeStateChange obj)
        {
            if (obj == PlayModeStateChange.EnteredPlayMode && LocalizationSettings.HasSettings && LocalizationEditorSettings.ShowLocaleMenuInGameView)
            {
                Show();
            }
            else if (s_GameViews.Count > 0)
            {
                Hide();
            }
        }

        static void ClearViews()
        {
            foreach (var gv in s_GameViews)
            {
                gv.toolbar.RemoveFromHierarchy();
            }
            s_GameViews.Clear();
        }

        static void AddToolbarsToGameViews()
        {
            Assembly assembly = typeof(EditorWindow).Assembly;
            Type type = assembly.GetType("UnityEditor.GameView");
            var gameViews = UnityEngine.Resources.FindObjectsOfTypeAll(type);

            ClearViews();

            foreach (EditorWindow gameView in gameViews)
            {
                var toolbar = new VisualElement();
                toolbar.style.flexDirection = FlexDirection.Row;
                toolbar.style.justifyContent = Justify.FlexEnd;
                toolbar.style.top = 22;

                var menu = new GameViewLanguageMenu();
                menu.style.backgroundImage = EditorStyles.popup.normal.background;
                menu.RegisterValueChangedCallback((evt) => LocalizationSettings.SelectedLocale = evt.newValue);
                toolbar.Add(menu);

                gameView.rootVisualElement.Add(toolbar);
                toolbar.BringToFront();
                s_GameViews.Add((toolbar, menu));
            }
            OnLanguageChanged(LocalizationSettings.SelectedLocale);
        }

        static List<Locale> GetChoices()
        {
            s_Locales.Clear();
            s_Locales.AddRange(LocalizationSettings.AvailableLocales.Locales);
            return s_Locales;
        }

        static void OnLanguageChanged(Locale locale)
        {
            foreach(var gv in s_GameViews)
            {
                gv.menu.SetValueWithoutNotify(locale);
            }
        }
    }
}