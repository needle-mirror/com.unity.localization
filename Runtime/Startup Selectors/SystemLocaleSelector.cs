using System.Globalization;
using UnityEngine;

namespace UnityEngine.Localization
{
    [CreateAssetMenu(menuName = "Localization/Locale Selectors/System Locale Selector")]
    public class SystemLocaleSelector : StartupLocaleSelector
    {
        public override Locale GetStartupLocale(LocalesProvider availableLocales)
        {
            Locale locale = null;
            if (Application.systemLanguage != SystemLanguage.Unknown)
            {
                locale = availableLocales.GetLocale(Application.systemLanguage);
            }

            if (locale == null)
            {
                var cultureInfo = CultureInfo.CurrentUICulture;
                locale = availableLocales.GetLocale(cultureInfo);
                if (locale == null)
                {
                    // Attempt to use CultureInfo fallbacks to find the closest locale
                    while (!Equals(cultureInfo, CultureInfo.InvariantCulture) && locale == null)
                    {
                        locale = availableLocales.GetLocale(cultureInfo);
                        cultureInfo = cultureInfo.Parent;
                    }

                    if (locale != null)
                    {
                        Debug.Log($"Locale '{CultureInfo.CurrentUICulture}' is not supported, however the parent locale '{locale.Identifier.CultureInfo}' is.");
                    }
                }
            }
            return locale;
        }
    }
}