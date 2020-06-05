# Sub-String

The `SubStringFormatter` lets you output parts of an input string.

## Examples
```Csharp
var people = new List<object>
    {new {Name = "Long John", City = "New York"}, new {Name = "Short Mary", City = "Massachusetts"},};

Smart.Format("{Name:substr(5)}", people.First());
// result: "John"

Smart.Format("{City:substr(0,3)}", people.First());
// result: "New"
```

> Note:

The **SubStringFormatter** is included in the default SmartFormatter instance.
