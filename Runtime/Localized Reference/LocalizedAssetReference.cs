using System;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization
{
    /// <summary>
    /// Used to reference a localized asset and provide an interface to loading and registering to changes.
    /// <seealso cref="LocalizedReference"/>
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    [Serializable]
    public class LocalizedAsset<TObject> : LocalizedReference where TObject : Object
    {
        AsyncOperationHandle<TObject>? m_CurrentLoadingOperation;
        ChangeHandler m_ChangeHandler;
        
        /// <summary>
        /// <inheritdoc cref="RegisterChangeHandler"/>
        /// </summary>
        /// <param name="value"></param>
        public delegate void ChangeHandler(TObject value);
        
        /// <summary>
        /// Register a handler that will be called whenever the LocalizedAsset has finished loading.
        /// When a handler is registered, the asset will then be automatically loaded whenever the <see cref="LocalizationSettings.SelectedLocaleChanged"/> is changed.
        /// <seealso cref="LoadAssetAsync"/> when not using a change handler.
        /// </summary>
        /// <param name="handler">A handler that will be called when the LocalizedAsset is ready for use.</param>
        public void RegisterChangeHandler(ChangeHandler handler)
        {
            LocalizationSettings.ValidateSettingsExist();
            m_ChangeHandler = handler ?? throw new ArgumentNullException(nameof(handler), "Handler must not be null");
            LocalizationSettings.SelectedLocaleChanged += HandleLocaleChange;

            ForceUpdate();
        }

        /// <summary>
        /// Removes the handler and stops listening to changes to <see cref="LocalizationSettings.SelectedLocaleChanged"/>.
        /// </summary>
        public void ClearChangeHandler()
        {
            LocalizationSettings.ValidateSettingsExist();
            LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChange;
            m_ChangeHandler = null;
        }

        /// <summary>
        /// Load the referenced asset as type TObject.
        /// </summary>
        /// <returns>The load operation.</returns>
        public AsyncOperationHandle<TObject> LoadAssetAsync()
        {
            LocalizationSettings.ValidateSettingsExist("Can not Load Asset.");
            return LocalizationSettings.AssetDatabase.GetLocalizedAssetAsync<TObject>(TableReference, TableEntryReference);
        }
        
        void ForceUpdate()
        {
            HandleLocaleChange(null);
        }

        void HandleLocaleChange(Locale _)
        {
            // Cancel any previous loading operations.
            if (m_CurrentLoadingOperation != null)
            {
                m_CurrentLoadingOperation.Value.Completed -= AutomaticLoadingCompleted;
            }

            m_CurrentLoadingOperation = LoadAssetAsync();
            if (m_CurrentLoadingOperation.Value.IsDone)
                AutomaticLoadingCompleted(m_CurrentLoadingOperation.Value);
            else
                m_CurrentLoadingOperation.Value.Completed += AutomaticLoadingCompleted;
        }

        void AutomaticLoadingCompleted(AsyncOperationHandle<TObject> loadOperation)
        {
            if (loadOperation.Status != AsyncOperationStatus.Succeeded)
            {
                m_CurrentLoadingOperation = null;
                return;
            }

            m_CurrentLoadingOperation = null;
            m_ChangeHandler(loadOperation.Result);
        }
    }

    /// <summary>
    /// Holds a reference to an <see cref="AssetTable"/> and <see cref="AssetTableEntry"/> which references a Texture asset.
    /// </summary>
    [Serializable]
    public class LocalizedTexture : LocalizedAsset<Texture> { }

    /// <summary>
    /// Holds a reference to an <see cref="AssetTable"/> and <see cref="AssetTableEntry"/> which references a AudioClip asset.
    /// </summary>
    [Serializable]
    public class LocalizedAudioClip : LocalizedAsset<AudioClip> { }

    /// <summary>
    /// Holds a reference to an <see cref="AssetTable"/> and <see cref="AssetTableEntry"/> which references a Sprite asset.
    /// </summary>
    [Serializable]
    public class LocalizedSprite : LocalizedAsset<Sprite> { }

    /// <summary>
    /// Holds a reference to an <see cref="AssetTable"/> and <see cref="AssetTableEntry"/> which references a GameObject asset/prefab.
    /// </summary>
    [Serializable]
    public class LocalizedGameObject : LocalizedAsset<GameObject> { }
}