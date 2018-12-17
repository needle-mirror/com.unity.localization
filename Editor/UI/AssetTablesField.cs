using System.Collections.Generic;
using System.Linq;
using UnityEditor.AddressableAssets;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Localization;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.Localization.UI
{
    /// <summary>
    /// Displays all the asset tables for the project collated by type.
    /// </summary>
    sealed class AssetTablesField : PopupField<AssetTableCollection>
    {
        const string k_EditorPrefValueKey = "Localization-SelectedAssetTable";
        const string k_NoTablesMessage = "No Asset Tables Found. Please Create One";

        internal class NoTables : AssetTableCollection
        {
            public override string ToString()
            {
                return null;
            }
        }

        public new class UxmlFactory : UxmlFactory<AssetTablesField> {}

        static List<AssetTableCollection> s_Tables;

        public AssetTablesField()
            : base(GetChoices(), GetDefaultIndex())
        {
            formatSelectedValueCallback = FormatSelectedLabel;
            formatListItemCallback = FormatListLabel;

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings != null)
            {
                settings.OnModification += AddressableSettingsModification;
            }
        }

        static int GetDefaultIndex()
        {
            var selection = EditorPrefs.GetString(k_EditorPrefValueKey, null);
            if (!string.IsNullOrEmpty(selection))
            {
                for (int i = 0; i < s_Tables.Count; ++i)
                {
                    if (s_Tables[i].ToString() == selection)
                        return i;
                }
            }

            return 0;
        }

        public override AssetTableCollection value
        {
            get { return base.value; }
            set
            {
                if (value == null)
                    EditorPrefs.DeleteKey(k_EditorPrefValueKey);
                else
                    EditorPrefs.SetString(k_EditorPrefValueKey, value.ToString());
                base.value = value; 
            }
        }

        static void AddressableSettingsModification(AddressableAssetSettings arg1, AddressableAssetSettings.ModificationEvent arg2, object arg3)
        {
            if (arg2 == AddressableAssetSettings.ModificationEvent.EntryAdded ||
                arg2 == AddressableAssetSettings.ModificationEvent.EntryModified ||
                arg2 == AddressableAssetSettings.ModificationEvent.EntryRemoved)
            {
                // Refresh choices
                GetChoices();
            }
        }

        /// <summary>
        /// Searches for the selectedTable in the AssetTableCollection list, if found it selects this collection and sends the value changeds event.
        /// </summary>
        /// <param name="selectedTable">Table to search for.</param>
        public void SetValueFromTable(LocalizedTable selectedTable)
        {
            var choices = GetChoices();
            foreach (var assetTableCollection in choices)
            {
                if (assetTableCollection.TableType == selectedTable.GetType() && assetTableCollection.TableName == selectedTable.TableName)
                {
                    value = assetTableCollection;
                    return;
                }
            }
        }

        static string FormatListLabel(AssetTableCollection atc)
        {
            return atc == null || atc is NoTables ? k_NoTablesMessage : ObjectNames.NicifyVariableName(atc.TableType.Name) + "/" + atc.TableName;
        }

        static string FormatSelectedLabel(AssetTableCollection atc)
        {
            return atc == null || atc is NoTables ? k_NoTablesMessage : atc.TableName;
        }

        static List<AssetTableCollection> GetChoices()
        {
            if(s_Tables == null)
                s_Tables = new List<AssetTableCollection>();

            s_Tables.Clear();
            var choices = LocalizationPlayerSettings.GetAssetTables<LocalizedTable>();
            if (choices.Count == 0)
                choices.Add(new NoTables());
            s_Tables.AddRange(choices);
            return s_Tables;
        }
    }
}
