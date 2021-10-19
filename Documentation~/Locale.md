# Locale

A [Locale](xref:UnityEngine.Localization.Locale) represents a language and region. It can also contain additional information, such as currency, calendar, and user-added custom data.

Locales wrap the .Net [CultureInfo](https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo) class. However, you can also define a custom Locale that the CultureInfo class does not support.

![The Locale Inspector view.](images/LocaleInspector.png)

| **Property**              | **Description** |
| ------------------------- | --------------- |
| **Locale Name**           | The name of the Locale. Set this value to customize how the Locale name should be presented to the user, such as in a language selection menu.
| **Locale Code**           | The culture name in the format `[language]-[region]`. The name is a combination of an [ISO 639](https://www.iso.org/iso-639-language-codes.html) two-letter lowercase culture code associated with a language and an [ISO 3166](https://www.iso.org/iso-3166-country-codes.html) two-letter uppercase subculture code associated with a country or region. For example, Language English would be 'en', Regional English(UK) would be 'en-GB' and Regional English(US) would be 'en-US'. It is possible to use any string value when representing a non-standard identifier.
| **Culture Info**          | This field shows the [Culture Info](https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo) match that was found for the Locale Code.
| **Use Custom Formatter**  | When region specific formatting is applied using [Smart Strings](Smart/SmartStrings.md), the [Culture Info](https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo) is used. This controls formatting such as time, date, currency and numbers. Enable the Custom Formatter field to be able to change a locale using a different formatter.
| **Custom Formatter Code** | A Custom code in the form `[language]-[region]` that you can use when formatting region specific values.
| **Sort Order**            | Use the sort order to override the order of Locales when sorted in a list. You can use the ordering of this list when displaying the Locales in a language selection menu. If Locales both have the same SortOrder then they will be sorted by their Locale Names. By default, Locales are sorted in ascending order with the lowest Sort Order first.
| **Metadata**              | Use this to attach additional [Metadata](Metadata.md) to a locale which can be used by scripts at runtime.

## Fallbacks

When the Localization system can not find a localized asset or string for a particular Locale, it can attempt to fallback to another Locale and use the localized value from that.
For example, a game may have a regional language such as French(Canadian) that only implements a subset of the game’s text, and then falls back to French(France) for the remainder. You can also use this to fallback to completely different Locales, such as when a game may only be partially translated and it is better to show the text in the game's default language rather than not at all.

To configure a Locale fallback, add the [Fallback Locale](xref:UnityEngine.Localization.Metadata.FallbackLocale) metadata to a Locale.

![Example of a French Locale configured to fallback to English.](images/FallbackMetadata.png)
_Example of a French Locale configured to fallback to English_
