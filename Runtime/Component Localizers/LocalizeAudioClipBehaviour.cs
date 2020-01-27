using System;
using UnityEngine.Events;

namespace UnityEngine.Localization.Components
{
    /// <summary>
    /// Component that can be used to Localize an Audio Clip asset.
    /// Provides an update event that can be used to automatically update the clip whenever the Locale changes.
    /// </summary>
    [AddComponentMenu("Localization/Asset/Localize Audio Clip")]
    public class LocalizeAudioClipBehaviour : LocalizedAssetBehaviour<AudioClip>
    {
        /// <summary>
        /// UnityEvent which can pass an AudioClip as an argument.
        /// </summary>
        [Serializable]
        public class UnityEventAudioClip : UnityEvent<AudioClip> {}

        [SerializeField]
        UnityEventAudioClip m_UpdateAsset = new UnityEventAudioClip();

        /// <summary>
        /// Event that will be called when the localized Audio Clip is ready, usually called after the Locale has changed
        /// or at initialization.
        /// </summary>
        public UnityEventAudioClip OnUpdateAsset
        {
            get => m_UpdateAsset;
            set => m_UpdateAsset = value;
        }

        /// <summary>
        /// Invokes the <see cref="OnUpdateAsset"/> event.
        /// </summary>
        /// <param name="localizedAsset"></param>
        protected override void UpdateAsset(AudioClip localizedAsset)
        {
            OnUpdateAsset.Invoke(localizedAsset);
        }
    }
}
