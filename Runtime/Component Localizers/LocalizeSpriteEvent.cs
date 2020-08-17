using System;
using UnityEngine.Events;

namespace UnityEngine.Localization.Components
{
    /// <summary>
    /// UnityEvent which can pass a Sprite as an argument.
    /// </summary>
    [Serializable]
    public class UnityEventSprite : UnityEvent<Sprite> {}

    /// <summary>
    /// Component that can be used to Localize a Sprite asset.
    /// Provides an update event that can be used to automatically update the sprite whenever the Locale changes.
    /// </summary>
    [AddComponentMenu("Localization/Asset/Localize Sprite Event")]
    public class LocalizeSpriteEvent : LocalizedAssetEvent<Sprite, LocalizedSprite, UnityEventSprite>
    {
    }
}
