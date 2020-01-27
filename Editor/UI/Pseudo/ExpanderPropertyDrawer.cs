using UnityEngine;
using UnityEngine.Localization.Pseudo;

namespace UnityEditor.Localization.UI
{
    [CustomPropertyDrawer(typeof(Expander))]
    class ExpanderPropertyDrawer : PropertyDrawer
    {
        SerializedProperty m_ExpansionRules;
        SerializedProperty m_Location;
        SerializedProperty m_MinimumStringLength;
        SerializedProperty m_PaddingCharacters;

        const float k_DefaultExpansion = 0.3f;
        const float k_RemoveButtonSize = 20;

        class Styles
        {
            public static readonly GUIContent addItem = new GUIContent("+");
            public static readonly GUIContent expansionAmount = new GUIContent("Expansion", "The amount to increase the string size. For example 0.3 would add 30% onto the length.");
            public static readonly GUIContent removeItem = new GUIContent("-");
            public static readonly GUIContent stringLength = new GUIContent("String Length", "The length the string should fall within for this rule to be applied.");
        }

        static (SerializedProperty min, SerializedProperty max, SerializedProperty rate) ExtractExpansionRuleProperties(SerializedProperty prop)
        {
            var min = prop.FindPropertyRelative("m_MinCharacters");
            var max = prop.FindPropertyRelative("m_MaxCharacters");
            var rate = prop.FindPropertyRelative("m_ExpansionAmount");
            return (min, max, rate);
        }

        void Init(SerializedProperty property)
        {
            m_ExpansionRules = property.FindPropertyRelative("m_ExpansionRules");
            m_Location = property.FindPropertyRelative("m_Location");
            m_MinimumStringLength = property.FindPropertyRelative("m_MinimumStringLength");
            m_PaddingCharacters = property.FindPropertyRelative("m_PaddingCharacters");
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
                position = DrawExpansionRules(position);

                EditorGUI.PropertyField(position, m_Location);
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                EditorGUI.PropertyField(position, m_MinimumStringLength);
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                position.height = EditorGUI.GetPropertyHeight(m_PaddingCharacters, true);
                EditorGUI.PropertyField(position, m_PaddingCharacters, true);
                EditorGUI.indentLevel--;
            }
        }

        Rect DrawExpansionRules(Rect position)
        {
            // Header
            float indent = EditorGUI.indentLevel * 15;
            float width = position.width - indent;
            var rangePos = new Rect(position.x + indent, position.y, width * 0.5f, position.height);
            var valuePos = new Rect(rangePos.xMax + 2, position.y, width - rangePos.width - 2 - k_RemoveButtonSize, position.height);
            var addBtnPos = new Rect(valuePos.xMax + 2, position.y, k_RemoveButtonSize - 2, position.height);
            GUI.Label(rangePos, Styles.stringLength);
            GUI.Label(valuePos, Styles.expansionAmount);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // Always have at least 1 item
            if (m_ExpansionRules.arraySize == 0)
            {
                // Add defaults
                m_ExpansionRules.InsertArrayElementAtIndex(0);
                var properties = ExtractExpansionRuleProperties(m_ExpansionRules.GetArrayElementAtIndex(0));
                properties.min.intValue = 0;
                properties.max.intValue = int.MaxValue;
                properties.rate.floatValue = k_DefaultExpansion;
            }

            // Add button
            if (GUI.Button(addBtnPos, Styles.addItem))
            {
                var addedItem = ExtractExpansionRuleProperties(m_ExpansionRules.AddArrayElement());
                addedItem.max.intValue = int.MaxValue;
            }

            for (int i = 0; i < m_ExpansionRules.arraySize; ++i)
            {
                DrawExpansionRuleItem(position, i);
                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            return position;
        }

        void DrawExpansionRuleItem(Rect position, int index)
        {
            var properties = ExtractExpansionRuleProperties(m_ExpansionRules.GetArrayElementAtIndex(index));

            EditorGUI.BeginChangeCheck();

            var rangePos = new Rect(position.x, position.y, position.width * 0.5f, position.height);
            var valuePos = new Rect(rangePos.xMax + 2, position.y, position.width - rangePos.width - 2 - k_RemoveButtonSize, position.height);
            var removeBtnPos = new Rect(valuePos.xMax + 2, position.y, k_RemoveButtonSize - 2, position.height);

            int prevMaxValue = 0;
            if (index > 0)
            {
                var previous = ExtractExpansionRuleProperties(m_ExpansionRules.GetArrayElementAtIndex(index - 1));
                prevMaxValue = previous.max.intValue;

                if (prevMaxValue > properties.min.intValue)
                    properties.min.intValue = prevMaxValue;
            }

            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 40;
            var newMax = EditorGUI.IntField(rangePos, $"{prevMaxValue} - ", properties.max.intValue);
            EditorGUIUtility.labelWidth = oldLabelWidth;
            var newExpansion = EditorGUI.FloatField(valuePos, properties.rate.floatValue);
            if (EditorGUI.EndChangeCheck())
            {
                properties.max.intValue = newMax;
                properties.rate.floatValue = newExpansion;
            }

            if (GUI.Button(removeBtnPos, Styles.removeItem))
            {
                m_ExpansionRules.DeleteArrayElementAtIndex(index);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Init(property);

            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (property.isExpanded)
            {
                height += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 2;
                height += EditorGUI.GetPropertyHeight(m_PaddingCharacters, true) + EditorGUIUtility.standardVerticalSpacing;
                height += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing); // Rule header and + button
                height += m_ExpansionRules.arraySize * (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            }
            return height;
        }
    }
}
