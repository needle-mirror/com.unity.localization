using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Localization;

namespace UnityEditor.Localization.UI
{
    [CustomEditor(typeof(Locale))]
    class LocaleEditor : UnityEditor.Editor
    {
        class Styles
        {
            public static readonly GUIContent code = new GUIContent("Locale Code");
            public static readonly GUIContent cultureNotFoundLabel = new GUIContent("Not Found");
            public static readonly GUIContent cultureInfo = new GUIContent("Culture Info");
        }

        SerializedProperty m_Name;
        SerializedProperty m_Code;
        SerializedProperty m_Metadata;

        CultureInfo m_CultureInfo;
        GUIContent m_CultureInfoLabel;

        void OnEnable()
        {
            m_Name = serializedObject.FindProperty("m_Name");
            m_Code = serializedObject.FindProperty("m_Identifier.m_Code");
            m_Metadata = serializedObject.FindProperty("m_Metadata");
            UpdateCultureInfo();
        }

        public override void OnInspectorGUI()
        {
            if (serializedObject.UpdateIfRequiredOrScript())
                UpdateCultureInfo();

            EditorGUILayout.PropertyField(m_Name);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_Code, Styles.code);
            if (EditorGUI.EndChangeCheck())
            {
                UpdateCultureInfo();
            }

            EditorGUILayout.LabelField(Styles.cultureInfo, m_CultureInfoLabel);

            EditorGUILayout.PropertyField(m_Metadata);

            serializedObject.ApplyModifiedProperties();
        }

        void UpdateCultureInfo()
        {
            try
            {
                m_CultureInfo = CultureInfo.GetCultureInfo(m_Code.stringValue);
                m_CultureInfoLabel = new GUIContent(m_CultureInfo.EnglishName);
            }
            catch (Exception)
            {
                m_CultureInfo = null;
                m_CultureInfoLabel = Styles.cultureNotFoundLabel;
            }
        }
    }
}
