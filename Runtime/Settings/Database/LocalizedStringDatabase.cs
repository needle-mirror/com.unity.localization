using System;
using UnityEngine.Localization.SmartFormat;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization.Settings
{
    /// <summary>
    /// Handles loading strings and their tables for the selected locale.
    /// </summary>
    [Serializable]
    public class LocalizedStringDatabase : LocalizedDatabase<StringTable, StringTableEntry>
    {
        [SerializeField]
        string m_NoTranslationFoundFormat = "No translation found for '{0}'";

        [SerializeReference]
        SmartFormatter m_SmartFormat = Smart.CreateDefaultSmartFormat();

        /// <summary>
        /// The message to display when a string can not be localized.
        /// The final string will be created using String.Format where format item 0 contains the original string.
        /// </summary>
        public string NoTranslationFoundFormat
        {
            get => m_NoTranslationFoundFormat;
            set => m_NoTranslationFoundFormat = value;
        }

        /// <summary>
        /// The <see cref="SmartFormatter"/> that will be used for all smart string operations.
        /// </summary>
        public SmartFormatter SmartFormatter
        {
            get => m_SmartFormat;
            set => m_SmartFormat = value;
        }

        /// <summary>
        /// Attempts to retrieve a string from the default StringTable.
        /// This function is asynchronous and may not have an immediate result.
        /// Check IsDone to see if the data is available, if it is false then use the Completed event or yield on the operation.
        /// </summary>
        /// <param name="tableEntryReference"></param>
        /// <returns></returns>
        public AsyncOperationHandle<string> GetLocalizedStringAsync(TableEntryReference tableEntryReference)
        {
            return GetLocalizedStringAsync(tableEntryReference, null);
        }

        /// <summary>
        /// Attempts to retrieve a string from the default StringTable.
        /// This function is asynchronous and may not have an immediate result.
        /// Check IsDone to see if the data is available, if it is false then use the Completed event or yield on the operation.
        /// </summary>
        /// <param name="tableEntryReference">A reference to the entry in the <see cref="LocalizedDatabase{TTable, TEntry}.DefaultTable"/></param>
        /// <param name="arguments">Arguments passed to SmartFormat or String.Format.</param>
        /// <returns></returns>
        public AsyncOperationHandle<string> GetLocalizedStringAsync(TableEntryReference tableEntryReference, params object[] arguments)
        {
            return GetLocalizedStringAsync(DefaultTable, tableEntryReference, arguments);
        }

        /// <summary>
        /// Attempts to retrieve a string from the requested table.
        /// This function is asynchronous and may not have an immediate result.
        /// Check IsDone to see if the data is available, if it is false then use the Completed event or yield on the operation.
        /// </summary>
        /// <param name="tableReference">A reference to the table to check for the string.</param>
        /// <param name="tableEntryReference">A reference to the entry in the <see cref="StringTable"/></param>
        /// <returns></returns>
        public AsyncOperationHandle<string> GetLocalizedStringAsync(TableReference tableReference, TableEntryReference tableEntryReference)
        {
            return GetLocalizedStringAsync(tableReference, tableEntryReference, null);
        }

        /// <summary>
        /// Attempts to retrieve a string from the requested table.
        /// The string will first be formatted with <see cref="SmartFormat"/> if <see cref="StringTableEntry.IsSmart"/> is enabled otherwise it will use String.Format.
        /// This function is asynchronous and may not have an immediate result.
        /// Check IsDone to see if the data is available, if it is false then use the Completed event or yield on the operation.
        /// </summary>
        /// <param name="tableReference">A reference to the table to check for the string.</param>
        /// <param name="tableEntryReference">A reference to the entry in the <see cref="StringTable"/></param>
        /// <param name="arguments">Arguments passed to SmartFormat or String.Format.</param>
        /// <returns></returns>
        public AsyncOperationHandle<string> GetLocalizedStringAsync(TableReference tableReference, TableEntryReference tableEntryReference, params object[] arguments)
        {
            tableReference.Validate();
            tableEntryReference.Validate();

            var initOp = LocalizationSettings.InitializationOperation;
            if (!initOp.IsDone)
                return ResourceManager.CreateChainOperation(initOp, (op) => GetLocalizedStringAsyncInternal(tableReference, tableEntryReference, arguments));
            return GetLocalizedStringAsyncInternal(tableReference, tableEntryReference, arguments);
        }

        /// <inheritdoc cref="GetLocalizedStringAsync"/>
        protected virtual AsyncOperationHandle<string> GetLocalizedStringAsyncInternal(TableReference tableReference, TableEntryReference tableEntryReference, object[] arguments)
        {
            var tableEntryOp = GetTableEntryAsync(tableReference, tableEntryReference);
            if (!tableEntryOp.IsDone)
                return ResourceManager.CreateChainOperation(tableEntryOp, (op) => GetLocalizedString_ProcessTableEntry(op, tableEntryReference, arguments));
            return GetLocalizedString_ProcessTableEntry(tableEntryOp, tableEntryReference, arguments);
        }

        /// <summary>
        /// Converts a <see cref="StringTableEntry"/> into a translated string.
        /// Any Smart Format or String.Format operations are performed here.
        /// </summary>
        /// <param name="entryOp">The handle generated from calling <see cref="LocalizedDatabase.GetTableEntryAsync"/>.</param>
        /// <param name="tableEntryReference"></param>
        /// <param name="arguments">Arguments to be passed to Smart Format or String.Format. If null then no formatting will be performed.</param>
        /// <returns></returns>
        protected virtual AsyncOperationHandle<string> GetLocalizedString_ProcessTableEntry(AsyncOperationHandle<TableEntryResult> entryOp, TableEntryReference tableEntryReference, object[] arguments)
        {
            if (entryOp.Status != AsyncOperationStatus.Succeeded || entryOp.Result.Entry == null)
            {
                string key = tableEntryReference.ResolveKeyName(entryOp.Result.Table?.Keys);
                return ResourceManager.CreateCompletedOperation(ProcessUntranslatedText(key) , null);
            }

            var entry = entryOp.Result.Entry;
            return ResourceManager.CreateCompletedOperation(entry.GetLocalizedString(arguments), null);
        }

        /// <summary>
        /// Returns a string to indicate that the entry could not be found for the key when calling <see cref="GetLocalizedStringAsync"/>.
        /// </summary>
        /// <param name="original">The <see cref="KeyDatabase"/> key or keyId if a <see cref="KeyDatabase"/> could not be found to convert a keyId.</param>
        /// <returns></returns>
        internal string ProcessUntranslatedText(string key)
        {
            return string.IsNullOrEmpty(NoTranslationFoundFormat) ? key : string.Format(NoTranslationFoundFormat, key);
        }
    }
}