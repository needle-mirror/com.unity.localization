using System.Collections.Generic;
using UnityEngine.Localization.SmartFormat.Core.Parsing;
using UnityEngine.Localization.SmartFormat.GlobalVariables;

namespace UnityEngine.Localization.SmartFormat.Core.Formatting
{
    /// <summary>
    /// Caches information about a format operation
    /// so that repeat calls can be optimized to run faster.
    /// </summary>
    public class FormatCache
    {
        /// <summary>
        /// Caches the parsed format.
        /// </summary>
        public Format Format { get; set; }

        /// <summary>
        /// Storage for any misc objects.
        /// This can be used by extensions that want to cache data,
        /// such as reflection information.
        /// </summary>
        public Dictionary<string, object> CachedObjects { get; } = new Dictionary<string, object>();

        /// <summary>
        /// Any <see cref="IGlobalVariableValueChanged"/> that may have been used during formatting.
        /// This can then be used to subscribe to update events in order to trigger a regeneration of the string.
        /// </summary>
        public List<IGlobalVariableValueChanged> GlobalVariableTriggers { get; } = new List<IGlobalVariableValueChanged>();
    }
}
