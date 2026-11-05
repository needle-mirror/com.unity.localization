#if ADDRESSABLES_ANALYZE && ADDRESSABLES_CD_SUPPORT

using System.Collections.Generic;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;

namespace UnityEditor.Localization.Addressables
{
    [InitializeOnLoad]
    class MigrateGroupsToContentDirectoryAnalyzeRule : AnalyzeRuleBase
    {
        public override string ruleName => "Localization: Migrate Groups to Content Directory";

        static MigrateGroupsToContentDirectoryAnalyzeRule() =>
            AnalyzeSystem.RegisterNewRule<MigrateGroupsToContentDirectoryAnalyzeRule>();

        protected internal override void PerformAnalysis(AddressableAssetSettings settings)
        {
            Results.Clear();

            var managed = new HashSet<AddressableAssetGroup>();
            MigrateGroupSchemaConverter.CollectLocalizationManagedGroups(settings, managed);

            foreach (var group in managed)
            {
                if (!MigrateGroupSchemaConverter.ShouldMigrateGroupToContentDirectory(group))
                    continue;

                var captured = group;
                Results.Add(new AnalyzeResultWithFixAction
                {
                    resultName =
                        $"{captured.Name}: Disable Bundled Asset Schema and enable Content Directory Schema.",
                    severity = UnityEditor.MessageType.Warning,
                    Key = captured.Guid,
                    FixAction = () => MigrateGroupSchemaConverter.ConvertGroupToContentDirectory(captured)
                });
            }
        }
    }
}

#endif
