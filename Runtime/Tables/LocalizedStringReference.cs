using System;
using UnityEngine.ResourceManagement;

namespace UnityEngine.Localization
{
    [Serializable]
    public class LocalizedStringReference : LocalizedReference
    {
        /// <summary>
        /// This function will load the requested string table and return the translated string.
        /// The Completed event will provide notification once the operation has finished and the string has been
        /// found or an error has occurred, this will be called during LateUpdate.
        /// It is possible that a string table may have already been loaded, such as during a previous operation
        /// or when using Preload mode, the IsDone property can be checked as it is possible the translated
        /// string is immediately available.
        /// </summary>
        /// <returns></returns>
        public IAsyncOperation<string> GetLocalizedString()
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString(TableName, Key);
        }

        /// <summary>
        /// This function will load the requested string table and return the translated string formatted using the Locale PluralForm.
        /// The Completed event will provide notification once the operation has finished and the string has been
        /// found or an error has occurred, this will be called during LateUpdate.
        /// It is possible that a string table may have already been loaded, such as during a previous operation
        /// or when using Preload mode, the IsDone property can be checked as it is possible the translated
        /// string is immediately available.
        /// </summary>
        /// <returns></returns>
        /// <param name="plural">A plural value to be used when translating the string.</param>
        public IAsyncOperation<string> GetLocalizedString(int plural)
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString(TableName, Key, plural);
        }

        /// <summary>
        /// This function will load the requested string table. This is useful when multiple strings are required.
        /// The Completed event will provide notification once the operation has finished and the string table has been
        /// found or an error has occurred, this will be called during LateUpdate.
        /// It is possible that a string table may have already been loaded, such as during a previous operation
        /// or when using Preload mode, the IsDone property can be checked as it is possible the translated
        /// string is immediately available.
        /// </summary>
        /// <returns></returns>
        public IAsyncOperation<StringTableBase> GetLocalizedStringTable()
        {
            return LocalizationSettings.StringDatabase.GetTable(TableName);
        }
    }
}