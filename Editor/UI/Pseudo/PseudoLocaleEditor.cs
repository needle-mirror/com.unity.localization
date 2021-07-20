using System;
using System.Globalization;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Localization.Pseudo;

namespace UnityEditor.Localization.UI
{
    [CustomEditor(typeof(PseudoLocale))]
    class PseudoLocaleEditor : UnityEditor.Editor
    {
        class Styles
        {
            public static readonly GUIContent identifier = new GUIContent("Source Locale", "The locale the pseudo localized values will be generated from.");
            public static readonly GUIContent methods = new GUIContent("Pseudo-Localization Methods", "The pseudo-localization transformations that will be applied in order(top to bottom).");
            public static readonly GUIContent preview = new GUIContent("Pseudo-Localization Preview", "Preview the result of applying the pseudo-localization methods to a sample string.");
            public static readonly GUIContent sortOrder = new GUIContent("Sort Order", "The order the Locales will appear in any sorted Lists. By default Locales are ordered by name however the Sort Order can be used to override this.");
        }

        const string k_PreviewTextPref = "Localization-Pseudo-PreviewText";
        const string k_PreviewTextExpandPref = "Localization-Pseudo-PreviewTextExpanded";

        SerializedProperty m_Name;
        SerializedProperty m_Identifier;
        SerializedProperty m_Metadata;
        SerializedProperty m_Methods;
        SerializedProperty m_SortOrder;

        ReorderableListExtended m_MethodsList;
        string m_PseudoPreviewText;

        string PreviewText
        {
            get => EditorPrefs.GetString(k_PreviewTextPref, "This is an example string");
            set => EditorPrefs.SetString(k_PreviewTextPref, value);
        }

        bool PreviewExpanded
        {
            get => EditorPrefs.GetBool(k_PreviewTextExpandPref, false);
            set => EditorPrefs.SetBool(k_PreviewTextExpandPref, value);
        }

        void OnEnable()
        {
            m_Name = serializedObject.FindProperty("m_Name");
            m_Identifier = serializedObject.FindProperty("m_Identifier");
            m_Metadata = serializedObject.FindProperty("m_Metadata");
            m_Methods = serializedObject.FindProperty("m_Methods");
            m_SortOrder = serializedObject.FindProperty("m_SortOrder");
            m_MethodsList = new ReorderableListExtended(m_Methods.serializedObject, m_Methods);
            m_MethodsList.headerHeight = 2;
            m_MethodsList.onAddDropdownCallback = AddMethod;
            UpdatePreviewText();
        }

        void UpdatePreviewText()
        {
            var pseudoLocale = target as PseudoLocale;

            // If we have a CharacterSubstitutor using LoopFromPrevious then we need to reset
            // its loop counter or it will keep changing when the editor updates.
            pseudoLocale.Reset();

            m_PseudoPreviewText = pseudoLocale.GetPseudoString(PreviewText);

            // Append details
            var originalText = new StringInfo(PreviewText);
            var pseudoText = new StringInfo(m_PseudoPreviewText);
            m_PseudoPreviewText += $"\n\nLength({originalText.LengthInTextElements}/{pseudoText.LengthInTextElements})";
        }

        public override void OnInspectorGUI()
        {
            if (serializedObject.UpdateIfRequiredOrScript())
                m_PseudoPreviewText = null;

            EditorGUILayout.PropertyField(m_Name);
            EditorGUILayout.PropertyField(m_Identifier, Styles.identifier);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_SortOrder, Styles.sortOrder);
            if (EditorGUI.EndChangeCheck())
            {
                LocalizationEditorSettings.EditorEvents.RaiseLocaleSortOrderChanged(this, target as PseudoLocale);
            }

            EditorGUILayout.PropertyField(m_Metadata);

            m_Methods.isExpanded = EditorGUILayout.Foldout(m_Methods.isExpanded, Styles.methods, true);
            if (m_Methods.isExpanded)
            {
                m_MethodsList.DoLayoutList();
            }

            PreviewExpanded = EditorGUILayout.Foldout(PreviewExpanded, Styles.preview, true);
            if (PreviewExpanded)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUI.BeginChangeCheck();
                var newPreviewText = EditorGUILayout.TextArea(PreviewText);
                if (EditorGUI.EndChangeCheck())
                {
                    PreviewText = newPreviewText;
                    m_PseudoPreviewText = null;
                }

                if (string.IsNullOrEmpty(m_PseudoPreviewText))
                    UpdatePreviewText();

                GUILayout.Label(m_PseudoPreviewText);

                GUILayout.EndVertical();
            }

            if (serializedObject.ApplyModifiedProperties())
                m_PseudoPreviewText = null;
        }

        void AddMethod(Rect rect, ReorderableList list)
        {
            var menu = new GenericMenu();
            var methodTypes = TypeCache.GetTypesDerivedFrom<IPseudoLocalizationMethod>();

            for (int i = 0; i < methodTypes.Count; ++i)
            {
                var mt = methodTypes[i];
                if (mt.IsAbstract)
                    continue;

                var name = ObjectNames.NicifyVariableName(mt.Name);
                menu.AddItem(new GUIContent(name), false, () =>
                {
                    var hasDefaultConstructor = mt.GetConstructors().Any(c => c.GetParameters().Length == 0);
                    if (!hasDefaultConstructor)
                    {
                        Debug.LogError($"Can not create an instance of {mt}, it does not have a default constructor.");
                        return;
                    }

                    var elementProp = list.serializedProperty.AddArrayElement();
                    elementProp.managedReferenceValue = Activator.CreateInstance(mt);
                    list.serializedProperty.serializedObject.ApplyModifiedProperties();
                });
            }

            menu.DropDown(rect);
        }
    }
}
