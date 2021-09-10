# Template Formatter

The Template Formatter lets you register reusable templates, and use them by name.
Templates are useful when you want to format text in a particular way across multiple smart strings. When the string needs to be modified, you only need to do it in the template.

![Diagram showing the breakdown of the Smart String and how each part is evaluated.](../images/SmartString-TemplateInspector.png)

| **Example Smart String**                 | **Arguments**                                                                            | **Result**           |
|------------------------------------------|------------------------------------------------------------------------------------------|----------------------|
| The initials are  {0:template(initials)} | [!code-cs[](../../DocCodeSamples.Tests/SmartStringSamples.cs#args-template-formatter-1)] | The initials are L C |
| Hello {0.template(fullName)}             | [!code-cs[](../../DocCodeSamples.Tests/SmartStringSamples.cs#args-template-formatter-1)] | Hello Lara Croft     |
