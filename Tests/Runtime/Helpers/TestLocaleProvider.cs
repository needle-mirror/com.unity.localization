using System.Collections.Generic;
using UnityEngine.Localization.Settings;

namespace UnityEngine.Localization.Tests
{
    public class TestLocaleProvider : ILocalesProvider
    {
        public List<Locale> Locales { get; set; } = new List<Locale>();

        public void AddLocale(Locale locale)
        {
            Locales.Add(locale);
        }

        public Locale GetLocale(LocaleIdentifier id)
        {
            return Locales.Find(o => o.Identifier == id);
        }

        public Locale GetLocale(string code)
        {
            return Locales.Find(o => o.Identifier == code);
        }

        public Locale GetLocale(SystemLanguage systemLanguage)
        {
            return Locales.Find(o => o.Identifier == systemLanguage);
        }

        public bool RemoveLocale(Locale locale)
        {
            return Locales.Remove(locale);
        }
    }
}
