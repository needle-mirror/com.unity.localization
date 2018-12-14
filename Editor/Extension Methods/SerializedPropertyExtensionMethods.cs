using System;
using System.Linq;

namespace UnityEditor.Localization
{
    internal static class SerializedPropertyExtensionMethods
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
                    var index = Convert.ToInt32(new string(property.propertyPath.Where(c => char.IsDigit(c)).ToArray()));
                    actualObject = ((TObject[])obj)[index];
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
    }
}