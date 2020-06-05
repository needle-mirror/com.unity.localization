# Smart Settings

```SmartSettings``` contain the following properties:

* ```ConvertCharacterStringLiterals``` - see description in chapter [[Character Literals in Format Strings]]
* ```FormatErrorAction```: The way to control how the formatter should deal with errors. It let's you maintain the tokens, ignore, throw an exception or write the error to the output string. The property ```ErrorAction``` of class ```SmartFormatter``` is depreciated.
* ```ParseErrorAction```: The way to control how the parser should deal with errors. It let's you maintain the tokens, ignore, throw an exception or write the error to the output string. The property ```ErrorAction``` of class ```Parser``` is depreciated.
* ```CaseSensitivity```: Determines whether placeholders are case-sensitive or not.

```SmartSettings``` are accessible in all parsing and formatting classes. ```SmartSettings``` have the scope for each ```SmartFormatter``` instance (meaning they are not static).
