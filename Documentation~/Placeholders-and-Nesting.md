# Named placeholders

*String.Format* references all args by index:
`String.Format("{0} {1}", person.FirstName, person.LastName)`

*Smart.Format* takes this a step further, and lets you use named placeholders instead:
`Smart.Format("{FirstName} {LastName}", person)`
In fact, *Smart.Format* supports several kinds of expressions:
`Smart.Format("{FirstName.ToUpper} {LastName.ToLower}", person)`

# Nesting

In **SmartFormat**, you can use nesting to avoid repetition, such as:
`Smart.Format("{User.Address:{Street}, {City}, {State} {Zip}}", data)`

Nesting is often used with conditionals, plurals, and lists.

## Nested Scope
When using nested placeholders, it is necessary to understand the scope that Reflection will use. A nested placeholder always starts off with the scope of the item that contains it.

The root scope for a template is **arg0**, meaning that `{User.Address}` is equivalent to `data.User.Address`.
Within the nested area, however, the "scope" has changed to `User.Address`, so nested expressions like `{Street}` are evaluated against `User.Address`.
To illustrate this, the following are all equivalent:
`{User.Name} {User.Address.City} {User.Address.State}`
`{User.Name} {User.Address:{City} {State}}`
`{User:{Name} {Address:{City} {State}}}`

Within any nested scope, you still have access to the outer scopes.  For example:
`{User.Address:{User.Name} {City} {State}}`
Here, `{User.Name}`, which is in the root scope, is still accessible from within the nested `User.Address` scope.
