using System;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace UnityEditor.Localization.Addressables
{
    /// <summary>
    /// Built-in Addressables group template display names. Must stay aligned with
    /// <see cref="AddressableAssetSettings"/> private fields <c>m_DefaultGroupTemplateName</c> and
    /// <c>m_ContentDirectoryGroupTemplateName</c> in the Addressables package.
    /// </summary>
    static class AddressableGroupTemplateResolution
    {
        internal const string PackedAssetsTemplateName = "Packed Assets";
        internal const string ContentDirectoryTemplateName = "Content Directory";

        internal static AddressableAssetGroupTemplate FindTemplate(AddressableAssetSettings settings, string templateDisplayName)
        {
            if (settings?.GroupTemplateObjects == null || string.IsNullOrEmpty(templateDisplayName))
                return null;

            foreach (var obj in settings.GroupTemplateObjects)
            {
                if (obj != null && obj.name == templateDisplayName && obj is AddressableAssetGroupTemplate t)
                    return t;
            }

            return null;
        }

        /// <summary>
        /// Adds the schemas a localization group needs, resolving <paramref name="templateName"/> from the
        /// group's own Addressables settings.
        /// </summary>
        internal static void AddSchemasFromTemplate(AddressableAssetGroup group, string templateName, params Type[] defaultTypes)
        {
            if (group == null)
                return;

            AddSchemasFromTemplate(group, FindTemplate(group.Settings, templateName), templateName, defaultTypes);
        }

        /// <summary>
        /// Adds the schemas a localization group needs. Copies every schema from <paramref name="template"/>
        /// so its build and load paths survive, or warns and creates <paramref name="defaultTypes"/> when no
        /// template resolved. Schema types the group already has are left alone.
        /// </summary>
        /// <remarks>
        /// <para>A resolved template supplies its whole schema set, so <paramref name="defaultTypes"/> is used
        /// only when there is no template. A caller needing a particular schema afterwards must re-read it
        /// with <see cref="AddressableAssetGroup.GetSchema{TSchema}"/> and handle null.</para>
        /// <para>The whole set is worked out before anything is added, so a group is never left half
        /// configured. Skipping types the group already has also keeps the call idempotent.</para>
        /// <para>Only the no-template path forces <c>NoHash</c> bundle naming, because only it invents a
        /// schema. A resolved template's naming is the user's own choice, so it is left alone.</para>
        /// <para>Copying a schema instance skips the conflict check that adding one by type runs, and a copy
        /// arrives enabled, so a conflicting copy is disabled here. Every caller goes through this method,
        /// including <see cref="GroupResolver"/> for brand new groups, so the rule lives in one place.</para>
        /// </remarks>
        internal static void AddSchemasFromTemplate(AddressableAssetGroup group, AddressableAssetGroupTemplate template, string templateName, params Type[] defaultTypes)
        {
            // No settings means nowhere to save the schema assets.
            if (group == null || group.Settings == null)
                return;

            if (template != null)
            {
                // Copy the list first. AddSchema saves assets, and saving the template re-sorts its own
                // live SchemaObjects list, which would break iteration over it.
                var templateSchemas = new List<AddressableAssetGroupSchema>(template.SchemaObjects);
                var addedAny = false;
                foreach (var templateSchema in templateSchemas)
                {
                    if (templateSchema == null || group.GetSchema(templateSchema.GetType()) != null)
                        continue;

                    var added = group.AddSchema(templateSchema, false);
                    addedAny |= added != null;
#if ADDRESSABLES_CD_SUPPORT
                    // Copying a schema instance skips the conflict check that adding one by type runs, and a
                    // copy arrives enabled. A template carrying both a bundled and a content directory schema
                    // would otherwise leave two enabled buildable schemas, which Addressables cannot build.
                    // First one wins, matching how Addressables resolves this itself.
                    if (added != null && added.CanEnableSchema() != string.Empty)
                        added.IsEnabled = false;
#endif
                }

                // Only reports a template that contributed nothing. Whether the schemas it did
                // contribute can actually build is left alone: Addressables keeps that behind an
                // internal IBuildableSchema, so checking would mean hardcoding its two built-in
                // layout schemas and rejecting any other.
                if (!addedAny && !HasAnySchema(group))
                {
                    Debug.LogError(
                        $"The Addressables group template '{templateName}' has no usable schemas, so group " +
                        $"'{group.Name}' was left without any. Add schemas to the template, or delete and " +
                        "recreate it, then try again.");
                }

                return;
            }

            Debug.LogWarning(
                $"Could not resolve the Addressables group template '{templateName}' for group '{group.Name}'. " +
                "Adding default schemas instead, so the group will use the Addressables default build and load paths. " +
                "Create or assign the group template to control them.");

            if (defaultTypes == null)
                return;

            var addedBundledSchema = false;
            foreach (var type in defaultTypes)
            {
                if (type == null || group.GetSchema(type) != null)
                    continue;

                addedBundledSchema |= group.AddSchema(type, false) is BundledAssetGroupSchema;
            }

            // A default BundledAssetGroupSchema uses hash naming, which makes paths long enough to break
            // on Windows. New groups get NoHash forced for the same reason.
            if (addedBundledSchema)
                GroupResolver.UpdateBundleNaming(group, null);
        }

        /// <summary>
        /// Returns whether the group has at least one schema. A schema turns null here once its asset is
        /// deleted, so a non-empty list does not mean a usable one.
        /// </summary>
        internal static bool HasAnySchema(AddressableAssetGroup group)
        {
            if (group?.Schemas == null)
                return false;

            foreach (var schema in group.Schemas)
            {
                if (schema != null)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the group's current schemas, for comparing against after adding more.
        /// </summary>
        internal static HashSet<AddressableAssetGroupSchema> SnapshotSchemas(AddressableAssetGroup group)
        {
            var snapshot = new HashSet<AddressableAssetGroupSchema>();
            if (group?.Schemas == null)
                return snapshot;

            foreach (var schema in group.Schemas)
            {
                if (schema != null)
                    snapshot.Add(schema);
            }

            return snapshot;
        }

        /// <summary>
        /// Registers schema assets added since <paramref name="existingBefore"/> so undo removes them.
        /// </summary>
        /// <remarks>
        /// Addressables writes each schema as its own asset and registers nothing for undo, so undoing
        /// the group alone would strand them. Pass null when every schema is new, as it is for a group
        /// that was just created.
        /// </remarks>
        internal static void RegisterCreatedSchemasUndo(AddressableAssetGroup group, HashSet<AddressableAssetGroupSchema> existingBefore, string undoName)
        {
            if (group?.Schemas == null)
                return;

            // Copy the list first. Registering undo serializes the group, and saving it re-sorts its own
            // live schema list, which would break iteration over it.
            var schemas = new List<AddressableAssetGroupSchema>(group.Schemas);
            foreach (var schema in schemas)
            {
                if (schema != null && (existingBefore == null || !existingBefore.Contains(schema)))
                    Undo.RegisterCreatedObjectUndo(schema, undoName);
            }
        }

        /// <summary>
        /// Registers a newly created template and its schema assets so undo removes them.
        /// </summary>
        /// <remarks>
        /// <see cref="AddressableAssetSettings.CreateAndAddGroupTemplate"/> writes the template and each of
        /// its schemas as separate assets and registers nothing for undo, so undoing the settings change
        /// alone would strand them. The template goes first, since undo unwinds in reverse.
        /// </remarks>
        internal static void RegisterCreatedTemplateUndo(AddressableAssetGroupTemplate template, string undoName)
        {
            if (template == null)
                return;

            Undo.RegisterCreatedObjectUndo(template, undoName);

            if (template.SchemaObjects == null)
                return;

            // Copy the list first. Registering undo serializes the template, and saving it re-sorts its
            // own live schema list, which would break iteration over it.
            var schemas = new List<AddressableAssetGroupSchema>(template.SchemaObjects);
            foreach (var schema in schemas)
            {
                if (schema != null)
                    Undo.RegisterCreatedObjectUndo(schema, undoName);
            }
        }

#if ADDRESSABLES_CD_SUPPORT
        /// <summary>
        /// Returns whether the template builds content directories rather than asset bundles.
        /// </summary>
        /// <remarks>
        /// <para>The first enabled buildable schema decides, mirroring what
        /// <see cref="AddSchemasFromTemplate(AddressableAssetGroup, AddressableAssetGroupTemplate, string, Type[])"/>
        /// leaves enabled on the group it creates. Reading the template's own schemas means a custom template
        /// counts, not just the built-in one.</para>
        /// <para>A template can hold both buildable schemas enabled, since adding one to a template skips the
        /// conflict check. Addressables sorts a template's schemas by type name, so the bundled schema comes
        /// first and such a template builds asset bundles.</para>
        /// <para>Only read <see cref="AddressableAssetGroupSchema.IsEnabled"/> here. A template's schema has no
        /// owning group, and both <c>CanEnableSchema</c> and enabling a schema dereference that group.</para>
        /// </remarks>
        internal static bool BuildsContentDirectories(AddressableAssetGroupTemplate template)
        {
            if (template?.SchemaObjects == null)
                return false;

            foreach (var schema in template.SchemaObjects)
            {
                if (schema == null || !schema.IsEnabled)
                    continue;

                if (schema is ContentDirectoryGroupSchema)
                    return true;
                if (schema is BundledAssetGroupSchema)
                    return false;
            }

            return false;
        }
#endif
    }
}
