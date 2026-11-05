#if ADDRESSABLES_ANALYZE && ADDRESSABLES_CD_SUPPORT

using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;

namespace UnityEditor.Localization.Addressables
{
    [InitializeOnLoad]
    class ActiveRulesToContentDirectoryAnalyzeRule : AnalyzeRuleBase
    {
        public override string ruleName => "Localization: Switch Group Rules to Content Directories";

        static ActiveRulesToContentDirectoryAnalyzeRule() =>
            AnalyzeSystem.RegisterNewRule<ActiveRulesToContentDirectoryAnalyzeRule>();

        protected internal override void PerformAnalysis(AddressableAssetSettings settings)
        {
            Results.Clear();

            // Asked before looking for the template: a project already on a custom content directory template
            // needs no switch, so it must not be told to re-create the built-in one.
            if (!MigrateGroupSchemaConverter.ActiveRulesNeedContentDirectoryTemplate(settings))
                return;

            var cdTemplate = AddressableGroupTemplateResolution.FindTemplate(settings,
                AddressableGroupTemplateResolution.ContentDirectoryTemplateName);

            if (cdTemplate == null)
            {
                Results.Add(new AnalyzeResultWithFixAction
                {
                    resultName =
                        "Re-create the built-in Content Directory Template.",
                    severity = UnityEditor.MessageType.Warning,
                    FixAction = () =>
                    {
                        if (!MigrateGroupSchemaConverter.EnsureContentDirectoryGroupTemplate(settings))
                            UnityEngine.Debug.LogWarning(
                                "Localization: could not re-create the built-in Content Directory group template.");
                    }
                });
                return;
            }

            Results.Add(new AnalyzeResultWithFixAction
            {
                resultName =
                    "Switch Group Template to Content Directory for new Groups.",
                severity = UnityEditor.MessageType.Warning,
                FixAction = () =>
                {
                    if (!MigrateGroupSchemaConverter.ApplyContentDirectoryTemplateToActiveRules(settings))
                        UnityEngine.Debug.LogWarning(
                            "Localization: could not switch the active group template to Content Directory " +
                            "(no template found, rules are not saved as an asset, or the rules already build " +
                            "content directories).");
                }
            });
        }
    }
}

#endif
