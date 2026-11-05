#if ADDRESSABLES_CD_SUPPORT

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityEditor.Localization.Addressables
{
    /// <summary>
    /// Shared helpers for migrating localization-owned Addressable groups between Content Directory and Packed Assets
    /// (bundled / content update) layouts. Uses public <see cref="AddressableAssetGroupSchema.IsEnabled"/> with an order
    /// consistent with Addressables schema enable rules.
    /// </summary>
    internal static class MigrateGroupSchemaConverter
    {
        /// <summary>
        /// Returns the project's Addressables settings asset, or null when Addressables is not configured.
        /// </summary>
        public static AddressableAssetSettings GetActiveAddressableSettings()
        {
            return AddressableAssetSettingsDefaultObject.GetSettings(false);
        }

        /// <summary>
        /// Ensures the built-in Content Directory group template exists, creating it with
        /// <see cref="AddressableAssetSettings.CreateAndAddGroupTemplate"/> when absent.
        /// </summary>
        /// <returns>True when the template is present after this call.</returns>
        public static bool EnsureContentDirectoryGroupTemplate(AddressableAssetSettings settings) =>
            EnsureGroupTemplate(
                settings,
                AddressableGroupTemplateResolution.ContentDirectoryTemplateName,
                "Pack assets into Content Directories.",
                "Add Content Directory group template",
                typeof(ContentDirectoryGroupSchema));

        /// <summary>
        /// Ensures the built-in Packed Assets group template exists, creating it with
        /// <see cref="AddressableAssetSettings.CreateAndAddGroupTemplate"/> when absent.
        /// </summary>
        /// <returns>True when the template is present after this call.</returns>
        public static bool EnsurePackedAssetsGroupTemplate(AddressableAssetSettings settings) =>
            EnsureGroupTemplate(
                settings,
                AddressableGroupTemplateResolution.PackedAssetsTemplateName,
                "Pack assets into asset bundles.",
                "Add Packed Assets group template",
                typeof(BundledAssetGroupSchema),
                typeof(ContentUpdateGroupSchema));

        /// <summary>
        /// Creates <paramref name="templateName"/> when the project has no template by that name.
        /// </summary>
        /// <remarks>Undoes as one step, including the template and schema assets it creates.</remarks>
        /// <returns>True when the template is present after this call.</returns>
        static bool EnsureGroupTemplate(AddressableAssetSettings settings, string templateName, string description, string undoName, params System.Type[] schemaTypes)
        {
            if (settings == null)
                return false;

            if (AddressableGroupTemplateResolution.FindTemplate(settings, templateName) != null)
                return true;

            using (new UndoScope(undoName, true))
            {
                Undo.RecordObject(settings, undoName);
                settings.CreateAndAddGroupTemplate(templateName, description, schemaTypes);
                EditorUtility.SetDirty(settings);

                // CreateAndAddGroupTemplate only reports success, so re-find what it made to register it.
                var template = AddressableGroupTemplateResolution.FindTemplate(settings, templateName);
                AddressableGroupTemplateResolution.RegisterCreatedTemplateUndo(template, undoName);
                return template != null;
            }
        }

        /// <summary>
        /// Collects every Addressable group that an active localization <see cref="GroupResolver"/> owns —
        /// via its <see cref="LocalizationGroupSchema"/> marker or a name matching what the resolver would
        /// currently produce (see <see cref="AddressableGroupRules.IsLocalizationGroup"/>) — regardless of
        /// whether the group currently holds any localization assets.
        /// </summary>
        public static void CollectLocalizationManagedGroups(AddressableAssetSettings settings, HashSet<AddressableAssetGroup> groups)
        {
            if (settings == null || groups == null)
                return;

            foreach (var group in settings.groups)
            {
                if (group != null && AddressableGroupRules.IsLocalizationGroup(group, null, settings))
                    groups.Add(group);
            }
        }

        /// <summary>
        /// Returns whether the group participates in localization-managed content and still has an enabled bundled schema.
        /// </summary>
        public static bool ShouldMigrateGroupToContentDirectory(AddressableAssetGroup group)
        {
            if (group == null)
                return false;

            var bags = group.GetSchema<BundledAssetGroupSchema>();
            if (bags == null || !bags.IsEnabled)
                return false;

            // if BundledAssetGroupSchema is enabled we need to go through the path to disable it event if we already
            // have a content directory
            if (bags.IsEnabled)
                return true;

            var cd = group.GetSchema<ContentDirectoryGroupSchema>();
            if (cd != null && cd.IsEnabled)
                return false;

            return true;
        }

        /// <summary>
        /// Applies the named template's schemas to <paramref name="group"/>, or default schemas with a
        /// warning when the template cannot be resolved.
        /// </summary>
        /// <returns>
        /// False, having logged an error and left the group untouched, when the template resolved but has no
        /// <paramref name="requiredType"/>. Checked before anything is added: schemas copied from a template
        /// arrive enabled, so a group abandoned part way through would hold two enabled buildable schemas.
        /// </returns>
        static bool TryApplyTemplateSchemas(AddressableAssetGroup group, string templateName, System.Type requiredType, params System.Type[] defaultTypes)
        {
            if (group.Settings == null)
            {
                Debug.LogError(
                    $"Cannot migrate group '{group.Name}': it has no Addressables settings, so there is " +
                    "nowhere to save its schemas.");
                return false;
            }

            var template = AddressableGroupTemplateResolution.FindTemplate(group.Settings, templateName);
            if (template != null && group.GetSchema(requiredType) == null && template.GetSchemaByType(requiredType) == null)
            {
                Debug.LogError(
                    $"Cannot migrate group '{group.Name}': the '{templateName}' group template has no " +
                    $"'{requiredType.Name}'. Add that schema to the template, then try again.");
                return false;
            }

            AddressableGroupTemplateResolution.AddSchemasFromTemplate(group, template, templateName, defaultTypes);
            return true;
        }

        /// <summary>
        /// Drops the nulls, so an optional schema can be passed straight to <see cref="Undo.RecordObjects"/>.
        /// </summary>
        static Object[] NonNullObjects(params Object[] objects)
        {
            var kept = new List<Object>(objects.Length);
            foreach (var o in objects)
            {
                if (o != null)
                    kept.Add(o);
            }

            return kept.ToArray();
        }

        /// <summary>
        /// Migrates a single group: disable <see cref="BundledAssetGroupSchema"/> and <see cref="ContentUpdateGroupSchema"/>,
        /// then ensure <see cref="ContentDirectoryGroupSchema"/> exists and is enabled.
        /// Order matches Addressables <c>CanEnableSchema</c> rules (bundled must be off before CD is valid).
        /// </summary>
        /// <remarks>Undoes as one step, including the schema assets the template copy creates.</remarks>
        public static void ConvertGroupToContentDirectory(AddressableAssetGroup group)
        {
            if (group == null)
                throw new System.ArgumentNullException(nameof(group));

            const string undoName = "Migrate localization group to Content Directory";

            using (new UndoScope(undoName, true))
            {
                var schemasBefore = AddressableGroupTemplateResolution.SnapshotSchemas(group);

                if (!TryApplyTemplateSchemas(
                        group,
                        AddressableGroupTemplateResolution.ContentDirectoryTemplateName,
                        typeof(ContentDirectoryGroupSchema),
                        typeof(ContentDirectoryGroupSchema)))
                    return;

                // Before the bail-out below, so undo still clears whatever the template copy created.
                AddressableGroupTemplateResolution.RegisterCreatedSchemasUndo(group, schemasBefore, undoName);

                var cd = group.GetSchema<ContentDirectoryGroupSchema>();
                if (cd == null)
                    return; // Addressables refused the add and logged why. Never disable the only layout that builds.

                var bags = group.GetSchema<BundledAssetGroupSchema>();
                var cugs = group.GetSchema<ContentUpdateGroupSchema>();

                // Each schema is its own asset, so recording the group alone would not capture IsEnabled.
                Undo.RecordObjects(NonNullObjects(group, bags, cugs, cd), undoName);

                if (bags != null)
                    bags.IsEnabled = false;

                if (cugs != null)
                    cugs.IsEnabled = false;

                cd.IsEnabled = true;

                EditorUtility.SetDirty(group);
            }
        }

        /// <summary>
        /// Migrates every group in <paramref name="groups"/> that <see cref="ShouldMigrateGroupToContentDirectory"/> reports.
        /// </summary>
        public static void ConvertGroupsToContentDirectory(IEnumerable<AddressableAssetGroup> groups)
        {
            if (groups == null)
                return;

            // One undo step for the whole fix, rather than one per group.
            using (new UndoScope("Migrate localization groups to Content Directory", true))
            {
                foreach (var g in groups)
                {
                    if (g != null && ShouldMigrateGroupToContentDirectory(g))
                        ConvertGroupToContentDirectory(g);
                }
            }
        }

        /// <summary>
        /// Returns whether the group uses an enabled <see cref="ContentDirectoryGroupSchema"/> and should migrate back to a Packed Assets layout.
        /// </summary>
        public static bool ShouldMigrateGroupToPackedAssets(AddressableAssetGroup group)
        {
            if (group == null)
                return false;

            var cd = group.GetSchema<ContentDirectoryGroupSchema>();
            return cd != null && cd.IsEnabled;
        }

        /// <summary>
        /// Migrates a single group from Content Directory to Packed Assets: disable <see cref="ContentDirectoryGroupSchema"/>,
        /// then ensure <see cref="BundledAssetGroupSchema"/> and <see cref="ContentUpdateGroupSchema"/> exist and are enabled.
        /// </summary>
        public static void ConvertGroupToPackedAssets(AddressableAssetGroup group)
        {
            if (group == null)
                throw new System.ArgumentNullException(nameof(group));

            const string undoName = "Migrate localization group to Packed Assets";

            using (new UndoScope(undoName, true))
            {
                var schemasBefore = AddressableGroupTemplateResolution.SnapshotSchemas(group);

                if (!TryApplyTemplateSchemas(
                        group,
                        AddressableGroupTemplateResolution.PackedAssetsTemplateName,
                        typeof(BundledAssetGroupSchema),
                        typeof(BundledAssetGroupSchema),
                        typeof(ContentUpdateGroupSchema)))
                    return;

                // Before the bail-out below, so undo still clears whatever the template copy created.
                AddressableGroupTemplateResolution.RegisterCreatedSchemasUndo(group, schemasBefore, undoName);

                var bags = group.GetSchema<BundledAssetGroupSchema>();
                if (bags == null)
                    return; // Addressables refused the add and logged why. Never disable the only layout that builds.

                var cd = group.GetSchema<ContentDirectoryGroupSchema>();
                // A template may deliberately omit the content update schema, so this one is optional.
                var cugs = group.GetSchema<ContentUpdateGroupSchema>();

                // Each schema is its own asset, so recording the group alone would not capture IsEnabled.
                Undo.RecordObjects(NonNullObjects(group, bags, cugs, cd), undoName);

                if (cd != null)
                    cd.IsEnabled = false;

                bags.IsEnabled = true;

                if (cugs != null)
                    cugs.IsEnabled = true;

                EditorUtility.SetDirty(group);
            }
        }

        /// <summary>
        /// Migrates every group in <paramref name="groups"/> that <see cref="ShouldMigrateGroupToPackedAssets"/> reports.
        /// </summary>
        public static void ConvertGroupsToPackedAssets(IEnumerable<AddressableAssetGroup> groups)
        {
            if (groups == null)
                return;

            // One undo step for the whole fix, rather than one per group.
            using (new UndoScope("Migrate localization groups to Packed Assets", true))
            {
                foreach (var g in groups)
                {
                    if (g != null && ShouldMigrateGroupToPackedAssets(g))
                        ConvertGroupToPackedAssets(g);
                }
            }
        }

        /// <summary>
        /// Returns true when the active persistent <see cref="AddressableGroupRules"/> still builds asset bundles
        /// and should use the project's Content Directory template.
        /// </summary>
        /// <remarks>Judged by what the rules' effective template builds, so a custom content directory template counts.</remarks>
        public static bool ActiveRulesNeedContentDirectoryTemplate(AddressableAssetSettings settings)
        {
            if (settings == null)
                return false;

            var instance = AddressableGroupRules.Instance;
            if (!EditorUtility.IsPersistent(instance))
                return false;

            // Deliberately does not require the built-in template. A project on a custom content directory
            // template needs no switch whether that template exists or not.
            return !AddressableGroupTemplateResolution.BuildsContentDirectories(instance.GetEffectiveGroupTemplate(settings));
        }

        /// <summary>
        /// Assigns the Content Directory group template from Addressables settings to the active rules asset.
        /// </summary>
        /// <returns>
        /// False when no CD template exists, rules are not saved as assets, or the rules already build content
        /// directories - assigning over a custom template would lose its build and load paths.
        /// </returns>
        public static bool ApplyContentDirectoryTemplateToActiveRules(AddressableAssetSettings settings)
        {
            if (settings == null)
                return false;

            var cdTemplate = AddressableGroupTemplateResolution.FindTemplate(settings, AddressableGroupTemplateResolution.ContentDirectoryTemplateName);
            if (cdTemplate == null)
                return false;

            var instance = AddressableGroupRules.Instance;
            if (!EditorUtility.IsPersistent(instance))
                return false;

            // Never overwrite a custom template that already builds content directories.
            if (AddressableGroupTemplateResolution.BuildsContentDirectories(instance.GetEffectiveGroupTemplate(settings)))
                return false;

            Undo.RecordObject(instance, "Use Content Directory Addressable Group Rules template");
            instance.GroupTemplate = cdTemplate;
            EditorUtility.SetDirty(instance);
            return true;
        }

        /// <summary>
        /// Returns true when the active persistent <see cref="AddressableGroupRules"/> builds content directories
        /// and should switch to the Packed Assets template (reverse of CD migration for new groups).
        /// </summary>
        /// <remarks>Judged by what the rules' effective template builds, so a custom content directory template counts.</remarks>
        public static bool ActiveRulesNeedPackedAssetsTemplate(AddressableAssetSettings settings)
        {
            if (settings == null)
                return false;

            var paTemplate = AddressableGroupTemplateResolution.FindTemplate(settings, AddressableGroupTemplateResolution.PackedAssetsTemplateName);
            if (paTemplate == null)
                return false;

            var instance = AddressableGroupRules.Instance;
            if (!EditorUtility.IsPersistent(instance))
                return false;

            // Any template that builds content directories can switch back, not just the built-in one.
            return AddressableGroupTemplateResolution.BuildsContentDirectories(instance.GetEffectiveGroupTemplate(settings));
        }

        /// <summary>
        /// Assigns the Packed Assets group template from Addressables settings to the active rules asset when it
        /// currently builds content directories.
        /// </summary>
        /// <returns>False when the template or persistent rules are unavailable, or the rules already build asset bundles.</returns>
        public static bool ApplyPackedAssetsTemplateToActiveRules(AddressableAssetSettings settings)
        {
            if (settings == null)
                return false;

            var paTemplate = AddressableGroupTemplateResolution.FindTemplate(settings, AddressableGroupTemplateResolution.PackedAssetsTemplateName);
            if (paTemplate == null)
                return false;

            var instance = AddressableGroupRules.Instance;
            if (!EditorUtility.IsPersistent(instance))
                return false;

            if (!AddressableGroupTemplateResolution.BuildsContentDirectories(instance.GetEffectiveGroupTemplate(settings)))
                return false;

            Undo.RecordObject(instance, "Use Packed Assets Addressable Group Rules template");
            instance.GroupTemplate = paTemplate;
            EditorUtility.SetDirty(instance);
            return true;
        }
    }
}

#endif
