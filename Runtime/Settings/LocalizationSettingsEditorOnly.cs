#if UNITY_EDITOR
namespace UnityEngine.Localization.Settings
{
    /// <summary>
    /// LocalizationSettings for editor only properties. To access these values <see cref="UnityEditor.Localization.LocalizationEditorSettings"/>
    /// </summary>
    public partial class LocalizationSettings : ScriptableObject
    {
        [SerializeField]
        bool m_ShowLocaleMenuInGameView = true;

        /// <summary>
        /// Should the locale selection menu be shown during play mode?
        /// </summary>
        internal static bool ShowLocaleMenuInGameView
        {
            get => Instance.m_ShowLocaleMenuInGameView;
            set => Instance.m_ShowLocaleMenuInGameView = value;
        }
    }
}
#endif
