namespace UnityEngine.Localization.SmartFormat.Core.Extensions
{
    public abstract class FormatterBase : IFormatter, ISerializationCallbackReceiver
    {
        [SerializeField]
        string[] m_Names;

        public string[] Names
        {
            get => m_Names;
            set => m_Names = value;
        }

        public abstract string[] DefaultNames { get; }

        public abstract bool TryEvaluateFormat(IFormattingInfo formattingInfo);

        public void OnAfterDeserialize()
        {
            if (Names == null || Names.Length == 0)
            {
                Names = DefaultNames;
            }
        }

        public void OnBeforeSerialize()
        {
        }
    }
}
