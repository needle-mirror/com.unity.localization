using UnityEngine;

namespace UnityEditor.Localization
{
    /// <summary>
    /// Adds context menus to Localize components.
    /// </summary>
    static partial class LocalizeContextMenuItem
    {
        [MenuItem("CONTEXT/AudioSource/Localize")]
        static void LocalizeAudioSource(MenuCommand command)
        {
            var target = command.context as AudioSource;
            LocalizeComponent.SetupForLocalization(target);
        }
    }
}