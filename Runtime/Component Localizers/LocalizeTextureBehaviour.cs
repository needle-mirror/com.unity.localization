using System;
using UnityEngine.Events;

namespace UnityEngine.Localization.Components
{
    /// <summary>
    /// Component that can be used to Localize a Texture asset.
    /// Provides an update event that can be used to automatically update the texture whenever the Locale changes.
    /// </summary>
    [AddComponentMenu("Localization/Asset/Localize Texture")]
    public class LocalizeTextureBehaviour : LocalizedAssetBehaviour<Texture>
    {
        /// <summary>
        /// UnityEvent which can pass a Texture as an argument.
        /// </summary>
        [Serializable]
        public class UnityEventTexture : UnityEvent<Texture> { }

        [SerializeField]
        UnityEventTexture m_UpdateAsset = new UnityEventTexture();

        /// <summary>
        /// Event that will be sent when the localized texture is ready.
        /// </summary>
        public UnityEventTexture OnUpdateAsset
        {
            get => m_UpdateAsset;
            set => m_UpdateAsset = value;
        }

        /// <summary>
        /// Invokes the <see cref="OnUpdateAsset"/> event.
        /// </summary>
        /// <param name="localizedAsset"></param>
        protected override void UpdateAsset(Texture localizedAsset)
        {
            OnUpdateAsset.Invoke(localizedAsset);
        }
    }
}