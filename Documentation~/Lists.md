# Lists

The `ListFormatter` repeats the contents of any ```IEnumerable```, e.g. an Array.

## Example

```c#
var items = new[] { "one", "two", "three" };
// important: do not use only "items" as the parameter
var result = Smart.Format("{0:list:{}|, |, and }", new object[] { items });
// Outputs: "one, two, and three"
```

## Syntax details
> { **IEnumerable** *: alias* : **template** | **spacer** *| finalSpacer* }

| IEnumerable | *alias* | template | spacer | *finalSpacer* |
|-------|-------|------------|----------|---------------|
| Any `IEnumerable` | "list" or "l" or implicit | required | required | *optional* |

* **template** will be repeated for each item
* **spacer** will be added after each item except the last
* *finalSpacer* if supplied, will replace the very final **spacer**

**Note**: The spacers may also in include character literals. So e.g. instead of a comma you could also use \n for listing each item on a new line.

## Nested IEnumerable Example

    var array1 = new[] { "Pepsi", "Coke", "Mt. Dew", "water" };
    var array2 = new[] { "pizza", "hamburger" };
    var array3 = new[] { "fries", "chips", "pretzels" };
    var array4 = new[] { "carrots", "corn", "green beans" };
    var arrayOfArrays = new[] { array1, array2, array3, array4 };

    var namedFormatString = "{Results:list:{:list:|, |, and }|; |; }";

    Console.WriteLine(SmartFormat.Smart.Format(namedFormatString, new {Results = arrayOfArrays}));

    //outputs:
    //Pepsi, Coke, Mt. Dew, and water; pizza, and hamburger; fries, chips, and pretzels; carrots, corn, and green beans
