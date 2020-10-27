using CsvHelper;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.Plugins.CSV.Columns
{
    public interface IKeyColumn
    {
        SharedTableData.SharedTableEntry ReadKey(CsvReader reader);
    }
}
