# Value Tuple source

The [Value Tuple source](xref:UnityEngine.Localization.SmartFormat.Extensions.ValueTupleSource) can iterate through a [Tuple’s](https://docs.microsoft.com/en-us/dotnet/api/system.tuple-7) values, evaluating each item against the sources and formatters.

| **Example Smart String** | **Arguments**                                                               | **Result**       |
|--------------------------|-----------------------------------------------------------------------------|------------------|
| It is {Name} {Key}       | [!code-cs[](../../DocCodeSamples.Tests/SmartStringSamples.cs#args-tuple-1)] | It is Juan Value |
