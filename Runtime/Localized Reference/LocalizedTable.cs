using System;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization
{
    /// <summary>
    /// A Localized Table allows for referencing a <see cref="LocalizationTable"/> at runtime.
    /// </summary>
    [Serializable]
    public abstract class LocalizedTable<TTable, TEntry>
        where TTable : DetailedLocalizationTable<TEntry>
        where TEntry : TableEntry
    {
        [SerializeField]
        TableReference m_TableReference;

        ChangeHandler m_ChangeHandler;
        AsyncOperationHandle<TTable>? m_CurrentLoadingOperation;

        protected abstract LocalizedDatabase<TTable, TEntry> Database { get; }

        /// <summary>
        /// The current loading operation for the table. A table may not be immediately available,
        /// so all operations are wrapped with an <see cref="AsyncOperationHandle"/>.
        /// </summary>
        public AsyncOperationHandle<TTable>? CurrentLoadingOperation
        {
            get => m_CurrentLoadingOperation;
            internal set => m_CurrentLoadingOperation = value;
        }

        /// <summary>
        /// <inheritdoc cref="RegisterChangeHandler"/>
        /// </summary>
        /// <param name="value"></param>
        public delegate void ChangeHandler(TTable value);

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
        /// Does <see cref="TableReference"/> contain a valid reference?
        /// </summary>
        public bool IsEmpty => TableReference.ReferenceType == TableReference.Type.Empty;

        /// <summary>
        /// Called whenever a table is available.
        /// When the first <see cref="ChangeHandler"/> is added, a loading operation will automatically start and the table will be sent to the event when completed.
        /// Any adding additional subscribers added after loading has completed will also be sent the latest table when they are added.
        /// This ensures that a subscriber will always have the correct table regardless of when it was added.
        /// The table will be refreshed whenever <see cref="LocalizationSettings.SelectedLocaleChanged"/> is changed.
        /// <seealso cref="GetTable"/> when not using the event.
        /// </summary>
        public event ChangeHandler TableChanged
        {
            add
            {
                if (value == null)
                    throw new ArgumentNullException();

                bool wasEmpty = m_ChangeHandler == null;
                m_ChangeHandler += value;

                if (wasEmpty)
                {
                    LocalizationSettings.ValidateSettingsExist();
                    LocalizationSettings.SelectedLocaleChanged += HandleLocaleChange;
                    ForceUpdate();
                }
                else if (CurrentLoadingOperation.HasValue && CurrentLoadingOperation.Value.IsDone)
                {
                    // Call the event with the latest value.
                    value(m_CurrentLoadingOperation.Value.Result);
                }
            }
            remove
            {
                if (value == null)
                    throw new ArgumentNullException();

                m_ChangeHandler -= value;

                if (m_ChangeHandler == null)
                    ClearLoadingOperation();
            }
        }

        /// <summary>
        /// Called when a value has been changed and an update may be required.
        /// </summary>
        protected void ForceUpdate()
        {
            if (m_ChangeHandler != null)
            {
                HandleLocaleChange(null);
            }
        }

        void HandleLocaleChange(Locale _)
        {
            // Cancel any previous loading operations.
            ClearLoadingOperation();

            // Don't try and load empty references.
            if (IsEmpty)
                return;

            CurrentLoadingOperation = GetTable();
            if (CurrentLoadingOperation.Value.IsDone)
                AutomaticLoadingCompleted(CurrentLoadingOperation.Value);
            else
                CurrentLoadingOperation.Value.Completed += AutomaticLoadingCompleted;
        }

        void AutomaticLoadingCompleted(AsyncOperationHandle<TTable> loadOperation)
        {
            if (loadOperation.Status != AsyncOperationStatus.Succeeded)
            {
                CurrentLoadingOperation = null;
                return;
            }

            m_ChangeHandler(loadOperation.Result);
        }

        void ClearLoadingOperation()
        {
            if (CurrentLoadingOperation.HasValue)
            {
                // We should only call this if we are not done as its possible that the internal list is null if its not been used.
                if (!CurrentLoadingOperation.Value.IsDone)
                    CurrentLoadingOperation.Value.Completed -= AutomaticLoadingCompleted;
                CurrentLoadingOperation = null;
            }
        }

        public AsyncOperationHandle<TTable> GetTable() => Database.GetTableAsync(TableReference);

        /// <summary>
        /// Returns a string representation including the <see cref="TableReference"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => TableReference;
    }
}
