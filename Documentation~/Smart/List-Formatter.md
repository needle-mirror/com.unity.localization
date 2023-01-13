# List Formatter

The [List Formatter](xref:UnityEngine.Localization.SmartFormat.Extensions.ListFormatter) is a Source and a Formatter. You can use it as a Formatter to repeat the contents of any [IEnumerable](https://docs.microsoft.com/en-us/dotnet/api/system.collections.ienumerable), such as an Array or List.

![Diagram showing the breakdown of the Smart String and how each part is evaluated.](../images/SmartString-ListFormatterSyntax.dot.svg)

The spacers can also include character literals, for example, instead of a comma you could also use **\n** to list each item on a new line.
Use the placeholder index to access the current index of the list being iterated. You can use this to access multiple lists.

| **Example Smart String**                                                                          | **Arguments**                                                                        | **Result**                                                                                                       |
|---------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------|
| {0:list:{}\|, \|, and }                                                                           | [!code-cs[](../../DocCodeSamples.Tests/SmartStringSamples.cs#args-list-formatter-1)] | one, two, and three                                                                                              |
| {theKey:ismatch(^.+123.+$):Fixed content if match\|No match content}                              | [!code-cs[](../../DocCodeSamples.Tests/SmartStringSamples.cs#args-list-formatter-2)] | Fixed content if match                                                                                           |
| {0:list:{:ismatch(^100\|200\|999$):{:0.00}\|'no match'}\|, \| and }                               | [!code-cs[](../../DocCodeSamples.Tests/SmartStringSamples.cs#args-list-formatter-3)] | 100.00, 200.00 and 'no match'                                                                                    |
| {0:list:{:ismatch(^100\|200\|999$):'match'\|'no match'}\|, \| and }                               | [!code-cs[](../../DocCodeSamples.Tests/SmartStringSamples.cs#args-list-formatter-3)] | 'match', 'match' and 'no match'                                                                                  |
| The index placeholder can be used to sync multiple lists such as:<br><br>{0:{} = {1.index}\|, } | [!code-cs[](../../DocCodeSamples.Tests/SmartStringSamples.cs#args-list-formatter-4)] | The index placeholder can be used to sync multiple lists such as:<br><br>1 = one, 2 = two, 3 = three, 4 = four |

## Localized String Example

This example shows how to pass a list as an argument to a LocalizedString.

> [!NOTE]
> The list must be one of the arguments, in this example it is argument 0.

[!code-cs[](../../DocCodeSamples.Tests/SmartStringSamples.cs#args-list-formatter-loc-string-example)]
