#if !UNITY_2019_2_OR_NEWER || PACKAGE_UGUI

using UnityEngine.UI;

namespace UnityEditor.Localization
{
    /// <summary>
    /// Adds context menus to Localize components.
    /// </summary>
    static partial class LocalizeContextMenuItem
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
    }
}

#endif