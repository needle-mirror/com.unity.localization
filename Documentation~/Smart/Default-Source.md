# Default source

The [default source](xref:UnityEngine.Localization.SmartFormat.Extensions.DefaultSource) replicates the [String.Format](https://docs.microsoft.com/en-us/dotnet/api/system.string.format) behavior and extracts an argument by the index.

The default source examines the current selector. If the selector can be parsed as an integer and is a valid index (within the number of provided arguments), the default source extracts the argument at the selected index and no further selectors are considered.

| **Example Smart String**         | **Arguments**                                                                     | **Result**                      |
|----------------------------------|-----------------------------------------------------------------------------------|---------------------------------|
| {0} {1} {2}                      | [!code-cs[](../../DocCodeSamples.Tests/SmartStringSamples.cs#args-one-two-three)] | 1 2 3                           |
| {1} {1} {2} {0}                  | [!code-cs[](../../DocCodeSamples.Tests/SmartStringSamples.cs#args-one-two-three)] | 2 2 3 1                         |
| Player {0} scored {1} points     | [!code-cs[](../../DocCodeSamples.Tests/SmartStringSamples.cs#args-string-int)]    | Player Potato scored 155 points |
| Player {0:Name} score {0:Points} | [!code-cs[](../../DocCodeSamples.Tests/SmartStringSamples.cs#args-anon-class)]    | Player One Scored 100 points    |
