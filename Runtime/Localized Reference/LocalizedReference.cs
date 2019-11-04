using System;
using UnityEngine.Localization.Tables;

namespace UnityEngine.Localization
{
    /// <summary>
    /// A Localized Reference allows for referencing an entry inside of a specific <see cref="LocalizedTable"/>.
    /// </summary>
    [Serializable]
    public abstract class LocalizedReference
    {
        [SerializeField]
        TableReference m_TableReference;

        [SerializeField]
        TableEntryReference m_TableEntryReference;

        /// <summary>
        /// A reference to the <see cref="LocalizedTable"/>.
        /// A table reference can be either the name of the table or the table name Guid.
        /// </summary>
        public TableReference TableReference
        {
            get => m_TableReference;
            set => m_TableReference = value;
        }

        /// <summary>
        /// A reference to the entry inside of <see cref="TableReference"/>.
        /// The entry reference can either be the entry id or the entry name.
        /// </summary>
        public TableEntryReference TableEntryReference
        {
            get => m_TableEntryReference;
            set => m_TableEntryReference = value;
        }

        /// <summary>
        /// Returns a string representation including the <see cref="TableReference"/> and <see cref="TableEntryReference"/>
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"[{TableReference}]{TableEntryReference}";
    }
}