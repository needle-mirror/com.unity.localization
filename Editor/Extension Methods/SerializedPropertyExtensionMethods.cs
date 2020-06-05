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
    }
}
