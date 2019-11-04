using System;

namespace UnityEngine.Localization.Components
{
    /// <summary>
    /// Base class for all asset localization components.
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    public abstract class LocalizedAssetBehaviour<TObject> : MonoBehaviour where TObject : Object
    {
        /// <summary>
        /// /// Reference to the localized asset entry and table.
        /// </summary>
        [Serializable]
        public class LocalizedAssetReference : LocalizedAsset<TObject>{ };

        [SerializeField]
        LocalizedAssetReference m_LocalizedAssetReference = new LocalizedAssetReference();

        /// <summary>
        /// Reference to the localized asset entry and table.
        /// </summary>
        public LocalizedAssetReference AssetReference
        {
            get => m_LocalizedAssetReference;
            set => m_LocalizedAssetReference = value;
        }

        /// <summary>
        /// Starts listening for changes to <see cref="AssetReference"/>.
        /// </summary>
        protected virtual void OnEnable()
        {
            AssetReference.RegisterChangeHandler(UpdateAsset);
        }

        /// <summary>
        /// Stops listening for changes to <see cref="AssetReference"/>.
        /// </summary>
        protected virtual void OnDisable()
        {
            AssetReference.ClearChangeHandler();
        }

        /// <summary>
        /// Called whenever the localized asset is updated, such as when the Locale changes or when initializing.
        /// </summary>
        /// <param name="localizedAsset"></param>
        protected abstract void UpdateAsset(TObject localizedAsset);
    }
}