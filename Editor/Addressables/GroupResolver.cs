using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.SmartFormat;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace UnityEditor.Localization.Addressables
{
    /// <summary>
    /// Provides support for placing assets into different <see cref="AddressableAssetGroup"/>s.
    /// </summary>
    /// <example>
    /// This example places all English assets into a local group and all other languages into a remote group which could then be downloaded after the game is released.
    /// <code source="../../DocCodeSamples.Tests/GroupResolverExample.cs"/>
    /// </example>
    [Serializable]
    public class GroupResolver
    {
        [Serializable]
        class LocaleGroupPair
        {
            public LocaleIdentifier localeIdentifier;
            public AddressableAssetGroup group;
        }

        [SerializeField] string m_SharedGroupName = "Localization-Assets-Shared";
        [SerializeField] AddressableAssetGroup m_SharedGroup;
        [SerializeField] string m_LocaleGroupNamePattern = "Localization-Assets-{LocaleName}";
        [SerializeField] List<LocaleGroupPair> m_LocaleGroups = new List<LocaleGroupPair>();
        [SerializeField] bool m_MarkEntriesReadOnly = true;

        // Inverted so existing serialized data defaults to respecting user groups.
        [SerializeField] bool m_AlwaysMoveToLocalizationGroup;

        /// <summary>
        /// The name to use when creating a new group.
        /// </summary>
        public string SharedGroupName { get => m_SharedGroupName; set => m_SharedGroupName = value; }

        /// <summary>
        /// The group to use for shared assets. If null then a new group will be created using <see cref="SharedGroupName"/>.
        /// </summary>
        public AddressableAssetGroup SharedGroup { get => m_SharedGroup; set => m_SharedGroup = value; }

        /// <summary>
        /// The name to use when generating a new <see cref="AddressableAssetGroup"/> for a <see cref="Locale"/>.
        /// </summary>
        public string LocaleGroupNamePattern { get => m_LocaleGroupNamePattern; set => m_LocaleGroupNamePattern = value; }

        /// <summary>
        /// Should new Entries be marked as read only? This will prevent editing them in the Addressables window.
        /// </summary>
        public bool MarkEntriesReadOnly { get => m_MarkEntriesReadOnly; set => m_MarkEntriesReadOnly = value; }

        /// <summary>
        /// When an asset is already in a group that the localization system does not manage, such as one the user created,
        /// it is left in that group instead of being moved into a localization group. This is the default.
        /// </summary>
        /// <remarks>
        /// Set this to <see langword="false"/> to always move localized assets into the localization groups.
        /// The Always Move To Localization Group option in the Addressable Group Rules inspector edits the same value with the opposite meaning.
        /// </remarks>
        public bool RespectUserGroups { get => !m_AlwaysMoveToLocalizationGroup; set => m_AlwaysMoveToLocalizationGroup = !value; }

        /// <summary>
        /// Creates a new default instance of <see cref="GroupResolver"/>.
        /// </summary>
        public GroupResolver() {}

        /// <summary>
        /// Creates a new instance which will store all assets into a single group.
        /// </summary>
        /// <param name="groupName">The name to use when creating a new group.</param>
        public GroupResolver(string groupName)
        {
            m_SharedGroupName = groupName;
            m_LocaleGroupNamePattern = groupName;
        }

        /// <summary>
        /// Creates a new instance which will store all assets into a single group;
        /// </summary>
        /// <param name="group">The group to use.</param>
        public GroupResolver(AddressableAssetGroup group)
        {
            m_SharedGroup = group;
            m_SharedGroupName = group.Name;
            m_LocaleGroupNamePattern = group.Name;
        }

        /// <summary>
        /// Creates an instance using custom group names for each Locale.
        /// </summary>
        /// <param name="localeGroupNamePattern">The name to use when creating a new Group for a selected <see cref="LocaleIdentifier"/>. The name is formatted using <see cref="Smart"/> where the argument passed will be the <see cref="LocaleIdentifier"/>. The default is <c>"Localization-Assets-{Code}"</c>."</param>
        /// <param name="sharedGroupName">The name of the group to use when an asset is shared by multiple Locales that do not all use the same group.</param>
        public GroupResolver(string localeGroupNamePattern, string sharedGroupName)
        {
            m_SharedGroupName = sharedGroupName;
            m_LocaleGroupNamePattern = localeGroupNamePattern;
        }

        /// <summary>
        /// Add a group for the <see cref="LocaleIdentifier"/>. If a Group is already assigned it will be replaced with the new group.
        /// </summary>
        /// <param name="identifier">The Locale Id to use for the selected group.</param>
        /// <param name="group">The group to add for the selected Locale Id, must not be <see langword="null"/>.</param>
        public void AddLocaleGroup(LocaleIdentifier identifier, AddressableAssetGroup group)
        {
            if (group == null)
                throw new ArgumentNullException(nameof(group));

            var expectedGroupPair = m_LocaleGroups.FirstOrDefault(g => g.localeIdentifier == identifier);
            if (expectedGroupPair == null)
            {
                expectedGroupPair = new LocaleGroupPair { localeIdentifier = identifier };
                m_LocaleGroups.Add(expectedGroupPair);
            }

            expectedGroupPair.group = group;
        }

        /// <summary>
        /// Removes the group for the chosen Locale Id.
        /// </summary>
        /// <param name="identifier">The Locale Id to be removed.</param>
        /// <returns>Returns <see langword="true"/> if an item was removed.</returns>
        public bool RemoveLocaleGroup(LocaleIdentifier identifier) => m_LocaleGroups.RemoveAll(g => g.localeIdentifier == identifier) > 0;

        /// <summary>
        /// Returns the active group assigned to the <see cref="LocaleIdentifier"/> or <see langword="null"/> if one has not been assigned.
        /// </summary>
        /// <param name="identifier">The Locale Id to search for.</param>
        /// <returns></returns>
        public AddressableAssetGroup GetLocaleGroup(LocaleIdentifier identifier) => m_LocaleGroups.FirstOrDefault(g => g.localeIdentifier == identifier)?.group;

        /// <summary>
        /// Add an asset to an <see cref="AddressableAssetGroup"/>.
        /// The asset will be moved into a group which either matches <see cref="SharedGroup"/> or <see cref="SharedGroupName"/>.
        /// </summary>
        /// <param name="asset">The asset to be added to a group.</param>
        /// <param name="locales">List of locales that depend on this asset or null if the asset is used by all.</param>
        /// <param name="aaSettings">The Addressables setting that will be used if a new group should be added.</param>
        /// <param name="createUndo">Should an Undo record be created?</param>
        /// <returns>The asset entry for the added asset.</returns>
        public virtual AddressableAssetEntry AddToGroup(Object asset, IList<LocaleIdentifier> locales, AddressableAssetSettings aaSettings, bool createUndo)
        {
            var guid = GetAssetGuid(asset);
            var assetEntry = aaSettings.FindAssetEntry(guid);

            if (assetEntry?.parentGroup != null)
            {
                if (assetEntry.parentGroup.Name == GetExpectedGroupName(locales, asset, aaSettings))
                    return assetEntry;

                // Addressables can drop an asset from the build during deduplication when a stale localization reference remains.
                if (ShouldLeaveInUserGroup(assetEntry.parentGroup, asset, aaSettings))
                    return assetEntry;
            }

            var group = GetGroup(locales, asset, aaSettings, createUndo);

            if (assetEntry == null)
            {
                if (createUndo)
                    Undo.RecordObjects(new Object[] { aaSettings, group }, "Add to group");
                assetEntry = aaSettings.CreateOrMoveEntry(guid, group, MarkEntriesReadOnly);
            }
            else
            {
                if (createUndo)
                    Undo.RecordObjects(new Object[] { aaSettings, group, assetEntry.parentGroup }, "Add to group");

                if (!group.entries.Contains(assetEntry))
                    aaSettings.MoveEntry(assetEntry, group, MarkEntriesReadOnly);
            }

            return assetEntry;
        }

        /// <summary>
        /// Returns the name of the group that the asset will be moved to when calling <see cref="AddToGroup"/>.
        /// </summary>
        /// <remarks>
        /// A subasset resolves using its main asset, so every reference to the same asset maps to one group.
        /// </remarks>
        /// <param name="locales"></param>
        /// <param name="asset"></param>
        /// <param name="aaSettings"></param>
        /// <returns></returns>
        public virtual string GetExpectedGroupName(IList<LocaleIdentifier> locales, Object asset, AddressableAssetSettings aaSettings)
        {
            asset = GetMainAsset(asset);

            if (locales == null || locales.Count == 0)
                return GetExpectedSharedGroupName(locales, asset, aaSettings);

            Locale locale;
            if (asset is Locale l && l.Identifier == locales[0])
                locale = l;
            else
                locale = LocalizationEditorSettings.GetLocale(locales[0].Code) ?? GetTemporaryLocale(locales[0]);

            var expectedGroupPair = m_LocaleGroups.FirstOrDefault(g => g.localeIdentifier == locales[0]);
            var expectedGroupName = expectedGroupPair?.group != null ? expectedGroupPair.group.Name : Smart.Format(LocaleGroupNamePattern, locale, asset);
            for (var i = 1; i < locales.Count; ++i)
            {
                var groupPair = m_LocaleGroups.FirstOrDefault(g => g.localeIdentifier == locales[i]);
                locale = LocalizationEditorSettings.GetLocale(locales[i]) ?? GetTemporaryLocale(locales[i]);
                var groupName = groupPair?.group != null ? groupPair.group.Name : Smart.Format(LocaleGroupNamePattern, locale, asset);
                if (expectedGroupName != groupName)
                {
                    // Use shared group
                    return GetExpectedSharedGroupName(locales, asset, aaSettings);
                }
            }

            return expectedGroupName;
        }

        /// <summary>
        /// Returns the name that the Shared group is expected to have.
        /// </summary>
        /// <param name="locales"></param>
        /// <param name="asset"></param>
        /// <param name="aaSettings"></param>
        /// <returns></returns>
        protected virtual string GetExpectedSharedGroupName(IList<LocaleIdentifier> locales, Object asset, AddressableAssetSettings aaSettings)
        {
            return SharedGroup != null ? SharedGroup.Name : SharedGroupName;
        }

        /// <summary>
        /// Returns true if the group is one that this resolver manages.
        /// </summary>
        /// <remarks>
        /// A group is managed when it was created by a localization resolver, is the <see cref="SharedGroup"/>,
        /// a group assigned through <see cref="AddLocaleGroup"/>, or a group whose name matches one produced by
        /// <see cref="GetExpectedGroupName"/> for a known <see cref="Locale"/>. Created groups carry a marker schema,
        /// so they stay recognized after the name pattern or the project locales change.
        /// <see cref="AddToGroup"/> treats assets in groups that no localization resolver manages as user owned and leaves them in place.
        /// The group name pattern may reference the asset.
        /// </remarks>
        /// <param name="group">The group to test.</param>
        /// <param name="asset">The asset being evaluated.</param>
        /// <param name="aaSettings">The Addressable asset settings.</param>
        /// <returns><see langword="true"/> if this resolver manages the group; otherwise <see langword="false"/>.</returns>
        public virtual bool IsLocalizationGroup(AddressableAssetGroup group, Object asset, AddressableAssetSettings aaSettings)
        {
            if (group == null)
                return false;

            if (group.GetSchema<LocalizationGroupSchema>() != null)
                return true;

            if (group.Name == GetExpectedGroupName(null, asset, aaSettings))
                return true;

            using (ListPool<LocaleIdentifier>.Get(out var single))
            {
                single.Add(default);

                foreach (var locale in LocalizationEditorSettings.GetLocales())
                {
                    single[0] = locale.Identifier;
                    if (group.Name == GetExpectedGroupName(single, asset, aaSettings))
                        return true;
                }

                foreach (var pair in m_LocaleGroups)
                {
                    if (pair.group != null)
                    {
                        if (group.Name == pair.group.Name)
                            return true;
                    }
                    else
                    {
                        single[0] = pair.localeIdentifier;
                        if (group.Name == GetExpectedGroupName(single, asset, aaSettings))
                            return true;
                    }
                }
            }

            return false;
        }

        internal bool ShouldLeaveInUserGroup(AddressableAssetGroup group, Object asset, AddressableAssetSettings aaSettings)
        {
            return RespectUserGroups && group != null &&
                !IsLocalizationGroup(group, asset, aaSettings) &&
                !AddressableGroupRules.IsLocalizationGroup(group, asset, aaSettings, this);
        }

        /// <summary>
        /// Returns the Addressable group for the asset.
        /// </summary>
        /// <param name="locales">The locales that depend on the asset.</param>
        /// <param name="asset">The asset that is to be added to an Addressable group.</param>
        /// <param name="aaSettings">The Addressable asset settings.</param>
        /// <param name="createUndo">Should an Undo record be created if changes are made?</param>
        /// <returns></returns>
        protected virtual AddressableAssetGroup GetGroup(IList<LocaleIdentifier> locales, Object asset, AddressableAssetSettings aaSettings, bool createUndo)
        {
            var groupName = GetExpectedGroupName(locales, asset, aaSettings);
            return FindOrCreateGroup(groupName, aaSettings, createUndo);
        }

        AddressableAssetGroup FindOrCreateGroup(string name, AddressableAssetSettings  aaSettings, bool createUndo) => aaSettings.FindGroup(name) ?? CreateNewGroup(name, MarkEntriesReadOnly, aaSettings, createUndo);

        static AddressableAssetGroup CreateNewGroup(string name, bool readOnly, AddressableAssetSettings aaSettings, bool createUndo)
        {
            if (createUndo)
                Undo.RecordObject(aaSettings, "Create group");
            var group = aaSettings.CreateGroup(name, false, readOnly, true, null, typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema), typeof(LocalizationGroupSchema));
            var schema = group.GetSchema<BundledAssetGroupSchema>();

            // Don't use hash as it creates very long file names that can cause issues on Windows.
            schema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.NoHash;

            if (createUndo)
                Undo.RegisterCreatedObjectUndo(group, "Create group");
            return group;
        }

        static Object GetMainAsset(Object asset)
        {
            if (asset == null)
                return asset;

            var path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path))
                return asset;

            var mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
            return mainAsset != null ? mainAsset : asset;
        }

        static readonly Dictionary<LocaleIdentifier, Locale> s_TemporaryLocales = new Dictionary<LocaleIdentifier, Locale>();

        static Locale GetTemporaryLocale(LocaleIdentifier identifier)
        {
            if (!s_TemporaryLocales.TryGetValue(identifier, out var locale) || locale == null)
            {
                locale = Locale.CreateLocale(identifier);
                s_TemporaryLocales[identifier] = locale;
            }
            return locale;
        }

        AddressableAssetEntry GetAssetEntry(Object asset, AddressableAssetSettings aaSettings) => aaSettings.FindAssetEntry(GetAssetGuid(asset));

        internal virtual string GetAssetGuid(Object asset)
        {
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out var guid, out long _))
                return guid;

            Debug.LogError("Failed to extract the asset Guid for " + asset.name, asset);
            return null;
        }
    }
}
