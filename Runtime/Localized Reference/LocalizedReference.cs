using System;
using UnityEngine.Localization.Tables;
using UnityEngine.Localization.Settings;

namespace UnityEngine.Localization
{
    /// <summary>
    /// Provides a way to reference a table entry inside of a specific <see cref="LocalizationTable"/>.
    /// </summary>
    [Serializable]
    public abstract class LocalizedReference
        #if UNITY_EDITOR
        : ISerializationCallbackReceiver
        #endif
    {
        [SerializeField]
        TableReference m_TableReference;

        [SerializeField]
        TableEntryReference m_TableEntryReference;

        [SerializeField]
        FallbackBehavior m_FallbackState = FallbackBehavior.UseProjectSettings;

        #if UNITY_EDITOR
        // This is so we can detect when a change is made via the inspector.
        protected TableReference m_CurrentTable;
        protected TableEntryReference m_CurrentTableEntry;
        #endif

        /// <summary>
        /// Provides a reference to the <see cref="LocalizationTable"/>.
        /// A table reference can be either the name of the table or the table collection name Guid.
        /// </summary>
        /// <remarks>
        /// Note: Changing this value triggers an update to any subscribers.
        /// See <seealso cref="SetReference(TableReference, TableEntryReference)"/> if you wish to change both the table and entry.
        /// </remarks>
        /// <example>
        /// This example shows the 2 ways a reference can be set.
        /// <code source="../../DocCodeSamples.Tests/LocalizedStringSamples.cs" region="localized-string-table-reference"/>
        /// </example>
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
        /// Provides a reference to the entry inside of the table.
        /// The entry reference can be the Key name or Id.
        /// </summary>
        /// Note: Changing this value triggers an update to any subscribers.
        /// See <seealso cref="SetReference(TableReference, TableEntryReference)"/> if you wish to change both the table and entry.
        /// <example>
        /// This example shows the 2 ways a reference can be set.
        /// <code source="../../DocCodeSamples.Tests/LocalizedStringSamples.cs" region="localized-string-table-entry-reference"/>
        /// </example>
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
        /// Can be used to override the default fallback state.
        /// </summary>
        public FallbackBehavior FallbackState
        {
            get => m_FallbackState;
            set => m_FallbackState = value;
        }

        /// <summary>
        /// Checks whether both <see cref="TableReference"/> and <see cref="TableEntryReference"/> contain valid references, and returns true if one of them is empty.
        /// </summary>
        public bool IsEmpty => TableReference.ReferenceType == TableReference.Type.Empty || TableEntryReference.ReferenceType == TableEntryReference.Type.Empty;

        /// <summary>
        /// Sets both the <see cref="TableReference"/> and <see cref="TableEntryReference"/> and triggers an update if there are any change subscribers.
        /// </summary>
        /// <param name="table">Reference to the table name of Guid.</param>
        /// <param name="entry">Reference to the entry Key name or Id.</param>
        /// <example>
        /// This example shows the different ways SetReference can be called.
        /// <code source="../../DocCodeSamples.Tests/LocalizedStringSamples.cs" region="localized-string-set-reference"/>
        /// </example>
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
        public override string ToString() => $"{TableReference}/{TableEntryReference.ToString(TableReference)}";

        protected internal abstract void ForceUpdate();

        /// <summary>
        /// Called when values are changed due to a change made via serialization, such as via the inspector.
        /// </summary>
        protected abstract void Reset();

        #if UNITY_EDITOR
        void ChangedThroughSerialization()
        {
            Reset();
            ForceUpdate();
        }

        public void OnBeforeSerialize() => UpdateIfChangedThroughSerialization();
        public void OnAfterDeserialize() => UpdateIfChangedThroughSerialization();
        void UpdateIfChangedThroughSerialization()
        {
            if (!m_CurrentTable.Equals(TableReference) || !m_CurrentTableEntry.Equals(TableEntryReference))
            {
                m_CurrentTable = TableReference;
                m_CurrentTableEntry = TableEntryReference;

                // We must defer as we can not call certain parts of Unity during serialization
                UnityEditor.EditorApplication.delayCall += ChangedThroughSerialization;
            }
        }

        #endif
    }
}
