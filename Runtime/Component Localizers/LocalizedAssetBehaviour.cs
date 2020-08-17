using UnityEngine.Events;

namespace UnityEngine.Localization.Components
{
    /// <summary>
    /// Base class for all asset localization components.
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    public abstract class LocalizedAssetBehaviour<TObject, TReference> : MonoBehaviour
        where TObject : Object
        where TReference : LocalizedAsset<TObject>, new()
    {
        [SerializeField]
        TReference m_LocalizedAssetReference = new TReference();

        /// <summary>
        /// Reference to the localized asset entry and table.
        /// </summary>
        public TReference AssetReference
        {
            get => m_LocalizedAssetReference;
            set
            {
                m_LocalizedAssetReference.AssetChanged -= UpdateAsset;
                m_LocalizedAssetReference = value;

                if (enabled)
                    m_LocalizedAssetReference.AssetChanged += UpdateAsset;
            }
        }

        /// <summary>
        /// Starts listening for changes to <see cref="AssetReference"/>.
        /// </summary>
        protected virtual void OnEnable() => m_LocalizedAssetReference.AssetChanged += UpdateAsset;

        /// <summary>
        /// Stops listening for changes to <see cref="AssetReference"/>.
        /// </summary>
        protected virtual void OnDisable() => m_LocalizedAssetReference.AssetChanged -= UpdateAsset;

        /// <summary>
        /// Called whenever the localized asset is updated, such as when the Locale changes or when initializing.
        /// </summary>
        /// <param name="localizedAsset"></param>
        protected abstract void UpdateAsset(TObject localizedAsset);
    }

    /// <summary>
    /// Localized Asset Behaviour component that will call a UnityEvent to update the localized asset.
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    /// <typeparam name="TReference"></typeparam>
    /// <typeparam name="TEvent"></typeparam>
    public class LocalizedAssetEvent<TObject, TReference, TEvent> : LocalizedAssetBehaviour<TObject, TReference>
        where TObject : Object
        where TReference : LocalizedAsset<TObject>, new()
        where TEvent : UnityEvent<TObject>, new()
    {
        [SerializeField]
        TEvent m_UpdateAsset = new TEvent();

        /// <summary>
        /// Event that will be called when the localized asset is ready, usually called after the Locale has changed
        /// or at initialization.
        /// </summary>
        public TEvent OnUpdateAsset
        {
            get => m_UpdateAsset;
            set => m_UpdateAsset = value;
        }

        /// <summary>
        /// Invokes the <see cref="OnUpdateAsset"/> event.
        /// </summary>
        /// <param name="localizedAsset"></param>
        protected override void UpdateAsset(TObject localizedAsset)
        {
            OnUpdateAsset.Invoke(localizedAsset);
        }
    }
}
