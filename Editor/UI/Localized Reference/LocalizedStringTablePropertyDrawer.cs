using System.Collections;
using System.Collections.Generic;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization;

namespace UnityEditor.Localization.UI
{
    [CustomPropertyDrawer(typeof(LocalizedStringTable), true)]
    class LocalizedStringTablePropertyDrawer : LocalizedTablePropertyDrawer<StringTableCollection>
    {
        static LocalizedStringTablePropertyDrawer()
        {
            GetProjectTableCollections = LocalizationEditorSettings.GetStringTableCollections;
        }
    }
}
