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
    public partial class LocalizedAsset<TObject> : LocalizedReference where TObject : Object
    {
        ChangeHandler m_ChangeHandler;
        AsyncOperationHandle<TObject>? m_CurrentLoadingOperation;

        /// <summary>
        /// <inheritdoc cref="RegisterChangeHandler"/>
        /// </summary>
        /// <param name="value"></param>
        public delegate void ChangeHandler(TObject value);

        /// <summary>
        /// The current loading operation for the Asset when using a <see cref="AssetChanged"/>.
        /// </summary>
        public AsyncOperationHandle<TObject>? CurrentLoadingOperation
        {
            get => m_CurrentLoadingOperation;
            internal set => m_CurrentLoadingOperation = value;
        }

        /// <summary>
        /// Called whenever a localized asset is available.
        /// When the first <see cref="ChangeHandler"/> is added, a loading operation will automatically start and the localized asset will be sent to the event when completed.
        /// Any adding additional subscribers added after loading has completed will also be sent the latest localized asset when they are added.
        /// This ensures that a subscriber will always have the correct localized asset regardless of when it was added.
        /// The asset will be refreshed whenever <see cref="LocalizationSettings.SelectedLocaleChanged"/> is changed.
        /// <seealso cref="LoadAssetAsync"/> when not using the event.
        /// </summary>
        public event ChangeHandler AssetChanged
        {
            add
            {
                if (value == null)
                    throw new ArgumentNullException();

                bool wasEmpty = m_ChangeHandler == null;
                m_ChangeHandler += value;

                if (wasEmpty)
                {
                    LocalizationSettings.ValidateSettingsExist();
                    LocalizationSettings.SelectedLocaleChanged += HandleLocaleChange;
                    ForceUpdate();
                }
                else if (CurrentLoadingOperation.HasValue && CurrentLoadingOperation.Value.IsDone)
                {
                    // Call the event with the latest value.
                    value(m_CurrentLoadingOperation.Value.Result);
                }
            }
            remove
            {
                if (value == null)
                    throw new ArgumentNullException();

                m_ChangeHandler -= value;

                if (m_ChangeHandler == null)
                {
                    LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChange;
                    ClearLoadingOperation();
                }
            }
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

        protected override void ForceUpdate()
        {
            if (m_ChangeHandler != null)
            {
                HandleLocaleChange(null);
            }
        }

        void HandleLocaleChange(Locale _)
        {
            // Cancel any previous loading operations.
            ClearLoadingOperation();

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

            m_ChangeHandler(loadOperation.Result);
        }

        internal void ClearLoadingOperation()
        {
            if (m_CurrentLoadingOperation.HasValue)
            {
                // We should only call this if we are not done as its possible that the internal list is null if its not been used.
                if (!m_CurrentLoadingOperation.Value.IsDone)
                    m_CurrentLoadingOperation.Value.Completed -= AutomaticLoadingCompleted;
                m_CurrentLoadingOperation = null;
            }
        }
    }

    /// <summary>
    /// Holds a reference to an <see cref="AssetTable"/> and <see cref="AssetTableEntry"/> which references a Texture asset.
    /// </summary>
    [Serializable]
    public class LocalizedTexture : LocalizedAsset<Texture> {}

    /// <summary>
    /// Holds a reference to an <see cref="AssetTable"/> and <see cref="AssetTableEntry"/> which references a AudioClip asset.
    /// </summary>
    [Serializable]
    public class LocalizedAudioClip : LocalizedAsset<AudioClip> {}

    /// <summary>
    /// Holds a reference to an <see cref="AssetTable"/> and <see cref="AssetTableEntry"/> which references a Sprite asset.
    /// </summary>
    [Serializable]
    public class LocalizedSprite : LocalizedAsset<Sprite> {}

    /// <summary>
    /// Holds a reference to an <see cref="AssetTable"/> and <see cref="AssetTableEntry"/> which references a GameObject asset/prefab.
    /// </summary>
    [Serializable]
    public class LocalizedGameObject : LocalizedAsset<GameObject> {}
}
