# Data Sources

| Data type  | What does it look for?                       | Examples |
| ---------- | -------------------------------------------- | -------- |
| Any object | Properties, Fields, or parameterless methods | `Smart.Format("{Prop.ToString}", new{ Prop = 999 })` |
| Dictionary | Keys                                         | `Smart.Format("{SomeKey}", new Dictionary<string, object>(){ { "SomeKey", 999 } } )` |
| Dynamics   | Works just like Dictionary                   | `Smart.Format("{Prop}", (dynamic)(new { Prop = 999 }))` |
| JObject (JSON)| Finds child elements by name                 | `Smart.Format("{Name}", JObject.Parse("{ 'Name':'John'}"))` |
| XML        | Finds child elements by name                 | `Smart.Format("{NodeName}", someXElement)` |
| Default    | Looks up arguments by index                  | `Smart.Format("{0}", 999)` |

## JSON
JSON also comes in handy when processing data in a web api application where the argument submitted from the browser to the controller is a ```JObject```. Another scenario is working with queries from SQL Server:
```SQL
SELECT 'John' AS [FirstName], 'Doe' AS [LastName], 32 AS [Age]
FOR JSON PATH, ROOT('Father')
```
You can parse the query result into a ```JObject``` and give it to SmartFormat as an argument. ```JObject``` may be an array for the **ListFormatter**.

## SmartObjects

**Note**:

The examples below use Dictionaries for simplicity. In the same manner other types than Dictionaries can be used as data sources.

With SmartObjects
* all objects used for Smart.Format can be collected in one place as the *first* argument
* the format string can be written like each object would be the first argument of Smart.Format
* there is no need to bother from which argument a value should come from

Example:

```csharp
var d1 = new Dictionary<string,string> { {"myKey", "myValue"} };
var nl = new Dictionary<string,string> { {"2ndKey", "my2ndValue" } };

var smartObj = new SmartObjects();
smartObj.AddRange(new object[] {d1, nl});

Smart.Format("{myKey} {2ndKey} and more...", smartObj);

// result:
// "myValue my2ndValue and more"
```

## More arguments to the format string

Like with ```string.Format``` it is possible to use several data sources as parameters to SmartFormat. The conecept however is a bit different:
```C#
var dict1 = new Dictionary<string, string>() { {"Name", "John"} };
var dict2 = new Dictionary<string, string>() { { "City", "Washington" } };

var result = Smart.Format("First dictionary: {0:{Name}}, second dictionary: {1:{City}}", dict1, dict2);

// alternative notation:
var result = Smart.Format("First dictionary: {0.Name}, second dictionary: {1.City}", dict1, dict2);

// result:
// "First dictionary: John, second dictionary: Washington"
```
