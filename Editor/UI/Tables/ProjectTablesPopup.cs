using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.Localization.Tables;
using UnityEngine.UIElements;

namespace UnityEditor.Localization.UI
{
    /// <summary>
    /// Displays all the asset tables for the project collated by type.
    /// </summary>
    class ProjectTablesPopup : PopupField<AssetTableCollection>
    {
        const string k_EditorPrefValueKey = "Localization-SelectedAssetTable";
        const string k_NoTablesMessage = "No Asset Tables Found. Please Create One";

        class NoTables : AssetTableCollection
        {
            public override string ToString()
            {
                return k_NoTablesMessage;
            }
        }

        public new class UxmlFactory : UxmlFactory<ProjectTablesPopup> {}

        static List<AssetTableCollection> s_Tables;

        public ProjectTablesPopup()
            : base(GetChoices(), GetDefaultIndex())
        {
            label = "Selected Table";
            formatSelectedValueCallback = FormatSelectedLabel;
            formatListItemCallback = FormatListLabel;

            LocalizationEditorSettings.OnModification += LocalizationSettingsModification;
        }

        ~ProjectTablesPopup() => LocalizationEditorSettings.OnModification -= LocalizationSettingsModification;

        void LocalizationSettingsModification(LocalizationEditorSettings.ModificationEvent evt, object obj)
        {
            if (evt == LocalizationEditorSettings.ModificationEvent.TableAdded)
            {
                GetChoices();
                SetValueFromTable((LocalizedTable)obj);
            }
            else if (evt == LocalizationEditorSettings.ModificationEvent.TableRemoved)
            {
                var choices = GetChoices();

                // Check if the table is currently selected.
                var table = (LocalizedTable)obj;
                if (value.TableType == table.GetType() && value.TableName == table.TableName)
                {
                    value = choices[0];
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
            var newValue = s_Tables.FindIndex(o => value.Equals(o));
            SetValueWithoutNotify(s_Tables[newValue > 0 ? newValue : 0]);
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
            return atc is NoTables ? atc.ToString() : ObjectNames.NicifyVariableName(atc.TableType.Name) + "/" + atc.TableName;
        }

        static string FormatSelectedLabel(AssetTableCollection atc) => atc.ToString();

        static List<AssetTableCollection> GetChoices()
        {
            if (s_Tables == null)
                s_Tables = new List<AssetTableCollection>();
            s_Tables.Clear();

            s_Tables.AddRange(LocalizationEditorSettings.GetAssetTablesCollection<StringTable>());
            s_Tables.AddRange(LocalizationEditorSettings.GetAssetTablesCollection<AssetTable>());

            if (s_Tables.Count == 0)
                s_Tables.Add(new NoTables());
            return s_Tables;
        }
    }
}
