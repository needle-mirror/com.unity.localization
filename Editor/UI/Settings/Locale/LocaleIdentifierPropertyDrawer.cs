using UnityEngine;
using UnityEngine.Localization;

namespace UnityEditor.Localization.UI
{
    class LocaleIdentifierPropertyDrawerData
    {
        public Locale selectedLocale;
        public SerializedProperty code;
        public bool undoPerformed;
    }

    [CustomPropertyDrawer(typeof(LocaleIdentifier))]
    class LocaleIdentifierPropertyDrawer : PropertyDrawerExtended<LocaleIdentifierPropertyDrawerData>
    {
        readonly float k_ExpandedHeight = (2.0f * EditorGUIUtility.singleLineHeight) + (2.0f * EditorGUIUtility.standardVerticalSpacing);

        public LocaleIdentifierPropertyDrawer() => Undo.undoRedoPerformed += UndoRedoPerformed;

        ~LocaleIdentifierPropertyDrawer() => Undo.undoRedoPerformed -= UndoRedoPerformed;

        void UndoRedoPerformed()
        {
            foreach (var propertyData in PropertyData)
            {
                propertyData.Value.undoPerformed = true;
            }
        }

        public override LocaleIdentifierPropertyDrawerData CreatePropertyData(SerializedProperty property)
        {
            var data =  new LocaleIdentifierPropertyDrawerData
            {
                code = property.FindPropertyRelative("m_Code")
            };
            FindLocaleFromIdentifier(data);
            return data;
        }

        void FindLocaleFromIdentifier(LocaleIdentifierPropertyDrawerData data)
        {
            data.undoPerformed = false;

            var assets = AssetDatabase.FindAssets("t:Locale");
            foreach (var assetGuid in assets)
            {
                var path = AssetDatabase.GUIDToAssetPath(assetGuid);
                var locale = AssetDatabase.LoadAssetAtPath<Locale>(path);

                if (data.code.stringValue == locale.Identifier.Code)
                {
                    data.selectedLocale = locale;
                    return;
                }
            }

            data.selectedLocale = null;
        }

        public override void OnGUI(LocaleIdentifierPropertyDrawerData data, Rect position, SerializedProperty property, GUIContent label)
        {
            var foldRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldRect, property.isExpanded, label);

            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginProperty(foldRect, GUIContent.none, property);
            var localeRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - foldRect.width, foldRect.height);
            var newSelectedLocale = EditorGUI.ObjectField(localeRect, data.selectedLocale, typeof(Locale), false);
            EditorGUI.EndProperty();
            if (EditorGUI.EndChangeCheck())
            {
                data.selectedLocale = newSelectedLocale as Locale;
                data.code.stringValue = data.selectedLocale != null ? data.selectedLocale.Identifier.Code : string.Empty;
            }

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                position.height = EditorGUIUtility.singleLineHeight;

                EditorGUI.BeginChangeCheck();
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(position, data.code);
                if (EditorGUI.EndChangeCheck() || data.undoPerformed)
                {
                    FindLocaleFromIdentifier(data);
                }
                EditorGUI.indentLevel--;
            }
        }

        public override float GetPropertyHeight(LocaleIdentifierPropertyDrawerData data, SerializedProperty property, GUIContent label)
        {
            return property.isExpanded ? k_ExpandedHeight : EditorGUIUtility.singleLineHeight;
        }
    }
}
