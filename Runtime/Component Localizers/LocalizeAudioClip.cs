using System;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

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
            get => m_AssetReference;
            set => m_AssetReference = value;
        }

        public LocalizationUnityEvent UpdateAsset
        {
            get => m_UpdateAsset;
            set => m_UpdateAsset = value;
        }

        protected override void OnLocaleChanged(Locale newLocale)
        {
            var loadOp = AssetReference.LoadAsset();
            loadOp.Completed += AssetLoaded;
        }

        protected virtual void AssetLoaded(AsyncOperationHandle<AudioClip> audioOperation)
        {
            if (audioOperation.Status != AsyncOperationStatus.Succeeded)
            {
                var error = "Failed to load audio clip: " + m_AssetReference;
                if (audioOperation.OperationException != null)
                    error += "\n" + audioOperation.OperationException;

                Debug.LogError(error, this);
                return;
            }

            UpdateAsset.Invoke(audioOperation.Result);
        }
    }
}