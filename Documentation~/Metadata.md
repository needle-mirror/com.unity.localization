# Metadata
Metadata is data that can be used to provide additional information about the item it is attached to.
Metadata is serialized using the SerializeReference attribute, this means that it must be marked as serializable,
can be shared across the same asset and does **not** inherit from UnityEngine.Object.
Metadata does not have to include serialized fields, it can also be empty and used to tag fields as having certain attributes.

Metadata can be attached:
- Locales
- Key Databases, refereed to as Shared Metadata.
- String Tables and the Table Entries
- Asset Tables and the Table Entires.

Metadata can be used for many purposes, some examples are:
- Embedding Font data for strings
- Storing Component specific values for each Locale. Such as different [RectTransform](https://docs.unity3d.com/ScriptReference/RectTransform.html) values to ensure the text is correct for each locale.
- Attaching comments and contextual information which can then be used by translators to provide better localization services.
- Running custom code/scripts. Metadata can contain code to be executed, which can be overridden for specific locales.
- Tagging. Metadata can be used to tag or indicate that the item has particular properties.

## Creating Metadata

New Metadata can be added by implementing the **IMetadata** interface.
Any class can be Metadata providing it is **serializable** and does not inherit from **UnityEngine.Object**, *UnityEngine.Objects* can be included in Metadata though.

For example, the following Metadata could be used to attach information to a Locale:
```c#
using System;
using UnityEngine.Localization.Metadata;

[Metadata(AllowedTypes = MetadataType.Locale)] // Hint to the editor to only show this type for a Locale
[Serializable]
public class Continent : IMetadata
{
    public string continentName;
    public float areaMillionKm2;
    public float populationSizeInBillions;
}
```

The *Metadata* attribute is used to add the Continent Metadata to the *Add* list for Locales.

![The Continent Metadata can now be added to Locales.](images/Metadata_Continent.png)
