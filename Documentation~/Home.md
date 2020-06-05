## SmartFormat.NET
SmartFormat is a lightweight templating library, with an emphasis on grammar.
It makes it easy for a data-driven template to have proper pluralization, gender conjugation, lists, and conditional language logic. Named placeholder give a more intuitive and less error-prone way to introduce variables.

> **Note:** Before getting started, carefully read the chapter about [[Common Pitfalls]]. It's a real time saver :)

#### [[Conditional]]
```c#
Smart.Format("Enabled? {0:Yes|No}", false);
// Outputs: No
Smart.Format("My birthday {0:was on|will be on} {0:MMMM d}", new DateTime(2016, 10, 04));
// Outputs - if the date in the parameter is the past from DateTime.Now: My birthday was on October 4
```
#### [[Pluralization]]
```c#
var emails = new List<string>() {"email1", "email2", "email3"};
Smart.Format("You have {0} new {0:message|messages}", emails.Count);
// Outputs: "You have 3 messages"
```
#### [Gender Conjugation](Choose)
Example with named placeholders and indexed parameter:
```c#
var user = new[]  { new { Name = "John", Gender = 0 }, new { Name = "Mary", Gender = 1 } };
Smart.Format("{Name} commented on {Gender:his|her|their} photo", user[1]);
// Outputs: "Mary commented on her photo"
```
The following code with indexed placeholders leads to the same result as above:
```c#
var user = new[] { new { Name = "John", Gender = 0 }, new { Name = "Mary", Gender = 1 } };
Smart.Default.Parser.UseAlternativeEscapeChar('\\'); // inmportant
Smart.Format("{1:{Name}} commented on {1:{Gender:his|her|their}} photo", user);
```

#### [[Lists]]
```c#
var Users = new[] { new { Name = "Jim" }, new { Name = "Pam" }, new { Name = "Dwight" }, new { Name = "Michael" } };
Smart.Format("{Users:{Name}|, | and } liked your comment", new object[] { new {Users = Users}});
// Outputs: "Jim, Pam, Dwight and Michael liked your comment"
```
#### [[Conditional logic|Choose]]
```c#
var emails = new List<string>() {"email1", "email2", "email3"};
Smart.Format("You have {Messages.Count:choose(0|1):no new messages|a new message|{} new messages}", new object[] {new {Messages = emails}});
// Outputs: "You have 3 new messages"
```
#### [[Named placeholders|Expressions and Nesting]]
```c#
var addrList = new[] {  new { Name = "Jim", Address = new {City = "New York", State = "NY"} } };
Smart.Format("{Name} from {Address.City}, {Address.State}", addrList);
// Outputs: "Jim from New York, NY"
```

#### Extra features

* Fully compatible with, and fully replaces, `String.Format`
  `string.Format("{0} {0:N2} {1:yyyy-MM-dd HH:mm:ss}", 5, new DateTime())`  outputs the same results as
  `Smart.Format("{0} {0:N2} {1:yyyy-MM-dd HH:mm:ss}", 5, new DateTime())`
* Extensible plugin architecture.  **Pluralization**, **Conditionals**, **Lists**, and **Named Placeholders** are all bundled plugins.  Creating your own is easy, too.
* Pure C# library.
* Fast, small, lightweight.  Even with all these features, `Smart.Format` performs nearly as well as `string.Format` -- creating output in *micro*seconds.
  It can even out-perform `string.Format`: parsing results can be cached, plugins can be disabled, output can be streamed.  It's **FAST**.
* Available via Nuget: `Install-Package SmartFormat.NET`
