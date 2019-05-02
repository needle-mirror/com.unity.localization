namespace UnityEngine.Localization
{
    public abstract class LocalizedTable : ScriptableObject
    {
        [SerializeField]
        LocaleIdentifier m_LocaleId;

        [SerializeField]
        string m_TableName = "Default";

        [SerializeField]
        KeyDatabase m_KeyDatabase;

        /// <summary>
        /// The locale this asset table supports.
        /// </summary>
        public LocaleIdentifier LocaleIdentifier
        {
            get => m_LocaleId;
            set => m_LocaleId = value;
        }

        /// <summary>
        /// The name of this asset table. Must be unique per locale.
        /// </summary>
        public string TableName
        {
            get => m_TableName;
            set => m_TableName = value;
        }

        /// <summary>
        /// Database of all keys used by this Table.
        /// </summary>
        public KeyDatabase Keys
        {
            get => m_KeyDatabase;
            set => m_KeyDatabase = value;
        }

        public override string ToString() => TableName + "(" + LocaleIdentifier + ")";
    }
}