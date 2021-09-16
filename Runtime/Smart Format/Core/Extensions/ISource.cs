namespace UnityEngine.Localization.SmartFormat.Core.Extensions
{
    /// <summary>
    /// Evaluates a selector.
    /// </summary>
    public interface ISource
    {
        /// <summary>
        /// Evaluates the <see cref="ISelectorInfo" /> based on the <see cref="ISelectorInfo.CurrentValue" />.
        /// If this extension cannot evaluate the Selector, returns False.
        /// Otherwise, sets the <see cref="ISelectorInfo.Result" /> and returns true.
        /// </summary>
        /// <param name="selectorInfo"></param>
        /// <returns></returns>
        bool TryEvaluateSelector(ISelectorInfo selectorInfo);
    }
}
