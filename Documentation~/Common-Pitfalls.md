# Common Pitfalls

## Smart.Format vs. SmartFormatter.Format
Most of the examples in this Wiki are using "Smart.Format(...)". This is for a good reason: Smart.Format() automatically initializes the SmartFormatter with default extensions. If you're using SmartFormatter.Format() directly, then it's your job to initialize. So for the beginning just leave SmartFormatter.Format() alone.
Note: ```Smart.Format(...)``` is just the short version for ```Smart.Default.Format(...)```.

## Error Handling
By default, SmartFormat sets ```ErrorAction``` for formatter and parser to ```ErrorAction.Ignore```. This can lead to confusing results. It's highly recommended, to turn exceptions on while developing and debugging the code:
```C#
Smart.Default.Settings.FormatErrorAction = ErrorAction.ThrowError;
Smart.Default.Settings.ParseErrorAction = ErrorAction.ThrowError;
```

## Error Tracking
Besides throwing and ```catch```ing exceptions it is possible, to trace any formatting or parsing errors by subscribing to the corresponding events. When using ```Smart.Format(...)``` these events are:
```C#
var badPlaceholders = new HashSet<string>();
Smart.Default.OnFormattingFailure += (sender, args) => { badPlaceholders.Add(args.Placeholder); };

var parsingErrorText = new HashSet<string>();
Smart.Default.Parser.OnParsingFailure += (sender, args) => { parsingErrorText.Add(args.RawText); };
```
These events fire no matter how ```ErrorAction``` of the formatter or parser are set. Opposed to exceptions, **all errors** will be reported, not only the first failure. Going this way you can decide in your code more fine grained, how to deal with errors.

## Escaping Curly Braces
Out of the box SmartFormat is a drop-in replacement for ```string.Format```. The consequence is, that curly braces are escaped the ```string.Format``` way. So if the desired output shall be ```{literal}```, it means doubling the open and closing curly braces:
```c#
string.Format("{{literal}}")
Smart.Format("{{literal}}")
```
This ```string.Format``` compatibility, however, causes problems when using SmartFormat's extended formatting capabilities, like
```c#
// This won't work with default settings!
Smart.Format("{0:{Persons:{Name}|, }}", model);
```
The reason is the double curly braces at the end of the format string, which the parser will escape, leading to a missing closing brace exception. Luckily, this is easy to solve:
```c#
// This will work
Smart.Default.Parser.UseAlternativeEscapeChar('\\');
Smart.Format("{0:{Persons:{Name}|, }}", model);
```
With this setting the output ```{literal}``` can be achieved by ```Smart.Format("\{literal\}")```.

So in short: Set ```Smart.Default.Parser.UseBraceEscaping()``` as soon as you're using complex formatters, use the default behavior in order to achieve ```string.Format``` compatibility.

## Formatting Numbers, Date, Time, Currency etc. Culture-aware
Same as with ```string.Format``` it may be necessary to supply the ```CultureInfo``` in order to generate properly formatted output strings. Example:
```Smart.Format(new CultureInfo("en-US"), "{0:C}", 1234) // Outputs: "$1,234.00"```

## The Template Formatter
Keeping the following steps is important.
1. Have a formatter
```
var formatter = Smart.CreateDefaultSmartFormat();
```
2. Create a ```TemplateFormatter``` object.
```
var templates = new TemplateFormatter(formatter);
```
3. Add the ```TemplateFormatter``` to the formatter extensions
```
formatter.AddExtensions(templates);
```
4. Finally register the templates
```
templates.Register("SomeTemplate", "The template content: {variable}");
```
Why? When adding a template, the value will be parsed using all extensions. At this stage the ```TemplateFormatter``` should already be part of the extensions. Only then it will be possible to have nested templates.

## Parameterless methods
Only instance methods of a type kind be used in a placeholder, **extension methods cannot**.

## Security Exception
When running on .NET Framework 4.x SmartFormat is using ```System.Runtime.Remoting.Messaging.CallContext.LogicalGet|SetData``` in its ```ListFormatter```.
With this in place, we know that [dotnetfiddle.net](https://www.dotnetfiddle.net/) will throw a security exception. When compiling SmartFormat for .Net Core, ```LogicalGet|SetData``` is replaced by ```AsyncLocal<T>```, which does not bring this issue. Unfortunately ```AsyncLocal<T>``` is supported only on .NET Framework version 4.6 or later.
