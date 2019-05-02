using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Localization;

namespace UnityEditor.Localization
{
    [CustomPropertyDrawer(typeof(LocalizedStringReference), true)]
    class LocalizedStringReferencePropertyDrawer : PropertyDrawer
    {
        KeyDatabase m_KeyDatabase;
        SerializedProperty m_TableName;
        SerializedProperty m_Key;
        SerializedProperty m_KeyId;
        bool m_UseKeyId;

        void Init(SerializedProperty property)
        {
            m_TableName = property.FindPropertyRelative("m_TableName");
            m_Key = property.FindPropertyRelative("m_Key");
            m_KeyId = property.FindPropertyRelative("m_KeyId");
            m_UseKeyId = string.IsNullOrEmpty(m_Key.stringValue);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property == null || label == null)
                return;

            Init(property);

            var dropDownPosition = EditorGUI.PrefixLabel(position, label);
            if (EditorGUI.DropdownButton(dropDownPosition, GetDropDownLabel(), FocusType.Keyboard))
            {
                PopupWindow.Show(dropDownPosition, new LocalizedReferencePopupWindow(new LocalizedStringReferenceTreeView(this)){ Width = dropDownPosition.width });
            }
        }

        GUIContent GetDropDownLabel()
        {
            if (!string.IsNullOrEmpty(m_TableName.stringValue) && (!string.IsNullOrEmpty(m_Key.stringValue) || m_KeyId.intValue != KeyDatabase.EmptyId))
            {
                if (m_KeyId.intValue == KeyDatabase.EmptyId)
                {
                    return new GUIContent(m_TableName.stringValue + "/" + m_Key.stringValue);
                }

                if (m_KeyDatabase == null)
                {
                    var tables = LocalizationEditorSettings.GetAssetTablesCollection<StringTable>();
                    var foundTableCollection = tables.Find(tbl => tbl.TableName == m_TableName.stringValue);
                    if (foundTableCollection != null && foundTableCollection.Keys != null)
                    {
                        m_KeyDatabase = foundTableCollection.Keys;
                    }
                }

                if (m_KeyDatabase != null)
                    return new GUIContent(m_TableName.stringValue + "/" + m_KeyDatabase.GetKey((uint)m_KeyId.intValue));
            }
            return new GUIContent("None");
        }

        public void SetValue(string table, KeyDatabase.KeyDatabaseEntry keyEntry)
        {
            m_TableName.stringValue = table;
            m_Key.stringValue = m_UseKeyId ? string.Empty : keyEntry.Key;
            m_KeyId.intValue = (int)(m_UseKeyId ? keyEntry.Id : KeyDatabase.EmptyId);

            // SetValue will be called by the Popup and outside of our OnGUI so we need to call ApplyModifiedProperties
            m_TableName.serializedObject.ApplyModifiedProperties();
        }
    }

    class LocalizedStringReferenceTreeView : TreeView
    {
        LocalizedStringReferencePropertyDrawer m_Drawer;

        public LocalizedStringReferenceTreeView(LocalizedStringReferencePropertyDrawer drawer)
            : base(new TreeViewState())
        {
            m_Drawer = drawer;

            showAlternatingRowBackgrounds = true;
            showBorder = true;
            Reload();
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem(-1, -1);
            var id = 1;

            root.AddChild(new LocalizedAssetRefTreeViewItem(null, null, id++, 0) { displayName = "None" });

            var tables = LocalizationEditorSettings.GetAssetTablesCollection<StringTableBase>();

            // TODO: We could cache this for reuse
            foreach (var table in tables)
            {
                var keys = table.Keys;
                var tableNode = new TreeViewItem(id++, 0, table.TableName);
                tableNode.icon = AssetDatabase.GetCachedIcon(AssetDatabase.GetAssetPath(table.Tables[0])) as Texture2D;
                root.AddChild(tableNode);
                foreach (var key in keys.Entries)
                {
                    tableNode.AddChild(new LocalizedAssetRefTreeViewItem(table, key, id++, 1));
                }
            }

            if (!root.hasChildren)
            {
                root.AddChild(new TreeViewItem(1, 0, "No String Tables Found."));
            }

            return root;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (FindItem(selectedIds[0], rootItem) is LocalizedAssetRefTreeViewItem keyNode)
            {
                if (keyNode.Table == null)
                    m_Drawer.SetValue(string.Empty, null);
                else
                    m_Drawer.SetValue(keyNode.Table.TableName, keyNode.KeyEntry);
            }
            SetSelection(new int[] { });
        }
    }
}