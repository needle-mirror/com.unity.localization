using System;
using System.Linq;
using System.Xml.Linq;
using UnityEngine.Localization.SmartFormat.Core.Extensions;

namespace UnityEngine.Localization.SmartFormat.Extensions
{
    [Serializable]
    public class XmlSource : ISource
    {
        // There is a bug with SerializeField that causes empty instances to not deserialize. This is a workaround while we wait for the fix (case 1183543)
        [SerializeField, HideInInspector]
        int dummyObject;

        public XmlSource(SmartFormatter formatter)
        {
            // Add some special info to the parser:
            formatter.Parser.AddAlphanumericSelectors(); // (A-Z + a-z)
            formatter.Parser.AddAdditionalSelectorChars("_");
            formatter.Parser.AddOperators(".");
        }

        public bool TryEvaluateSelector(ISelectorInfo selectorInfo)
        {
            var element = selectorInfo.CurrentValue as XElement;
            if (element != null)
            {
                var selector = selectorInfo.SelectorText;
                // Find elements that match a selector
                var selectorMatchedElements = element.Elements()
                    .Where(x => x.Name.LocalName == selector).ToList();
                if (selectorMatchedElements.Any())
                {
                    selectorInfo.Result = selectorMatchedElements;
                    return true;
                }
            }

            return false;
        }
    }
}
