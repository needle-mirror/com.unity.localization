using System;
using UnityEngine.Localization.SmartFormat.Core.Extensions;
using UnityEngine.Localization.SmartFormat.Core.Formatting;
using UnityEngine.Localization.SmartFormat.Utilities;

namespace UnityEngine.Localization.SmartFormat.Extensions
{
    [Serializable]
    public class ValueTupleSource : ISource
    {
        private readonly SmartFormatter _formatter;

        public ValueTupleSource(SmartFormatter formatter)
        {
            _formatter = formatter;
        }

        public bool TryEvaluateSelector(ISelectorInfo selectorInfo)
        {
            if (!(selectorInfo is FormattingInfo formattingInfo)) return false;
            if (!(formattingInfo.CurrentValue != null && formattingInfo.CurrentValue.IsValueTuple())) return false;

            var savedCurrentValue = formattingInfo.CurrentValue;
            foreach (var obj in formattingInfo.CurrentValue.GetValueTupleItemObjectsFlattened())
            {
                foreach (var sourceExtension in _formatter.SourceExtensions)
                {
                    formattingInfo.CurrentValue = obj;
                    var handled = sourceExtension.TryEvaluateSelector(formattingInfo);
                    if (handled)
                    {
                        formattingInfo.CurrentValue = savedCurrentValue;
                        return true;
                    }
                }
            }

            formattingInfo.CurrentValue = savedCurrentValue;

            return false;
        }
    }
}
