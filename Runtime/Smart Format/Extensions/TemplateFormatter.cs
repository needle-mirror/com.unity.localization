using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Localization.SmartFormat.Core.Extensions;
using UnityEngine.Localization.SmartFormat.Core.Parsing;

namespace UnityEngine.Localization.SmartFormat.Extensions
{
    /// <summary>
    /// Template Formatter allows for registering reusable templates, and use them by name.
    /// </summary>
    [Serializable]
    public class TemplateFormatter : FormatterBase
    {
        [SerializeReference]
        SmartFormatter m_Formatter;

        private IDictionary<string, Format> m_Templates;

        IDictionary<string, Format> Templates
        {
            get
            {
                if (m_Templates == null)
                {
                    var stringComparer = m_Formatter.Settings.GetCaseSensitivityComparer();
                    m_Templates = new Dictionary<string, Format>(stringComparer);
                }

                return m_Templates;
            }
        }

        public TemplateFormatter(SmartFormatter formatter)
        {
            m_Formatter = formatter;
            Names = DefaultNames;
        }

        public override string[] DefaultNames => new[] { "template", "t" };

        /// <summary>
        /// This method is called by the <see cref="SmartFormatter" /> to obtain the formatting result of this extension.
        /// </summary>
        /// <param name="formattingInfo"></param>
        /// <returns>Returns true if successful, else false.</returns>
        public override bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            var templateName = formattingInfo.FormatterOptions;
            if (templateName == "")
            {
                if (formattingInfo.Format.HasNested) return false;
                templateName = formattingInfo.Format.RawText;
            }

            Format template;
            if (!Templates.TryGetValue(templateName, out template))
            {
                if (Names.Contains(formattingInfo.Placeholder.FormatterName))
                    throw new FormatException(
                        $"Formatter '{formattingInfo.Placeholder.FormatterName}' found no registered template named '{templateName}'");

                return false;
            }

            formattingInfo.Write(template, formattingInfo.CurrentValue);
            return true;
        }

        /// <summary>
        /// Register a new template.
        /// </summary>
        /// <param name="templateName">A name for the template, which is not already registered.</param>
        /// <param name="template">The string to be used as a template.</param>
        public void Register(string templateName, string template)
        {
            var parsed = m_Formatter.Parser.ParseFormat(template, m_Formatter.GetNotEmptyFormatterExtensionNames());
            Templates.Add(templateName, parsed);
        }

        /// <summary>
        /// Remove a template by its name.
        /// </summary>
        /// <param name="templateName"></param>
        /// <returns></returns>
        public bool Remove(string templateName)
        {
            return Templates.Remove(templateName);
        }

        /// <summary>
        /// Remove all templates.
        /// </summary>
        public void Clear()
        {
            Templates.Clear();
        }
    }
}
