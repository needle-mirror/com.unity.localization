using System;
using System.Collections.Generic;
using UnityEditor.Localization.Plugins.CSV.Columns;
using UnityEngine;

namespace UnityEditor.Localization.Plugins.CSV
{
    [Serializable]
    [StringTableCollectionExtension]
    public class CsvExtension : CollectionExtension
    {
        [SerializeReference]
        List<CsvColumns> m_ColumnsList = ColumnMapping.CreateDefaultMapping();

        [SerializeField]
        string m_ConnectedFile;

        public List<CsvColumns> Columns => m_ColumnsList;

        public string File
        {
            get => m_ConnectedFile;
            set => m_ConnectedFile = value;
        }
    }
}
