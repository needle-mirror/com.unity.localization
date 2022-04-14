# Reflection source

The [Reflection source](xref:UnityEngine.Localization.SmartFormat.Extensions.ReflectionSource) is used to select using [.Net Reflection](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/reflection). Named placeholders are evaluated against the current object.
The Reflection Source can be used to extract an object from a property, field or method.

**Note**: When using a method, the method must be parameterless and not return void.

| **Example Smart String**                                  | **Arguments**                                                                                                                                                       | **Result**                                                                            |
|-----------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------|
| The value is {gameObject.name}                            | [!code-cs[](../../DocCodeSamples.Tests/SmartStringSamples.cs#args-reflection-1)]                                                                                    | The value is Camera                                                                   |
| You can call methods to modify values such as {0.ToUpper} | [!code-cs[](../../DocCodeSamples.Tests/SmartStringSamples.cs#args-reflection-2)]                                                                                    | You can call methods to modify values such as HELLO WORLD                             |
| My name is {Name} and I am {Age} years old.               | [!code-cs[](../../DocCodeSamples.Tests/SmartStringSamples.cs#args-reflection-3a)] [!code-cs[](../../DocCodeSamples.Tests/SmartStringSamples.cs#args-reflection-3b)] | My name is Juan Pérez and I am 39 years old.                                          |
| Mr {name.ToCharArray.0} {surname}                         | [!code-cs[](../../DocCodeSamples.Tests/SmartStringSamples.cs#args-reflection-4)]                                                                                    | Mr Y Taro                                                                             |
