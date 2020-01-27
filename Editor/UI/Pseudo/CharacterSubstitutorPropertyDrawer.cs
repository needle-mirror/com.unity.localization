using UnityEngine;
using UnityEngine.Localization.Pseudo;

namespace UnityEditor.Localization.UI
{
    [CustomPropertyDrawer(typeof(CharacterSubstitutor))]
    class CharacterSubstitutorPropertyDrawer : PropertyDrawer
    {
        SerializedProperty m_SubstitutionMethod;
        SerializedProperty m_ReplacementsMap;
        SerializedProperty m_ReplacementList;
        SerializedProperty m_ListMode;
        bool m_IsAccenter;

        const float k_RemoveButtonSize = 20;

        static char[] s_DefferedAddTypicalCharacters;

        class Styles
        {
            public static readonly GUIContent addItem = new GUIContent("+");
            public static readonly GUIContent addTypicalCharacterSet = new GUIContent("Add Typical Character Set");
            public static readonly GUIContent original = new GUIContent("Original");
            public static readonly GUIContent removeItem = new GUIContent("-");
            public static readonly GUIContent replacement = new GUIContent("Replacement");
            public static readonly GUIContent replacementCharacters = new GUIContent("Replacement Characters");
        }

        void Init(SerializedProperty property)
        {
            // Don't cache as it causes issues when using PropertyField with a list
            m_SubstitutionMethod = property.FindPropertyRelative("m_SubstitutionMethod");
            m_ReplacementsMap = property.FindPropertyRelative("m_ReplacementsMap");
            m_ReplacementList = property.FindPropertyRelative("m_ReplacementList");
            m_ListMode = property.FindPropertyRelative("m_ListMode");
            m_IsAccenter = property.managedReferenceFullTypename.EndsWith("Accenter");
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Init(property);

            position.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                if (!m_IsAccenter)
                {
                    EditorGUI.PropertyField(position, m_SubstitutionMethod);
                    position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                }

                var method = (CharacterSubstitutor.SubstitutionMethod)m_SubstitutionMethod.intValue;
                if (method == CharacterSubstitutor.SubstitutionMethod.Map)
                {
                    DrawReplacementRules(position, m_ReplacementsMap);
                }
                else if (method == CharacterSubstitutor.SubstitutionMethod.List)
                {
                    EditorGUI.BeginDisabledGroup(m_ReplacementList.arraySize <= 1);
                    EditorGUI.PropertyField(position, m_ListMode);
                    EditorGUI.EndDisabledGroup();
                    position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                    // Add typical characters button
                    EditorGUI.PropertyField(position, m_ReplacementList, true);
                    position.y += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * (2 + m_ReplacementList.arraySize);

                    if (s_DefferedAddTypicalCharacters != null)
                    {
                        foreach (var c in s_DefferedAddTypicalCharacters)
                        {
                            var element = m_ReplacementList.AddArrayElement();
                            element.intValue = c;
                        }

                        s_DefferedAddTypicalCharacters = null;
                    }

                    position.xMin += EditorGUIUtility.labelWidth;
                    if (EditorGUI.DropdownButton(position, Styles.addTypicalCharacterSet, FocusType.Keyboard))
                    {
                        var menu = new GenericMenu();

                        foreach (var lang in TypicalCharacterSets.s_TypicalCharacterSets)
                        {
                            menu.AddItem(new GUIContent(lang.Key.ToString()), false, (obj) =>
                            {
                                // We defer the add operation as we are not inside of the SerializedProperty Update/ApplyModifiedChanges.
                                s_DefferedAddTypicalCharacters = lang.Value;
                            }, null);
                        }
                        menu.DropDown(position);
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        internal static Rect DrawReplacementRules(Rect position, SerializedProperty property)
        {
            // Header
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, Styles.replacementCharacters);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            if (!property.isExpanded)
                return position;

            float indent = EditorGUI.indentLevel * 15;
            float width = position.width - indent;
            var originalPos = new Rect(position.x + indent, position.y, width * 0.5f, position.height);
            var replacementPos = new Rect(originalPos.xMax + 2, position.y, width - originalPos.width - 2 - k_RemoveButtonSize, position.height);
            var btnPos = new Rect(replacementPos.xMax + 2, position.y, k_RemoveButtonSize - 2, position.height);
            GUI.Label(originalPos, Styles.original);
            GUI.Label(replacementPos, Styles.replacement);

            if (GUI.Button(btnPos, Styles.addItem))
            {
                var element = property.AddArrayElement();
                var original = element.FindPropertyRelative("original");
                var replacement = element.FindPropertyRelative("replacement");
                original.intValue = ('A' + property.arraySize - 1);
                replacement.intValue = 0;
            }

            for (int i = 0; i < property.arraySize; ++i)
            {
                originalPos.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                replacementPos.y = btnPos.y = originalPos.y;

                var element = property.GetArrayElementAtIndex(i);
                var original = element.FindPropertyRelative("original");
                var replacement = element.FindPropertyRelative("replacement");

                EditorGUI.PropertyField(originalPos, original, GUIContent.none);
                EditorGUI.PropertyField(replacementPos, replacement, GUIContent.none);

                if (GUI.Button(btnPos, Styles.removeItem))
                {
                    property.DeleteArrayElementAtIndex(i);
                }

                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            position.y = originalPos.y;
            return position;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Init(property);

            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (property.isExpanded)
            {
                if (!m_IsAccenter)
                    height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                var method = (CharacterSubstitutor.SubstitutionMethod)m_SubstitutionMethod.intValue;

                if (method == CharacterSubstitutor.SubstitutionMethod.Map)
                {
                    height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    if (m_ReplacementsMap.isExpanded)
                    {
                        height += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * (1 + m_ReplacementsMap.arraySize);
                    }
                }
                else if (method == CharacterSubstitutor.SubstitutionMethod.List)
                {
                    height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                    height += EditorGUI.GetPropertyHeight(m_ReplacementList, true);
                    if (m_ReplacementList.isExpanded)
                    {
                        height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    }
                }
            }
            return height;
        }
    }
}
