# Is Match Formatter

Use the [Is Match Formatter](xref:UnityEngine.Localization.SmartFormat.Extensions.IsMatchFormatter) to evaluate [regular expressions](https://en.wikipedia.org/wiki/Regular_expression) to control the output.
The Is Match Formatter is not included by default. You can add it to the Formatters in the [Smart Format Settings](../LocalizationSettings.md#formatters).

**Note**: In the options part of the formatter, not all RegEx characters are valid, for example curly braces and parentheses aren’t supported. This is because the same parser is used as for all other extensions.

![Diagram showing the breakdown of the Smart String and how each part is evaluated when using IsMatch.](../images/SmartString-IsMatchSyntax.dot.svg)

| **Example Smart String**                                             | **Arguments**                                                                 | **Result**                      |
|----------------------------------------------------------------------|-------------------------------------------------------------------------------|---------------------------------|
| {theKey:ismatch(^.+123.+$):Okay - {}\|No match content}              | [!code-cs[](../../DocCodeSamples.Tests/SmartStringSamples.cs#args-ismatch-1)] | Okay - Some123Content           |
| {theKey:ismatch(^.+123.+$):Fixed content if match\|No match content} | [!code-cs[](../../DocCodeSamples.Tests/SmartStringSamples.cs#args-ismatch-1)] | Fixed content if match          |
| {0:list:{:ismatch(^100\|200\|999$):{:0.00}\|'no match'}\|, \| and }  | [!code-cs[](../../DocCodeSamples.Tests/SmartStringSamples.cs#args-ismatch-2)] | 100.00, 200.00 and 'no match'   |
| {0:list:{:ismatch(^100\|200\|999$):'match'\|'no match'}\|, \| and }  | [!code-cs[](../../DocCodeSamples.Tests/SmartStringSamples.cs#args-ismatch-2)] | 'match', 'match' and 'no match' |
