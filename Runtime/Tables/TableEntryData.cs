using System;
using UnityEngine.Localization.Metadata;

namespace UnityEngine.Localization.Tables
{
    [Serializable]
    class TableEntryData
    {
        [SerializeField]
        uint m_Id;

        [SerializeField]
        string m_Localized;

        [SerializeField]
        MetadataCollection m_Metadata = new MetadataCollection();

        /// <summary>
        /// The Key for this table entry. Must be unique for each table.
        /// </summary>
        public uint Id
        {
            get => m_Id;
            set => m_Id = value;
        }

        /// <summary>
        /// Localized table entry. This can be either a translated string or for assets the guid.
        /// </summary>
        public string Localized
        {
            get => m_Localized;
            set => m_Localized = value;
        }

        /// <summary>
        /// TODO: DOC
        /// </summary>
        public MetadataCollection Metadata
        {
            get => m_Metadata;
            set => m_Metadata = value;
        }

        public TableEntryData() {}

        /// <summary>
        /// TODO: DOC
        /// </summary>
        /// <param name="id"></param>
        public TableEntryData(uint id) => Id = id;

        /// <summary>
        /// TODO: DOC
        /// </summary>
        /// <param name="id"></param>
        /// <param name="localized"></param>
        public TableEntryData(uint id, string localized) : this(id)
        {
            Localized = localized;
        }
    }
}
