#if ADDRESSABLES_ANALYZE && ADDRESSABLES_CD_SUPPORT

using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;

namespace UnityEditor.Localization.Addressables
{
    [InitializeOnLoad]
    class ActiveRulesToPackedAssetAnalyzeRule : AnalyzeRuleBase
    {
        public override string ruleName => "Localization: Switch Group Rules to Asset Bundles";

        static ActiveRulesToPackedAssetAnalyzeRule() =>
            AnalyzeSystem.RegisterNewRule<ActiveRulesToPackedAssetAnalyzeRule>();

        protected internal override void PerformAnalysis(AddressableAssetSettings settings)
        {
            Results.Clear();

            // Checked first, unlike the Content Directory rule: Packed Assets is also the template new groups
            // fall back to, so its absence is worth reporting whether or not a switch is wanted.
            var paTemplate = AddressableGroupTemplateResolution.FindTemplate(settings,
                AddressableGroupTemplateResolution.PackedAssetsTemplateName);

            if (paTemplate == null)
            {
                Results.Add(new AnalyzeResultWithFixAction
                {
                    resultName =
                        "Re-create the built-in Packed Assets Template.",
                    severity = UnityEditor.MessageType.Warning,
                    FixAction = () =>
                    {
                        if (!MigrateGroupSchemaConverter.EnsurePackedAssetsGroupTemplate(settings))
                            UnityEngine.Debug.LogWarning(
                                "Localization: could not re-create the built-in Packed Assets group template.");
                    }
                });
                return;
            }

            if (!MigrateGroupSchemaConverter.ActiveRulesNeedPackedAssetsTemplate(settings))
                return;

            Results.Add(new AnalyzeResultWithFixAction
            {
                resultName =
                    "Switch Group Template to Packed Assets for new Groups.",
                severity = UnityEditor.MessageType.Warning,
                FixAction = () =>
                {
                    if (!MigrateGroupSchemaConverter.ApplyPackedAssetsTemplateToActiveRules(settings))
                        UnityEngine.Debug.LogWarning(
                            "Localization: could not switch the active group template to Packed Assets " +
                            "(no template found, rules are not saved as an asset, or the rules already build " +
                            "asset bundles).");
                }
            });
        }
    }
}

#endif
