using System;
using System.Collections.Generic;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Localization;
using UnityEngine.Experimental.UIElements;

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

        static string FormatLabel(Type t)
        {
            return ObjectNames.NicifyVariableName(t.Name);
        }

        static List<Type> GetChoices()
        {
            var choices = new List<Type>();
            AssemblyScanner.FindSubclasses<LocalizedTable>(choices);
            return choices;
        }
    }
}
