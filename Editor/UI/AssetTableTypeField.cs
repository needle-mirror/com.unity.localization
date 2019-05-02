using System;
using System.Collections.Generic;
using UnityEngine.Localization;

#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#else
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#endif

namespace UnityEditor.Localization.UI
{
    class AssetTableTypeField : PopupField<Type>
    {
        public new class UxmlFactory : UxmlFactory<AssetTableTypeField> {}

        public AssetTableTypeField()
            : base(GetChoices(), 0)
        {
            formatSelectedValueCallback = FormatLabel;
            formatListItemCallback = FormatLabel;
        }

        static string FormatLabel(Type t) => ObjectNames.NicifyVariableName(t.Name);

        public static List<Type> GetChoices()
        {
            var choices = new List<Type>();
            AssemblyScanner.FindSubclasses<LocalizedTable>(choices);
            return choices;
        }
    }
}
