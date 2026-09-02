using UnityEngine;

namespace UnityEditor.Localization
{
    static class EditorContent
    {
        // This package ships no translations of its own, so its editor strings come from the editor's
        // own dictionary, which is what a null group asks for. Only this line changes if that stops
        // being true.
        const string k_GroupName = null;

        public static GUIContent TextContent(string text, string tooltip = null)
        {
#if UNITY_6000_7_OR_NEWER
            return EditorGUIUtility.TrTextContentWithIcon(text, tooltip, (Texture)null, k_GroupName);
#else
            return EditorGUIUtility.TrTextContent(text, tooltip);
#endif
        }

        public static GUIContent TextContent(string text, Texture icon)
        {
#if UNITY_6000_7_OR_NEWER
            return EditorGUIUtility.TrTextContent(text, icon, k_GroupName);
#else
            return EditorGUIUtility.TrTextContent(text, icon);
#endif
        }

        public static GUIContent TextContent(string text, string tooltip, Texture icon)
        {
#if UNITY_6000_7_OR_NEWER
            return EditorGUIUtility.TrTextContentWithIcon(text, tooltip, icon, k_GroupName);
#else
            return EditorGUIUtility.TrTextContentWithIcon(text, tooltip, icon);
#endif
        }

        public static GUIContent TempContent(string text)
        {
#if UNITY_6000_7_OR_NEWER
            return EditorGUIUtility.TrTempContent(text, k_GroupName);
#else
            return EditorGUIUtility.TrTempContent(text);
#endif
        }

        public static GUIContent IconContent(string iconName)
        {
#if UNITY_6000_7_OR_NEWER
            return EditorGUIUtility.TrIconContent(iconName, null, k_GroupName);
#else
            return EditorGUIUtility.TrIconContent(iconName);
#endif
        }
    }
}
