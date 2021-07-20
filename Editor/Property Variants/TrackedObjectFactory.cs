#if ENABLE_PROPERTY_VARIANTS || PACKAGE_DOCS_GENERATION

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Localization.Bridge;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.PropertyVariants;
using UnityEngine.Localization.PropertyVariants.TrackedObjects;
using UnityEngine.Localization.PropertyVariants.TrackedProperties;
using UnityEngine.Localization.Tables;
using Object = UnityEngine.Object;

namespace UnityEditor.Localization.PropertyVariants
{
    static class TrackedObjectFactory
    {
        static readonly Dictionary<string, Func<ITrackedProperty>> k_PropertyCreators;
        static readonly Dictionary<Type, Func<LocalizedReference>> k_LocalizedAssetCreators;

        static TrackedObjectFactory()
        {
            k_PropertyCreators = new Dictionary<string, Func<ITrackedProperty>>
            {
                ["ArraySize"] = () => new ArraySizeTrackedProperty(),
                ["byte"] = () => new ByteTrackedProperty(),
                ["sbyte"] = () => new SByteTrackedProperty(),
                ["char"] = () => new CharTrackedProperty(),
                ["short"] = () => new ShortTrackedProperty(),
                ["ushort"] = () => new UShortTrackedProperty(),
                ["int"] = () => new IntTrackedProperty(),
                ["uint"] = () => new UIntTrackedProperty(),
                ["long"] = () => new LongTrackedProperty(),
                ["ulong"] = () => new ULongTrackedProperty(),
                ["Enum"] = () => new EnumTrackedProperty(),
                ["bool"] = () => new BoolTrackedProperty(),
                ["float"] = () => new FloatTrackedProperty(),
                ["double"] = () => new DoubleTrackedProperty(),
                ["string"] = () => new StringTrackedProperty()
            };
        }

        public static TrackedObject CreateTrackedObject(Object target)
        {
            var trackedObjectTypes = TypeCache.GetTypesWithAttribute<CustomTrackedObjectAttribute>();

            Type foundType = null;
            var typeToMatch = target.GetType();
            foreach (var typ in trackedObjectTypes)
            {
                if (!typeof(TrackedObject).IsAssignableFrom(typ))
                {
                    Debug.LogWarning($"Type {typ} must implement {nameof(TrackedObject)} when using {nameof(CustomTrackedObjectAttribute)}");
                    continue;
                }

                var attr = typ.GetCustomAttribute<CustomTrackedObjectAttribute>();
                if (attr.ObjectType == typeToMatch)
                {
                    foundType = typ;
                    break;
                }
                if (attr.SupportsInheritedTypes && attr.ObjectType.IsAssignableFrom(typeToMatch))
                {
                    // We could have a priority value here to allow for overriding CustomTrackedObjects.
                    foundType = typ;
                }
            }

            if (foundType != null)
            {
                var instance = (TrackedObject)Activator.CreateInstance(foundType);
                instance.Target = target;
                return instance;
            }

            Debug.LogWarning($"Could not find a TrackedObject for {typeToMatch}.");
            return null;
        }

        public static ITrackedProperty CreateTrackedProperty(Object target, PropertyModification propertyModification)
        {
            // We need to know the type. We could try to infer it from the property value but this is safer.
            var path = propertyModification.propertyPath;
            var serializedObject = new SerializedObject(target);
            var serializedProperty = serializedObject.FindProperty(path);

            if (serializedProperty.propertyType == SerializedPropertyType.ObjectReference)
            {
                ScriptAttributeUtilityBridge.GetFieldInfoAndStaticTypeFromProperty(serializedProperty, out var type);

                var collection = LocalizationProjectSettings.NewAssetTable.IsEmpty ? null : LocalizationEditorSettings.GetAssetTableCollection(LocalizationProjectSettings.NewAssetTable.TableReference);
                if (collection == null)
                    return new UnityObjectProperty {PropertyPath = path, PropertyType = type};

                Undo.RecordObject(collection.SharedData, "Update table");
                var newEntry = AddNewEntry(target, collection.SharedData);
                EditorUtility.SetDirty(collection.SharedData);

                // Find a LocalizedAsset for this type or fallback to LocalizedObject.
                var allLocalizedAssetTypes = TypeCache.GetTypesDerivedFrom<LocalizedAssetBase>();
                var locAssetType = allLocalizedAssetTypes.FirstOrDefault(t => t.BaseType.GenericTypeArguments.Length == 1 && t.BaseType.GetGenericArguments()[0] == type);

                var locAsset = new LocalizedAssetProperty
                {
                    PropertyPath = path,
                    LocalizedObject = locAssetType == null ? new LocalizedObject() : (LocalizedAssetBase)Activator.CreateInstance(locAssetType)
                };
                locAsset.LocalizedObject.SetReference(LocalizationProjectSettings.NewAssetTable.TableReference, newEntry.Id);

                return locAsset;
            }

            if (serializedProperty.propertyType == SerializedPropertyType.String)
            {
                var collection = LocalizationProjectSettings.NewStringTable.IsEmpty ? null : LocalizationEditorSettings.GetStringTableCollection(LocalizationProjectSettings.NewStringTable.TableReference);
                if (collection != null)
                {
                    Undo.RecordObject(collection.SharedData, "Update table");
                    var newEntry = AddNewEntry(target, collection.SharedData);
                    EditorUtility.SetDirty(collection.SharedData);

                    var locString = new LocalizedStringProperty { PropertyPath = path };
                    locString.LocalizedString.SetReference(LocalizationProjectSettings.NewStringTable.TableReference, newEntry.Id);

                    return locString;
                }
            }

            if (!k_PropertyCreators.TryGetValue(serializedProperty.type, out var create)) return null;

            var prop = create();
            prop.PropertyPath = path;
            return prop;
        }

        internal static SharedTableData.SharedTableEntry AddNewEntry(Object target, SharedTableData sharedTableData)
        {
            if (!(target is Component component))
                return sharedTableData.AddKey();

            using (StringBuilderPool.Get(out var sb))
            {
                sb.Append(component.name);

                var currentParent = component.transform.parent;

                while (currentParent != null)
                {
                    sb.Insert(0, currentParent.name + "/");
                    currentParent = currentParent.parent;
                }

                if (!string.IsNullOrEmpty(component.gameObject.scene.name))
                    sb.Insert(0, component.gameObject.scene.name + "/");

                var name = sb.ToString();
                var entry = sharedTableData.AddKey(name);
                return entry;
            }
        }
    }
}

#endif
