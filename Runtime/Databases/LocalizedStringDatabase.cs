using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;

namespace UnityEngine.Localization
{
    /// <summary>
    /// Handles loading strings and their tables for the selected locale.
    /// </summary>
    public class LocalizedStringDatabase : LocalizedDatabase, IPreloadRequired
    {
        public const string StringTableLabel = "StringTable";

        Dictionary<string, IAsyncOperation<StringTableBase>> m_Tables = new Dictionary<string, IAsyncOperation<StringTableBase>>();

        IAsyncOperation<StringTableBase> m_DefaultTable;

        IAsyncOperation m_PreloadOperation;

        [SerializeField]
        string m_NoTranslationFoundFormat = "No translation found for '{0}'";

        /// <summary>
        /// The message to display when a string can not be localized.
        /// The final string will be created using String.Format where format item 0 contains the original string.
        /// </summary>
        public string NoTranslationFoundFormat
        {
            get { return m_NoTranslationFoundFormat; }
            set { m_NoTranslationFoundFormat = value; }
        }

        public IAsyncOperation PreloadOperation
        {
            get
            {
                if (m_PreloadOperation == null)
                {
                    m_PreloadOperation = Addressables.LoadAssets<StringTableBase>(new object[] { StringTableLabel, LocalizationSettings.SelectedLocale.Identifier.Code }, TableLoaded, Addressables.MergeMode.Intersection);
                }
                return m_PreloadOperation;
            }
        }

        /// <summary>
        /// Called for each table, as it is loaded during a preload operation.
        /// </summary>
        /// <param name="asyncOperation"></param>
        void TableLoaded(IAsyncOperation<StringTableBase> asyncOperation)
        {
            if (!asyncOperation.HasLoadedSuccessfully())
                return;
            Debug.AssertFormat(!m_Tables.ContainsKey(asyncOperation.Result.TableName), "A string table with the same key `{0}` already exists. Something went wrong during preloading.", asyncOperation.Result.TableName);
            asyncOperation.Retain();
            m_Tables[asyncOperation.Result.TableName] = asyncOperation;
        }

        /// <summary>
        /// Returns the named table.
        /// This function is asynchronous and may not have an immediate result.
        /// Check IsDone to see if the data is available, if it is false then use the Completed event or yield on the operation.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public virtual IAsyncOperation<StringTableBase> GetTable(string tableName)
        {
            IAsyncOperation<StringTableBase> asyncOp;
            if (m_Tables.TryGetValue(tableName, out asyncOp))
            {
                return asyncOp;
            }

            var tableAddress = string.Format("{0} - {1}", LocalizationSettings.SelectedLocale.Identifier.Code, tableName);
            asyncOp = Addressables.LoadAsset<StringTableBase>(tableAddress);
            m_Tables[tableName] = asyncOp;
            return asyncOp;
        }

        /// <summary>
        /// Attempts to retrieve a string from a StringTable.
        /// This function is asynchronous and may not have an immediate result.
        /// Check IsDone to see if the data is available, if it is false then use the Completed event or yield on the operation.
        /// </summary>
        /// <param name="tableName">The name of the string table to look for the translated text in.</param>
        /// <param name="original">The original text or key that should be used to find the translated text.</param>
        /// <returns></returns>
        public virtual IAsyncOperation<string> GetLocalizedString(string tableName, string original)
        {
            if (string.IsNullOrEmpty(tableName))
                return new CompletedOperation<string>().Start("tableName can not be empty or null.", original, null);

            if (string.IsNullOrEmpty(original))
                return new CompletedOperation<string>().Start("original can not be empty or null.", original, null);

            var initOp = LocalizationSettings.InitializationOperation;
            if (!initOp.IsDone)
                return AsyncOperationCache.Instance.Acquire<ChainOperation<string, LocalizationSettings>>().Start(null, original, initOp, (op) => GetLocalizedString_LoadTable(tableName, original));
            return GetLocalizedString_LoadTable(tableName, original);
        }

        /// <summary>
        /// Attempts to retrieve a string from the default StringTable.
        /// This function is asynchronous and may not have an immediate result.
        /// Check IsDone to see if the data is available, if it is false then use the Completed event or yield on the operation.
        /// </summary>
        /// <param name="original">The original text or key that should be used to find the translated text.</param>
        /// <returns></returns>
        public virtual IAsyncOperation<string> GetLocalizedString(string original)
        {
            if (m_DefaultTable == null)
            {
                if (string.IsNullOrEmpty(DefaultTableName))
                {
                    Debug.LogWarning("DefaultTableName is empty.");
                    return new CompletedOperation<string>().Start(null, original, null);
                }

                m_DefaultTable = GetTable(DefaultTableName);
            }

            if (!m_DefaultTable.IsDone)
                return AsyncOperationCache.Instance.Acquire<ChainOperation<string, StringTableBase>>().Start(null, original, m_DefaultTable, (op) => GetLocalizedString_FindString(m_DefaultTable, original));
            return GetLocalizedString_FindString(m_DefaultTable, original);
        }

        /// <summary>
        /// Attempts to retrieve a plural string from the table.
        /// This function is asynchronous and may not have an immediate result.
        /// Check IsDone to see if the data is available, if it is false then use the Completed event or yield on the operation.
        /// </summary>
        /// <param name="tableName">Table to search for the original string.</param>
        /// <param name="original">Original string or key to find the localized string.</param>
        /// <param name="n">Plural value</param>
        /// <returns></returns>
        public virtual IAsyncOperation<string> GetLocalizedString(string tableName, string original, int n)
        {
            if (string.IsNullOrEmpty(tableName))
                return new CompletedOperation<string>().Start("tableName can not be empty or null.", original, null);

            if (string.IsNullOrEmpty(original))
                return new CompletedOperation<string>().Start("original can not be empty or null.", original, null);

            var initOp = LocalizationSettings.InitializationOperation;
            if (!initOp.IsDone)
                return AsyncOperationCache.Instance.Acquire<ChainOperation<string, LocalizationSettings>>().Start(null, original, initOp, (op) => GetLocalizedString_LoadTable(tableName, original, n));
            return GetLocalizedString_LoadTable(tableName, original, n);
        }

        /// <summary>
        /// Attempts to retrieve a plural string from the default StringTable.
        /// This function is asynchronous and may not have an immediate result.
        /// Check IsDone to see if the data is available, if it is false then use the Completed event or yield on the operation.
        /// </summary>
        /// <param name="original">Original string or key to find the localized string.</param>
        /// <param name="n">Plural value</param>
        /// <returns></returns>
        public virtual IAsyncOperation<string> GetLocalizedString(string original, int n)
        {
            if (m_DefaultTable == null)
                m_DefaultTable = GetTable(DefaultTableName);

            if (!m_DefaultTable.IsDone)
                return AsyncOperationCache.Instance.Acquire<ChainOperation<string, StringTableBase>>().Start(null, original, m_DefaultTable, (op) => GetLocalizedString_FindString(m_DefaultTable, original, n));
            return GetLocalizedString_FindString(m_DefaultTable, original, n);
        }

        IAsyncOperation<string> GetLocalizedString_LoadTable(string tableName, string original)
        {
            var tableOp = GetTable(tableName);
            if (!tableOp.IsDone)
                return AsyncOperationCache.Instance.Acquire<ChainOperation<string, StringTableBase>>().Start(null, original, tableOp, (op) => GetLocalizedString_FindString(tableOp, original));
            return GetLocalizedString_FindString(tableOp, original);
        }

        IAsyncOperation<string> GetLocalizedString_LoadTable(string tableName, string original, int n)
        {
            var tableOp = GetTable(tableName);
            if (!tableOp.IsDone)
                return AsyncOperationCache.Instance.Acquire<ChainOperation<string, StringTableBase>>().Start(null, original, tableOp, (op) => GetLocalizedString_FindString(tableOp, original, n));
            return GetLocalizedString_FindString(tableOp, original, n);
        }

        IAsyncOperation<string> GetLocalizedString_FindString(IAsyncOperation<StringTableBase> table, string original)
        {
            if (table.HasLoadedSuccessfully())
            {
                var translatedText = table.Result.GetLocalizedString(original);
                if (string.IsNullOrEmpty(translatedText))
                {
                    // TODO: Fallback table support if null
                    translatedText = ProcessUntranslatedText(original);
                }

                return AsyncOperationCache.Instance.Acquire<CompletedOperation<string>>().Start(table.Context, original, translatedText);
            }
            return new CompletedOperation<string>().Start(table.Context, table.Key, null, table.OperationException);
        }

        IAsyncOperation<string> GetLocalizedString_FindString(IAsyncOperation<StringTableBase> table, string original, int n)
        {
            if (table.HasLoadedSuccessfully())
            {
                var translatedText = table.Result.GetLocalizedPluralString(original, n);
                if (string.IsNullOrEmpty(translatedText))
                {
                    // TODO: Fallback table support if null
                    translatedText = ProcessUntranslatedText(translatedText);
                }

                return AsyncOperationCache.Instance.Acquire<CompletedOperation<string>>().Start(table.Context, original, translatedText);
            }
            return new CompletedOperation<string>().Start(table.Context, table.Key, null, table.OperationException);
        }

        protected virtual string ProcessUntranslatedText(string original)
        {
            return string.IsNullOrEmpty(NoTranslationFoundFormat) ? original : string.Format(NoTranslationFoundFormat, original);
        }

        /// <inheritdoc />
        public override void OnLocaleChanged(Locale locale)
        {
            foreach (var asyncOperation in m_Tables)
            {
                // Free up the asset and the IAsyncOperation
                Addressables.ReleaseAsset(asyncOperation.Value.Result);
                asyncOperation.Value.Release();
            }

            m_Tables.Clear();
            m_DefaultTable = null;
        }
    }
}