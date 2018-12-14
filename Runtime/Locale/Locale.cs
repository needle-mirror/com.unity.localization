using System;
using System.Globalization;

namespace UnityEngine.Localization
{
    /// <summary>
    /// The identifier containing the identification information for a language or regional variant.
    /// </summary>
    [Serializable]
    public struct LocaleIdentifier
    {
        [SerializeField] string m_Code;
        CultureInfo m_CultureInfo;

        /// <summary>
        /// Represents an undefined Local Identifier. One that does not define any language or region.
        /// </summary>
        public static LocaleIdentifier Undefined { get { return new LocaleIdentifier("undefined"); } }

        /// <summary>
        /// The culture name in the format [language]-[region].
        /// </summary>
        /// <remarks>
        /// For example, Language English would be 'en', Regional English(UK) would be 'en-GB' and Regional English(US) would be 'en-US'.
        /// </remarks>
        public string Code { get { return m_Code; } }

        /// <summary>
        /// A <see cref="CultureInfo"/> representation of the Locale.
        /// </summary>
        /// <remarks>
        /// The id is used to query for a <see cref="CultureInfo"/> unless its value is 0, in which case the <see cref="Code"/> will be used.
        /// </remarks>
        public CultureInfo CultureInfo
        {
            get
            {
                if (m_CultureInfo == null)
                {
                    try
                    {
                        m_CultureInfo = CultureInfo.GetCultureInfo(m_Code);
                    }
                    catch (Exception)
                    {
                        // If a culture info can not be found then we do not consider this an error. It could be a custom locale.
                    }
                }
                return m_CultureInfo;
            }
        }

        public LocaleIdentifier(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                this = Undefined;
                return;
            }

            m_Code = code;
            m_CultureInfo = null;
        }

        public LocaleIdentifier(CultureInfo culture)
        {
            if (culture == null)
                throw new ArgumentNullException("culture");

            m_Code = culture.Name;
            m_CultureInfo = culture;
        }

        public LocaleIdentifier(SystemLanguage systemLanguage)
            : this(SystemLanguageConverter.GetSystemLanguageCultureCode(systemLanguage))
        {
        }

        public static implicit operator LocaleIdentifier(string code)
        {
            return new LocaleIdentifier(code);
        }

        public static implicit operator LocaleIdentifier(CultureInfo culture)
        {
            return new LocaleIdentifier(culture);
        }

        public static implicit operator LocaleIdentifier(SystemLanguage systemLanguage)
        {
            return new LocaleIdentifier(systemLanguage);
        }

        public override string ToString()
        {
            return string.Format("{0}({1})", CultureInfo != null ? CultureInfo.EnglishName : "Custom" , Code);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is LocaleIdentifier && Equals((LocaleIdentifier)obj);
        }

        public bool Equals(LocaleIdentifier other)
        {
            return Code == other.Code;
        }

        public override int GetHashCode()
        {
            if (CultureInfo != null)
                return CultureInfo.GetHashCode();
            return !string.IsNullOrEmpty(Code) ? Code.GetHashCode() : base.GetHashCode();
        }

        public static bool operator==(LocaleIdentifier l1, LocaleIdentifier l2)
        {
            return l1.Equals(l2);
        }

        public static bool operator!=(LocaleIdentifier l1, LocaleIdentifier l2)
        {
            return !l1.Equals(l2);
        }
    }

    /// <summary>
    /// A Locale represents a language. It supports regional variations and can be configured with an optional fallback locale.
    /// </summary>
    public class Locale : ScriptableObject
    {
        [SerializeField] LocaleIdentifier m_Identifier;
        [SerializeField] Locale m_Fallback;

        /// <summary>
        /// The identifier contains the identifying information such as the id and culture Code for this Locale.
        /// </summary>
        public LocaleIdentifier Identifier
        {
            get { return m_Identifier; }
            set { m_Identifier = value; }
        }

        /// <summary>
        /// An optional fallback locale can be configured to link Locales that may serve as good alternatives when no localization
        /// data is available for the current locale.
        /// </summary>
        /// <remarks>
        /// For example, regional variations could be configured for some specific words such as when the selected Locale is English(UK),
        /// then the word Color could be localized to ‘Colour’ however other words would fall back from English(UK) to English(US).
        /// Note: The fallback locale must be added to the active LocalizationSettings or it will not be possible to retrieve it.
        /// </remarks>
        public Locale FallbackLocale
        {
            get
            {
                return m_Fallback;
            }
            set
            {
                m_Fallback = value;
                ValidateFallback();
            }
        }

        protected virtual void OnEnable()
        {
            ValidateFallback();
        }

        /// <summary>
        /// Check we don't have a fallback locale chain that leads back to this locale and an infinite loop.
        /// </summary>
        void ValidateFallback()
        {
            Locale parent = m_Fallback;
            while (parent != null)
            {
                if (parent == this)
                {
                    Debug.LogWarning("Cyclic fallback linking detected. Can not set fallback locale as it would create an infinite loop.", this);
                    m_Fallback = null;
                }
                parent = parent.FallbackLocale;
            }
        }

        public static Locale CreateLocale(string code)
        {
            var locale = CreateInstance<Locale>();
            locale.m_Identifier = new LocaleIdentifier(code);
            if (locale.m_Identifier.CultureInfo != null)
            {
                locale.name = locale.m_Identifier.CultureInfo.EnglishName;
            }
            return locale;
        }

        public static Locale CreateLocale(LocaleIdentifier identifier)
        {
            var locale = CreateInstance<Locale>();
            locale.m_Identifier = identifier;
            if (locale.m_Identifier.CultureInfo != null)
            {
                locale.name = locale.m_Identifier.CultureInfo.EnglishName;
            }
            return locale;
        }

        public static Locale CreateLocale(SystemLanguage language)
        {
            return CreateLocale(new LocaleIdentifier(SystemLanguageConverter.GetSystemLanguageCultureCode(language)));
        }

        public static Locale CreateLocale(CultureInfo cultureInfo)
        {
            return CreateLocale(new LocaleIdentifier(cultureInfo));
        }

        public override string ToString()
        {
            return name;
        }
    }
}
