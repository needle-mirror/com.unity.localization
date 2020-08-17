using System;
using System.Collections.Generic;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization.Settings
{
    /// <summary>
    /// The localization settings is the core component to the localization system.
    /// It provides the entry point to all player based localization features.
    /// </summary>
    public partial class LocalizationSettings : ScriptableObject
    {
        /// <summary>
        /// The name to use when retrieving the LocalizationSettings from CustomObject API.
        /// </summary>
        internal const string ConfigName = "com.unity.localization.settings";

        internal const string LocaleLabel = "Locale";
        internal const string PreloadLabel = "Preload";

        [SerializeReference]
        List<IStartupLocaleSelector> m_StartupSelectors = new List<IStartupLocaleSelector>
        {
            new CommandLineLocaleSelector(),
            new SystemLocaleSelector(),
            new SpecificLocaleSelector()
        };

        [SerializeReference]
        ILocalesProvider m_AvailableLocales = new LocalesProvider();

        [SerializeReference]
        LocalizedAssetDatabase m_AssetDatabase = new LocalizedAssetDatabase();

        [SerializeReference]
        LocalizedStringDatabase m_StringDatabase = new LocalizedStringDatabase();

        InitializationOperation m_InitializingOperation;
        AsyncOperationHandle<LocalizationSettings>? m_InitializingOperationHandle;

        AsyncOperationHandle<Locale>? m_SelectedLocaleAsync;
        Locale m_SelectedLocale;

        static LocalizationSettings s_Instance;

        /// <summary>
        /// Called when the <see cref="SelectedLocale"/> is changed.
        /// This will be called after <see cref="InitializationOperation"/> is completed so any preloading operations will be finished.
        /// </summary>
        public event Action<Locale> OnSelectedLocaleChanged;

        /// <summary>
        /// Indicates if there is a LocalizationSettings present. If one is not found then it will attempt to find one however
        /// unlike <see cref="Instance"/> it will not create a default, if one can not be found.
        /// </summary>
        /// <value><c>true</c> if has settings; otherwise, <c>false</c>.</value>
        public static bool HasSettings
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = GetInstanceDontCreateDefault();
                return s_Instance != null;
            }
        }

        /// <summary>
        /// The localization system may not be immediately ready. Loading Locales, preloading assets etc.
        /// This operation can be used to check when the system is ready. You can yield on this in a coroutine to wait.
        /// </summary>
        public static AsyncOperationHandle<LocalizationSettings> InitializationOperation => Instance.GetInitializationOperation();

        /// <summary>
        /// Does the LocalizationSettings exist and contain a string database?
        /// </summary>
        /// <value><c>true</c> if has string database; otherwise, <c>false</c>.</value>
        public static bool HasStringDatabase => HasSettings && s_Instance.m_StringDatabase != null;

        /// <summary>
        /// Singleton instance for the Localization Settings.
        /// </summary>
        public static LocalizationSettings Instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = GetOrCreateSettings();
                return s_Instance;
            }
            set => s_Instance = value;
        }

        /// <summary>
        /// <inheritdoc cref="IStartupLocaleSelector"/>
        /// </summary>
        public static List<IStartupLocaleSelector> StartupLocaleSelectors => Instance.GetStartupLocaleSelectors();

        /// <summary>
        /// <inheritdoc cref="AvailableLocales"/>
        /// </summary>
        public static ILocalesProvider AvailableLocales
        {
            get => Instance.GetAvailableLocales();
            set => Instance.SetAvailableLocales(value);
        }

        /// <summary>
        /// The asset database is responsible for providing localized assets.
        /// </summary>
        public static LocalizedAssetDatabase AssetDatabase
        {
            get => Instance.GetAssetDatabase();
            set => Instance.SetAssetDatabase(value);
        }

        /// <summary>
        /// The string database is responsible for providing localized string assets.
        /// </summary>
        public static LocalizedStringDatabase StringDatabase
        {
            get => Instance.GetStringDatabase();
            set => Instance.SetStringDatabase(value);
        }

        /// <summary>
        /// The current selected <see cref="Locale"/>. This is the Locale that will be used by default when localizing assets and strings.
        /// If you are calling this before <see cref="InitializationOperation"/> has been completed then a null value may be returned.
        /// See <see cref="SelectedLocaleAsync"/> for a version that will ensure the Locale is not null(when possible).
        /// </summary>
        public static Locale SelectedLocale
        {
            get => Instance.GetSelectedLocale();
            set => Instance.SetSelectedLocale(value);
        }

        /// <summary>
        /// The current selected Locale. This is the Locale that will be used by default when localizing assets and strings.
        /// If <see cref="InitializationOperation"/> has not been completed yet then this will wait for the <see cref="AvailableLocales"/> part to complete first.
        /// It will not wait for the entire <see cref="InitializationOperation"/> but just the part that initializes the Locales.
        /// </summary>
        public static AsyncOperationHandle<Locale> SelectedLocaleAsync
        {
            get => Instance.GetSelectedLocaleAsync();
        }

        /// <summary>
        /// Event that is sent when the <see cref="SelectedLocale"/> is changed.
        /// </summary>
        public static event Action<Locale> SelectedLocaleChanged
        {
            add => Instance.OnSelectedLocaleChanged += value;
            remove => Instance.OnSelectedLocaleChanged -= value;
        }

        /// <summary>
        /// The <see cref="ResourceManager"/> to use for Addressable operations. Primarily used for Completed and Chain operations.
        /// </summary>
        internal static ResourceManager ResourceManager => AddressableAssets.Addressables.ResourceManager;

        void OnEnable()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
            }

            #if UNITY_EDITOR
            // Properties may persist during runs in the editor, so we reset them here to keep each play consistent.
            m_SelectedLocale = null;
            m_SelectedLocaleAsync = null;
            m_InitializingOperationHandle = null;
            #endif
        }

        #if UNITY_EDITOR
        void OnDisable()
        {
            // Properties may persist during runs in the editor, so we reset them here to keep each play consistent.
            m_SelectedLocale = null;
            m_SelectedLocaleAsync = null;
            m_InitializingOperationHandle = null;
        }

        #endif

        /// <summary>
        /// Used to ensure a <see cref="LocalizationSettings"/> exists in the project.
        /// Throws an exception if <see cref="HasSettings"/> is false.
        /// </summary>
        /// <param name="error"></param>
        internal static void ValidateSettingsExist(string error = "")
        {
            if (!HasSettings)
            {
                throw new Exception($"There is no active LocalizationSettings.\n {error}");
            }
        }

        /// <summary>
        /// <inheritdoc cref="InitializationOperation"/>
        /// </summary>
        /// <returns></returns>
        public virtual AsyncOperationHandle<LocalizationSettings> GetInitializationOperation()
        {
            if (m_InitializingOperationHandle == null)
            {
                // TODO: We need to reuse these operations however its not possible to reset the internal state at the moment.
                m_InitializingOperation = new InitializationOperation();
                m_InitializingOperation.Init(this);
                m_InitializingOperationHandle = ResourceManager.StartOperation(m_InitializingOperation, default);
            }

            return m_InitializingOperationHandle.Value;
        }

        /// <summary>
        /// <inheritdoc cref="IStartupLocaleSelector"/>
        /// </summary>
        /// <returns>\</returns>
        public List<IStartupLocaleSelector> GetStartupLocaleSelectors() => m_StartupSelectors;

        /// <summary>
        /// <inheritdoc cref="AvailableLocales"/>
        /// </summary>
        /// <param name="available"></param>
        public void SetAvailableLocales(ILocalesProvider available) => m_AvailableLocales = available;

        /// <summary>
        /// <inheritdoc cref="AvailableLocales"/>
        /// </summary>
        /// <returns>\</returns>
        public ILocalesProvider GetAvailableLocales() => m_AvailableLocales;

        /// <summary>
        /// <inheritdoc cref="AssetDatabase"/>
        /// </summary>
        /// <param name="database"></param>
        public void SetAssetDatabase(LocalizedAssetDatabase database) => m_AssetDatabase = database;

        /// <summary>
        /// <inheritdoc cref="AssetDatabase"/>
        /// </summary>
        /// <returns></returns>
        public virtual LocalizedAssetDatabase GetAssetDatabase() => m_AssetDatabase;

        /// <summary>
        /// Sets the string database to be used for localizing all strings.
        /// </summary>
        /// <param name="database"></param>
        public void SetStringDatabase(LocalizedStringDatabase database) => m_StringDatabase = database;

        /// <summary>
        /// Returns the string database being used to localize all strings.
        /// </summary>
        /// <returns>The string database.</returns>
        public virtual LocalizedStringDatabase GetStringDatabase() => m_StringDatabase;

        /// <summary>
        /// Sends out notifications when the locale has changed. Ensures the the events are sent in the correct order.
        /// </summary>
        /// <param name="locale">The new locale.</param>
        void SendLocaleChangedEvents(Locale locale)
        {
            if (m_StringDatabase != null)
                m_StringDatabase.OnLocaleChanged(locale);

            if (m_AssetDatabase != null)
                m_AssetDatabase.OnLocaleChanged(locale);

            m_InitializingOperationHandle = null;
            var initOp = GetInitializationOperation();
            if (initOp.Status == AsyncOperationStatus.Succeeded)
            {
                OnSelectedLocaleChanged?.Invoke(locale);
            }
            else
            {
                initOp.Completed += (o) =>
                {
                    // Don't send the change event until preloading is completed.
                    OnSelectedLocaleChanged?.Invoke(locale);
                };
            }
        }

        /// <summary>
        /// Uses <see cref="StartupLocaleSelectors"/> to select the most appropriate <see cref="Locale"/>.
        /// </summary>
        protected virtual Locale SelectLocale()
        {
            if (m_AvailableLocales == null)
            {
                Debug.LogError("AvailableLocales is null, can not pick a Locale.");
                return null;
            }

            if (m_AvailableLocales.Locales == null)
            {
                Debug.LogError("AvailableLocales.Locales is null, can not pick a Locale.");
                return null;
            }

            foreach (var sel in m_StartupSelectors)
            {
                var locale = sel.GetStartupLocale(m_AvailableLocales);
                if (locale != null)
                {
                    return locale;
                }
            }

            using (StringBuilderPool.Get(out var sb))
            {
                sb.AppendLine("No Locale could be selected:");

                if (m_AvailableLocales.Locales.Count == 0)
                {
                    sb.AppendLine("No Locales were available. Did you build the Addressables?");
                }
                else
                {
                    sb.AppendLine($"The following ({m_AvailableLocales.Locales.Count}) Locales were considered:");
                    foreach (var locale in m_AvailableLocales.Locales)
                    {
                        sb.AppendLine($"\t{locale}");
                    }
                }

                sb.AppendLine($"The following ({m_StartupSelectors.Count}) IStartupLocaleSelectors were used:");
                foreach (var selector in m_StartupSelectors)
                {
                    sb.AppendLine($"\t{selector}");
                }

                Debug.LogError(sb.ToString(), this);
            }

            return null;
        }

        /// <summary>
        /// Collates all the extensions that have the <see cref="IInitialize"/> interface.
        /// </summary>
        /// <returns></returns>
        internal List<IInitialize> GetInitializers()
        {
            var postInitializers = new List<IInitialize>();

            // Startup selectors
            foreach (var startSelector in m_StartupSelectors)
            {
                if (startSelector is IInitialize selectorInit)
                {
                    postInitializers.Add(selectorInit);
                }
            }

            if (m_AvailableLocales is IInitialize localeInit)
                postInitializers.Add(localeInit);

            if (m_AssetDatabase is IInitialize assetInit)
                postInitializers.Add(assetInit);

            if (m_StringDatabase is IInitialize stringInit)
                postInitializers.Add(stringInit);

            return postInitializers;
        }

        /// <summary>
        /// <inheritdoc cref="SelectedLocale"/>
        /// </summary>
        /// <param name="locale"></param>
        public void SetSelectedLocale(Locale locale)
        {
            if (!ReferenceEquals(m_SelectedLocale, locale))
            {
                m_SelectedLocale = locale;

                if (locale == null)
                    m_SelectedLocaleAsync = null;
                else
                    m_SelectedLocaleAsync = ResourceManager.CreateCompletedOperation(m_SelectedLocale, null);

                SendLocaleChangedEvents(locale);
            }
        }

        /// <summary>
        /// <inheritdoc cref="SelectedLocaleAsync"/>
        /// </summary>
        /// <returns></returns>
        public virtual AsyncOperationHandle<Locale> GetSelectedLocaleAsync()
        {
            if (!Application.isPlaying)
                return ResourceManager.CreateCompletedOperation<Locale>(null, null);

            if (!m_SelectedLocaleAsync.HasValue)
            {
                if (m_AvailableLocales is IPreloadRequired localesProvider && !localesProvider.PreloadOperation.IsDone)
                {
                    m_SelectedLocaleAsync = ResourceManager.CreateChainOperation<Locale>(localesProvider.PreloadOperation, (op) => ResourceManager.CreateCompletedOperation(GetSelectedLocale(), null));
                }
                else
                {
                    m_SelectedLocaleAsync = ResourceManager.CreateCompletedOperation(GetSelectedLocale(), null);
                }
            }
            return m_SelectedLocaleAsync.Value;
        }

        /// <summary>
        /// <inheritdoc cref="SelectedLocale"/>
        /// </summary>
        /// <returns>\</returns>
        public virtual Locale GetSelectedLocale()
        {
            if (m_SelectedLocale != null)
                return m_SelectedLocale;

            if (m_AvailableLocales is IPreloadRequired localesProvider && !localesProvider.PreloadOperation.IsDone)
            {
                Debug.LogWarning("Calling GetSelectedLocale when AvailableLocales preloading has not completed. Consider using SelectedLocaleAsync.");
                return null;
            }

            m_SelectedLocale = SelectLocale();
            return m_SelectedLocale;
        }

        /// <summary>
        /// Indicates that the Locale is no longer available.
        /// If the locale is the current <see cref="SelectedLocale"/> then a new one will be found using <see cref="StartupLocaleSelectors"/>.
        /// </summary>
        /// <param name="locale"></param>
        public void OnLocaleRemoved(Locale locale)
        {
            if (ReferenceEquals(m_SelectedLocale, locale))
                SetSelectedLocale(null);
        }

        /// <summary>
        /// Returns the singleton of the LocalizationSettings but does not create a default one if no active settings are found.
        /// </summary>
        /// <returns></returns>
        public static LocalizationSettings GetInstanceDontCreateDefault()
        {
            if (s_Instance != null)
                return s_Instance;

            LocalizationSettings settings;
            #if UNITY_EDITOR
            UnityEditor.EditorBuildSettings.TryGetConfigObject(ConfigName, out settings);
            #else
            settings = FindObjectOfType<LocalizationSettings>();
            #endif
            return settings;
        }

        static LocalizationSettings GetOrCreateSettings()
        {
            var settings = GetInstanceDontCreateDefault();
            if (settings == null)
            {
                Debug.LogWarning("Could not find localization settings. Default will be used.");

                settings = CreateInstance<LocalizationSettings>();
                settings.name = "Default Localization Settings";
            }

            return settings;
        }
    }
}
