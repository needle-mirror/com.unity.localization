using System;
using UnityEngine.Events;

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
            get { return m_AssetReference; }
            set { m_AssetReference = value; }
        }
        public LocalizationBehaviourUnityEvent UpdateAsset
        {
            get { return m_UpdateAsset; }
            set { m_UpdateAsset = value; }
        }

        protected override void OnLocaleChanged(Locale newLocale)
        {
            var loadOp = AssetReference.LoadAsset();
            loadOp.Completed += (op) => AssetLoaded(op.Result);
        }

        protected virtual void AssetLoaded(Texture2D tex)
        {
            m_UpdateAsset.Invoke(tex);
        }
    }
}