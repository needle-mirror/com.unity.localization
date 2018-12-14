using UnityEditor.AddressableAssets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using Object = UnityEngine.Object;

namespace UnityEditor.Localization
{
    public static class LocalizationAddressableSettings
    {
        public const string LocaleGroupName = "Localization-Locales";
        public const string AssetTableTypeGroupName = "Localization-Assets-{0}";
        
        public const string AssetTableGroupName = "Localization-AssetTables";
        public const string StringTableGroupName = "Localization-StringTables";

        static AddressableAssetGroup GetGroup(AddressableAssetSettings settings, string groupName, bool create = false)
        {
            var group = settings.FindGroup(groupName);
            if (group == null && create)
            {
                group = settings.CreateGroup(groupName, false, false, true);
            }

            return group;
        }

        static string FindUniqueAssetAddress(string address)
        {
            var aaSettings = AddressableAssetSettingsDefaultObject.Settings;
            if (aaSettings == null)
                return address;

            var validAddress = address;
            int index = 1;
            bool foundExisting = true;
            while (foundExisting)
            {
                if (index > 1000)
                {
                    Debug.LogError("Unable to create valid address for new Addressable Asset.");
                    return address;
                }
                foundExisting = false;
                foreach (var g in aaSettings.groups)
                {
                    if (g.Name == validAddress)
                    {
                        foundExisting = true;
                        validAddress = address + index.ToString();
                        index++;
                        break;
                    }
                }
            }

            return validAddress;
        }

        /// <summary>
        /// Add the Locale to the Addressables system, so that it can be used by the Localization system during runtime.
        /// </summary>
        /// <param name="locale"></param>
        /// <returns>True if the locale was added; false if it was not such as when the locale is already added or the 
        /// operation was canceled when creating the settings asset.</returns>
        public static bool AddLocale(Locale locale)
        {
            if (!EditorUtility.IsPersistent(locale))
            {
                Debug.LogError("Only persistent assets can be addressable. The asset needs to be saved on disk.");
                return false;
            }

            var aaSettings = AddressableAssetSettingsDefaultObject.Settings;
            if (aaSettings == null)
                return false;

            // TODO: Allow for locales to not be in the group, As long as they have the label it should be fine. See AddAssetToTable

            var group = GetGroup(aaSettings, LocaleGroupName, true);
            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(locale));

            // Has the locale already been added?
            if (group.GetAssetEntry(guid) != null)
                return false;

            var assetEntry = aaSettings.CreateOrMoveEntry(guid, group, true);
            assetEntry.address = locale.name;

            aaSettings.AddLabel(LocalizationSettings.LocaleLabel);
            assetEntry.SetLabel(LocalizationSettings.LocaleLabel, true);
            return true;
        }

        /// <summary>
        /// Removes the locale from the Addressables system.
        /// </summary>
        /// <param name="locale"></param>
        /// <returns>True if success or false if the locale could not be removed, such as if it was not added.</returns>
        public static bool RemoveLocale(Locale locale)
        {
            var aaSettings = AddressableAssetSettingsDefaultObject.Settings;
            if (aaSettings == null)
                return false;

            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(locale));
            if (aaSettings.FindAssetEntry(guid) == null)
                return false;

            aaSettings.RemoveAssetEntry(guid);
            return true;
        }

        /// <summary>
        /// Returns all locales that are part of the Addressables system.
        /// </summary>
        /// <returns></returns>
        public static List<Locale> GetLocales()
        {
            var locales = new List<Locale>();

            var aaSettings = AddressableAssetSettingsDefaultObject.Settings;
            if (aaSettings == null)
                return locales;

            // Get all Locales with the label
            var localeGuids = AssetDatabase.FindAssets("t:Locale");

            foreach (var localeGuid in localeGuids)
            {
                var assetEntry = aaSettings.FindAssetEntry(localeGuid);
                if (assetEntry != null && assetEntry.labels.Contains(LocalizationSettings.LocaleLabel))
                {
                    var locale = AssetDatabase.LoadAssetAtPath<Locale>(AssetDatabase.GUIDToAssetPath(localeGuid));
                    locales.Add(locale);
                }
            }
            return locales;
        }

        /// <summary>
        /// Add a localized asset to the asset table.
        /// This function will ensure the localization system adds the asset to the addressables system and sets the asset up for use.
        /// </summary>
        public static void AddAssetToTable<T>(AddressableAssetTableT<T> table, string key, T asset) where T: Object
        {
            if(!EditorUtility.IsPersistent(table))
            {
                Debug.LogError("Only persistent assets can be addressable. The asset needs to be saved on disk.");
                return;
            }

            // Add the asset to the addressables system and setup the table with the key to guid mapping.
            var aaSettings = AddressableAssetSettingsDefaultObject.Settings;
            if (aaSettings == null)
                return;

            // Has the asset already been added? Perhaps it is being used by multiple tables or the user has added it manually.
            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
            var entry = aaSettings.FindAssetEntry(guid);
            if(entry == null)
            {
                var groupName = string.Format(AssetTableTypeGroupName, typeof(T).ToString());
                var group = GetGroup(aaSettings, groupName, true);
                entry = aaSettings.CreateOrMoveEntry(guid, group, true);
                entry.address = FindUniqueAssetAddress(asset.name);
            }

            // TODO: A better way? Can assets have dependencies?
            // We need a way to mark that the asset has a dependency on this table. So we add a label with the table guid.
            var tableGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(table));
            Debug.Assert(!string.IsNullOrEmpty(tableGuid));

            aaSettings.AddLabel(table.LocaleIdentifier.Code);
            entry.SetLabel(table.LocaleIdentifier.Code, true);
            table.AddAsset(key, guid);
        }

        /// <summary>
        /// Remove the asset mapping from the table and also cleanup the addressables if necessary.
        /// </summary>
        public static void RemoveAssetFromTable<T>(AddressableAssetTableT<T> table, string key, T asset) where T : Object
        {
            // Clear the asset but keep the key
            table.AddAsset(key, string.Empty);

            var aaSettings = AddressableAssetSettingsDefaultObject.Settings;
            if (aaSettings == null)
                return;

            // Determine if the asset is being referenced by any other tables with the same locale, if not then we can
            // remove the locale label and if no other labels exist also remove the asset from the addressables system.
            var assetGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
            var tableGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(table));
            var tableGroup = GetGroup(aaSettings, AssetTableGroupName);
            if(tableGroup != null)
            {
                foreach (var tableEntry in tableGroup.entries)
                {
                    var tableToCheck = tableEntry.guid == tableGuid ? table : AssetDatabase.LoadAssetAtPath<AddressableAssetTableT<T>>(AssetDatabase.GUIDToAssetPath(tableEntry.guid));
                    if (tableToCheck != null && tableToCheck.LocaleIdentifier == table.LocaleIdentifier)
                    {
                        var guidHash = Hash128.Parse(assetGuid);
                        foreach(var item in tableToCheck.AssetMap.Values)
                        {
                            // The asset is referenced elsewhere so we can not remove the label or asset.
                            if (item.GuidHash == guidHash)
                                return;
                        }
                    }
                }
            }

            // Remove the current locale
            var assetEntry = aaSettings.FindAssetEntry(assetGuid);
            if (assetEntry != null)
            {
                aaSettings.AddLabel(table.LocaleIdentifier.Code);
                assetEntry.SetLabel(table.LocaleIdentifier.Code, false);

                // No other references so safe to remove.
                if (assetEntry.labels.Count <= 1) // 1 for the Asset table label
                {
                    aaSettings.RemoveAssetEntry(assetEntry.guid);
                }
            }
        }

        /// <summary>
        /// Add or update the Addressables data for the table.
        /// Ensures the table is in the correct group and has all the required labels.
        /// </summary>
        /// <param name="table"></param>
        public static void AddOrUpdateAssetTable(LocalizedTable table)
        {
            if (!EditorUtility.IsPersistent(table))
            {
                Debug.LogError("Only persistent assets can be addressable. The asset needs to be saved on disk.");
                return;
            }

            var aaSettings = AddressableAssetSettingsDefaultObject.Settings;
            if (aaSettings == null)
                return;

            var isStringTable = table is StringTableBase;

            // Has the asset already been added?
            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(table));
            var entry = aaSettings.FindAssetEntry(guid);
            if (entry == null)
            {
                var groupName = isStringTable ? StringTableGroupName : AssetTableGroupName;
                var group = GetGroup(aaSettings, groupName, true);
                entry = aaSettings.CreateOrMoveEntry(guid, group, true);
            }

            entry.address = string.Format("{0} - {1}", table.LocaleIdentifier.Code, table.TableName);
            entry.labels.Clear(); // Locale may have changed so clear the old one.

            // Label the table type
            var label = isStringTable ? LocalizedStringDatabase.StringTableLabel : LocalizedAssetDatabase.AssetTableLabel;
            aaSettings.AddLabel(label);
            entry.SetLabel(label, true);

            // Label the locale
            aaSettings.AddLabel(table.LocaleIdentifier.Code);
            entry.SetLabel(table.LocaleIdentifier.Code, true);
        }
    }
}