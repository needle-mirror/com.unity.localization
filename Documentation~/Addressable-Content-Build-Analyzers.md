# Content build analyze rules

Addressables can build your Localization content as content directories or as AssetBundles. 

Four Addressable Analyze rules help switch existing Localization content between content directories and AssetBundles. They require **Addressables 4.0.0 or later**.

Find them in the Analyzer window (**Window > Asset Management > Addressables > Analyze**), alongside the other [Localization analyzers](Addressables.md#addressable-analyzers). Run a rule to see its report, then select **Fix Selected Rules** to apply the changes.

## When to use each rule

The rules come in two pairs:

- **Switch Group Rules...** changes which **Group Template** your active **Addressable Group Rules** use. This affects only new groups — it does not touch existing groups.
- **Migrate Groups...** swaps the content build type of the Localization groups you already have.

To move a whole project to content directories, run **Switch Group Rules to Content Directories** first so new groups build as content directories, then run **Migrate Groups to Content Directory** to convert the existing groups.

## Which groups count as "Localization groups"

The **Migrate Groups...** rules only touch groups your Addressable Group Rules own. A group counts as owned when it either:

- Was created by the Localization system (has the LocalizationGroupSchema attached), or
- Has a name that matches what your current Group Rules would produce for it.

This means an empty group — for example, a locale with no translations yet — is still found and migrated. A group you created and manage yourself is left alone, even if it happens to hold localized assets.

## Localization: Switch Group Rules to Content Directories

Points your active Addressable Group Rules at the **Content Directory** template, so new Localization groups build as content directories. If the built-in Content Directory template is missing from your project, the rule will offer to re-create it first.

## Localization: Switch Group Rules to Asset Bundles

The reverse of the rule above. Points your active Addressable Group Rules back at the **Packed Assets** template, so new groups build as AssetBundles. Offers to re-create the Packed Assets template if it is missing.

## Localization: Migrate Groups to Content Directory

Finds existing Localization groups that still build AssetBundles and converts them. For each group it disables the `BundledAssetGroupSchema` and `ContentUpdateGroupSchema`, then adds the `ContentDirectoryGroupSchema` copied from your Content Directory template. 

## Localization: Migrate Groups to Asset Bundles

The reverse of the rule above. Finds Localization groups that build content directories and converts them back to AssetBundles, copying the schemas from your Packed Assets template.
