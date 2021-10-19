# List Formatter

The [List Formatter](xref:UnityEngine.Localization.SmartFormat.Extensions.ListFormatter) is both a Source and a Formatter. You can use it as a Source to extract specific values from an [IList](https://docs.microsoft.com/en-us/dotnet/api/system.collections.ilist) by using an index.

| **Example Smart String**                                                                | **Arguments**                                                              | **Result**                                                                                             |
|-----------------------------------------------------------------------------------------|----------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------|
| The value of the item at index 1 is {0.1}                                               | [!code-cs[](../../DocCodeSamples.Tests/SmartStringSamples.cs#args-list-1)] | The value of the item at index 1 is 2                                                                  |
| {0.0} {0.1} {0.2}                                                                       | [!code-cs[](../../DocCodeSamples.Tests/SmartStringSamples.cs#args-list-2)] | 1 Hello World                                                                                          |
| The index placeholder can be used to sync multiple lists such as {0:{} = {1.index}\|, } | [!code-cs[](../../DocCodeSamples.Tests/SmartStringSamples.cs#args-list-3)] | The index placeholder can be used to sync multiple lists such as 1 = one, 2 = two, 3 = three, 4 = four |
