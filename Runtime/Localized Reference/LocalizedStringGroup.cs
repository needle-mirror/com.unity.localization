using System;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization
{
    /// <summary>
    /// Aggregates multiple <see cref="LocalizedString"/> entries into a single <see cref="List{T}"/> of strings.
    /// </summary>
    /// <remarks>
    /// Use this for binding to choice-style UI, such as
    /// dropdowns in either UI Toolkit or uGUI.
    /// Each child <see cref="LocalizedString"/> independently tracks locale changes. The combined list
    /// updates whenever any entry changes.
    /// </remarks>
    [Serializable]
    public partial class LocalizedStringGroup : ILocalizedStringList
    {
        [SerializeField]
        List<LocalizedString> m_Strings = new List<LocalizedString>();

        List<LocalizedString.ChangeHandler> m_ChildHandlers;
        string[] m_CachedValues;
        int m_LoadedCount;
        List<string> m_CurrentList;
        LocalizedListChangeHandler m_ListChanged;

        /// <summary>The list of localized strings whose resolved values form the bound list.</summary>
        public List<LocalizedString> Strings
        {
            get => m_Strings ?? (m_Strings = new List<LocalizedString>());
            set => m_Strings = value;
        }

        /// <summary>
        /// Invoked when any of the child localized strings have changed.
        /// Subscribing to this event automatically begins loading the localized values.
        /// </summary>
        /// <remarks>
        /// Mutations to <see cref="Strings"/> that occur after subscribers are attached are not observed.
        /// Remove and re-add the listeners to re-subscribe.
        /// </remarks>
        public event LocalizedListChangeHandler ListChanged
        {
            add
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                if (m_ListChanged == null)
                    SubscribeChildren();

                m_ListChanged += value;

                if (m_CachedValues != null && m_LoadedCount == m_Strings.Count)
                    value(GetCurrentList());
            }
            remove
            {
                m_ListChanged -= value;
                if (m_ListChanged == null)
                    UnsubscribeChildren();
            }
        }

        /// <summary>
        /// Forces a synchronous load of all child strings and returns the combined list.
        /// </summary>
        /// <remarks>
        /// If you provide a <paramref name="target"/> list, it is cleared and populated with the result.
        /// Otherwise, this method allocates a new list.
        /// </remarks>
        /// <param name="target">Optional caller-owned list to populate to avoid allocations.</param>
        /// <returns>A combined list of all child strings.</returns>
        public List<string> GetLocalizedList(List<string> target = null)
        {
            var result = target ?? new List<string>(m_Strings?.Count ?? 0);
            result.Clear();
            if (m_Strings == null)
                return result;
            for (int i = 0; i < m_Strings.Count; i++)
            {
                var entry = m_Strings[i];
                result.Add(entry == null || entry.IsEmpty ? string.Empty : entry.GetLocalizedString());
            }
            return result;
        }

        /// <summary>
        /// Asynchronously loads all child strings and aggregates them into the resulting list.
        /// </summary>
        /// <param name="target">Optional caller-owned list to populate to avoid allocations.</param>
        /// <returns>A combined list of all child strings.</returns>
        public AsyncOperationHandle<List<string>> GetLocalizedListAsync(List<string> target = null)
        {
            var resultList = target ?? new List<string>(m_Strings?.Count ?? 0);
            resultList.Clear();

            if (m_Strings == null || m_Strings.Count == 0)
                return AddressablesInterface.ResourceManager.CreateCompletedOperation(resultList, null);

            var handles = new List<AsyncOperationHandle>(m_Strings.Count);
            for (int i = 0; i < m_Strings.Count; i++)
            {
                var entry = m_Strings[i];
                if (entry != null && !entry.IsEmpty)
                    handles.Add(entry.GetLocalizedStringAsync());
            }

            if (handles.Count == 0)
            {
                for (int i = 0; i < m_Strings.Count; i++)
                    resultList.Add(string.Empty);
                return AddressablesInterface.ResourceManager.CreateCompletedOperation(resultList, null);
            }

            var group = AddressablesInterface.CreateGroupOperation(handles);
            var strings = m_Strings;
            return AddressablesInterface.ResourceManager.CreateChainOperation(group, op =>
            {
                int handleIndex = 0;
                for (int i = 0; i < strings.Count; i++)
                {
                    var entry = strings[i];
                    if (entry == null || entry.IsEmpty)
                    {
                        resultList.Add(string.Empty);
                    }
                    else
                    {
                        var h = op.Result[handleIndex++];
                        resultList.Add(h.Convert<string>().Result ?? string.Empty);
                    }
                }
                return AddressablesInterface.ResourceManager.CreateCompletedOperation(resultList, null);
            });
        }

        void SubscribeChildren()
        {
            if (m_Strings == null)
                return;

            m_ChildHandlers = new List<LocalizedString.ChangeHandler>(m_Strings.Count);
            m_CachedValues = new string[m_Strings.Count];
            m_LoadedCount = 0;

            for (int i = 0; i < m_Strings.Count; i++)
            {
                var index = i;
                var entry = m_Strings[i];
                if (entry == null)
                {
                    m_ChildHandlers.Add(null);
                    m_CachedValues[index] = string.Empty;
                    m_LoadedCount++;
                    continue;
                }

                LocalizedString.ChangeHandler handler = v =>
                {
                    if (m_CachedValues == null || index >= m_CachedValues.Length)
                        return;
                    if (m_CachedValues[index] == null)
                        m_LoadedCount++;
                    m_CachedValues[index] = v ?? string.Empty;
                    if (m_LoadedCount >= m_CachedValues.Length)
                        m_ListChanged?.Invoke(GetCurrentList());
                };
                m_ChildHandlers.Add(handler);
                if (!entry.IsEmpty)
                    entry.StringChanged += handler;
                else
                {
                    m_CachedValues[index] = string.Empty;
                    m_LoadedCount++;
                }
            }
        }

        void UnsubscribeChildren()
        {
            if (m_Strings != null && m_ChildHandlers != null)
            {
                for (int i = 0; i < m_Strings.Count && i < m_ChildHandlers.Count; i++)
                {
                    var entry = m_Strings[i];
                    var handler = m_ChildHandlers[i];
                    if (entry != null && handler != null && !entry.IsEmpty)
                        entry.StringChanged -= handler;
                }
            }
            m_ChildHandlers = null;
            m_CachedValues = null;
            m_LoadedCount = 0;
        }

        List<string> GetCurrentList()
        {
            var target = m_CurrentList ?? (m_CurrentList = new List<string>());
            target.Clear();
            var count = m_Strings?.Count ?? 0;
            for (int i = 0; i < count; i++)
                target.Add(m_CachedValues != null && i < m_CachedValues.Length && m_CachedValues[i] != null ? m_CachedValues[i] : string.Empty);
            return target;
        }
    }
}
