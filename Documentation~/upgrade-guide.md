# Upgrade guides

## Upgrade Guide to 1.0.0

- `LocalizedStringEvent` arguments have been deprecated. If your project uses this argument, the Inspector displays a warning and offers to upgrade by transferring them into the String Reference Local Variables field. Note the Smart String will need to be updated to use the local variable name. For example `{appleCount}` would need to be changed to `{local-variable-name.appleCount}`.
- `GlobalVariables` namespace has been renamed to `PersistentVariables` and associated scripts have also been renamed. The [API Updater](https://docs.unity3d.com/Manual/APIUpdater.html) automatically upgrades relevant scripts.
- `IGlobalVariable/IVariable` interface has changed. `object SourceValue { get; }` property has been changed into a method `object GetSourceValue(ISelectorInfo selector);`. This will need to be fixed manually. In most cases the `selector` value can be ignored however it can also be used to provide context to the current format operation to allow for more control over the returned value.
- Types marked with `ObsoleteAttribute` are no longer shown in dropdown menus.
