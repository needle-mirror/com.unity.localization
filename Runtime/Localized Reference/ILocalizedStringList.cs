using System;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization
{
    /// <summary>
    /// Delegate type for changes to a localized list of strings.
    /// </summary>
    /// <param name="newList">Optional list to populate to avoid allocations</param>
    public delegate void LocalizedListChangeHandler(List<string> newList);

    /// <summary>
    /// Common interface for localized sources that resolve to a <see cref="List{T}"/> of strings,
    /// such as <see cref="LocalizedStringList"/> and <see cref="LocalizedStringGroup"/>.
    /// </summary>
    public interface ILocalizedStringList
    {
        /// <summary>
        /// Invoked when the localized list has changed. Subscribing automatically begins loading.
        /// </summary>
        event LocalizedListChangeHandler ListChanged;

        /// <summary>
        /// Forces a synchronous load of the localized list.
        /// </summary>
        /// <remarks>
        /// If you provide a <paramref name="target"/> list, it is cleared and populated with the result.
        /// Otherwise, this method allocates a new list.
        /// </remarks>
        /// <param name="target">Optional list to populate to avoid allocations.</param>
        /// <returns>The populated list — either <paramref name="target"/> or a new list.</returns>
        List<string> GetLocalizedList(List<string> target = null);

        /// <summary>
        /// Asynchronously loads the localized list.
        /// </summary>
        /// <remarks>
        /// If you provide a <paramref name="target"/> list, it is cleared and populated with the result.
        /// Otherwise, this method allocates a new list.
        /// </remarks>
        /// <param name="target">Optional list to populate to avoid allocations.</param>
        AsyncOperationHandle<List<string>> GetLocalizedListAsync(List<string> target = null);
    }
}
