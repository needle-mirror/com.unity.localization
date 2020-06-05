# isMatch

The `IsMatchFormatter` lets you evaluate regular expressions to control the output.

## Examples
```Csharp
var data = new Dictionary<string,object>() { {"theKey", "Some123Content"}};

Smart.Format("{theKey:ismatch(^.+123.+$):Okay - {}|No match content}", data);
// result: Okay - Some123Content

Smart.Format("{theKey:ismatch(^.+123.+$):Fixed content if match|No match content}", data);
// result: Fixed content if match
```

Working with lists:
```CSharp
var myList = new List<int> {100, 200, 300};

Smart.Format(CultureInfo.InvariantCulture, "{0:list:{:ismatch(^100|200|999$):{:0.00}|'no match'}|, | and }", myList));
// result: 100.00, 200.00 and 'no match'

Smart.Format(CultureInfo.InvariantCulture, "{0:list:{:ismatch(^100|200|999$):'match'|'no match'}|, | and }", myList));
// result: 'match', 'match' and 'no match'
```

## Syntax Details
> { **Any Value** **: ismatch(*RegExpression*) :** output if match | output if no match ** }

> Note:

The **IsMatchFormatter** is not included in the default SmartFormatter instance.
In case you'll need it, it must be added like so:

```CSharp
Smart.Default.FormatterExtensions.Add(new IsMatchFormatter { RegexOptions = RegexOptions.CultureInvariant });
```

## Limitations

In the *options* part of the **IsMatchFormatter** not all *RegEx* characters are valid.
E.g.: No curly braces or parenthesis.
This is because the same parser is used as for all other extensions.
