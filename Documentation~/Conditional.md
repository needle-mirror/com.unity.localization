# Conditional

Conditional Formatting is has very powerful functionality, and it's one of the diamonds of this project.
Conditional Formatting will kick in on any placeholder that contains a "|" (pipe character) after the ":".

## Syntax

The behavior of Conditional Formatting varies depending on the data type of the placeholder.

| Data type | Syntax | Example |
|---|---|---|
| Number (Integer, Double, etc...)  | "{0:one\|default}"  |  "{0} {0:item\|items}" |
|   | "{0:zero\|one\|default}" | "{0:no items\|one item\|many items}" |
|   | "{0:negative\|zero\|one\|default}" | "{0:negative items\|no items\|one item\|many items}" |
| boolean  | "{0:true\|false}" | "Enabled? {0:Yes\|No}" |
| string   | "{0:default\|null or empty}" | "Text: {0:{0}\|No text to display}" |
| DateTime | "{0:before\|after}" (as compared to Date.Now) | "My birthday {0:was on\|will be on} {0:MMMM d}" |
| TimeSpan | "{0:negative\|positive}"       | "My birthday {0:was {0} ago\|will be in {0} from now}" |
|          | "{0:negative\|zero\|positive}" | "My birthday {0:was {0} ago\|is today!\|will be in {0} from now}" |
| Object   | "{0:default\|nothing}"         | "Property: {0:{0}\|(Nothing)}"

## Complex conditions (for numbers)

Each parameter is separated by "|". The comparison is followed by a "?" and then the text. The last (default) entry does not contain a comparison nor a "?". Valid comparisons: `>=` `>` `=` `<` `<=` `!=`.

Comparisons can be combined using "&" for AND or "/" for OR.

## Example:

```C#
var addrList = new[] { new { Name = "Jim", Age = 32, Address = new { City = "New York", State = "NY" } } };
var result = Smart.Format("{Age:cond:>=55?Senior Citizen|>=30?Adult|>=18?Young Adult|>12?Teenager|>2?Child|Baby}", addrList);
// result: Adult
```
