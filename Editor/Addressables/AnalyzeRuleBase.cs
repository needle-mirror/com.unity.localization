#if ADDRESSABLES_ANALYZE

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.AddressableAssets.Build.AnalyzeRules;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

namespace UnityEditor.Localization.Addressables
{
    abstract class AnalyzeRuleBase : AnalyzeRule
    {
        public const string NoIssuesFoundMessage = "No issues found";

        protected internal class AnalyzeResultWithFixAction : AnalyzeResult
        {
            public Action FixAction { get; set; }

            /// <summary>
            /// Optional stable identifier (e.g. an Addressables group Guid) used to disambiguate
            /// results that would otherwise share the same <see cref="AnalyzeResult.resultName"/>.
            /// Addressables does not enforce unique group names, so this is used as a fallback
            /// to tell such results apart in the UI and when fixing selected results.
            /// </summary>
            public string Key { get; set; }
        }

        internal List<AnalyzeResult> Results { get; } = new List<AnalyzeResult>();

        public override bool CanFix => true;

#if ADDRESSABLES_CD_SUPPORT
        public override bool SupportsFixSelectedResults => true;

        public override void FixSelectedResults(AddressableAssetSettings settings, IReadOnlyCollection<string> selectedResultNames)
        {
            if (selectedResultNames == null || selectedResultNames.Count == 0)
                return;

            var selectedNames = selectedResultNames as HashSet<string> ?? new HashSet<string>(selectedResultNames);

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Fix Selected Results");
            int group = Undo.GetCurrentGroup();
            foreach (var analyzeResult in Results)
            {
                if (analyzeResult is AnalyzeResultWithFixAction fix && selectedNames.Contains(analyzeResult.resultName))
                    fix.FixAction?.Invoke();
            }
            Undo.CollapseUndoOperations(group);
        }
#endif

        public override List<AnalyzeResult> RefreshAnalysis(AddressableAssetSettings settings)
        {
            Results.Clear();
            PerformAnalysis(settings);
            EnsureUniqueResultNames();
            if (Results.Count == 0)
                Results.Add(new AnalyzeResult { resultName = NoIssuesFoundMessage });

            return Results;
        }

        /// <summary>
        /// Addressables does not enforce unique group names, so two results can end up sharing an
        /// identical <see cref="AnalyzeResult.resultName"/> (e.g. two groups with the same name).
        /// The Analyze window merges rows with identical names into a single entry, so such results
        /// would otherwise be indistinguishable and impossible to fix independently of one another.
        /// This appends a disambiguator - the result's <see cref="AnalyzeResultWithFixAction.Key"/>
        /// when available, otherwise a running index - only to the results that actually collide.
        /// </summary>
        void EnsureUniqueResultNames()
        {
            Dictionary<string, List<AnalyzeResult>> resultsByName = null;
            foreach (var result in Results)
            {
                if (string.IsNullOrEmpty(result.resultName))
                    continue;

                if (resultsByName == null)
                    resultsByName = new Dictionary<string, List<AnalyzeResult>>();
                if (!resultsByName.TryGetValue(result.resultName, out var resultsWithName))
                    resultsByName[result.resultName] = resultsWithName = new List<AnalyzeResult>();
                resultsWithName.Add(result);
            }

            if (resultsByName == null)
                return;

            foreach (var resultsWithName in resultsByName.Values)
            {
                if (resultsWithName.Count < 2)
                    continue;

                var originalName = resultsWithName[0].resultName;
                for (int i = 0; i < resultsWithName.Count; ++i)
                {
                    var result = resultsWithName[i];
                    var key = (result as AnalyzeResultWithFixAction)?.Key;
                    result.resultName = !string.IsNullOrEmpty(key)
                        ? $"{originalName} ({key})"
                        : $"{originalName} ({i + 1})";
                }
            }
        }

        /// <summary>
        /// Adds a fixable result for every group in <paramref name="groups"/> that has no usable schemas.
        /// The fix copies the schemas from whichever template the active <see cref="AddressableGroupRules"/>
        /// resolves, so a Content Directory project repairs to a Content Directory schema rather than to a
        /// bundled one. With no template at all it warns and adds bundled and content update defaults.
        /// </summary>
        /// <remarks>
        /// The template is resolved when the fix runs, not when the analysis runs, so a rules asset edited
        /// in between is honoured.
        /// </remarks>
        internal void AddGroupsWithNoSchemasResults(IEnumerable<AddressableAssetGroup> groups, AddressableAssetSettings settings)
        {
            if (groups == null)
                return;

            foreach (var group in groups)
            {
                if (group == null || group.Schemas.Any(s => s != null))
                    continue;

                Results.Add(new AnalyzeResultWithFixAction
                {
                    resultName = $"{group.Name}:Addressables Group Contains No Schemas",
                    severity = MessageType.Error,
                    Key = group.Guid,
                    FixAction = () => AddressableGroupTemplateResolution.AddSchemasFromTemplate(
                        group,
                        AddressableGroupRules.Instance.GetEffectiveGroupTemplate(settings),
                        AddressableGroupTemplateResolution.PackedAssetsTemplateName,
                        typeof(BundledAssetGroupSchema),
                        typeof(ContentUpdateGroupSchema))
                });
            }
        }

        protected internal abstract void PerformAnalysis(AddressableAssetSettings settings);

        public override void ClearAnalysis()
        {
            Results.Clear();
        }

        public override void FixIssues(AddressableAssetSettings settings)
        {
            foreach (var analyzeResult in Results)
            {
                if (analyzeResult is AnalyzeResultWithFixAction fix)
                    fix.FixAction?.Invoke();
            }
        }
    }
}

#endif
