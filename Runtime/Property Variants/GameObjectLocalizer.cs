#if ENABLE_PROPERTY_VARIANTS || PACKAGE_DOCS_GENERATION

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Localization.PropertyVariants.TrackedObjects;
using UnityEngine.Localization.Settings;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization.PropertyVariants
{
    /// <summary>
    /// The GameObject GameObject Localizer component is responsible for storing and applying all Localized Property Variants Configurations
    /// for the <see cref="GameObject"/> it is attached to.
    /// </summary>
    /// <example>
    /// This shows how to configure a <see cref="GameObjectLocalizer"/> to apply changes to a [Text](https://docs.unity3d.com/Packages/com.unity.ugui@latest/index.html?subfolder=/api/UnityEngine.UI.Text.html)
    /// component for the Font, Font Size and Text properties.
    /// <code source="../../DocCodeSamples.Tests/GameObjectLocalizerSamples.cs" region="setup-text"/>
    /// </example>
    /// <example>
    /// This shows how to configure a <see cref="GameObjectLocalizer"/> to apply changes to a [TextMeshProUGUI](https://docs.unity3d.com/Packages/com.unity.textmeshpro@latest/index.html?subfolder=/api/TMPro.TextMeshProUGUI.html)
    /// component for the Font, Font Size and Text properties.
    /// <code source="../../DocCodeSamples.Tests/GameObjectLocalizerSamples.cs" region="setup-tmp-text"/>
    /// </example>
    /// <example>
    /// This shows how to configure a <see cref="GameObjectLocalizer"/> to apply changes to a [Dropdown](https://docs.unity3d.com/Packages/com.unity.ugui@latest/index.html?subfolder=/api/UnityEngine.UI.Dropdown.html)
    /// for the options values.
    /// <code source="../../DocCodeSamples.Tests/GameObjectLocalizerSamples.cs" region="setup-dropdown"/>
    /// </example>
    /// <example>
    /// This shows how to configure a <see cref="GameObjectLocalizer"/> to apply changes to a [TMP_Dropdown](https://docs.unity3d.com/Packages/com.unity.textmeshpro@latest/index.html?subfolder=/api/TMPro.TMP_Dropdown.html)
    /// for the options values.
    /// <code source="../../DocCodeSamples.Tests/GameObjectLocalizerSamples.cs" region="setup-tmp-dropdown"/>
    /// </example>
    /// <example>
    /// This shows how to configure a <see cref="GameObjectLocalizer"/> to apply changes to a <see cref="RectTransform"/> for the x, y and width properties.
    /// This can be useful when you need to make adjustments due to changes in text length for a particular <see cref="Locale"/>.
    /// <code source="../../DocCodeSamples.Tests/GameObjectLocalizerSamples.cs" region="setup-rect-transform"/>
    /// </example>
    /// <example>
    /// This shows how to configure a <see cref="GameObjectLocalizer"/> to apply changes to a custom <see cref="MonoBehaviour"/> script.
    /// <code source="../../DocCodeSamples.Tests/GameObjectLocalizerSamples.cs" region="user-script"/>
    /// </example>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class GameObjectLocalizer : MonoBehaviour
    {
        [SerializeReference]
        List<TrackedObject> m_TrackedObjects = new List<TrackedObject>();

        internal AsyncOperationHandle? CurrentOperation
        {
            get;
            private set;
        }

        void OnEnable()
        {
            LocalizationSettings.SelectedLocaleChanged += SelectedLocaleChanged;
        }

        void OnDisable()
        {
            LocalizationSettings.SelectedLocaleChanged -= SelectedLocaleChanged;
        }

        IEnumerator Start()
        {
            var localeOp = LocalizationSettings.SelectedLocaleAsync;
            if (!localeOp.IsDone)
                yield return localeOp;

            SelectedLocaleChanged(localeOp.Result);
        }

        void SelectedLocaleChanged(AsyncOperationHandle<Locale> localeOp) => SelectedLocaleChanged(localeOp.Result);

        void SelectedLocaleChanged(Locale locale)
        {
            // Ignore null, this will reset the driven properties back to their defaults.
            if (locale == null)
                return;

            ApplyLocaleVariant(locale);
        }

        /// <summary>
        /// The objects that are being tracked by this Localizer.
        /// </summary>
        public List<TrackedObject> TrackedObjects => m_TrackedObjects;

        /// <summary>
        ///  Returns the <see cref="TrackedObjects"/> for the target component or creates a new instance if <paramref name="create"/> is set to <c>true</c>.
        /// </summary>
        /// <typeparam name="T">The Type of TrackedObject that should be found or added.</typeparam>
        /// <param name="target">The Target Object to track.</param>
        /// <param name="create">Creates a new Tracked Object if one can not be found.</param>
        /// <returns></returns>
        public T GetTrackedObject<T>(Object target, bool create = true) where T : TrackedObject, new()
        {
            var trackedObject = GetTrackedObject(target);
            if (trackedObject != null)
                return (T)trackedObject;

            if (!create)
                return default;

            var component = target as Component;
            if (component == null)
                return null;

            if (component.gameObject != gameObject)
            {
                throw new Exception("Tracked Objects must share the same GameObject as the GameObjectLocalizer. " +
                    $"The Component {component} is attached to the GameObject {component.gameObject}" +
                    $" but the GameObjectLocalizer is attached to {gameObject}.");
            }

            var newTrackedObject = new T {Target = target};
            TrackedObjects.Add(newTrackedObject);
            return newTrackedObject;
        }

        /// <summary>
        /// Returns the <see cref="TrackedObjects"/> for the target component or <c>null</c> if one does not exist.
        /// See <seealso cref="GetTrackedObject{T}"/> for a version that will create a new TrackedObject if one does not already exist.
        /// </summary>
        /// <param name="target">The target object to search for.</param>
        /// <returns>The <see cref="TrackedObjects"/> or <c>null</c> if one could not be found.</returns>
        public TrackedObject GetTrackedObject(Object target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            foreach (var t in TrackedObjects)
            {
                if (t?.Target == target)
                    return t;
            }
            return null;
        }

        /// <summary>
        /// Apply all variants for the selected <see cref="Locale"/> to this GameObject.
        /// </summary>
        /// <param name="locale">The <see cref="Locale"/> to apply to the GameObject.</param>
        /// <returns>A handle to any loading operations or <c>default</c> if the operation was immediate.</returns>
        public AsyncOperationHandle ApplyLocaleVariant(Locale locale) => ApplyLocaleVariant(locale, LocalizationSettings.ProjectLocale);

        /// <summary>
        /// Apply all variants for the selected <see cref="Locale"/> to this GameObject.
        /// When a value cannot be found, the <paramref name="fallback"/> value is used.
        /// </summary>
        /// <param name="locale">The <see cref="Locale"/> to apply to the GameObject.</param>
        /// <param name="fallback">The fallback <see cref="Locale"/> to use when a value does not exist for <paramref name="locale"/>.</param>
        /// <returns>A handle to any loading operations or <c>default</c> if the operation was immediate.</returns>
        public AsyncOperationHandle ApplyLocaleVariant(Locale locale, Locale fallback)
        {
            if (CurrentOperation.HasValue)
            {
                if (!CurrentOperation.Value.IsDone)
                    Debug.LogWarning("Attempting to Apply Variant when the previous operation has not yet completed.", this);
                AddressablesInterface.Release(CurrentOperation.Value);
                CurrentOperation = null;
            }

            var asyncOperations = ListPool<AsyncOperationHandle>.Get();

            foreach (var to in TrackedObjects)
            {
                if (to == null)
                    continue;

                var operation = to.ApplyLocale(locale, fallback);

                if (!operation.IsDone)
                    asyncOperations.Add(operation);
            }

            if (asyncOperations.Count == 1)
            {
                AddressablesInterface.Acquire(asyncOperations[0]);
                CurrentOperation = asyncOperations[0];
                ListPool<AsyncOperationHandle>.Release(asyncOperations);
                return CurrentOperation.Value;
            }

            if (asyncOperations.Count > 1)
            {
                // We need to acquire the operations or CreateGenericGroupOperation will release them when it is released.
                foreach (var asyncOperationHandle in asyncOperations)
                {
                    AddressablesInterface.Acquire(asyncOperationHandle);
                }
                CurrentOperation = AddressablesInterface.ResourceManager.CreateGenericGroupOperation(asyncOperations);
                return CurrentOperation.Value;
            }

            ListPool<AsyncOperationHandle>.Release(asyncOperations);
            return default;
        }
    }
}

#endif
