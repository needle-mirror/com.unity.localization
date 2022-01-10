# Creating a custom source

You can create custom sources using the [ISource](xref:UnityEngine.Localization.SmartFormat.Core.Extensions.ISource) interface.

To use a custom Source, add it to the [Sources](LocalizationSettings.md#sources) list in the **LocalizationSettings**.

The following example shows how to create a source that generates a random number. This could then be combined with a [choose Formatter](Choose-Formatter.md) to produce different random responses.

[!code-cs[](../../DocCodeSamples.Tests/RandomValueSource.cs)]

| **Example Smart String**                                | **Result** |
|---------------------------------------------------------|------------|
| {random:choose(1\|2\|3}:Hello\|Greetings\|Welcome\|Hi}! | Welcome!   |
