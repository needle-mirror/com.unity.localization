using System.Collections.Generic;
using UnityEngine.Localization;

#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#else
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#endif

namespace UnityEditor.Localization.UI
{
    /// <summary>
    /// Displays all the asset tables for the project collated by type.
    /// </summary>
    class AssetTablesField : PopupField<AssetTableCollection>
    {
        const string k_EditorPrefValueKey = "Localization-SelectedAssetTable";
        const string k_NoTablesMessage = "No Asset Tables Found. Please Create One";

        internal class NoTables : AssetTableCollection
        {
            public override string ToString()
            {
                return null;
            }

            public override string TableName => null;
        }

        public new class UxmlFactory : UxmlFactory<AssetTablesField> {}

        static List<AssetTableCollection> s_Tables;

        public AssetTablesField()
            : base(GetChoices(), GetDefaultIndex())
        {
            formatSelectedValueCallback = FormatSelectedLabel;
            formatListItemCallback = FormatListLabel;

            LocalizationEditorSettings.OnModification += LocalizationSettingsModification;
        }

        ~AssetTablesField() => LocalizationEditorSettings.OnModification -= LocalizationSettingsModification;

        void LocalizationSettingsModification(LocalizationEditorSettings.ModificationEvent evt, object obj)
        {
            if (evt == LocalizationEditorSettings.ModificationEvent.TableAdded)
            {
                GetChoices();
            }
            else if (evt == LocalizationEditorSettings.ModificationEvent.TableRemoved)
            {
                var choices = GetChoices();
                var table = (LocalizedTable)obj;

                if (value.Tables.Contains(table))
                {
                    // Find the new collection
                    var newValue = choices.Find(o => o.Tables.Contains(table));
                    value = newValue ?? choices[0];
                }
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
            get => base.value;
            set
            {
                if (value == null)
                    EditorPrefs.DeleteKey(k_EditorPrefValueKey);
                else
                    EditorPrefs.SetString(k_EditorPrefValueKey, value.ToString());
                base.value = value; 
            }
        }

        public void RefreshLabels()
        {
            GetChoices();
            SetValueWithoutNotify(value);
        }

        /// <summary>
        /// Searches for the selectedTable in the AssetTableCollection list, if found it selects this collection and sends the value changed event.
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
            var choices = LocalizationEditorSettings.GetAssetTablesCollection<LocalizedTable>();
            if (choices.Count == 0)
                choices.Add(new NoTables());
            s_Tables.AddRange(choices);
            return s_Tables;
        }
    }
}
