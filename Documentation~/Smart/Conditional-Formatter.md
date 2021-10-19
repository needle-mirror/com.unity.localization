# Conditional Formatter

Conditional formatting occurs on any placeholder that contains a pipe character (|) after the colon (:).
To invoke the [Conditional Formatter](xref:UnityEngine.Localization.SmartFormat.Extensions.ConditionalFormatter) explicitly use the name "conditional" or "cond". It can also be used implicitly when no name is provided.

| **Data Type** | **Syntax** | **Example** |
| ------------- | ---------- | ----------- |
| **Number (Integer, Double, etc...)** | {0:one\|default} | {0} {0:item\|items} |
| | {0} {0:item\|items} | {0:no items\|one item\|many items} |
| | {0:negative\|zero\|one\|default} | {0:negative items\|no items\|one item\|many items}
| **bool** | {0:true\|false} | Enabled? {0:Yes\|No} |
| **string** | {0:default\|null or empty} | Text: {0:{0}\|No text to display}
| **DateTime** | {0:before\|after} (as compared to Date.Now) | My birthday {0:was on\|will be on} {0:MMMM d} |
| **TimeSpan** | {0:negative\|positive} | My birthday {0:was {0} ago\|will be in {0} from now} |
| | {0:negative\|zero\|positive} | My birthday {0:was {0} ago\|is today!\|will be in {0} from now} |
| **object** | {0:default\|nothing} | Property: {0:{0}\|(Nothing)} |

Each parameter is separated by "|". The comparison is followed by a "?" and then the text. The last (default) entry does not contain a comparison nor a "?".

The following comparisons are supported:

- **>=**
- **\>**
- **=**
- **\<**
- **\<=**
- **!=**

To combine comparisons, use "&" for AND or "/" for OR.

| **Example Smart String** | **Arguments** | **Result** |
| ------------------------ | ------------- | ---------- |
| {0:cond:>10?Greater Than 10\|=10?Equals to 10\|Less than 10} | `5` | Less than 10 |
| {Age:cond:>=55?Senior Citizen\|>=30?Adult\|>=18?Young Adult\|>12?Teenager\|>2?Child\|Baby} | [!code-cs[](../../DocCodeSamples.Tests/SmartStringSamples.cs#args-cond-1)] | Adult |
