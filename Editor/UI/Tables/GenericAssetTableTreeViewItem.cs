using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.UI
{
    class GenericAssetTableTreeViewItem<T1> : TreeViewItem
    {
        public virtual KeyDatabase.KeyDatabaseEntry KeyEntry { get; set; }

        public string Key
        {
            get => KeyEntry.Key;
            set => KeyEntry.Key = value;
        }

        public uint KeyId => KeyEntry.Id;

        public bool Selected { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        /// <summary>
        /// Called during the setup of the tree view.
        /// </summary>
        /// <param name="tables"></param>
        public virtual void Initialize(List<LocalizedTable> tables, int startIdx) { }

        /// <summary>
        /// Called before the key entry is deleted.
        /// </summary>
        public virtual void OnDeleteKey(){}
    }
}