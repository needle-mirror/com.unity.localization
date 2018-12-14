using System;

namespace UnityEngine.Localization
{
    public class CommandLineLocaleSelector : StartupLocaleSelector
    {
        [SerializeField]
        string m_CommandLineArgument = "-language =";

        /// <summary>
        /// The command line argument used to assign the locale.
        /// </summary>
        public string CommandLineArgument
        {
            get { return m_CommandLineArgument; }
            set { m_CommandLineArgument = value; }
        }

        public override Locale GetStartupLocale(LocalesProvider availableLocales)
        {
            if (string.IsNullOrEmpty(m_CommandLineArgument))
                return null;

            foreach (var arg in Environment.GetCommandLineArgs())
            {
                if (arg.StartsWith(m_CommandLineArgument, StringComparison.OrdinalIgnoreCase))
                {
                    var argValue = arg.Substring(m_CommandLineArgument.Length);
                    var foundLocale = availableLocales.GetLocale(argValue);

                    if (foundLocale != null)
                        Debug.LogFormat("Found a matching locale({0}) for command line argument: `{1}`.", argValue, foundLocale);
                    else
                        Debug.LogWarningFormat("Could not find a matching locale for command line argument: `{0}`", argValue);

                    return foundLocale;
                }
            }
            return null;
        }
    }
}