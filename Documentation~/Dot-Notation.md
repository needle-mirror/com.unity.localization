# Dot Notation

SmartFormat.NET lets you use dot notation inside ```{place.holders}```. This is the same concept as accessing class members in C#.

## string.Format
In C# you would use the following code for a static format expression:
```csharp
var addr = new Address();
var result = $"Address: {addr.City.ZipCode} {addr.City.Name}, {addr.City.AreaCode}"
```
From a performance perspective this is the fastest way to get a formatted result. However, the format string is defined during compile time. In case the format string should be variable, ```string.Format``` is limited to indexed placeholders like ```"{0}, {1}"```. Not very handy for a big number of placeholders.

**Performance Index: 1**

## Smart.Format Using Reflection
The most obvious usage of dot notation is with a class instance as an argument to ```Smart.Format```. Let's assume an ```Address``` class having a ```City``` class as one of its properties. Then the code is:
```csharp
var result = Smart.Format("Address: {City.ZipCode} {City.Name}, {City.AreaCode}", new Address());
```
This time the format string can be assigned and changed at runtime. Accessing class members with reflection brings a performance penalty:

**Performance Index: 70**

## Smart.Format Using a Dictionary
Another option to make use of dot notation is with a ```Dictionary``` hierarchy as an argument to ```Smart.Format```.
You simple write some code to create a hierarchical projection of all required class members to ```Dictionary```. For our ```Address``` class with a property of class ```City``` the code could be:
```csharp
public Dictionary<string, string> ToDictionary()
{
    return new Dictionary<string, string>
    {
        {nameof(Name), Name},
        {nameof(ZipCode), ZipCode},
        {nameof(AreaCode), AreaCode}
    };
}
```
This dictionary is the value of a parent Dictionary key-value-pair with a key of "City". Now you can use dot notation in the same way as with reflection:
```csharp
var result = Smart.Format("Address: {City.ZipCode} {City.Name}, {City.AreaCode}", new Address().ToDictionary());
```
This is almost 6 times faster than reflection, coming with a certain memory penalty.

**Note**: Well known ORM Frameworks have built-in support to project entities or query results to Dictionaries (e.g. MS Entity Framework or LLBLGen Pro, just to name two of them). So here it's easy to take advantage of ```Dictionary``` arguments to ```Smart.Format``` with almost no additional coding.

**Performance Index: 12**

## Conclusion

In case performance of string formatting is a major requirement, and convenient dot notation is appreciated, then the projection of a class to a ```Dictionary``` hierarchy is a good alternative.

## Performance Test Setup

The tests for calculating the performance indexes where setup as follows:
* The instance of the ```SmartFormatter``` had only 2 source extensions: ```ReflectionSource``` or ```DictionarySource```, and ```DefaultSource```.
* The instance of the ```SmartFormatter``` had only 1 formatter extension (```DefaultFormatter```)
* Each test ran with 100,000 iterations
* Release build running with NUnit 3 on .Net Framework 4.5

The performance test is part of the ```DictionaryFormatter``` unit tests.
