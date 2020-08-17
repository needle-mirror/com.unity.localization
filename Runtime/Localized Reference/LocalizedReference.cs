using System;
using UnityEngine.Localization.Tables;

namespace UnityEngine.Localization
{
    /// <summary>
    /// A Localized Reference allows for referencing an entry inside of a specific <see cref="LocalizationTable"/>.
    /// </summary>
    [Serializable]
    public abstract class LocalizedReference
    {
        [SerializeField]
        TableReference m_TableReference;

        [SerializeField]
        TableEntryReference m_TableEntryReference;

        /// <summary>
        /// A reference to the <see cref="LocalizationTable"/>.
        /// A table reference can be either the name of the table or the table collection name Guid.
        /// </summary>
        public TableReference TableReference
        {
            get => m_TableReference;

            set
            {
                if (value.Equals(m_TableReference))
                    return;

                m_TableReference = value;
                ForceUpdate();
            }
        }

        /// <summary>
        /// A reference to the entry inside of <see cref="TableReference"/>.
        /// The entry reference can either be the entry id or the entry name.
        /// </summary>
        public TableEntryReference TableEntryReference
        {
            get => m_TableEntryReference;

            set
            {
                if (value.Equals(m_TableEntryReference))
                    return;

                m_TableEntryReference = value;
                ForceUpdate();
            }
        }

        /// <summary>
        /// Does both <see cref="TableReference"/> and <see cref="TableEntryReference"/> contain valid references or is one of them Empty?
        /// </summary>
        public bool IsEmpty => TableReference.ReferenceType == TableReference.Type.Empty || TableEntryReference.ReferenceType == TableEntryReference.Type.Empty;

        /// <summary>
        /// Sets both the <see cref="TableReference"/> and <see cref="TableEntryReference"/>
        /// </summary>
        /// <param name="table"></param>
        /// <param name="entry"></param>
        public void SetReference(TableReference table, TableEntryReference entry)
        {
            bool update = false;

            if (!m_TableReference.Equals(table))
            {
                m_TableReference = table;
                update = true;
            }

            if (!m_TableEntryReference.Equals(entry))
            {
                m_TableEntryReference = entry;
                update = true;
            }

            if (update)
            {
                ForceUpdate();
            }
        }

        /// <summary>
        /// Returns a string representation including the <see cref="TableReference"/> and <see cref="TableEntryReference"/>
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"[{TableReference}]{TableEntryReference}";

        /// <summary>
        /// Called when a value has been changed and an update may be required.
        /// </summary>
        protected abstract void ForceUpdate();
    }
}
