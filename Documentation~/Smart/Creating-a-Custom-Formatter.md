# Creating a custom formatter

You can create a custom Formatter by inheriting from the [FormatterBase](xref:UnityEngine.Localization.SmartFormat.Core.Extensions.FormatterBase) class.

This example shows how to create a formatter to format an integer that represents bytes.

[!code-cs[](../../DocCodeSamples.Tests/SmartStringSamples.cs#byte-formatter)]

| **Example Smart String**    | **Arguments** | **Result**               |
| --------------------------- | ------------- | ------------------------ |
| The file size is {0:byte()} | `100`         | The file size is 100 B   |
|                             | `1000`        | The file size is 0.98 KB |
|                             | `1234`        | The file size is 1.21 KB |
|                             | `10000000`    | The file size is 9.5 MB  |
|                             | `2000000000`  | The file size is 1.9 GB  |
