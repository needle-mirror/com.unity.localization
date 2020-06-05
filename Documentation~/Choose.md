# Choose

The `ChooseFormatter` lets you add simple logic to your templates.

## Examples
```c#
Smart.Format("{0:choose(1|2|3):one|two|three|other}", someNumber)
Smart.Format("{0:choose(True|False):yes|no}", someBoolean)
Smart.Format("{0:choose(Male|Female):his|her|their}", genderEnum)
Smart.Format("{0:choose(null): N/A | {} }", valueOrNull)
```

## Syntax Details
> { **Any Value** **: choose(*1|2|3*) :** output 1 | output 2 | output 3 *| default* }

| Any Value | choose | (choices) | outputs | *default* |
| --------- | ------ | ----- | -------- | -------- | --------- |
| Any value | "choose" or "c" | pipe-separated list of choices | pipe-separated list of outputs | (optional) output if nothing matched |

* The value can be anything.  It will be converted to string (using `ToString`), and then matched against the choices.  This works great for numbers, booleans, strings, enums, and most simple types.
* Choices are case-sensitive by default.  This is tricky with some values, such as `true.ToString() == "True"`.  This will be improved in a future version.
* `null` is supported too: `{0:choose(null): NULL | NOT NULL }`
