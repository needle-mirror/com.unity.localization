using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Localization.UI
{
    class LocalizationPreferencesProvider : SettingsProvider
    {
        static readonly GUIContent showGameViewToolbar = new GUIContent("Locale Game View Menu", "Shows a menu for changing the selected locale in the GameView during playmode.");

        public LocalizationPreferencesProvider()
            : base("Preferences/Localization", SettingsScope.User)
        {
            keywords = new HashSet<string>(new[] { "Localization", "Locale GameView Menu" });
        }

        public override void OnGUI(string searchContext)
        {
            GUILayout.Space(10);

            const float indent = 8;

            // Small indent to match the other preference editors.
            var rect = EditorGUILayout.GetControlRect();
            rect.xMin += indent;

            EditorGUI.BeginChangeCheck();
            LocalizationEditorSettings.ShowLocaleMenuInGameView = EditorGUI.Toggle(rect, showGameViewToolbar, LocalizationEditorSettings.ShowLocaleMenuInGameView);
            if (EditorGUI.EndChangeCheck() && Application.isPlaying)
            {
                if (LocalizationEditorSettings.ShowLocaleMenuInGameView)
                    GameViewLanguageMenu.Show();
                else
                    GameViewLanguageMenu.Hide();
            }
        }

        [SettingsProvider]
        static SettingsProvider CreateProjectSettingsProvider() => new LocalizationPreferencesProvider();
    }
}
