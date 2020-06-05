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
            RemoveSelected,
            AddAsset,
            AddAllAssets
        }

        class Styles
        {
            public static readonly GUIContent localeDetails = new GUIContent("Locale Details");
            public static readonly GUIContent[] toolbarButtons =
            {
                new GUIContent("Locale Generator", "Opens the Locale Generator window."),
                new GUIContent("Remove Selected"),
                new GUIContent("Add", "Add a new Locale asset."),
                new GUIContent("Add All", "Add all Locale assets from the project.")
            };
        }

        public LocalesProviderListView ListView { get; set; }

        const float k_MinListHeight = 200;
        const float k_ToolbarHeight = 20;

        public LocalesProviderPropertyDrawer()
        {
            Undo.undoRedoPerformed += UndoPerformed;
            LocalizationEditorSettings.EditorEvents.LocaleAdded += EditorEvents_LocaleAddedOrRemoved;
            LocalizationEditorSettings.EditorEvents.LocaleRemoved += EditorEvents_LocaleAddedOrRemoved;
        }

        void EditorEvents_LocaleAddedOrRemoved(Locale obj)
        {
            ListView?.Reload();
        }

        ~LocalesProviderPropertyDrawer()
        {
            Undo.undoRedoPerformed -= UndoPerformed;
            LocalizationEditorSettings.EditorEvents.LocaleAdded -= EditorEvents_LocaleAddedOrRemoved;
            LocalizationEditorSettings.EditorEvents.LocaleRemoved -= EditorEvents_LocaleAddedOrRemoved;
        }

        void UndoPerformed() => ListView.Reload();

        void Init(SerializedProperty _)
        {
            if (ListView != null)
                return;

            ListView = new LocalesProviderListView();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Init(property);

            position.height = EditorStyles.label.lineHeight;
            EditorGUI.LabelField(position, label);
            position.y += position.height + EditorGUIUtility.standardVerticalSpacing;

            position.height = Mathf.Max(k_MinListHeight, ListView.totalHeight);
            var listPos = EditorGUI.PrefixLabel(position, GUIContent.none);
            ListView.OnGUI(listPos);
            position.y += position.height;

            position.height = k_ToolbarHeight;
            DrawToolbar(position);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Init(property);
            float height = EditorStyles.label.lineHeight; // Header
            height += Mathf.Max(k_MinListHeight, ListView.totalHeight) + EditorGUIUtility.standardVerticalSpacing; // List
            height += k_ToolbarHeight + EditorGUIUtility.standardVerticalSpacing; // Toolbar
            return height;
        }

        void DrawToolbar(Rect rect)
        {
            var commandName = Event.current.commandName;
            var controlId = GUIUtility.GetControlID(FocusType.Passive);

            var fieldPos = EditorGUI.PrefixLabel(rect, GUIContent.none);
            var selection = (ToolBarChoices)GUI.Toolbar(fieldPos, -1, Styles.toolbarButtons);
            switch (selection)
            {
                case ToolBarChoices.LocaleGeneratorWindow:
                    LocaleGeneratorWindow.ShowWindow();
                    break;
                case ToolBarChoices.RemoveSelected:
                {
                    var selectedLocales = ListView.GetSelection();
                    for (int i = selectedLocales.Count - 1; i >= 0; --i)
                    {
                        var item = ListView.GetRows()[selectedLocales[i]] as SerializedLocaleItem;
                        LocalizationEditorSettings.RemoveLocale(item.SerializedObject.targetObject as Locale, true);
                    }
                    ListView.SetSelection(new int[0]);
                    ListView.Reload();
                }
                break;
                case ToolBarChoices.AddAsset:
                    EditorGUIUtility.ShowObjectPicker<Locale>(null, false, string.Empty, controlId);
                    break;
                case ToolBarChoices.AddAllAssets:
                {
                    var assets = AssetDatabase.FindAssets("t:Locale");
                    for (int i = 0; i < assets.Length; ++i)
                    {
                        LocalizationEditorSettings.AddLocale(AssetDatabase.LoadAssetAtPath<Locale>(AssetDatabase.GUIDToAssetPath(assets[i])), true);
                    }
                    ListView.Reload();
                }
                break;
            }

            if (commandName == "ObjectSelectorClosed" && EditorGUIUtility.GetObjectPickerControlID() == controlId && EditorGUIUtility.GetObjectPickerObject() != null)
            {
                LocalizationEditorSettings.AddLocale(EditorGUIUtility.GetObjectPickerObject() as Locale, true);
                ListView.Reload();
            }
        }
    }
}
