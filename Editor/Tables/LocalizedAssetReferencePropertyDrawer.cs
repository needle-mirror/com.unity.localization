using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Localization;

namespace UnityEditor.Localization
{
    [CustomPropertyDrawer(typeof(LocalizedAssetReference), true)]
    internal class LocalizedAssetReferencePropertyDrawer : PropertyDrawer
    {
        public LocalizedAssetReference AssetReference { get; private set; }

        SerializedProperty m_TableName;
        SerializedProperty m_Key;

        public string NoAssetString
        {
            get { return AssetReference.AssetType != null ? string.Format("None ({0})", AssetReference.AssetType.Name) : "None"; }
        }

        void Init(SerializedProperty property)
        {
            AssetReference = property.GetActualObjectForSerializedProperty<LocalizedAssetReference>(fieldInfo);
            m_TableName = property.FindPropertyRelative("m_TableName");
            m_Key = property.FindPropertyRelative("m_Key");
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property == null || label == null)
                return;

            Init(property);

            var dropDownPosition = EditorGUI.PrefixLabel(position, label);
            if (EditorGUI.DropdownButton(dropDownPosition, GetDropDownLabel(), FocusType.Keyboard))
            {
                PopupWindow.Show(dropDownPosition, new LocalizedReferencePopupWindow(new LocalizedAssetReferenceTreeView(this)) { Width = dropDownPosition.width });
            }
        }

        GUIContent GetDropDownLabel()
        {
            if (!string.IsNullOrEmpty(m_TableName.stringValue) && !string.IsNullOrEmpty(m_Key.stringValue))
            {
                var icon = EditorGUIUtility.ObjectContent(null, AssetReference.AssetType);
                return new GUIContent(m_TableName.stringValue + "/" + m_Key.stringValue, icon.image);
            }
            return new GUIContent(NoAssetString);
        }

        public void SetValue(string table, string key)
        {
            m_TableName.stringValue = table;
            m_Key.stringValue = key;

            // SetValue will be called by the Popup and outside of our OnGUI so we need to call ApplyModifiedProperties
            m_TableName.serializedObject.ApplyModifiedProperties();
        }
    }

    class LocalizedAssetRefTreeViewItem : TreeViewItem
    {
        public AssetTableCollection Table { get; set; }
        public string Key { get; set; }
        
        public LocalizedAssetRefTreeViewItem(AssetTableCollection table, string key, int id, int depth) : 
            base(id, depth)
        {
            Table = table;
            Key = key;
            displayName = Key;
        }
    }

    class LocalizedAssetReferenceTreeView : TreeView
    {
        LocalizedAssetReferencePropertyDrawer m_Drawer;

        public LocalizedAssetReferenceTreeView(LocalizedAssetReferencePropertyDrawer drawer)
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
            int id = 1;

            root.AddChild(new LocalizedAssetRefTreeViewItem(null, null, id++, 0) { displayName = m_Drawer.NoAssetString });

            var tables = LocalizationPlayerSettings.GetAssetTables<LocalizedAssetTable>();
            foreach (var table in tables)
            {
                if (m_Drawer.AssetReference.AssetType != null && table.AssetType != m_Drawer.AssetReference.AssetType)
                    continue;

                var keys = table.GetKeys();
                var tableNode = new TreeViewItem(id++, 0, table.TableName);
                tableNode.icon = AssetDatabase.GetCachedIcon(AssetDatabase.GetAssetPath(table.Tables[0])) as Texture2D;
                root.AddChild(tableNode);

                foreach (var key in keys)
                {
                    tableNode.AddChild(new LocalizedAssetRefTreeViewItem(table, key, id++, 1));
                }
            }

            if(!root.hasChildren)
            {
                root.AddChild(new TreeViewItem(1, 0, "No Asset Tables Found."));
            }

            return root;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            var keyNode = FindItem(selectedIds[0], rootItem) as LocalizedAssetRefTreeViewItem;
            if(keyNode != null)
            {
                if(keyNode.Table == null)
                    m_Drawer.SetValue(string.Empty, string.Empty);
                else
                    m_Drawer.SetValue(keyNode.Table.TableName, keyNode.Key);
            }
            SetSelection(new int[] { });
        }
    }
}