using System;
using System.Globalization;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Pseudo;

namespace UnityEngine.Localization
{
    /// <summary>
    /// The identifier containing the identification information for a language or regional variant.
    /// </summary>
    /// <example>
    /// This example shows the various ways to create a LocaleIdentifier.
    /// <code>
    /// public class LocaleIdentifierIdExample1 : MonoBehaviour
    /// {
    ///     void Start()
    ///     {
    ///         // Create a locale identifier to represent English
    ///         var localeEnglishSystemLanguage = new LocaleIdentifier(SystemLanguage.English);
    ///         var localeEnglishCode = new LocaleIdentifier("en");
    ///         var localeEnglishCi = new LocaleIdentifier(CultureInfo.GetCultureInfo("en"));
    ///
    ///         Debug.Log(localeEnglishSystemLanguage);
    ///         Debug.Log(localeEnglishCode);
    ///         Debug.Log(localeEnglishCi);
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <example>
    /// This shows how to create a Locale for English and a regional Locale for English(UK).
    /// <code>
    /// public class LocaleIdentifierIdExample2 : MonoBehaviour
    /// {
    ///     void Start()
    ///     {
    ///         // Create a Locale to represent English.
    ///         var localeId = new LocaleIdentifier(SystemLanguage.English);
    ///         var locale = Locale.CreateLocale(localeId);
    ///         Debug.Log("English locale: " + locale);
    ///
    ///         // Create a regional Locale to represent English UK.
    ///         var regionalLocaleId = new LocaleIdentifier("en-GB");
    ///         var regionalLocale = Locale.CreateLocale(regionalLocaleId);
    ///         Debug.Log("English(en-GB) locale: " + regionalLocale);
    ///     }
    /// }
    /// </code>
    /// </example>
    [Serializable]
    public struct LocaleIdentifier : IEquatable<LocaleIdentifier>, IComparable<LocaleIdentifier>
    {
        [SerializeField] string m_Code;
        CultureInfo m_CultureInfo;

        /// <summary>
        /// Represents an undefined Local Identifier. One that does not define any language or region.
        /// </summary>
        public static LocaleIdentifier Undefined => new LocaleIdentifier("undefined");

        /// <summary>
        /// The culture name in the format [language]-[region].
        /// </summary>
        /// <remarks>
        /// For example, Language English would be 'en', Regional English(UK) would be 'en-GB' and Regional English(US) would be 'en-US'.
        /// </remarks>
        public string Code => m_Code;

        /// <summary>
        /// A <see cref="CultureInfo"/> representation of the Locale.
        /// The <see cref="Code"/> is used to query for a valid <see cref="CultureInfo"/>.
        /// </summary>
        /// <example>
        /// This example shows how the CultureInfo can be retrieved after creating a LocaleIdentifier using a Code.
        /// <code>
        /// public class LocaleIdentifierCultureInfoExample : MonoBehaviour
        /// {
        ///     void Start()
        ///     {
        ///         var localeIdentifier = new LocaleIdentifier("en");
        ///         Debug.Log("Code 'en' maps to the CultureInfo: " + localeIdentifier.CultureInfo.EnglishName);
        ///     }
        /// }
        /// </code>
        /// </example>
        public CultureInfo CultureInfo
        {
            get
            {
                if (m_CultureInfo == null && m_Code != null)
                {
                    try
                    {
                        m_CultureInfo = CultureInfo.GetCultureInfo(m_Code);
                    }
                    catch (CultureNotFoundException)
                    {
                        // If a culture info can not be found then we do not consider this an error. It could be a custom locale.
                    }
                }
                return m_CultureInfo;
            }
        }

        /// <summary>
        /// Create a LocaleIdentifier from a culture code string.
        /// </summary>
        /// <param name="code"></param>
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

        /// <summary>
        /// Create a LocaleIdentifier from a CultureInfo instance.
        /// </summary>
        /// <param name="culture"></param>
        /// <exception cref="ArgumentNullException">Thrown if the culture is null.</exception>
        public LocaleIdentifier(CultureInfo culture)
        {
            if (culture == null)
                throw new ArgumentNullException(nameof(culture));

            m_Code = culture.Name;
            m_CultureInfo = culture;
        }

        /// <summary>
        /// Create a LocaleIdentifier from a <see cref="UnityEngine.SystemLanguage"/> enum value.
        /// </summary>
        /// <param name="systemLanguage"></param>
        public LocaleIdentifier(SystemLanguage systemLanguage)
            : this(SystemLanguageConverter.GetSystemLanguageCultureCode(systemLanguage))
        {
        }

        #pragma warning disable CA2225 // CA2225: Operator overloads have named alternates

        /// <summary>
        /// Create a LocaleIdentifier from a culture code string.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static implicit operator LocaleIdentifier(string code) => new LocaleIdentifier(code);

        /// <summary>
        /// Create a LocaleIdentifier from a CultureInfo instance.
        /// </summary>
        /// <param name="culture"></param>
        /// <exception cref="ArgumentNullException">Thrown if the culture is null.</exception>
        /// <returns></returns>
        public static implicit operator LocaleIdentifier(CultureInfo culture) => new LocaleIdentifier(culture);

        /// <summary>
        /// Create a LocaleIdentifier from a <see cref="UnityEngine.SystemLanguage"/> enum value.
        /// </summary>
        /// <param name="systemLanguage"></param>
        /// <returns></returns>
        public static implicit operator LocaleIdentifier(SystemLanguage systemLanguage) => new LocaleIdentifier(systemLanguage);

        #pragma warning restore CA2225

        /// <summary>
        /// Returns a string representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{(CultureInfo != null ? CultureInfo.EnglishName : "Custom")}({Code})";

        /// <summary>
        /// Compare the LocaleIdentifier to another LocaleIdentifier.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            return obj is LocaleIdentifier identifier && Equals(identifier);
        }

        /// <summary>
        /// Compare the LocaleIdentifier to another LocaleIdentifier.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(LocaleIdentifier other) => Code == other.Code;

        /// <summary>
        /// Returns the hash code of <see cref="CultureInfo"/> or <see cref="Code"/> if it is null.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            if (CultureInfo != null)
                return CultureInfo.GetHashCode();
            return !string.IsNullOrEmpty(Code) ? Code.GetHashCode() : base.GetHashCode();
        }

        /// <summary>
        /// Compare to another LocaleIdentifer.
        /// Performs a comparison against the <see cref="CultureInfo.EnglishName"/> property.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(LocaleIdentifier other)
        {
            if (CultureInfo == null || other.CultureInfo == null)
                return 1;
            return string.Compare(CultureInfo.EnglishName, other.CultureInfo.EnglishName);
        }

        /// <summary>
        /// Compare the LocaleIdentifier to another LocaleIdentifier.
        /// </summary>
        /// <param name="l1"></param>
        /// <param name="l2"></param>
        /// <returns></returns>
        public static bool operator==(LocaleIdentifier l1, LocaleIdentifier l2) => l1.Equals(l2);

        /// <summary>
        /// Compare the LocaleIdentifier to another LocaleIdentifier.
        /// </summary>
        /// <param name="l1"></param>
        /// <param name="l2"></param>
        /// <returns></returns>
        public static bool operator!=(LocaleIdentifier l1, LocaleIdentifier l2) => !l1.Equals(l2);
    }

    /// <summary>
    /// A Locale represents a language. It supports regional variations and can be configured with an optional fallback Locale via metadata.
    /// </summary>
    public class Locale : ScriptableObject, IComparable<Locale>
    {
        [SerializeField]
        LocaleIdentifier m_Identifier;

        [SerializeField]
        [MetadataType(MetadataType.Locale)]
        MetadataCollection m_Metadata = new MetadataCollection();

        [SerializeField]
        ushort m_SortOrder = 10000; // Default to a large value so new Locales are always at the end of a list.

        /// <summary>
        /// The identifier contains the identifying information such as the id and culture Code for this Locale.
        /// </summary>
        public LocaleIdentifier Identifier
        {
            get => m_Identifier;
            set => m_Identifier = value;
        }

        /// <summary>
        /// Optional Metadata. It is possible to attach additional data to the Locale providing
        /// it implements the <see cref="IMetadata"/> interface and is serializable.
        /// </summary>
        public MetadataCollection Metadata
        {
            get => m_Metadata;
            set => m_Metadata = value;
        }

        /// <summary>
        /// The sort order can be used to override the order of Locales when sorted in a list.
        /// If Locales both have the same SortOrder then they will be sorted by name.
        /// </summary>
        public ushort SortOrder
        {
            get => m_SortOrder;
            set => m_SortOrder = value;
        }

        /// <summary>
        /// Create a new <see cref="Locale"/> using the culture code.
        /// </summary>
        /// <param name="code">Culture code.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Create a new <see cref="Locale"/> using the provided <see cref="LocaleIdentifier"/>.
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Create a <see cref="Locale"/> using the system language enum value.
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public static Locale CreateLocale(SystemLanguage language)
        {
            return CreateLocale(new LocaleIdentifier(SystemLanguageConverter.GetSystemLanguageCultureCode(language)));
        }

        /// <summary>
        /// Create a <see cref="Locale"/> using a CultureInfo.
        /// </summary>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public static Locale CreateLocale(CultureInfo cultureInfo)
        {
            return CreateLocale(new LocaleIdentifier(cultureInfo));
        }

        /// <summary>
        /// Compares the Locales <see cref="name"/> properties.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Locale other)
        {
            // Sort by the sort order if they are different
            if (SortOrder != other.SortOrder)
            {
                return SortOrder.CompareTo(other.SortOrder);
            }

            // If they are both the same type then use the name to sort
            if (GetType() == other.GetType())
            {
                return string.Compare(name, other.name);
            }

            // Normal Locale's go before PseudoLocale's
            if (other is PseudoLocale)
                return -1;
            return 1;
        }

        /// <summary>
        /// Returns <see cref="name"/>
        /// </summary>
        /// <returns></returns>
        public override string ToString() => name;
    }
}
