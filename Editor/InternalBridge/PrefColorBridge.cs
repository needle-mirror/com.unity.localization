using UnityEngine;

namespace UnityEditor.Localization.Bridge
{
    [InitializeOnLoad]
    static class PrefColorBridge
    {
        static readonly PrefColor k_DrivenProperty;

        static PrefColorBridge()
        {
            // We need to InitializeOnLoad so that the color preference gets added and is shown in the Preferences/Colors window.
            k_DrivenProperty = new PrefColor("Localization/Driven Property", 0, 0.6037736f, 0, 0.7607843f, 0, 1, 0, 1);
        }

        public static Color DrivenProperty => k_DrivenProperty.Color;
    }
}
