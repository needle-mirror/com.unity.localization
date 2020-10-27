using System;
using System.Collections.Generic;
using CsvHelper;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.Plugins.CSV.Columns
{
    [Serializable]
    public abstract class CsvColumns
    {
        public abstract void WriteBegin(StringTableCollection collection, CsvWriter csvWriter);
        public virtual void WriteEnd(StringTableCollection collection) {}

        public abstract void ReadBegin(StringTableCollection collection, CsvReader csvReader);
        public virtual void ReadEnd(StringTableCollection collection) {}

        public abstract void WriteRow(SharedTableData.SharedTableEntry keyEntry, IList<StringTableEntry> tableEntries, CsvWriter writer);
        public abstract void ReadRow(SharedTableData.SharedTableEntry keyEntry, CsvReader reader);
    }
}
