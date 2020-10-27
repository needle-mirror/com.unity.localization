using System.Collections.Generic;
using System.Linq;

namespace UnityEditor.Localization.Plugins.CSV.Columns
{
    public static class ColumnMapping
    {
        public static List<CsvColumns> CreateDefaultMapping(bool includeComments = true)
        {
            var columns = new List<CsvColumns>();

            columns.Add(new KeyIdColumns { IncludeSharedComments = includeComments });
            AddLocaleMappings(columns, includeComments);
            return columns;
        }

        public static void AddLocaleMappings(IList<CsvColumns> cells, bool includeComments = true)
        {
            var projectLocales = LocalizationEditorSettings.GetLocales();
            foreach (var locale in projectLocales)
            {
                // The locale is already mapped so we can ignore it
                if (cells.Any(c => c is LocaleColumns lc && lc.LocaleIdentifier == locale.Identifier))
                    continue;

                cells.Add(new LocaleColumns { LocaleIdentifier = locale.Identifier, IncludeComments = includeComments });
            }
        }
    }
}
