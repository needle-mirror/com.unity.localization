using System;
using UnityEngine;

namespace UnityEditor.Localization.UI
{
    [AttributeUsage(AttributeTargets.Class)]
    class TableCollectionEditorAttribute : Attribute
    {
        private Type m_EditorType;

        public Type EditorTargetType
        {
            get => m_EditorType;
            set
            {
                if (!typeof(LocalizationTableCollection).IsAssignableFrom(value))
                {
                    Debug.LogError($"Table Editors target must inherit from LocalizationTableCollection. Can not use {value.Name}.");
                    return;
                }
                m_EditorType = value;
            }
        }

        public TableCollectionEditorAttribute(Type tableType)
        {
            EditorTargetType = tableType;
        }
    }
}
