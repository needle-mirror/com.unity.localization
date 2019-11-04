using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.Localization.Tables;
using UnityEngine.UIElements;

namespace UnityEditor.Localization.UI
{
    class TableTypePopup : PopupField<Type>
    {
        public new class UxmlFactory : UxmlFactory<TableTypePopup> {}

        public TableTypePopup()
            : base(GetChoices(), 0)
        {
            formatSelectedValueCallback = FormatLabel;
            formatListItemCallback = FormatLabel;
        }

        static string FormatLabel(Type t) => ObjectNames.NicifyVariableName(t.Name);

        static List<Type> GetChoices()
        {
            var choices = new List<Type>();
            foreach (var typ in TypeCache.GetTypesDerivedFrom<LocalizedTable>())
            {
                if (!typ.IsAbstract && !typ.IsGenericType)
                    choices.Add(typ);
            }
            return choices;
        }
    }
}
