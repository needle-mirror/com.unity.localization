using System;
using UnityEngine.Localization.Metadata;

[Serializable]
[Metadata(AllowedTypes = MetadataType.StringTableEntry)]
public class TranslationStatus : IMetadata
{
    public enum TranslationState
    {
        Initial,
        Translated,
        Reviewed,
        Final,
    }

    public TranslationState translationStatus = TranslationState.Initial;
}
