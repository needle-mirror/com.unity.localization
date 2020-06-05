# Plurals

The `PluralFormatter` uses language-specific pluralization rules to choose different text for **singular** and **plural** forms.  Some languages can have as many as 5 different forms.

## Example

```c#
Smart.Format("There {0:plural:is 1 item|are {} items}.", number);
// outputs "There is 1 item." or "There are 2 items."
```

## Syntax details
> { **Number** *: plural(lang)* : **singular** | **plural** *| more...* }

| Number | *:plural* | *(lang)* | singular | plural | *more...* |
|--------|-----------|--------|----------|--------|-----------|
| Any Number, like `int` or `double` | "plural" or "p" or *implicit* | *2-letter language code* | the singular text | the plural text | *more plural forms* |

* English has two plural forms: 1 is singular, everything else is plural.
  However, some languages have complex rules, with as many as 6 plural forms!  Special cases for 0, 2-4, or singular used for 21, 31, etc.
  So, for these languages, you should specify all plural forms, in smallest-to-largest order (eg. *zero|one|two|few|many*)
* The default language rule is *english*.  To use a specific rule, there are 3 ways:
 * Change the default: `Smart.Default.GetFormatterExtension<PluralLocalizationFormatter>().DefaultTwoLetterISOLanguageName = "ru";`
 * Supply a culture info: `Smart.Format(GetCultureInfo("ru"), "{0:банан|банана|бананов}", args...)`
 * Specify the *(lang)* like this: `{0:plural(ru):банан|банана|бананов}`
