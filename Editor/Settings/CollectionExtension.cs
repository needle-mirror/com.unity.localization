using UnityEngine;
using System;

namespace UnityEditor.Localization
{
    /// <summary>
    /// Allows for attaching additional data or functionality to a <see cref="StringTableCollection"/> or <see cref="AssetTableCollection"/>.
    /// </summary>
    [Serializable]
    public class CollectionExtension
    {
        [SerializeField, HideInInspector]
        LocalizedTableCollection m_Collection;

        /// <summary>
        /// The collection this extension is attached to.
        /// </summary>
        public LocalizedTableCollection TargetCollection => m_Collection;

        internal void Init(LocalizedTableCollection target)
        {
            m_Collection = target;
        }
    }
}
