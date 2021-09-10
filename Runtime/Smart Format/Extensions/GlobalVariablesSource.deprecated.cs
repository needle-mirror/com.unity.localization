using System;

namespace UnityEngine.Localization.SmartFormat.Extensions
{
    /// <inheritdoc/>
    [Serializable]
    [Obsolete("Please use PersistentVariablesSource instead (UnityUpgradable) -> PersistentVariablesSource")]
    public class GlobalVariablesSource : PersistentVariablesSource
    {
        /// <summary>
        /// Creates a new instance of the source,
        /// </summary>
        /// <param name="formatter"></param>
        public GlobalVariablesSource(SmartFormatter formatter) :
            base(formatter)
        {
        }
    }
}
