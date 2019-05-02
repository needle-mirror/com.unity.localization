using System;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization.Components
{
    [AddComponentMenu("Localization/Generic/Texture2D")]
    public class LocalizeTexture2D : LocalizationBehaviour
    {
        [Serializable]
        public class LocalizationBehaviourAssetReference : LocalizedAssetReferenceT<Texture2D> { };

        [Serializable]
        public class LocalizationBehaviourUnityEvent : UnityEvent<Texture2D> { };

        [SerializeField]
        LocalizationBehaviourAssetReference m_AssetReference = new LocalizationBehaviourAssetReference();

        [SerializeField]
        LocalizationBehaviourUnityEvent m_UpdateAsset = new LocalizationBehaviourUnityEvent();

        public LocalizationBehaviourAssetReference AssetReference
        {
            get => m_AssetReference;
            set => m_AssetReference = value;
        }
        public LocalizationBehaviourUnityEvent UpdateAsset
        {
            get => m_UpdateAsset;
            set => m_UpdateAsset = value;
        }

        protected override void OnLocaleChanged(Locale newLocale)
        {
            var loadOp = AssetReference.LoadAsset();
            loadOp.Completed += AssetLoaded;
        }

        protected virtual void AssetLoaded(AsyncOperationHandle<Texture2D> texOperation)
        {
            if (texOperation.Status != AsyncOperationStatus.Succeeded)
            {
                var error = "Failed to load texture: " + m_AssetReference;
                if (texOperation.OperationException != null)
                    error += "\n" + texOperation.OperationException;

                Debug.LogError(error, this);
                return;
            }

            m_UpdateAsset.Invoke(texOperation.Result);
        }
    }
}