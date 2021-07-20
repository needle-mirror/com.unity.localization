using System;
using System.Collections;
using System.Linq;

namespace UnityEditor.Localization
{
    static class SerializedPropertyExtensionMethods
    {
        public static TObject GetActualObjectForSerializedProperty<TObject>(this SerializedProperty property, System.Reflection.FieldInfo field) where TObject : class
        {
            try
            {
                if (property == null || field == null)
                    return null;
                var serializedObject = property.serializedObject;
                if (serializedObject == null)
                {
                    return null;
                }
                var targetObject = serializedObject.targetObject;
                var obj = field.GetValue(targetObject);
                if (obj == null)
                {
                    return null;
                }
                TObject actualObject = null;
                if (obj.GetType().IsArray)
                {
                    var index = Convert.ToInt32(new string(property.propertyPath.Where(char.IsDigit).ToArray()));
                    actualObject = ((TObject[])obj)[index];
                }
                else if (typeof(IList).IsAssignableFrom(obj.GetType()))
                {
                    var index = Convert.ToInt32(new string(property.propertyPath.Where(char.IsDigit).ToArray()));
                    actualObject = ((IList)obj)[index] as TObject;
                }
                else
                {
                    actualObject = obj as TObject;
                }
                return actualObject;
            }
            catch
            {
                return null;
            }
        }

        public static SerializedProperty AddArrayElement(this SerializedProperty property)
        {
            property.InsertArrayElementAtIndex(property.arraySize);
            return property.GetArrayElementAtIndex(property.arraySize - 1);
        }

        public static SerializedProperty InsertArrayElement(this SerializedProperty property, int index)
        {
            property.InsertArrayElementAtIndex(index);
            return property.GetArrayElementAtIndex(index);
        }

        public static void ApplyPropertyModification(this SerializedProperty property, PropertyModification modification)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                property.objectReferenceValue = modification.objectReference;
                return;
            }

            switch (property.type)
            {
                case "ArraySize":
                case "int":
                case "byte":
                case "sbyte":
                case "short":
                case "ushort":
                case "Enum":
                    property.intValue = int.Parse(modification.value);
                    break;

                case "long":
                case "ulong":
                    property.longValue = long.Parse(modification.value);
                    break;

                case "float":
                    property.floatValue = float.Parse(modification.value);
                    break;

                case "double":
                    property.doubleValue = double.Parse(modification.value);
                    break;

                case "string":
                    property.stringValue = modification.value;
                    break;

                case "bool":
                    property.boolValue = modification.value == "1";
                    break;
            }
        }
    }
}
