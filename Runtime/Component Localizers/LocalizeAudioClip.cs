using System;
using UnityEngine.Events;

namespace UnityEngine.Localization.Components
{
    [AddComponentMenu("Localization/Generic/Audio Clip")]
    public class LocalizeAudioClip : LocalizationBehaviour
    {
        [Serializable]
        public class LocalizationAssetReference : LocalizedAssetReferenceT<AudioClip> { };

        [Serializable]
        public class LocalizationUnityEvent : UnityEvent<AudioClip> { };

        [SerializeField]
        LocalizationAssetReference m_AssetReference = new LocalizationAssetReference();

        [SerializeField]
        LocalizationUnityEvent m_UpdateAsset = new LocalizationUnityEvent();

        public LocalizationAssetReference AssetReference
        {
            get { return m_AssetReference; }
            set { m_AssetReference = value; }
        }
        public LocalizationUnityEvent UpdateAsset
        {
            get { return m_UpdateAsset; }
            set { m_UpdateAsset = value; }
        }

        protected override void OnLocaleChanged(Locale newLocale)
        {
            var loadOp = AssetReference.LoadAsset();
            loadOp.Completed += (op) => AssetLoaded(op.Result);
        }

        protected virtual void AssetLoaded(AudioClip tex)
        {
            UpdateAsset.Invoke(tex);
        }
    }
}