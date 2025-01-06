using System;
using UnityEngine.Localization.SmartFormat.Core.Extensions;

namespace UnityEngine.Localization.SmartFormat.PersistentVariables
{
    /// <summary>
    /// Base class for all single source variables.
    /// Inhterit from this class for storage for a single serialized source value that will send a value changed event when <see cref="Value"/> is changed.
    /// This will trigger any <see cref="LocalizedString"/> that is currently using the variable to update.
    /// </summary>
    /// <typeparam name="T">The value type to store in this variable.</typeparam>
    [Serializable]
    public partial class Variable<T> : IVariableValueChanged
    #if UNITY_EDITOR
        , ISerializationCallbackReceiver
    #endif
    {
        [SerializeField]
        T m_Value;

        /// <summary>
        /// Called when <see cref="Value"/> is changed.
        /// </summary>
        public event Action<IVariable> ValueChanged;

        /// <summary>
        /// The value for this variable.
        /// Changing this will trigger the <see cref="ValueChanged"/> event.
        /// </summary>
        public T Value
        {
            get => m_Value;
            set
            {
                if (m_Value != null && m_Value.Equals(value))
                    return;

                m_Value = value;
                SendValueChangedEvent();
            }
        }

        /// <inheritdoc/>
        public object GetSourceValue(ISelectorInfo _) => Value;

        void SendValueChangedEvent() => ValueChanged?.Invoke(this);

        public override string ToString() => Value.ToString();

        #if UNITY_EDITOR
        T m_OldValue;

        public void OnBeforeSerialize()
        {
            m_OldValue = m_Value;
        }

        public void OnAfterDeserialize()
        {
            // This lets us send value changed events when the user makes changes through the inspector.
            // If an Undo event occurs we will lose the ValueChanged reference though.
            if (m_OldValue != null && !m_OldValue.Equals(m_Value))
            {
                // We need to defer the event as it may call internal Unity api and this is not allowed from within OnAfterDeserialize.
                UnityEditor.EditorApplication.delayCall += SendValueChangedEvent;
                m_OldValue = m_Value;
            }
        }

        #endif
    }

    /// <summary>
    /// An <see cref="IVariable"/> implementation that holds a single bool value.
    /// </summary>
    /// <remarks>
    /// This class is serializable. You can use it in the Inspector.
    /// Modifying its <see cref="Variable{T}.Value"/> triggers the <see cref="Variable{T}.ValueChanged"/> event,
    /// automatically updating any [Smart Strings](xref:smart-strings) displaying the value.
    /// </remarks>
    /// <example>
    /// The following example shows how to create a <see cref="BoolVariable"/> and use it in a <see cref="LocalizedString"/>.
    /// <code source="../../../../DocCodeSamples.Tests/PersistentVariablesSamples.cs" region="bool-variable-sample"/>
    /// </example>
    /// <example>
    /// The following example shows how to create a <see cref="SByteVariable"/> and use it in a <see cref="LocalizedString"/>.
    /// <code source="../../../../DocCodeSamples.Tests/PersistentVariablesSamples.cs" region="sbyte-variable-sample"/>
    /// </example>
    /// <example>
    /// The following example shows how to create a <see cref="DoubleVariable"/> and use it in a <see cref="LocalizedString"/>.
    /// <code source="../../../../DocCodeSamples.Tests/PersistentVariablesSamples.cs" region="double-variable-sample"/>
    /// </example>
    [Serializable]
    [DisplayName("Boolean")]
    public partial class BoolVariable : Variable<bool> {}

    /// <summary>
    /// An <see cref="IVariable"/> implementation that holds a single signed byte value.
    /// </summary>
    /// <inheritdoc cref="BoolVariable"/>
    [Serializable]
    [DisplayName("Signed Byte")]
    public partial class SByteVariable : Variable<sbyte> {}

    /// <summary>
    /// An <see cref="IVariable"/> implementation that holds a single byte value.
    /// </summary>
    /// <inheritdoc cref="BoolVariable"/>
    [Serializable]
    [DisplayName("Byte")]
    public partial class ByteVariable : Variable<byte> {}

    /// <summary>
    /// An <see cref="IVariable"/> implementation that holds a single short value.
    /// </summary>
    /// <inheritdoc cref="BoolVariable"/>
    [Serializable]
    [DisplayName("Short")]
    public partial class ShortVariable : Variable<short> {}

    /// <summary>
    /// An <see cref="IVariable"/> implementation that holds a single unsigned short value.
    /// </summary>
    /// <inheritdoc cref="BoolVariable"/>
    [Serializable]
    [DisplayName("Unsigned Short")]
    public partial class UShortVariable : Variable<ushort> {}

    /// <summary>
    /// An <see cref="IVariable"/> implementation that holds a single integer value.
    /// </summary>
    /// <inheritdoc cref="BoolVariable"/>
    [Serializable]
    [DisplayName("Integer")]
    public partial class IntVariable : Variable<int> {}

    /// <summary>
    /// An <see cref="IVariable"/> implementation that holds a single unsigned integer value.
    /// </summary>
    /// <inheritdoc cref="BoolVariable"/>
    [Serializable]
    [DisplayName("Unsigned Integer")]
    public partial class UIntVariable : Variable<uint> {}

    /// <summary>
    /// An <see cref="IVariable"/> implementation that holds a single long value.
    /// </summary>
    /// <inheritdoc cref="BoolVariable"/>
    [Serializable]
    [DisplayName("Long")]
    public partial class LongVariable : Variable<long> {}

    /// <summary>
    /// An <see cref="IVariable"/> implementation that holds a single unsigned long value.
    /// </summary>
    /// <inheritdoc cref="BoolVariable"/>
    [Serializable]
    [DisplayName("Unsigned Long")]
    public partial class ULongVariable : Variable<ulong> {}

    /// <summary>
    /// An <see cref="IVariable"/> implementation that holds a single string value.
    /// </summary>
    /// <inheritdoc cref="BoolVariable"/>
    [Serializable]
    [DisplayName("String")]
    public partial class StringVariable : Variable<string> {}

    /// <summary>
    /// An <see cref="IVariable"/> implementation that holds a single float value.
    /// </summary>
    /// <inheritdoc cref="BoolVariable"/>
    [Serializable]
    [DisplayName("Float")]
    public partial class FloatVariable : Variable<float> {}

    /// <summary>
    /// An <see cref="IVariable"/> implementation that holds a single double value.
    /// </summary>
    /// <inheritdoc cref="BoolVariable"/>
    [Serializable]
    [DisplayName("Double")]
    public partial class DoubleVariable : Variable<double> {}

    /// <summary>
    /// An <see cref="IVariable"/> implementation that can reference an <see cref="Object"/> instance.
    /// </summary>
    /// <inheritdoc cref="BoolVariable"/>
    [Serializable]
    [DisplayName("Object Reference")]
    public partial class ObjectVariable : Variable<Object> {}
}
