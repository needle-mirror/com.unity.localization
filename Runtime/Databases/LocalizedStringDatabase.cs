using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization
{
    /// <summary>
    /// Handles loading strings and their tables for the selected locale.
    /// </summary>
    public class LocalizedStringDatabase : LocalizedDatabase, IPreloadRequired
    {
        public const string StringTableLabel = "StringTable";

        Dictionary<string, AsyncOperationHandle<StringTableBase>> m_Tables = new Dictionary<string, AsyncOperationHandle<StringTableBase>>();

        AsyncOperationHandle<StringTableBase>? m_DefaultTable;

        AsyncOperationHandle? m_PreloadOperation;

        [SerializeField]
        string m_NoTranslationFoundFormat = "No translation found for '{0}'";

        /// <summary>
        /// The message to display when a string can not be localized.
        /// The final string will be created using String.Format where format item 0 contains the original string.
        /// </summary>
        public string NoTranslationFoundFormat
        {
            get => m_NoTranslationFoundFormat;
            set => m_NoTranslationFoundFormat = value;
        }

        public AsyncOperationHandle PreloadOperation
        {
            get
            {
                if (m_PreloadOperation == null)
                {
                    m_PreloadOperation = Addressables.LoadAssetsAsync<StringTableBase>(new object[] { StringTableLabel, LocalizationSettings.SelectedLocale.Identifier.Code }, TableLoaded, Addressables.MergeMode.Intersection);
                }
                return m_PreloadOperation.Value;
            }
        }

        /// <summary>
        /// Called for each table, as it is loaded during a preload operation.
        /// </summary>
        /// <param name="asyncOperation"></param>
        void TableLoaded(StringTableBase table)
        {
            Debug.AssertFormat(!m_Tables.ContainsKey(table.TableName), "A string table with the same key `{0}` already exists. Something went wrong during preloading.", table.TableName);
            m_Tables[table.TableName] = LocalizationSettings.ResourceManager.CreateCompletedOperation(table, null);
        }

        /// <summary>
        /// Returns the named table.
        /// This function is asynchronous and may not have an immediate result.
        /// Check IsDone to see if the data is available, if it is false then use the Completed event or yield on the operation.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public virtual AsyncOperationHandle<StringTableBase> GetTable(string tableName)
        {
            if (m_Tables.TryGetValue(tableName, out var asyncOp))
            {
                return asyncOp;
            }

            var tableAddress = $"{LocalizationSettings.SelectedLocale.Identifier.Code} - {tableName}";
            asyncOp = Addressables.LoadAssetAsync<StringTableBase>(tableAddress);
            m_Tables[tableName] = asyncOp;
            return asyncOp;
        }

        /// <summary>
        /// Attempts to retrieve a string from a StringTable.
        /// This function is asynchronous and may not have an immediate result.
        /// Check IsDone to see if the data is available, if it is false then use the Completed event or yield on the operation.
        /// </summary>
        /// <param name="tableName">The name of the string table to look for the translated text in.</param>
        /// <param name="keyId">The Id of the key, taken from the KeyDatabase, that should be used to find the translated text.</param>
        /// <returns></returns>
        public virtual AsyncOperationHandle<string> GetLocalizedString(string tableName, uint keyId)
        {
            if (string.IsNullOrEmpty(tableName))
                return LocalizationSettings.ResourceManager.CreateCompletedOperation<string>(null, nameof(tableName) + " can not be empty or null");

            if (keyId == KeyDatabase.EmptyId)
                return LocalizationSettings.ResourceManager.CreateCompletedOperation<string>(null, nameof(keyId) + " can not be empty.");

            var initOp = LocalizationSettings.InitializationOperation;
            if (!initOp.Value.IsDone)
                return LocalizationSettings.ResourceManager.CreateChainOperation(initOp.Value, (op) => GetLocalizedString_LoadTable(tableName, keyId));

            return GetLocalizedString_LoadTable(tableName, keyId);
        }

        public virtual AsyncOperationHandle<string> GetLocalizedString(string tableName, string key)
        {
            if (string.IsNullOrEmpty(tableName))
                return LocalizationSettings.ResourceManager.CreateCompletedOperation<string>(null, nameof(tableName) + " can not be empty or null");

            if (string.IsNullOrEmpty(key))
                return LocalizationSettings.ResourceManager.CreateCompletedOperation<string>(null, nameof(key) + " can not be empty or null.");

            var initOp = LocalizationSettings.InitializationOperation;
            if (!initOp.Value.IsDone)
                return LocalizationSettings.ResourceManager.CreateChainOperation(initOp.Value, (op) => GetLocalizedString_LoadTable(tableName, key));

            return GetLocalizedString_LoadTable(tableName, key);
        }

        /// <summary>
        /// Attempts to retrieve a string from the default StringTable.
        /// This function is asynchronous and may not have an immediate result.
        /// Check IsDone to see if the data is available, if it is false then use the Completed event or yield on the operation.
        /// </summary>
        /// <param name="keyId">The Id of the key, taken from the KeyDatabase, that should be used to find the translated text.</param>
        /// <returns></returns>
        public virtual AsyncOperationHandle<string> GetLocalizedString(uint keyId)
        {
            if (m_DefaultTable == null)
            {
                if (string.IsNullOrEmpty(DefaultTableName))
                {
                    Debug.LogWarning("DefaultTableName is empty.");
                    return LocalizationSettings.ResourceManager.CreateCompletedOperation<string>(null, nameof(DefaultTableName) + " is empty. Either specify a default table name in the StringDatabase or use the version of GetLocalizedString that includes a table name.");
                }

                m_DefaultTable = GetTable(DefaultTableName);
            }

            if (!m_DefaultTable.Value.IsDone)
                LocalizationSettings.ResourceManager.CreateChainOperation(m_DefaultTable.Value, (op) => GetLocalizedString_FindString(m_DefaultTable.Value, keyId));
            return GetLocalizedString_FindString(m_DefaultTable.Value, keyId);
        }

        /// <summary>
        /// Attempts to retrieve a plural string from the table.
        /// This function is asynchronous and may not have an immediate result.
        /// Check IsDone to see if the data is available, if it is false then use the Completed event or yield on the operation.
        /// </summary>
        /// <param name="tableName">Table to search for the original string.</param>
        /// <param name="keyId">The Id of the key, taken from the KeyDatabase, that should be used to find the translated text.</param>
        /// <param name="n">Plural value</param>
        /// <returns></returns>
        public virtual AsyncOperationHandle<string> GetLocalizedString(string tableName, uint keyId, int n)
        {
            if (string.IsNullOrEmpty(tableName))
                return LocalizationSettings.ResourceManager.CreateCompletedOperation<string>(null, nameof(tableName) + " can not be empty or null");

            if (keyId == KeyDatabase.EmptyId)
                return LocalizationSettings.ResourceManager.CreateCompletedOperation<string>(null, nameof(keyId) + " can not be empty.");

            var initOp = LocalizationSettings.InitializationOperation;
            if (!initOp.Value.IsDone)
                LocalizationSettings.ResourceManager.CreateChainOperation(initOp.Value, (op) => GetLocalizedString_LoadTable(tableName, keyId, n));
            return GetLocalizedString_LoadTable(tableName, keyId, n);
        }

        /// <summary>
        /// Attempts to retrieve a plural string from the table.
        /// This function is asynchronous and may not have an immediate result.
        /// Check IsDone to see if the data is available, if it is false then use the Completed event or yield on the operation.
        /// </summary>
        /// <param name="tableName">Table to search for the original string.</param>
        /// <param name="key">The string key taken from the KeyDatabase, that should be used to find the translated text.</param>
        /// <param name="n">Plural value</param>
        /// <returns></returns>
        public virtual AsyncOperationHandle<string> GetLocalizedString(string tableName, string key, int n)
        {
            if (string.IsNullOrEmpty(tableName))
                return LocalizationSettings.ResourceManager.CreateCompletedOperation<string>(null, nameof(tableName) + " can not be empty or null");

            if (string.IsNullOrEmpty(key))
                return LocalizationSettings.ResourceManager.CreateCompletedOperation<string>(null, nameof(key) + " can not be empty or null.");

            var initOp = LocalizationSettings.InitializationOperation;
            if (!initOp.Value.IsDone)
                LocalizationSettings.ResourceManager.CreateChainOperation(initOp.Value, (op) => GetLocalizedString_LoadTable(tableName, key, n));
            return GetLocalizedString_LoadTable(tableName, key, n);
        }

        /// <summary>
        /// Attempts to retrieve a plural string from the default StringTable.
        /// This function is asynchronous and may not have an immediate result.
        /// Check IsDone to see if the data is available, if it is false then use the Completed event or yield on the operation.
        /// </summary>
        /// <param name="keyId">The Id of the key, taken from the KeyDatabase, that should be used to find the translated text.</param>
        /// <param name="n">Plural value</param>
        /// <returns></returns>
        public virtual AsyncOperationHandle<string> GetLocalizedString(uint keyId, int n)
        {
            if (m_DefaultTable == null)
                m_DefaultTable = GetTable(DefaultTableName);

            if (!m_DefaultTable.Value.IsDone)
                LocalizationSettings.ResourceManager.CreateChainOperation(m_DefaultTable.Value, (op) => GetLocalizedString_FindString(m_DefaultTable.Value, keyId, n));
            return GetLocalizedString_FindString(m_DefaultTable.Value, keyId, n);
        }

        AsyncOperationHandle<string> GetLocalizedString_LoadTable(string tableName, uint keyId)
        {
            var tableOp = GetTable(tableName);
            if (!tableOp.IsDone)
                return LocalizationSettings.ResourceManager.CreateChainOperation(tableOp, (op) => GetLocalizedString_FindString(tableOp, keyId));
            return GetLocalizedString_FindString(tableOp, keyId);
        }

        AsyncOperationHandle<string> GetLocalizedString_LoadTable(string tableName, string key)
        {
            var tableOp = GetTable(tableName);
            if (!tableOp.IsDone)
                return LocalizationSettings.ResourceManager.CreateChainOperation(tableOp, (op) => GetLocalizedString_FindString(tableOp, key));
            return GetLocalizedString_FindString(tableOp, key);
        }

        AsyncOperationHandle<string> GetLocalizedString_LoadTable(string tableName, uint keyId, int n)
        {
            var tableOp = GetTable(tableName);
            if (!tableOp.IsDone)
                return LocalizationSettings.ResourceManager.CreateChainOperation(tableOp, (op) => GetLocalizedString_FindString(tableOp, keyId, n));
            return GetLocalizedString_FindString(tableOp, keyId, n);
        }

        AsyncOperationHandle<string> GetLocalizedString_LoadTable(string tableName, string key, int n)
        {
            var tableOp = GetTable(tableName);
            if (!tableOp.IsDone)
                return LocalizationSettings.ResourceManager.CreateChainOperation(tableOp, (op) => GetLocalizedString_FindString(tableOp, key, n));
            return GetLocalizedString_FindString(tableOp, key, n);
        }

        AsyncOperationHandle<string> GetLocalizedString_FindString(AsyncOperationHandle<StringTableBase> tableOperation, uint keyId)
        {
            if (tableOperation.Status != AsyncOperationStatus.Succeeded)
            {
                var error = "Failed to load table: " + tableOperation.DebugName;
                Debug.LogError(error);
                if (tableOperation.OperationException != null)
                {
                    error += "\n" + tableOperation.OperationException;
                    Debug.LogException(tableOperation.OperationException);
                }

                return LocalizationSettings.ResourceManager.CreateCompletedOperation<string>(null, error);
            }
            
            var translatedText = tableOperation.Result.GetLocalizedString(keyId);
            if (string.IsNullOrEmpty(translatedText))
            {
                // TODO: Fallback table support if null

                var key = tableOperation.Result.Keys.GetKey(keyId);

                if (key == null)
                    Debug.LogWarning("No key could be found with the Id " + keyId, tableOperation.Result.Keys);

                translatedText = ProcessUntranslatedText(key);
            }

            return LocalizationSettings.ResourceManager.CreateCompletedOperation(translatedText, null);
        }

        AsyncOperationHandle<string> GetLocalizedString_FindString(AsyncOperationHandle<StringTableBase> tableOperation, string key)
        {
            if (tableOperation.Status != AsyncOperationStatus.Succeeded)
            {
                var error = "Failed to load table: " + tableOperation.DebugName;
                Debug.LogError(error);
                if (tableOperation.OperationException != null)
                {
                    error += "\n" + tableOperation.OperationException;
                    Debug.LogException(tableOperation.OperationException);
                }

                return LocalizationSettings.ResourceManager.CreateCompletedOperation<string>(null, error);
            }

            var translatedText = tableOperation.Result.GetLocalizedString(key);
            if (string.IsNullOrEmpty(translatedText))
            {
                // TODO: Fallback table support if null
                translatedText = ProcessUntranslatedText(key);
            }

            return LocalizationSettings.ResourceManager.CreateCompletedOperation(translatedText, null);
        }

        AsyncOperationHandle<string> GetLocalizedString_FindString(AsyncOperationHandle<StringTableBase> tableOperation, uint keyId, int n)
        {
            if (tableOperation.Status != AsyncOperationStatus.Succeeded)
            {
                var error = "Failed to load table: " + tableOperation.DebugName;
                Debug.LogError(error);
                if (tableOperation.OperationException != null)
                {
                    error += "\n" + tableOperation.OperationException;
                    Debug.LogException(tableOperation.OperationException);
                }
                return LocalizationSettings.ResourceManager.CreateCompletedOperation<string>(null, error);
            }

            var translatedText = tableOperation.Result.GetLocalizedPluralString(keyId, n);
            if (string.IsNullOrEmpty(translatedText))
            {
                // TODO: Fallback table support if null
                translatedText = ProcessUntranslatedText(translatedText);
            }

            return LocalizationSettings.ResourceManager.CreateCompletedOperation(translatedText, null);
        }

        AsyncOperationHandle<string> GetLocalizedString_FindString(AsyncOperationHandle<StringTableBase> tableOperation, string key, int n)
        {
            if (tableOperation.Status != AsyncOperationStatus.Succeeded)
            {
                var error = "Failed to load table: " + tableOperation.DebugName;
                Debug.LogError(error);
                if (tableOperation.OperationException != null)
                {
                    error += "\n" + tableOperation.OperationException;
                    Debug.LogException(tableOperation.OperationException);
                }
                return LocalizationSettings.ResourceManager.CreateCompletedOperation<string>(null, error);
            }

            var translatedText = tableOperation.Result.GetLocalizedPluralString(key, n);
            if (string.IsNullOrEmpty(translatedText))
            {
                // TODO: Fallback table support if null
                translatedText = ProcessUntranslatedText(translatedText);
            }

            return LocalizationSettings.ResourceManager.CreateCompletedOperation(translatedText, null);
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
                Addressables.Release(asyncOperation.Value.Result);
            }

            m_Tables.Clear();
            m_DefaultTable = null;
        }
    }
}