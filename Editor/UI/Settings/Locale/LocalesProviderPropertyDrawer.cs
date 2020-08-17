using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace UnityEditor.Localization.UI
{
    [CustomPropertyDrawer(typeof(LocalesProvider))]
    class LocalesProviderPropertyDrawer : PropertyDrawer
    {
        enum ToolBarChoices
        {
            LocaleGeneratorWindow,
            AddAllAssets
        }

        class Styles
        {
            public static readonly GUIContent localeDetails = new GUIContent("Locale Details");
            public static readonly GUIContent[] toolbarButtons =
            {
                new GUIContent("Locale Generator", "Opens the Locale Generator window."),
                new GUIContent("Add All", "Add all Locale assets from the project.")
            };
        }

        public ReorderableList LocalesList { get; set; }

        int m_ControlId;

        const float k_ToolbarHeight = 20;

        public LocalesProviderPropertyDrawer()
        {
            Undo.undoRedoPerformed += UndoPerformed;
            LocalizationEditorSettings.EditorEvents.LocaleAdded += EditorEvents_LocaleAddedOrRemoved;
            LocalizationEditorSettings.EditorEvents.LocaleRemoved += EditorEvents_LocaleAddedOrRemoved;
        }

        void EditorEvents_LocaleAddedOrRemoved(Locale obj)
        {
            RefreshLocalesList();
        }

        ~LocalesProviderPropertyDrawer()
        {
            Undo.undoRedoPerformed -= UndoPerformed;
            LocalizationEditorSettings.EditorEvents.LocaleAdded -= EditorEvents_LocaleAddedOrRemoved;
            LocalizationEditorSettings.EditorEvents.LocaleRemoved -= EditorEvents_LocaleAddedOrRemoved;
        }

        void UndoPerformed() => RefreshLocalesList();

        void RefreshLocalesList()
        {
            var serializedLocales = new List<SerializedLocaleItem>();
            foreach (var locale in LocalizationEditorSettings.GetLocales())
            {
                serializedLocales.Add(new SerializedLocaleItem(locale));
            }
            LocalesList.list = serializedLocales;
        }

        void Init(SerializedProperty _)
        {
            if (LocalesList != null)
                return;

            LocalesList = new ReorderableList(null, typeof(Locale));
            RefreshLocalesList();
            LocalesList.onAddCallback = AddLocale;
            LocalesList.onRemoveCallback = RemoveSelectedLocale;
            LocalesList.drawElementCallback = DrawLocaleElement;
            LocalesList.onReorderCallback = ListReordered;
            LocalesList.onSelectCallback = SelectLocale;
            LocalesList.headerHeight = 2;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Init(property);

            position.height = EditorStyles.label.lineHeight;
            EditorGUI.LabelField(position, label);
            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

            position.height = LocalesList.GetHeight();
            var listPos = EditorGUI.PrefixLabel(position, GUIContent.none);
            LocalesList.DoList(listPos);
            position.y += position.height;

            position.height = k_ToolbarHeight;
            DrawToolbar(position);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Init(property);
            float height = EditorStyles.label.lineHeight; // Header
            height += LocalesList.GetHeight();
            height += k_ToolbarHeight + EditorGUIUtility.standardVerticalSpacing; // Toolbar
            return height;
        }

        void DrawToolbar(Rect rect)
        {
            var commandName = Event.current.commandName;
            m_ControlId = GUIUtility.GetControlID(FocusType.Passive);

            var fieldPos = EditorGUI.PrefixLabel(rect, GUIContent.none);
            var selection = (ToolBarChoices)GUI.Toolbar(fieldPos, -1, Styles.toolbarButtons);
            switch (selection)
            {
                case ToolBarChoices.LocaleGeneratorWindow:
                    LocaleGeneratorWindow.ShowWindow();
                    break;
                case ToolBarChoices.AddAllAssets:
                {
                    var assets = AssetDatabase.FindAssets("t:Locale");
                    for (int i = 0; i < assets.Length; ++i)
                    {
                        LocalizationEditorSettings.AddLocale(AssetDatabase.LoadAssetAtPath<Locale>(AssetDatabase.GUIDToAssetPath(assets[i])), true);
                    }
                }
                break;
            }

            if (commandName == "ObjectSelectorClosed" && EditorGUIUtility.GetObjectPickerControlID() == m_ControlId && EditorGUIUtility.GetObjectPickerObject() != null)
            {
                LocalizationEditorSettings.AddLocale(EditorGUIUtility.GetObjectPickerObject() as Locale, true);
            }
        }

        void AddLocale(ReorderableList list)
        {
            EditorGUIUtility.ShowObjectPicker<Locale>(null, false, string.Empty, m_ControlId);
        }

        void RemoveSelectedLocale(ReorderableList localesList)
        {
            var selectedLocale = localesList.list[localesList.index] as SerializedLocaleItem;
            if (selectedLocale == null)
                return;

            LocalizationEditorSettings.RemoveLocale(selectedLocale.Reference, true);
            RefreshLocalesList();
        }

        void DrawLocaleElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var locale = LocalesList.list[index] as SerializedLocaleItem;
            if (locale == null)
                return;

            var split = rect.SplitHorizontal(0.75f);
            locale.SerializedObject.Update();
            EditorGUI.PropertyField(split.left, locale.NameProp, GUIContent.none);
            EditorGUI.PropertyField(split.right, locale.IdentifierCodeProp, GUIContent.none);
            locale.SerializedObject.ApplyModifiedProperties();
        }

        void ListReordered(ReorderableList localesList)
        {
            for (int i = 0; i < localesList.list.Count; ++i)
            {
                var serializedLocale = localesList.list[i] as SerializedLocaleItem;
                serializedLocale.SortOrderProp.intValue = i;
                serializedLocale.SerializedObject.ApplyModifiedProperties();
            }

            var selected = localesList.list[localesList.index] as SerializedLocaleItem;
            LocalizationEditorSettings.EditorEvents.RaiseLocaleSortOrderChanged(this, selected.Reference);
        }

        void SelectLocale(ReorderableList localesList)
        {
            if (localesList.index != -1)
            {
                var locale = localesList.list[localesList.index] as SerializedLocaleItem;
                EditorGUIUtility.PingObject(locale.Reference);
            }
        }
    }
}
