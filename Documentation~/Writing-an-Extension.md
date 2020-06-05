# How Do I Write An Extension?

First of all, there are 2 types of extensions: *Source extensions* and *Formatter extensions*.
Source extensions are applied to the selector strings of a placeholder, and Formatter extensions are applied to the format strings of a placeholder.  If you are unfamiliar with this terminology, see [[Format String Terminology|Terminology]].

## Creating a Formatter Extension

Let's create a Formatter extension.  This extension will override the formatting of boolean values, so that instead of outputting `"True"` or `"False"`, they will output `"Yes"` or `"No"`.

To begin, let's create a class that implements `IFormatter`.
<pre>public class BooleanFormatter : IFormatter
{
    public void EvaluateFormat(object current, Format format, ref bool handled, IOutput output, FormatDetails formatDetails)
    {
    }
}</pre>

First, we need to check if the current object being formatted is a `boolean`.  If it isn't, we just `return;` and that's it.
<pre>        if (!current is bool) return; </pre>
Next, we will determine our output:
<pre>        string outputText;
        if ((bool)current == true)
        {
            outputText = "Yes";
        }
        else
        {
            outputText = "No";
        }
</pre>
Lastly, we write the output to the result, and set the `handled` flag:
<pre>        output.Write(outputText);
        handled = true;
</pre>
Setting the `handled` flag is always necessary for each extension, because it prevents the remaining extensions from outputting the same object.

Here's the completed code:
<pre>
public class BooleanFormatter : IFormatter
{
    public void EvaluateFormat(object current, Format format, ref bool handled, IOutput output, FormatDetails formatDetails)
    {
        if (!current is bool) return;
        string outputText;
        if ((bool)current == true)
        {
            outputText = "Yes";
        }
        else
        {
            outputText = "No";
        }
        output.Write(outputText);
        handled = true;
    }
}
</pre>
Obviously, this example could be simplified to about 3 lines of code, but it's easier to understand the long-hand version.

Finally, to use the extension, you need to add the extension to your formatter.
Here's an example of how to do that:
<pre> public void Test_BooleanFormatterExtension()
{
    // Add the extension to the default formatter:
    Smart.Default.AddExtensions(new BooleanFormatter());

    // Let's try it out:
    var format = "Arg0: {0}, Arg1: {1}, Arg2: {2:this format is ignored in this example}";
    var args = new object[]{ "Zero", true, false };
    var expected = "Arg0: Zero, Arg1: Yes, Arg2: No";
    var actual = Smart.Format(format, args);

    Assert.AreEqual(expected, actual);
}
</pre>

That's all there is to a simple extension!

This simple example ignores the **Item Format**, so the text `"this format is ignored in this example"` doesn't do anything.  To enhance the plugin, we could analyze the `format` parameter to get alternate "Yes" and "No" text.  Take a look at the [[Conditional Formatting|Conditional Formatting Syntax]] for an example of how this is done.

## A Hello World Example

### Implement `IFormatter`
```c#
public class HelloFormatter : IFormatter
{
    private string[] names = new[] {"hello", "hi"};
    public string[] Names { get { return names; } set { this.names = value; } }

    public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
    {
        var iCanHandleThisInput = formattingInfo.CurrentValue is bool;
        if (!iCanHandleThisInput)
            return false;

        formattingInfo.Write("HELLO ");
        if ((bool) formattingInfo.CurrentValue)
            formattingInfo.Write(formattingInfo.FormatterOptions);
        else
            formattingInfo.Write(formattingInfo.Format.GetLiteralText());

        return true;
    }
}
```

### Example usage:
```c#
Smart.Default.AddExtensions(new HelloFormatter());

Smart.Format("{value:hello(world):earth}", new { value = true });
// Outputs: "HELLO world"
Smart.Format("{value:hi(world):earth}", new { value = false });
// Outputs: "HELLO earth"
```

## Terminology

A **Format String** is the template that defines how the data should be formatted.

Let's analyze the following Format String: <pre>"The user {Name} was born in {Birthday:MMMM}, is {Age:000} {Age:year|years} old, and lives in {Address.City}."</pre>

**Placeholders** are defined by `{` and `}`, so this example has 5 placeholders: `{Name}`, `{Birthday:MMMM}`, `{Age:000}`, `{Age:year|years}`, and `{Address.City}`.

**Literal text** is the area in-between placeholders: `The user `, ` was born in `, `, is `, ` `, ` old, and lives in `.

Each placeholder starts with a list of **Selectors**, such as `Name` and `Birthday`.
Selectors are separated by a period `.`, known as an **Operator**, such as `Address`**.**`City`.
Selectors determine what data will be used in the output.

If the placeholder contains a colon `:`, the remaining text is known as the **Item Format**.
For example, `MMMM`, `000`, and `year|years` are Item Formats.
Note that there is a distinction between the entire *Format String* and the placeholder's *Item Format*.
The Item Format determines how that data will be formatted.  For example, `MMMM` tells the DateTime to output the name of the Month, like `January`.  `000` tells an integer to use 3 digits.  Please see [[Microsoft's Formatting Types Reference|http://msdn.microsoft.com/en-us/library/26etazsy.aspx]] for complete documentation of Item Formats.
