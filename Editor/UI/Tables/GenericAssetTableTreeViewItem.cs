using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine.Localization.Tables;

#if !UNITY_6000_2_OR_NEWER
using TreeViewItem = UnityEditor.IMGUI.Controls.TreeViewItem;
#else
using TreeViewItem = UnityEditor.IMGUI.Controls.TreeViewItem<int>;
#endif

namespace UnityEditor.Localization.UI
{
    class GenericAssetTableTreeViewItem<T1> : TreeViewItem
    {
        public virtual SharedTableData.SharedTableEntry SharedEntry { get; set; }

        public string Key
        {
            get => SharedEntry.Key;
            set => SharedEntry.Key = value;
        }

        public long KeyId => SharedEntry.Id;

        /// <summary>
        /// Called during the setup of the tree view.
        /// </summary>
        public virtual void Initialize(LocalizationTableCollection collection, int startIdx, List<LocalizationTable> sortedTables) {}

        /// <summary>
        /// Called before the key entry is deleted.
        /// </summary>
        public virtual void OnDeleteKey() {}
    }
}
