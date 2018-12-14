namespace UnityEngine.Localization
{
    /// <summary>
    /// Base class for all StringTables.
    /// </summary>
    public abstract class StringTableBase : LocalizedTable
    {
        PluralForm m_PluralHandler;

        /// <summary>
        /// <inheritdoc cref="PluralForm"/>
        /// </summary>
        public virtual PluralForm PluralHandler
        {
            get
            {
                if (m_PluralHandler == null)
                {
                    m_PluralHandler = PluralForm.CreatePluralForm(LocaleIdentifier.Code);
                    Debug.Assert(m_PluralHandler != null, "Could not find plural form for code: " + LocaleIdentifier.Code);
                }

                return m_PluralHandler;
            }
            set { m_PluralHandler = value; }
        }

        /// <summary>
        /// Returns the localized version of the string or null if one can not be found.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract string GetLocalizedString(string key);

        /// <summary>
        /// Gets the localized plural string using the <see cref="PluralHandler"/>.
        /// </summary>
        /// <returns>The localized plural string. E.G '{0} files removed' or null if one can not be found.</returns>
        /// <param name="key">Key or the original singular form.</param>
        /// <param name="n">Plural amount</param>
        public abstract string GetLocalizedPluralString(string key, int n);
    }
}