using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Localization
{
    public enum PreloadBehavior
    {
        Preload,
        OnDemand
    }

    /// <summary>
    /// The localization settings is the core component to the localization system.
    /// It provides the entry point to all player based localization features.
    /// </summary>
    public class LocalizationSettings : ScriptableObject
    {
        /// <summary>
        /// The name to use when retrieving the LocalizationSettings from CustomObject API.
        /// </summary>
        public const string ConfigName = "com.unity.localization.settings";

        /// <summary>
        /// Label used when searching Addressable assets for Locales.
        /// </summary>
        public const string LocaleLabel = "Locale";

        [SerializeField]
        PreloadBehavior m_PreloadBehavior = PreloadBehavior.OnDemand;

        [SerializeField]
        StartupLocaleSelector m_LocaleSelector;

        [SerializeField]
        LocalesProvider m_AvailableLocales;

        [SerializeField]
        LocalizedAssetDatabase m_AssetDatabase;

        [SerializeField]
        LocalizedStringDatabase m_StringDatabase;

        InitializationOperation m_InitializingOperation;

        Locale m_SelectedLocale;

        public event Action<Locale> OnSelectedLocaleChanged;

        static LocalizationSettings s_Instance;

        /// <summary>
        /// Indicates if there is a LocalizationSettings present. If one is not found will attempt to find one however
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
        public static InitializationOperation InitializationOperation
        {
            get { return Instance.GetInitializationOperation(); }
        }

        /// <summary>
        /// Does the LocalizationSettings exist and contain a string database?
        /// </summary>
        /// <value><c>true</c> if has string database; otherwise, <c>false</c>.</value>
        public static bool HasStringDatabase
        {
            get { return HasSettings && s_Instance.m_StringDatabase != null; }
        }

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
            set { s_Instance = value; }
        }

        /// <summary>
        /// TODO: DOC
        /// </summary>
        public PreloadBehavior PreloadBehavior
        {
            get { return Instance.GetPreloadBehavior(); }
            set { Instance.SetPreloadBehavior(value); }
        }

        /// <summary>
        /// <inheritdoc cref="StartupLocaleSelector"/>
        /// </summary>
        public static StartupLocaleSelector StartupLocaleSelector
        {
            get { return Instance.GetStartupLocaleSelector(); }
            set { Instance.SetStartupLocaleSelector(value); }
        }

        /// <summary>
        /// <inheritdoc cref="AvailableLocales"/>
        /// </summary>
        public static LocalesProvider AvailableLocales
        {
            get { return Instance.GetAvailableLocales(); }
            set { Instance.SetAvailableLocales(value); }
        }

        /// <summary>
        /// The asset database is responsible for providing localized assets.
        /// </summary>
        public static LocalizedAssetDatabase AssetDatabase
        {
            get { return Instance.GetAssetDatabase(); }
            set { Instance.SetAssetDatabase(value); }
        }

        /// <summary>
        /// The string database is responsible for providing localized string assets.
        /// </summary>
        public static LocalizedStringDatabase StringDatabase
        {
            get { return Instance.GetStringDatabase(); }
            set { Instance.SetStringDatabase(value); }
        }

        /// <summary>
        /// The current selected locale. This is the locale that will be used when localizing assets.
        /// </summary>
        public static Locale SelectedLocale
        {
            get { return Instance.GetSelectedLocale(); }
            set { Instance.SetSelectedLocale(value); }
        }

        /// <summary>
        /// Event that is sent when the <see cref="SelectedLocale"/> is changed.
        /// </summary>
        public static event Action<Locale> SelectedLocaleChanged
        {
            add { Instance.OnSelectedLocaleChanged += value; }
            remove { Instance.OnSelectedLocaleChanged -= value; }
        }

        void OnEnable()
        {
            if (s_Instance == null)
            {
                s_Instance = this;
            }

            #if UNITY_EDITOR
            // Properties may persist during runs in the editor, so we reset them here to keep each play consistent.
            m_SelectedLocale = null;
            m_InitializingOperation = null;
            #endif

            if (Application.isPlaying && m_AvailableLocales != null && m_LocaleSelector != null)
                GetInitializationOperation();
        }

        #if UNITY_EDITOR
        void OnDisable()
        {
            // Properties may persist during runs in the editor, so we reset them here to keep each play consistent.
            m_SelectedLocale = null;
            m_InitializingOperation = null;
        }
        #endif

        /// <summary>
        /// TODO: DOC
        /// </summary>
        /// <returns></returns>
        public PreloadBehavior GetPreloadBehavior()
        {
            return m_PreloadBehavior;
        }

        /// <summary>
        /// TODO: DOC
        /// </summary>
        /// <param name="behavior"></param>
        public void SetPreloadBehavior(PreloadBehavior behavior)
        {
            m_PreloadBehavior = behavior;
        }

        /// <summary>
        /// <inheritdoc cref="InitializationOperation"/>
        /// </summary>
        public InitializationOperation GetInitializationOperation()
        {
            if (m_InitializingOperation == null)
            {
                m_InitializingOperation = new InitializationOperation();
                m_InitializingOperation.Start(this);
            }

            return m_InitializingOperation;
        }

        /// <summary>
        /// <inheritdoc cref="StartupLocaleSelector"/>
        /// </summary>
        public void SetStartupLocaleSelector(StartupLocaleSelector selector)
        {
            m_LocaleSelector = selector;
        }

        /// <summary>
        /// <inheritdoc cref="StartupLocaleSelector"/>
        /// </summary>
        public StartupLocaleSelector GetStartupLocaleSelector()
        {
            return m_LocaleSelector;
        }

        /// <summary>
        /// <inheritdoc cref="AvailableLocales"/>
        /// </summary>
        public void SetAvailableLocales(LocalesProvider available)
        {
            m_AvailableLocales = available;
        }

        /// <summary>
        /// <inheritdoc cref="AvailableLocales"/>
        /// </summary>
        public LocalesProvider GetAvailableLocales()
        {
            return m_AvailableLocales;
        }

        /// <summary>
        /// <inheritdoc cref="AssetDatabase"/>
        /// </summary>
        /// <param name="database"></param>
        public void SetAssetDatabase(LocalizedAssetDatabase database)
        {
            m_AssetDatabase = database;
        }

        /// <summary>
        /// <inheritdoc cref="AssetDatabase"/>
        /// </summary>
        /// <returns></returns>
        public LocalizedAssetDatabase GetAssetDatabase()
        {
            return m_AssetDatabase;
        }

        /// <summary>
        /// Sets the string database to be used for localizing all strings.
        /// </summary>
        public void SetStringDatabase(LocalizedStringDatabase database)
        {
            m_StringDatabase = database;
        }

        /// <summary>
        /// Returns the string database being used to localize all strings.
        /// </summary>
        /// <returns>The string database.</returns>
        public LocalizedStringDatabase GetStringDatabase()
        {
            return m_StringDatabase;
        }

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

            if (m_PreloadBehavior == PreloadBehavior.Preload)
            {
                var initOp = GetInitializationOperation();

                initOp.ResetStatus();
                initOp.Start(this);

                initOp.Completed += (o) =>
                {
                    // Don't send the change event until preloading is completed.
                    if (OnSelectedLocaleChanged != null)
                        OnSelectedLocaleChanged(locale);
                };
            }
            else if (OnSelectedLocaleChanged != null)
            {
                OnSelectedLocaleChanged(locale);
            }
        }

        /// <summary>
        /// Uses the Startup locale selector to select the most appropriate locale.
        /// Does not send the locale changed event.
        /// </summary>
        internal void InitializeSelectedLocale()
        {
            Debug.Assert(m_AvailableLocales != null, "Available locales is null, can not pick a locale.");
            m_SelectedLocale = StartupLocaleSelector.GetStartupLocale(m_AvailableLocales);
            Debug.Assert(m_SelectedLocale != null, "No locale could be selected. Please check available locales and startup selector.");
        }

        /// <summary>
        /// <inheritdoc cref="SelectedLocale"/>
        /// </summary>
        public void SetSelectedLocale(Locale locale)
        {
            if (!ReferenceEquals(m_SelectedLocale, locale))
            {
                m_SelectedLocale = locale;
                SendLocaleChangedEvents(locale);
            }
        }

        /// <summary>
        /// <inheritdoc cref="SelectedLocale"/>
        /// </summary>
        public Locale GetSelectedLocale()
        {
            return m_SelectedLocale;
        }

        public void OnLocaleRemoved(Locale locale)
        {
            if (locale == GetSelectedLocale())
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
                settings = CreateDefault();
                settings.name = "Default Localization Settings";
            }

            return settings;
        }

        /// <summary>
        /// Creates a <see cref="LocalizationSettings"/> setup with all the default delegates needed to localize your application.
        /// </summary>
        /// <param name="createdDependencies">Optional list which will be populated with additional scriptable objects that are created for the localization settings.</param>
        /// <returns>The new localization settings asset.</returns>
        public static LocalizationSettings CreateDefault(List<ScriptableObject> createdDependencies = null)
        {
            var localizationSettings = CreateInstance<LocalizationSettings>();
            localizationSettings.name = "Localization Settings";

            // Selector
            var localeSelectorCollection = CreateInstance<StartupLocaleSelectorCollection>();
            localizationSettings.m_LocaleSelector = localeSelectorCollection;
            localeSelectorCollection.name = "Locale Selector Collection";
            var commandLine = CreateInstance<CommandLineLocaleSelector>();
            commandLine.name = "Command Line Locale Selector";
            var systemLocale = CreateInstance<SystemLocaleSelector>();
            systemLocale.name = "System Locale Selector";
            var defaultLocale = CreateInstance<SpecificLocaleSelector>();
            defaultLocale.name = "Default Locale Selector";
            localeSelectorCollection.StartupSelectors = new List<StartupLocaleSelector> { commandLine, systemLocale, defaultLocale };

            // Locales
            var localesCollection = CreateInstance<AddressableLocalesProvider>();
            localesCollection.name = "Locales Provider";
            localizationSettings.m_AvailableLocales = localesCollection;

            // Asset Database
            var assetDb = CreateInstance<LocalizedAssetDatabase>();
            assetDb.name = "Asset Database";
            localizationSettings.m_AssetDatabase = assetDb;

            // String Database
            var resourcesStringDatabase = CreateInstance<LocalizedStringDatabase>();
            resourcesStringDatabase.name = "String Database";
            localizationSettings.m_StringDatabase = resourcesStringDatabase;

            if (createdDependencies != null)
            {
                createdDependencies.Add(localeSelectorCollection);
                createdDependencies.Add(localesCollection);
                createdDependencies.Add(assetDb);
                createdDependencies.Add(resourcesStringDatabase);
                createdDependencies.AddRange(localeSelectorCollection.StartupSelectors.Cast<ScriptableObject>());
            }
            return localizationSettings;
        }
    }
}