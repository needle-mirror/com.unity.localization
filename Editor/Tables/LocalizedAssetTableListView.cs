using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Localization;
using Object = UnityEngine.Object;

namespace UnityEditor.Localization
{
    public class LocalizedAssetTableListViewItem<TObject> : GenericAssetTableTreeViewItem where TObject : Object
    {
        static Dictionary<string, TObject> s_CachedAssets = new Dictionary<string, TObject>();
        List<KeyValuePair<string, TObject>> m_Assets = new List<KeyValuePair<string, TObject>>();

        static TObject GetAssetFromCache(string guid)
        {
            TObject foundAsset;
            if (s_CachedAssets.TryGetValue(guid, out foundAsset))
                return foundAsset;

            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<TObject>(path);
            if (asset != null)
            {
                s_CachedAssets[guid] = asset;
            }

            return asset;
        }

        public TObject GetAsset(AddressableAssetTableT<TObject> table)
        {
            var foundIndex = m_Assets.FindIndex(o => o.Key == table.LocaleIdentifier.Code);
            if (foundIndex == -1)
            {
                var guid = table.GetGuidFromKey(Key);
                TObject asset = null;
                if (!string.IsNullOrEmpty(guid))
                {
                    asset = GetAssetFromCache(guid);
                    m_Assets.Add(new KeyValuePair<string, TObject>(table.LocaleIdentifier.Code, asset));
                }
                return asset;
            }
            return m_Assets[foundIndex].Value;
        }

        public void SetAsset(TObject asset, AddressableAssetTableT<TObject> table)
        {
            var oldAsset = GetAsset(table);

            if (oldAsset != null)
                LocalizationAddressableSettings.RemoveAssetFromTable(table, Key, asset);

            if (asset != null)
                LocalizationAddressableSettings.AddAssetToTable(table, Key, asset as TObject);

            // Update cache
            var foundIndex = m_Assets.FindIndex(o => o.Key == table.LocaleIdentifier.Code);
            if (foundIndex == -1)
            {
                m_Assets.Add(new KeyValuePair<string, TObject>(table.LocaleIdentifier.Code, asset));
            }
            else
            {
                m_Assets[foundIndex] = new KeyValuePair<string, TObject>(table.LocaleIdentifier.Code, asset);
            }
        }

        public void UpdateSearchString(List<AddressableAssetTableT<TObject>> tables)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Key);
            foreach(var table in tables)
            {
                var asset = GetAsset(table);
                if(asset != null)
                {
                    sb.AppendLine(asset.name);
                }
            }
            SearchString = sb.ToString();
        }
    }

    public class LocalizedAssetTableListView<T> : GenericAssetTableListView<AddressableAssetTableT<T>, LocalizedAssetTableListViewItem<T>> where T : Object
    {
        class Texts
        {
            public GUIContent[] previewLabels = new[] 
            {
                new GUIContent("Single Line"),
                new GUIContent("Small"),
                new GUIContent("Medium"),
                new GUIContent("Large")
            };
            public GUIContent rowSize = new GUIContent("Row size");
        }

        static Texts s_Texts = new Texts();

        static readonly bool k_HasThumbnail = EditorGUIUtility.HasObjectThumbnail(typeof(T));

        enum RowPreviewSize
        {
            Single,
            Small,
            Medium,
            Large
        }

        RowPreviewSize m_RowSize = RowPreviewSize.Single;

        public new float totalHeight
        {
            get
            {
                if (!k_HasThumbnail)
                    return base.totalHeight;
                return base.totalHeight + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
        }

        protected override LocalizedAssetTableListViewItem<T> CreateTreeViewItem(int id, string itemKey)
        {
            var item = base.CreateTreeViewItem(id, itemKey);
            item.UpdateSearchString(Tables);
            return item;
        }

        public override void OnGUI(Rect rect)
        {
            // Allow for changing the row height if we can show a bigger version(e.g textures).
            if (k_HasThumbnail)
            {
                var rowSzRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.BeginChangeCheck();
                rowSzRect = EditorGUI.PrefixLabel(rowSzRect, s_Texts.rowSize);
                m_RowSize = (RowPreviewSize)GUI.Toolbar(rowSzRect, (int)m_RowSize, s_Texts.previewLabels, EditorStyles.toolbarButton);
                if (EditorGUI.EndChangeCheck())
                {
                    float width = 50;
                    switch (m_RowSize)
                    {
                        case RowPreviewSize.Single:
                            rowHeight = EditorGUIUtility.singleLineHeight;
                            break;
                        case RowPreviewSize.Small:
                            width = rowHeight = 64;
                            break;
                        case RowPreviewSize.Medium:
                            width = rowHeight = 128;
                            break;
                        case RowPreviewSize.Large:
                            width = rowHeight = 256;
                            break;
                    }

                    for (int i = 1; i < multiColumnHeader.state.columns.Length; ++i)
                    {
                        multiColumnHeader.state.columns[i].width = width;
                        if (m_RowSize == RowPreviewSize.Single)
                        {
                            multiColumnHeader.state.columns[i].minWidth = 50;
                            multiColumnHeader.state.columns[i].maxWidth = float.MaxValue;
                        }
                        else
                        {
                            multiColumnHeader.state.columns[i].minWidth = width;
                            multiColumnHeader.state.columns[i].maxWidth = width;
                        }
                    }
                }
                rect.yMin += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            base.OnGUI(rect);
        }

        protected override void DrawItemField(Rect cellRect, int col, LocalizedAssetTableListViewItem<T> item, AddressableAssetTableT<T> table)
        {
            EditorGUI.BeginChangeCheck();

            if (m_RowSize != RowPreviewSize.Single)
                cellRect.width = rowHeight;

            var asset = item.GetAsset(table);
            var newAsset = EditorGUI.ObjectField(cellRect, asset, typeof(T), false);
            if (EditorGUI.EndChangeCheck())
            {
                item.SetAsset(newAsset as T, table);
                item.UpdateSearchString(Tables);
            }
        }
    }
}