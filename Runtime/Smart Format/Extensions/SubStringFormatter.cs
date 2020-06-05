using System;
using UnityEngine.Localization.SmartFormat.Core.Extensions;

namespace UnityEngine.Localization.SmartFormat.Extensions
{
    /// <summary>
    /// Formatter to access part of a string.
    /// </summary>
    [Serializable]
    public class SubStringFormatter : FormatterBase
    {
        [SerializeField]
        char m_ParameterDelimiter = ',';

        [SerializeField]
        string m_NullDisplayString = "(null)";

        public SubStringFormatter()
        {
            Names = DefaultNames;
        }

        public override string[] DefaultNames => new[] {"substr"};

        /// <summary>
        /// The delimiter to separate parameters, defaults to comma.
        /// </summary>
        public char ParameterDelimiter
        {
            get => m_ParameterDelimiter;
            set => m_ParameterDelimiter = value;
        }

        /// <summary>
        /// Get or set the string to display for NULL values, defaults to "(null)".
        /// </summary>
        public string NullDisplayString
        {
            get => m_NullDisplayString;
            set => m_NullDisplayString = value;
        }

        /// <summary>
        /// Tries to process the given <see cref="IFormattingInfo"/>.
        /// </summary>
        /// <param name="formattingInfo">Returns true if processed, otherwise false.</param>
        /// <returns></returns>
        public override bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            if (formattingInfo.FormatterOptions == string.Empty) return false;
            var parameters = formattingInfo.FormatterOptions.Split(ParameterDelimiter);

            var currentValue = formattingInfo.CurrentValue?.ToString();
            if (currentValue == null)
            {
                formattingInfo.Write(NullDisplayString);
                return true;
            }

            var startPos = int.Parse(parameters[0]);
            var length = parameters.Length > 1 ? int.Parse(parameters[1]) : 0;
            if (startPos < 0)
                startPos = currentValue.Length + startPos;
            if (startPos > currentValue.Length)
                startPos = currentValue.Length;
            if (length < 0)
                length = currentValue.Length - startPos + length;
            if (startPos + length > currentValue.Length)
                length = 0;
            var substring = parameters.Length > 1
                ? currentValue.Substring(startPos, length)
                : currentValue.Substring(startPos);

            formattingInfo.Write(substring);

            return true;
        }
    }
}
