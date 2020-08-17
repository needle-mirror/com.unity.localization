using System;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace UnityEngine.Localization
{
    /// <summary>
    /// Provides runtime access to a <see cref="StringTable"/> for the current selected <see cref="Locale"/>.
    /// When accessing multiple localized strings it may be more convenient to use a <see cref="LocalizedStringTable"/> instead of multiple <see cref="LocalizedString"/>.
    /// This will fetch the table on demand or provide a callback whenever the table has finished loading, such as when the selected locale was changed.
    /// </summary>
    /// <example>
    /// This example shows how a <see cref="StringTable"/> can be used directly in order to get translated strings for multiple entries
    /// <code>
    /// public class LocalizedStringTableExample : MonoBehaviour
    /// {
    ///     public LocalizedStringTable m_StringTable = new LocalizedStringTable{ TableReference = "My Strings" };
    ///
    ///     string m_TranslatedStringHello;
    ///     string m_TranslatedStringGoodbye;
    ///     string m_TranslatedStringThisIsATest;
    ///
    ///     void OnEnable()
    ///     {
    ///         m_StringTable.RegisterChangeHandler(LoadStrings);
    ///     }
    ///
    ///     void OnDisable()
    ///     {
    ///         m_StringTable.ClearChangeHandler();
    ///     }
    ///
    ///     void LoadStrings(StringTable stringTable)
    ///     {
    ///         m_TranslatedStringHello = GetLocalizedString(stringTable, "Hello");
    ///         m_TranslatedStringGoodbye = GetLocalizedString(stringTable, "Goodbye");
    ///         m_TranslatedStringThisIsATest = GetLocalizedString(stringTable, "This is a test");
    ///     }
    ///
    ///     static string GetLocalizedString(StringTable table, string entryName)
    ///     {
    ///         var entry = table.GetEntry(entryName);
    ///         return entry.GetLocalizedString(); // We can pass in optional arguments for Smart Format or String.Format here.
    ///     }
    ///
    ///     void OnGUI()
    ///     {
    ///         if (!LocalizationSettings.InitializationOperation.IsDone)
    ///         {
    ///             GUILayout.Label("Initializing Localization");
    ///             return;
    ///         }
    ///
    ///         GUILayout.Label(m_TranslatedStringThisIsATest);
    ///         GUILayout.Label(m_TranslatedStringHello);
    ///         GUILayout.Label(m_TranslatedStringGoodbye);
    ///     }
    /// }
    /// </code>
    /// </example>
    [Serializable]
    public class LocalizedStringTable : LocalizedTable<StringTable, StringTableEntry>
    {
        protected override LocalizedDatabase<StringTable, StringTableEntry> Database => LocalizationSettings.StringDatabase;
    }
}
