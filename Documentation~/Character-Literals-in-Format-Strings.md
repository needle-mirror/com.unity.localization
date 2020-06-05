# Character Literals in Format Strings

Whenever the format string comes from code, by default the compiler will take care of converting the character literals into their Unicode representation:

Example:
```Csharp
string.Format("\t")
Smart.Format("\t")
// result in both cases: TAB
```

Things are different, when the format string is read from a file or resource. Example:
```Csharp
string.Format(@"Read from file \t")
// result: "Read from file \t"
```

There is a property ```ConvertCharacterStringLiterals``` in ```SmartSettings```. This setting is relevant for interpreting character literals. If true (the default), character string literals will be treated just like the compiler would do.

**ConvertCharacterStringLiterals = true**

```string.Format("\t")``` and ```Smart.Format(@"\t")``` will return a "TAB" character

Character literals are allowed in all formatters.

**ConvertCharacterStringLiterals = false**

Character string literals are not converted.
```string.Format(@"\t")``` and ```SmartFormat(@"\t")```  will return the 2 characters "\\" and "t"

**Note**:

This is a **breaking change** to Smart.Format versions prior to 2.1.  Users may notice this e.g., when the format string contains file system paths. In order to achieve the "old" behavior, ```ConvertCharacterStringLiterals``` in ```SmartSettings``` must be set to ```false``` or the backslashes must be escaped.
