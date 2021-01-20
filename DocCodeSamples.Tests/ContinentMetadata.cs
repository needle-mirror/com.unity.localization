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
