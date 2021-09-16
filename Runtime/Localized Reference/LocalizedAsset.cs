using System;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization
{
    #if MODULE_AUDIO || PACKAGE_DOCS_GENERATION
    /// <summary>
    /// Provides a specialized <see cref="LocalizedAsset{TObject}"/> which can be used to localize [AudioClip](https://docs.unity3d.com/ScriptReference/AudioClip.html) assets.
    /// </summary>
    [Serializable]
    public class LocalizedAudioClip : LocalizedAsset<AudioClip> {}
    #endif

    /// <summary>
    /// Provides a specialized <see cref="LocalizedAsset{TObject}"/> which can be used to localize [Prefabs](https://docs.unity3d.com/Manual/Prefabs.html).
    /// </summary>
    [Serializable]
    public class LocalizedGameObject : LocalizedAsset<GameObject> {}

    /// <summary>
    /// Provides a specialized <see cref="LocalizedAsset{TObject}"/> which can be used to localize [Materials](https://docs.unity3d.com/ScriptReference/Material.html).
    /// </summary>
    [Serializable]
    public class LocalizedMaterial : LocalizedAsset<Material> {}

    /// <summary>
    /// Provides a <see cref="LocalizedAsset{TObject}"/> which you can use to localize any [UnityEngine.Object](https://docs.unity3d.com/ScriptReference/Object.html).
    /// </summary>
    [Serializable]
    public class LocalizedObject : LocalizedAsset<Object> {}

    /// <summary>
    /// Provides a <see cref="LocalizedAsset{TObject}"/> which you can use to localize [Sprites](https://docs.unity3d.com/ScriptReference/Sprite.html).
    /// </summary>
    [Serializable]
    public class LocalizedSprite : LocalizedAsset<Sprite> {}

    /// <summary>
    /// Provides a <see cref="LocalizedAsset{TObject}"/> which you can use to localize [Textures](https://docs.unity3d.com/ScriptReference/Texture.html) assets.
    /// </summary>
    [Serializable]
    public class LocalizedTexture : LocalizedAsset<Texture> {}

    #if PACKAGE_TMP || PACKAGE_DOCS_GENERATION
    /// <summary>
    /// Provides a <see cref="LocalizedAsset{TObject}"/> which you can use to localize a TextMeshPro <see cref="TMPro.TMP_FontAsset"/>.
    /// </summary>
    [Serializable]
    public class LocalizedTmpFont : LocalizedAsset<TMPro.TMP_FontAsset> {}
    #endif

    /// <summary>
    /// Provides a <see cref="LocalizedAsset{TObject}"/> which you can use to localize a <see cref="Font"/>.
    /// </summary>
    [Serializable]
    public class LocalizedFont : LocalizedAsset<Font> {}

    /// <summary>
    /// Base class for all localized assets.
    /// </summary>
    public abstract class LocalizedAssetBase : LocalizedReference
    {
        /// <summary>
        /// Returns the localized asset as a [UnityEngine.Object](https://docs.unity3d.com/ScriptReference/Object.html).
        /// </summary>
        /// <returns></returns>
        public abstract AsyncOperationHandle<Object> LoadAssetAsObjectAsync();
    }

    /// <summary>
    /// Provides a way to reference an <see cref="AssetTableEntry"/> inside of a specific <see cref="AssetTable"/> and request the localized asset.
    /// </summary>
    /// <typeparam name="TObject">The type that should be supported. This can be any type that inherits from [UnityEngine.Object](https://docs.unity3d.com/ScriptReference/Object.html).</typeparam>
    /// <example>
    /// This example shows how a <see cref="LocalizedAsset{TObject}"/> can be used to localize a [Prefabs](https://docs.unity3d.com/Manual/Prefabs.html).
    /// See also <seealso cref="LocalizedGameObject"/> and <seealso cref="Components.LocalizedGameObjectEvent"/>.
    /// <code source="../../DocCodeSamples.Tests/LocalizedAssetSamples.cs" region="localized-prefab"/>
    /// </example>
    [Serializable]
    public partial class LocalizedAsset<TObject> : LocalizedAssetBase where TObject : Object
    {
        ChangeHandler m_ChangeHandler;

        /// <summary>
        /// Delegate used by <see cref="AssetChanged"/>.
        /// </summary>
        /// <param name="value">The localized asset.</param>
        public delegate void ChangeHandler(TObject value);

        /// <inheritdoc/>
        public override bool WaitForCompletion
        {
            set
            {
                if (value == WaitForCompletion)
                    return;

                base.WaitForCompletion = value;
                #if !UNITY_WEBGL // WebGL does not support WaitForCompletion
                if (value && CurrentLoadingOperation.HasValue && !CurrentLoadingOperation.Value.IsDone)
                    CurrentLoadingOperation.Value.WaitForCompletion();
                #endif
            }
        }

        /// <summary>
        /// The current loading operation for the asset when using <see cref="AssetChanged"/>. This is <c>null</c> if a loading operation is not available.
        /// </summary>
        public AsyncOperationHandle<TObject>? CurrentLoadingOperation
        {
            get;
            internal set;
        }

        /// <summary>
        /// Provides a callback that will be invoked when the asset is available or has changed.
        /// </summary>
        /// <remarks>
        /// The following events will trigger an update:
        /// - The first time the action is added to the event.
        /// - The <seealso cref="LocalizationSettings.SelectedLocale"/> changing.
        /// - The <see cref="TableReference"/> or <see cref="TableEntryReference"/> changing.
        ///
        /// When the first <see cref="ChangeHandler"/> is added, a loading operation (see <see cref="CurrentLoadingOperation"/>) will automatically
        /// start and the localized asset will be sent to the subscriber when completed. Any adding additional subscribers added after
        /// loading has completed will also be sent the latest localized asset when they are added.
        /// This ensures that a subscriber will always have the correct localized value regardless of when it was added.
        /// </remarks>
        /// <example>
        /// This example shows how the <see cref="AssetChanged"/> event could be used to change the Font on some localized Text.
        /// <code source="../../DocCodeSamples.Tests/LocalizedAssetSamples.cs" region="localized-text-font"/>
        /// </example>
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
                    ForceUpdate();

                    // We subscribe after the first update as its possible that a SelectedLocaleChanged may be fired
                    // during ForceUpdate when using WaitForCompletion and we want to avoid this.
                    LocalizationSettings.SelectedLocaleChanged += HandleLocaleChange;
                }
                else if (CurrentLoadingOperation.HasValue && CurrentLoadingOperation.Value.IsDone)
                {
                    // Call the event with the latest value.
                    value(CurrentLoadingOperation.Value.Result);
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
        /// Returns <c>true</c> if <seealso cref="AssetChanged"/> has any subscribers.
        /// </summary>
        public bool HasChangeHandler => m_ChangeHandler != null;

        /// <summary>
        /// Provides a localized asset from a <see cref="AssetTable"/> with the <see cref="TableReference"/> and the
        /// the asset that matches <see cref="TableEntryReference"/>.
        /// </summary>
        /// <remarks>
        /// The asset may have already been loaded, either during a previous operation or if Preload mode is used. Check the <see cref="AsyncOperationHandle.IsDone"/> property to see if the asset is already loaded and therefore is immediately available.
        /// See [Async operation handling](https://docs.unity3d.com/Packages/com.unity.addressables@latest/index.html?subfolder=/manual/AddressableAssetsAsyncOperationHandle.html) for further details.
        /// </remarks>
        /// <returns>Returns the loading operation for the request.</returns>
        /// <example>
        /// This example shows how <see cref="LoadAssetAsync"/> can be used to request a sprite asset when the <see cref="LocalizationSettings.SelectedLocale"/> changes.
        /// <code source="../../DocCodeSamples.Tests/LocalizedAssetSamples.cs" region="localized-sprite"/>
        /// </example>
        public AsyncOperationHandle<TObject> LoadAssetAsync()
        {
            LocalizationSettings.ValidateSettingsExist("Can not Load Asset.");
            return LocalizationSettings.AssetDatabase.GetLocalizedAssetAsync<TObject>(TableReference, TableEntryReference, locale: LocaleOverride);
        }

        class ConvertToObjectOperation : WaitForCurrentOperationAsyncOperationBase<Object>
        {
            AsyncOperationHandle<TObject> m_Operation;

            public void Init(AsyncOperationHandle<TObject> operation)
            {
                AddressablesInterface.ResourceManager.Acquire(operation);
                m_Operation = operation;
                CurrentOperation = operation;
            }

            protected override void Execute()
            {
                if (m_Operation.IsDone)
                    OnCompleted(m_Operation);
                else
                    m_Operation.Completed += OnCompleted;
            }

            void OnCompleted(AsyncOperationHandle<TObject> op)
            {
                Complete(op.Result, op.Status == AsyncOperationStatus.Succeeded, null);
            }

            protected override void Destroy()
            {
                AddressablesInterface.Release(m_Operation);
                GenericPool<ConvertToObjectOperation>.Release(this);
            }
        }

        /// <inheritdoc/>
        public override AsyncOperationHandle<Object> LoadAssetAsObjectAsync()
        {
            var wrappedOperation = LoadAssetAsync();
            var operation = GenericPool<ConvertToObjectOperation>.Get();
            operation.Init(wrappedOperation);
            return AddressablesInterface.ResourceManager.StartOperation(operation, default);
        }

        /// <summary>
        /// Provides a localized asset from a <see cref="AssetTable"/> with the <see cref="TableReference"/> and the
        /// the asset that matches <see cref="TableEntryReference"/>.
        /// Uses [WaitForCompletion](xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle.WaitForCompletion) to force the loading to complete synchronously.
        /// Please note that [WaitForCompletion](xref:UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle.WaitForCompletion) is not supported on
        /// [WebGL](https://docs.unity3d.com/Packages/com.unity.addressables@latest/index.html?subfolder=/manual/SynchronousAddressables.html#webgl).
        /// </summary>
        /// <returns>Returns the localized asset.</returns>
        public TObject LoadAsset()
        {
            return LoadAssetAsync().WaitForCompletion();
        }

        /// <inheritdoc/>
        protected internal override void ForceUpdate()
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

            #if UNITY_EDITOR
            m_CurrentTable = TableReference;
            m_CurrentTableEntry = TableEntryReference;

            // Dont update if we have no selected Locale
            if (!LocalizationSettings.Instance.IsPlayingOrWillChangePlaymode && LocaleOverride == null && LocalizationSettings.SelectedLocale == null)
                return;
            #endif

            if (IsEmpty)
            {
                #if UNITY_EDITOR
                // If we are empty and playing or previewing then we should force an update.
                if (!LocalizationSettings.Instance.IsPlayingOrWillChangePlaymode)
                    m_ChangeHandler(null);
                #endif
                return;
            }

            CurrentLoadingOperation = LoadAssetAsync();
            AddressablesInterface.Acquire(CurrentLoadingOperation.Value);

            if (!CurrentLoadingOperation.Value.IsDone)
            {
                #if !UNITY_WEBGL
                if (WaitForCompletion)
                {
                    CurrentLoadingOperation.Value.WaitForCompletion();
                }
                else
                #endif
                {
                    CurrentLoadingOperation.Value.Completed += AutomaticLoadingCompleted;
                    return;
                }
            }

            AutomaticLoadingCompleted(CurrentLoadingOperation.Value);
        }

        void AutomaticLoadingCompleted(AsyncOperationHandle<TObject> loadOperation)
        {
            if (loadOperation.Status != AsyncOperationStatus.Succeeded)
            {
                CurrentLoadingOperation = null;
                return;
            }

            m_ChangeHandler(loadOperation.Result);
        }

        internal void ClearLoadingOperation()
        {
            if (CurrentLoadingOperation.HasValue)
            {
                if (CurrentLoadingOperation.Value.IsValid())
                {
                    // We should only call this if we are not done as its possible that the internal list is null if its not been used.
                    if (!CurrentLoadingOperation.Value.IsDone)
                        CurrentLoadingOperation.Value.Completed -= AutomaticLoadingCompleted;
                    AddressablesInterface.Release(CurrentLoadingOperation.Value);
                }
                CurrentLoadingOperation = null;
            }
        }

        /// <inheritdoc/>
        protected override void Reset()
        {
            ClearLoadingOperation();
        }
    }
}
