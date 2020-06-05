using System;
using System.Collections.Generic;
using UnityEngine.Localization.SmartFormat.Core.Extensions;
using UnityEngine.Localization.SmartFormat.Core.Parsing;

namespace UnityEngine.Localization.SmartFormat.Extensions
{
    [Serializable]
    public class ChooseFormatter : FormatterBase
    {
        [SerializeField]
        char m_SplitChar = '|';

        public char SplitChar
        {
            get => m_SplitChar;
            set => m_SplitChar = value;
        }

        public ChooseFormatter()
        {
            Names = DefaultNames;
        }

        public override string[] DefaultNames => new[] {"choose", "c"};

        public override bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            if (formattingInfo.FormatterOptions == "") return false;
            var chooseOptions = formattingInfo.FormatterOptions.Split(SplitChar);
            var formats = formattingInfo.Format.Split(SplitChar);
            if (formats.Count < 2) return false;

            var chosenFormat = DetermineChosenFormat(formattingInfo, formats, chooseOptions);

            formattingInfo.Write(chosenFormat, formattingInfo.CurrentValue);

            return true;
        }

        private static Format DetermineChosenFormat(IFormattingInfo formattingInfo, IList<Format> choiceFormats,
            string[] chooseOptions)
        {
            var currentValue = formattingInfo.CurrentValue;
            var currentValueString = currentValue == null ? "null" : currentValue.ToString();

            var chosenIndex = Array.IndexOf(chooseOptions, currentValueString);

            // Validate the number of formats:
            if (choiceFormats.Count < chooseOptions.Length)
                throw formattingInfo.FormattingException("You must specify at least " + chooseOptions.Length +
                    " choices");
            if (choiceFormats.Count > chooseOptions.Length + 1)
                throw formattingInfo.FormattingException("You cannot specify more than " + (chooseOptions.Length + 1) +
                    " choices");
            if (chosenIndex == -1 && choiceFormats.Count == chooseOptions.Length)
                throw formattingInfo.FormattingException("\"" + currentValueString +
                    "\" is not a valid choice, and a \"default\" choice was not supplied");

            if (chosenIndex == -1) chosenIndex = choiceFormats.Count - 1;

            var chosenFormat = choiceFormats[chosenIndex];
            return chosenFormat;
        }
    }
}
