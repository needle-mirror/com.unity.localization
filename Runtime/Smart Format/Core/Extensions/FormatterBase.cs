namespace UnityEngine.Localization.SmartFormat.Core.Extensions
{
    public abstract class FormatterBase : IFormatter
    {
        [SerializeField]
        string[] m_Names;

        public string[] Names
        {
            get => m_Names;
            set => m_Names = value;
        }

        public abstract bool TryEvaluateFormat(IFormattingInfo formattingInfo);
    }
}
