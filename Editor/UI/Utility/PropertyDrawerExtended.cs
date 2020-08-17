using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Localization.UI
{
    /// <summary>
    /// Supports drawing properties for lists.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    abstract class PropertyDrawerExtended<TData> : PropertyDrawer
    {
        protected Dictionary<string, TData> PropertyData = new Dictionary<string, TData>();

        public virtual TData GetDataForProperty(SerializedProperty property)
        {
            // We use both the property name and the serialized object hash for the key as its possible the serialized object may have been disposed.
            var propertyKey = property.serializedObject.GetHashCode() + property.propertyPath;
            if (!PropertyData.TryGetValue(propertyKey, out var propertyData))
            {
                propertyData = CreatePropertyData(property);
                PropertyData.Add(propertyKey, propertyData);
            }
            return propertyData;
        }

        public abstract TData CreatePropertyData(SerializedProperty property);
        public abstract void OnGUI(TData data, Rect position, SerializedProperty property, GUIContent label);
        public abstract float GetPropertyHeight(TData data, SerializedProperty property, GUIContent label);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var data = GetDataForProperty(property);
            OnGUI(data, position, property, label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var data = GetDataForProperty(property);
            return GetPropertyHeight(data, property, label);
        }

        protected void ClearPropertyDataCache() => PropertyData.Clear();
    }
}
