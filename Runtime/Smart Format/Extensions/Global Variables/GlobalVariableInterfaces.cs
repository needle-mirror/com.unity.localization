using System;

namespace UnityEngine.Localization.SmartFormat.GlobalVariables
{
    /// <summary>
    /// Collection that contains <see cref="IGlobalVariable"/>.
    /// </summary>
    public interface IVariableGroup
    {
        bool TryGetValue(string key, out IGlobalVariable value);
    }

    /// <summary>
    /// Represents a variable that can be provided through a <see cref="GlobalVariableSource"/> instead of as a string format argument.
    /// A global variable can be a single variable, in which case the value should be returned in the <see cref="SourceValue"/> or a
    /// class with multiple variables which can then be further extracted with additional string format arguments.
    /// </summary>
    public interface IGlobalVariable
    {
        /// <summary>
        /// The value that will be used when the smart string matches this variable. This value can then be further used by additional sources/formatters.
        /// </summary>
        object SourceValue { get; }
    }

    public interface IGlobalVariableValueChanged : IGlobalVariable
    {
        /// <summary>
        /// This event is sent when the global variable has changed or wishes to trigger an update to any <see cref="LocalizedString"/> that is currently using it.
        /// </summary>
        event Action<IGlobalVariable> ValueChanged;
    }
}
