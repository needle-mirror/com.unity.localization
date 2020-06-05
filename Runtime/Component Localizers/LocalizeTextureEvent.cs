using System;
using UnityEngine.Events;

namespace UnityEngine.Localization.Components
{
    /// <summary>
    /// UnityEvent which can pass a Texture as an argument.
    /// </summary>
    [Serializable]
    public class UnityEventTexture : UnityEvent<Texture> {}

    /// <summary>
    /// Component that can be used to Localize a Texture asset.
    /// Provides an update event that can be used to automatically update the texture whenever the Locale changes.
    /// </summary>
    [AddComponentMenu("Localization/Asset/Localize Texture Event")]
    public class LocalizeTextureEvent : LocalizedAssetEvent<Texture, LocalizedTexture, UnityEventTexture>
    {
    }
}
