using System;
using UnityEngine;

namespace UnityEditor.Localization.UI
{
    static class TypeUtility
    {
        public static void PopulateMenuWithCreateItems(GenericMenu menu, Type baseType, Action<Type> selected, Type requiredAttribute = null)
        {
            var foundTypes = TypeCache.GetTypesDerivedFrom(baseType);
            for (int i = 0; i < foundTypes.Count; ++i)
            {
                var type = foundTypes[i];

                if (type.IsAbstract || type.IsGenericType)
                    continue;

                if (requiredAttribute != null && !Attribute.IsDefined(type, requiredAttribute))
                    continue;

                menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(type.Name)), false, () =>
                {
                    selected(type);
                });
            }
        }
    }
}
