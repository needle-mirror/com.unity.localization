# Is Smart.Format compatible with string.Format?

Absolutely.  The core of `Smart.Format` supports **all** the features of `String.Format`.
You can read about the standard features of `String.Format` [[at Microsoft's Composite Formatting reference page|http://msdn.microsoft.com/en-us/library/txafckwd.aspx]].  The main features are:

## Numbered Placeholders
Numbers correspond to the argument index:
```c#
Smart.Format("{0} {1}", "Hello", "World");
// Outputs: "Hello World"
```

## Item Formats
Determines how to format items, such as numbers and dates:
```c#
Smart.Format("{0:N3} | {1:MMMM, yyyy}", 5.5, new DateTime(2010,3,4));
// Outputs: "5.500 | March, 2010"
```

## Alignment
Inserts spaces before or after the item, aligning it with other items:
```c#
Smart.Format("|{0,-10}|{1,10}|", "Left", "Right");
// Outputs: "|Left      |     Right|"
```

## Brace Escaping
Escape braces using double-braces:
```c#
Smart.Format("{{0}} {{{0}}} {{}}", "Zero");
// Outputs: "{0} {Zero} {}"
```

## Localization
`Smart.Format` uses the current culture info for formatting, or you can supply your own `IFormatProvider`:
```c#
Smart.Format(CultureInfo.GetCultureInfo("en-US"), "{0:C}", 1234) // Outputs: "$1,234.00"
Smart.Format(CultureInfo.GetCultureInfo("ja-JP"), "{0:C}", 1234) // Outputs: "¥1234"
```
