#if ADDRESSABLES_ANALYZE && ADDRESSABLES_CD_SUPPORT

using System.Collections.Generic;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;

namespace UnityEditor.Localization.Addressables
{
    [InitializeOnLoad]
    class MigrateGroupsToPackedAssetsAnalyzeRule : AnalyzeRuleBase
    {
        public override string ruleName => "Localization: Migrate Groups to Asset Bundles";

        static MigrateGroupsToPackedAssetsAnalyzeRule() =>
            AnalyzeSystem.RegisterNewRule<MigrateGroupsToPackedAssetsAnalyzeRule>();

        protected internal override void PerformAnalysis(AddressableAssetSettings settings)
        {
            Results.Clear();

            var managed = new HashSet<AddressableAssetGroup>();
            MigrateGroupSchemaConverter.CollectLocalizationManagedGroups(settings, managed);

            foreach (var group in managed)
            {
                if (!MigrateGroupSchemaConverter.ShouldMigrateGroupToPackedAssets(group))
                    continue;

                var captured = group;
                Results.Add(new AnalyzeResultWithFixAction
                {
                    resultName =
                        $"{captured.Name}: Disable Content Directory Schema and enable Bundled Asset Schema.",
                    severity = UnityEditor.MessageType.Warning,
                    Key = captured.Guid,
                    FixAction = () => MigrateGroupSchemaConverter.ConvertGroupToPackedAssets(captured)
                });
            }
        }
    }
}

#endif
