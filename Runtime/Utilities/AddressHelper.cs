namespace UnityEngine.Localization
{
    class AddressHelper
    {
        const char Seperator = '_';

        const string k_AssetLabelPrefix = "Locale-";

        /// <summary>
        /// TODO: DOC
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="tableName"></param>
        /// <param name="localeId"></param>
        /// <returns></returns>
        public static string GetTableAddress(string tableName, LocaleIdentifier localeId)
        {
            return $"{tableName}{Seperator}{localeId.Code}";
        }

        /// <summary>
        /// TODO: DOC
        /// </summary>
        /// <param name="localeIdentifier"></param>
        /// <returns></returns>
        public static string FormatAssetLabel(LocaleIdentifier localeIdentifier) => k_AssetLabelPrefix + localeIdentifier.Code;

        /// <summary>
        /// TODO: DOC
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public static bool IsLocaleLabel(string label) => label.StartsWith(k_AssetLabelPrefix, System.StringComparison.InvariantCulture);

        /// <summary>
        /// TODO: DOC
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public static LocaleIdentifier LocaleLabelToId(string label)
        {
            Debug.Assert(IsLocaleLabel(label));
            return label.Substring(k_AssetLabelPrefix.Length, label.Length - k_AssetLabelPrefix.Length);
        }
    }
}
