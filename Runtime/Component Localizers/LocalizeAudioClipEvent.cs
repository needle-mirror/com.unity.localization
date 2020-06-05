using System;
using UnityEngine.Events;

namespace UnityEngine.Localization.Components
{
    /// <summary>
    /// UnityEvent which can pass an AudioClip as an argument.
    /// </summary>
    [Serializable]
    public class UnityEventAudioClip : UnityEvent<AudioClip> {}

    /// <summary>
    /// Component that can be used to Localize an Audio Clip asset.
    /// Provides an update event that can be used to automatically update the clip whenever the Locale changes.
    /// </summary>
    [AddComponentMenu("Localization/Asset/Localize Audio Clip Event")]
    public class LocalizeAudioClipEvent : LocalizedAssetEvent<AudioClip, LocalizedAudioClip, UnityEventAudioClip>
    {
    }
}
