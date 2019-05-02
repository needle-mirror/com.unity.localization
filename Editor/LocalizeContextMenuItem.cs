using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.Localization
{
    /// <summary>
    /// Adds context menus to Localize components.
    /// </summary>
    static class LocalizeContextMenuItem
    {
        [MenuItem("CONTEXT/Text/Localize")]
        static void LocalizeUIText(MenuCommand command)
        {
            var target = command.context as Text;
            LocalizeComponent.SetupForLocalization(target);
        }

        [MenuItem("CONTEXT/RawImage/Localize")]
        static void LocalizeUIRawImage(MenuCommand command)
        {
            var target = command.context as RawImage;
            LocalizeComponent.SetupForLocalization(target);
        }

        [MenuItem("CONTEXT/AudioSource/Localize")]
        static void LocalizeAudioSource(MenuCommand command)
        {
            var target = command.context as AudioSource;
            LocalizeComponent.SetupForLocalization(target);
        }
    }
}