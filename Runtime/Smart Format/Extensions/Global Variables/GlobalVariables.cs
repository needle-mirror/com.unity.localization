using System;

namespace UnityEngine.Localization.SmartFormat.GlobalVariables
{
    [Serializable]
    public class GlobalVariable<T> : IGlobalVariableValueChanged
        #if UNITY_EDITOR
        , ISerializationCallbackReceiver
        #endif
    {
        [SerializeField]
        T m_Value;

        public event Action<IGlobalVariable> ValueChanged;

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

        public object SourceValue => Value;

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
    /// A <see cref="IGlobalVariable"/> that holds a single float value.
    /// </summary>
    [Serializable]
    [DisplayName("Float")]
    public class FloatGlobalVariable : GlobalVariable<float> {}

    /// <summary>
    /// A <see cref="IGlobalVariable"/> that holds a single string value.
    /// </summary>
    [Serializable]
    [DisplayName("String")]
    public class StringGlobalVariable : GlobalVariable<string> {}

    /// <summary>
    /// A <see cref="IGlobalVariable"/> that holds a single integer value.
    /// </summary>
    [Serializable]
    [DisplayName("Integer")]
    public class IntGlobalVariable : GlobalVariable<int> {}

    /// <summary>
    /// A <see cref="IGlobalVariable"/> that holds a single bool value.
    /// </summary>
    [Serializable]
    [DisplayName("Boolean")]
    public class BoolGlobalVariable : GlobalVariable<bool> {}
}
