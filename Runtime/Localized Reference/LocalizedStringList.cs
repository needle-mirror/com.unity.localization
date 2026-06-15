using System;
using System.Collections.Generic;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization
{
    /// <summary>
    /// A <see cref="LocalizedString"/> whose resolved value is a delimited list that is split into a
    /// <see cref="List{T}"/> of strings.
    /// </summary>
    /// <remarks>
    /// Use this for binding to choice-style UI, such as
    /// dropdowns in either UI Toolkit or uGUI.
    /// The table entry value must be a delimited string. For example, <c>"Option A,Option B,Option C"</c>.
    /// Whitespace around the separator is not trimmed.
    /// </remarks>
    [Serializable]
    public partial class LocalizedStringList : LocalizedString, ILocalizedStringList
    {
        [SerializeField]
        string m_Separator = ",";

        List<string> m_CurrentList;

        LocalizedListChangeHandler m_ListChanged;

        /// <summary>
        /// The string used to split the localized value into individual list items.
        /// </summary>
        /// <remarks>
        /// The default value is <c>","</c>.
        /// </remarks>
        public string Separator
        {
            get => m_Separator;
            set => m_Separator = value;
        }

        /// <summary>
        /// Invoked when the localized list has changed, such as when the selected locale changes.
        /// Subscribing to this event will automatically begin loading the localized value.
        /// </summary>
        public event LocalizedListChangeHandler ListChanged
        {
            add
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                if (m_ListChanged == null)
                    StringChanged += OnStringChangedInternal;

                m_ListChanged += value;

                if (CurrentLoadingOperationHandle.IsValid() && CurrentLoadingOperationHandle.IsDone && m_CurrentList != null)
                    value(m_CurrentList);
            }
            remove
            {
                m_ListChanged -= value;
                if (m_ListChanged == null)
                    StringChanged -= OnStringChangedInternal;
            }
        }

        /// <summary>Creates a new empty <see cref="LocalizedStringList"/>.</summary>
        public LocalizedStringList() {}

        /// <summary>Creates a new <see cref="LocalizedStringList"/> referencing the given table and entry.</summary>
        /// <param name="tableReference">Reference to the String Table Collection.</param>
        /// <param name="entryReference">Reference to the String Table Collection entry whose value is a delimited list.</param>
        public LocalizedStringList(TableReference tableReference, TableEntryReference entryReference)
            : base(tableReference, entryReference) {}

        internal List<string> CurrentList => m_CurrentList;

        /// <summary>
        /// Forces a synchronous load and returns the split list.
        /// </summary>
        /// <remarks>
        /// If you provide a <paramref name="target"/> list, it is cleared and populated with the result.
        /// Otherwise, this method allocates a new list.
        /// </remarks>
        /// <param name="target">Optional list to populate to avoid allocations.</param>
        public List<string> GetLocalizedList(List<string> target = null)
        {
            var result = target ?? new List<string>();
            SplitInto(GetLocalizedString(), result);
            return result;
        }

        /// <summary>
        /// Asynchronously loads the localized value and splits it into the resulting list.
        /// </summary>
        /// <param name="target">Optional list to populate to avoid allocations.</param>
        public AsyncOperationHandle<List<string>> GetLocalizedListAsync(List<string> target = null)
        {
            var stringHandle = GetLocalizedStringAsync();
            var resultList = target ?? new List<string>();
            return AddressablesInterface.ResourceManager.CreateChainOperation(stringHandle, op =>
            {
                SplitInto(op.Result, resultList);
                return AddressablesInterface.ResourceManager.CreateCompletedOperation(resultList, null);
            });
        }

        void OnStringChangedInternal(string value)
        {
            if (m_CurrentList == null)
                m_CurrentList = new List<string>();
            SplitInto(value, m_CurrentList);
            m_ListChanged?.Invoke(m_CurrentList);
        }

        void SplitInto(string value, List<string> target)
        {
            target.Clear();
            if (value == null)
                return;
            var sep = string.IsNullOrEmpty(m_Separator) ? "," : m_Separator;
            target.AddRange(value.Split(new[] { sep }, StringSplitOptions.None));
        }
    }
}
