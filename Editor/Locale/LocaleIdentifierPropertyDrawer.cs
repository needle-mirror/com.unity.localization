using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace UnityEditor.Localization
{
    [CustomPropertyDrawer(typeof(LocaleIdentifier))]
    public class LocaleIdentifierPropertyDrawer : PropertyDrawer
    {
        class PropertyData
        {
            public Locale selectedLocale;
            public SerializedProperty code;
            public bool undoPerformed;
        }

        // Its possible that the PropertyDrawer may be used to draw more than one item(arrays, lists)
        Dictionary<string, PropertyData> m_PropertyDataPerPropertyPath = new Dictionary<string, PropertyData>();
        PropertyData m_Property;

        readonly float k_ExpandedHeight = (2.0f * EditorGUIUtility.singleLineHeight) + (2.0f * EditorGUIUtility.standardVerticalSpacing);

        public LocaleIdentifierPropertyDrawer()
        {
            Undo.undoRedoPerformed += UndoRedoPerformed;
        }

        ~LocaleIdentifierPropertyDrawer()
        {
            Undo.undoRedoPerformed -= UndoRedoPerformed;
        }

        void UndoRedoPerformed()
        {
            foreach (var propertyData in m_PropertyDataPerPropertyPath)
            {
                propertyData.Value.undoPerformed = true;
            }
        }

        void Init(SerializedProperty property)
        {
            // We use both the property name and the serialized object hash for the key as its possible the serialized object may have been disposed.
            var propertyKey = property.serializedObject.GetHashCode() + property.propertyPath;

            if (m_PropertyDataPerPropertyPath.TryGetValue(propertyKey, out m_Property))
                return;

            m_Property = new PropertyData()
            {
                code = property.FindPropertyRelative("m_Code"),
            };
            FindLocaleFromIdentifier();
            m_PropertyDataPerPropertyPath.Add(propertyKey, m_Property);
        }

        void FindLocaleFromIdentifier()
        {
            m_Property.undoPerformed = false;

            var assets = AssetDatabase.FindAssets("t:Locale");
            foreach (var assetGuid in assets)
            {
                var path = AssetDatabase.GUIDToAssetPath(assetGuid);
                var locale = AssetDatabase.LoadAssetAtPath<Locale>(path);

                if (m_Property.code.stringValue == locale.Identifier.Code)
                {
                    m_Property.selectedLocale = locale;
                    return;
                }
            }

            m_Property.selectedLocale = null;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Init(property);

            var foldRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldRect, property.isExpanded, label);

            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginProperty(foldRect, GUIContent.none, property);
            var localeRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - foldRect.width, foldRect.height);
            var newSelectedLocale = EditorGUI.ObjectField(localeRect, m_Property.selectedLocale, typeof(Locale), false);
            EditorGUI.EndProperty();
            if (EditorGUI.EndChangeCheck())
            {
                m_Property.selectedLocale = newSelectedLocale as Locale;
                m_Property.code.stringValue = m_Property.selectedLocale != null ? m_Property.selectedLocale.Identifier.Code : string.Empty;
            }

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                position.height = EditorGUIUtility.singleLineHeight;

                EditorGUI.BeginChangeCheck();
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(position, m_Property.code);
                if (EditorGUI.EndChangeCheck() || m_Property.undoPerformed)
                {
                    FindLocaleFromIdentifier();
                }
                EditorGUI.indentLevel--;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return property.isExpanded ? k_ExpandedHeight : EditorGUIUtility.singleLineHeight;
        }
    }
}