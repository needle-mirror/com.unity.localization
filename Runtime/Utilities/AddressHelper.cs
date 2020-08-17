namespace UnityEngine.Localization
{
    class AddressHelper
    {
        const char Seperator = '_';

        const string k_AssetLabelPrefix = "Locale-";

        public static string GetTableAddress(string tableName, LocaleIdentifier localeId)
        {
            return $"{tableName}{Seperator}{localeId.Code}";
        }

        public static string FormatAssetLabel(LocaleIdentifier localeIdentifier) => k_AssetLabelPrefix + localeIdentifier.Code;

        public static bool IsLocaleLabel(string label) => label.StartsWith(k_AssetLabelPrefix, System.StringComparison.InvariantCulture);

        public static LocaleIdentifier LocaleLabelToId(string label)
        {
            Debug.Assert(IsLocaleLabel(label));
            return label.Substring(k_AssetLabelPrefix.Length, label.Length - k_AssetLabelPrefix.Length);
        }
    }
}
