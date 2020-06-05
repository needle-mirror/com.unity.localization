using System;
using System.Globalization;
using UnityEngine.Localization.SmartFormat.Core.Extensions;
using UnityEngine.Localization.SmartFormat.Net.Utilities;
using UnityEngine.Localization.SmartFormat.Utilities;

namespace UnityEngine.Localization.SmartFormat.Extensions
{
    [Serializable]
    public class TimeFormatter : FormatterBase
    {
        [SerializeField]
        TimeSpanFormatOptions m_DefaultFormatOptions = TimeSpanUtility.DefaultFormatOptions;

        private string m_DefaultTwoLetterIsoLanguageName = "en";

        public TimeFormatter()
        {
            Names = DefaultNames;
        }

        public override string[] DefaultNames => new[] {"timespan", "time", "t", ""};

        /// <summary>
        /// Determines the options for time formatting.
        /// </summary>
        public TimeSpanFormatOptions DefaultFormatOptions
        {
            get => m_DefaultFormatOptions;
            set => m_DefaultFormatOptions = value;
        }

        /// <summary>
        /// The ISO language name, which will be used for getting the <see cref="TimeTextInfo"/>.
        /// </summary>
        public string DefaultTwoLetterISOLanguageName
        {
            get => m_DefaultTwoLetterIsoLanguageName;
            set
            {
                if (CommonLanguagesTimeTextInfo.GetTimeTextInfo(value) == null)
                    throw new ArgumentException($"Language '{value}' for {nameof(value)} is not implemented.");
                m_DefaultTwoLetterIsoLanguageName = value;
            }
        }

        public override bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            var format = formattingInfo.Format;
            var current = formattingInfo.CurrentValue;

            if (format != null && format.HasNested) return false;
            string options;
            if (formattingInfo.FormatterOptions != "")
                options = formattingInfo.FormatterOptions;
            else if (format != null)
                options = format.GetLiteralText();
            else
                options = "";

            TimeSpan fromTime;

            switch (current)
            {
                case TimeSpan timeSpan:
                    fromTime = timeSpan;
                    break;
                case DateTime dateTime:
                    if (formattingInfo.FormatterOptions != "")
                    {
                        fromTime = SystemTime.Now().ToUniversalTime().Subtract(dateTime.ToUniversalTime());
                    }
                    else
                    {
                        return false;
                    }
                    break;
                case DateTimeOffset dateTimeOffset:
                    if (formattingInfo.FormatterOptions != "")
                    {
                        fromTime = SystemTime.OffsetNow().UtcDateTime.Subtract(dateTimeOffset.UtcDateTime);
                    }
                    else
                    {
                        return false;
                    }
                    break;
                default:
                    return false;
            }

            var timeTextInfo = GetTimeTextInfo(formattingInfo.FormatDetails.Provider);
            if (timeTextInfo == null) return false;
            var formattingOptions = TimeSpanFormatOptionsConverter.Parse(options);
            var timeString = fromTime.ToTimeString(formattingOptions, timeTextInfo);
            formattingInfo.Write(timeString);
            return true;
        }

        private TimeTextInfo GetTimeTextInfo(IFormatProvider provider)
        {
            // Return the default if there is no provider:
            if (provider == null)
                return CommonLanguagesTimeTextInfo.GetTimeTextInfo(DefaultTwoLetterISOLanguageName);

            // See if the provider can give us what we want:
            var timeTextInfo = (TimeTextInfo)provider.GetFormat(typeof(TimeTextInfo));
            if (timeTextInfo != null) return timeTextInfo;

            // See if there is a rule for this culture:
            if (!(provider is CultureInfo cultureInfo))
                return CommonLanguagesTimeTextInfo.GetTimeTextInfo(DefaultTwoLetterISOLanguageName);

            timeTextInfo = CommonLanguagesTimeTextInfo.GetTimeTextInfo(cultureInfo.TwoLetterISOLanguageName);
            // If cultureInfo was supplied,
            // we will always return, even if null:
            return timeTextInfo;
        }
    }
}
