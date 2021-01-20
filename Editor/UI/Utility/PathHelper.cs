using System.Collections;
using UnityEngine;

namespace UnityEditor.Localization
{
    class PathHelper
    {
        internal static string MakePathRelative(string path)
        {
            if (path.Contains(Application.dataPath))
            {
                var length = Application.dataPath.Length - "Assets".Length;
                return path.Substring(length, path.Length - length);
            }

            return path;
        }
    }
}
