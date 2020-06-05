using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Localization.UI
{
    /// <summary>
    /// Displays all the asset tables for the project collated by type.
    /// </summary>
    class ProjectTablesPopup : PopupField<LocalizedTableCollection>
    {
        class NoTables : LocalizedTableCollection
        {
            public override string DefaultTableGroupName => null;
            protected internal override Type TableType => null;
            protected internal override Type RequiredExtensionAttribute => null;
            public override string ToString() => k_NoTablesMessage;
        }

        const string k_EditorPrefValueKey = "Localization-SelectedAssetTable";
        const string k_NoTablesMessage = "No Asset Tables Found. Please Create One";

        public new class UxmlFactory : UxmlFactory<ProjectTablesPopup> {}

        static readonly NoTables k_NoTables = NoTables.CreateInstance<NoTables>();
        static List<LocalizedTableCollection> s_Tables;

        public ProjectTablesPopup()
            : base(GetChoices(), GetDefaultIndex())
        {
            label = "Selected Table Collection";
            formatSelectedValueCallback = FormatSelectedLabel;
            formatListItemCallback = FormatListLabel;

            LocalizationEditorSettings.EditorEvents.CollectionAdded += OnCollectionAdded;
            LocalizationEditorSettings.EditorEvents.CollectionRemoved += OnCollectionRemoved;
        }

        void OnCollectionAdded(LocalizedTableCollection col)
        {
            bool isEmpty = value is NoTables;
            GetChoices();

            // If we currently have no tables then select the new collection.
            if (isEmpty)
                value = col;
        }

        void OnCollectionRemoved(LocalizedTableCollection col)
        {
            var choices = GetChoices();

            if (value == col)
                value = choices[0];
        }

        ~ProjectTablesPopup()
        {
            LocalizationEditorSettings.EditorEvents.CollectionAdded -= OnCollectionAdded;
            LocalizationEditorSettings.EditorEvents.CollectionRemoved -= OnCollectionRemoved;
        }

        static int GetDefaultIndex()
        {
            var selection = EditorPrefs.GetString(k_EditorPrefValueKey, null);
            if (!string.IsNullOrEmpty(selection))
            {
                for (int i = 0; i < s_Tables.Count; ++i)
                {
                    if (s_Tables[i]?.ToString() == selection)
                        return i;
                }
            }

            return 0;
        }

        public override LocalizedTableCollection value
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
            var newValue = Mathf.Clamp(s_Tables.FindIndex(o => value.Equals(o)), 0, s_Tables.Count);
            SetValueWithoutNotify(s_Tables[newValue]);
        }

        static string FormatListLabel(LocalizedTableCollection atc)
        {
            return atc is NoTables ? atc.ToString() : ObjectNames.NicifyVariableName(atc.TableType.Name) + "/" + atc.TableCollectionName;
        }

        static string FormatSelectedLabel(LocalizedTableCollection atc) => atc.ToString();

        static List<LocalizedTableCollection> GetChoices()
        {
            if (s_Tables == null)
                s_Tables = new List<LocalizedTableCollection>();
            s_Tables.Clear();

            s_Tables.AddRange(LocalizationEditorSettings.Instance.TableCollectionCache.StringTableCollections);
            s_Tables.AddRange(LocalizationEditorSettings.Instance.TableCollectionCache.AssetTableCollections);

            if (s_Tables.Count == 0)
                s_Tables.Add(k_NoTables);
            return s_Tables;
        }
    }
}
