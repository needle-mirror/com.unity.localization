# Dictionary source

The [Dictionary source](xref:UnityEngine.Localization.SmartFormat.Extensions.DictionarySource) extracts objects with a matching Key from an [IDictionary](https://docs.microsoft.com/en-us/dotnet/api/system.collections.idictionary) or [IDictionary<string, object>](https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.idictionary-).

When evaluating an IDictionary, if the Key is not a string, the Dictionary source first converts it to a string using the object’s ToString method. The case sensitivity of the comparison is determined by the Smart Form [Case Sensitivity](../LocalizationSettings.md#smart-format-settings) option.

| **Example Smart String**                 | **Arguments**                                                              | **Result**                       |
|------------------------------------------|----------------------------------------------------------------------------|----------------------------------|
| {SomeKey}                                | [!code-cs[](../../DocCodeSamples.Tests/SmartStringSamples.cs#args-dict-1)] | 999                              |
| Hello {Name} {Surname}                   | [!code-cs[](../../DocCodeSamples.Tests/SmartStringSamples.cs#args-dict-2)] | Hello Gordon Freeman             |
| {Name} {Surname} scored {1:Score} points | [!code-cs[](../../DocCodeSamples.Tests/SmartStringSamples.cs#args-dict-3)] | Gordon Freeman scored 153 points |

The following example shows how a dictionary could be provided as an argument.

[!code-cs[](../../DocCodeSamples.Tests/SmartStringSamples.cs#args-dict-example)]
