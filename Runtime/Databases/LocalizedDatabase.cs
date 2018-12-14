
namespace UnityEngine.Localization
{
    public abstract class LocalizedDatabase : ScriptableObject
    {
        // Tables by type
        // StringDb plurals etc
        // AssetDb
        // Fallbacks here. fallback should be handled by the table as it can be custom.
        // Load table should also load fallback tables.

        [SerializeField]
        string m_DefaultTableName;

        public string DefaultTableName
        {
            get { return m_DefaultTableName; }
            set
            {
                m_DefaultTableName = value;
                // TODO: Update tables.
            }
        }

        /// <summary>
        /// Called before the LocaleChanged event is sent out in order to give the database a chance to prepare.
        /// </summary>
        public abstract void OnLocaleChanged(Locale locale);
    }
}