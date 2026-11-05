namespace UnityEditor.Localization.Platform.Utility
{
    internal static class ApplePlatformName
    {
        public static string Get(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.iOS:
                    return "iOS";
                case BuildTarget.tvOS:
                    return "tvOS";
                case BuildTarget.StandaloneOSX:
                    return "macOS";
                #if ENABLE_LOCALIZATION_XCODE_SUPPORT
                case BuildTarget.VisionOS:
                    return "visionOS";
                #endif
                default:
                    return target.ToString();
            }
        }
    }
}
