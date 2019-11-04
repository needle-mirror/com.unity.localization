using System;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.UI
{
    [AttributeUsage(AttributeTargets.Class)]
    class TableEditorAttribute : Attribute
    {
        private Type m_EditorType;

        public Type EditorTargetType
        {
            get => m_EditorType;
            set
            {
                if (!typeof(LocalizedTable).IsAssignableFrom(value))
                {
                    Debug.LogError($"Table Editors target must inherit from LocalizedTable. Can not use {value.Name}.");
                    return;
                }
                m_EditorType = value;
            }
        }

        public TableEditorAttribute(Type tableType)
        {
            EditorTargetType = tableType;
        }
    }
}