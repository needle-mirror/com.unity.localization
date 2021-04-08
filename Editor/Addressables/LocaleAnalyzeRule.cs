using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.AnalyzeRules;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace UnityEditor.Localization.Addressables
{
    [InitializeOnLoad]
    class LocaleAnalyzeRule : AnalyzeRule
    {
        class LocaleResult : AnalyzeResult
        {
            public Action FixAction { get; set; }
        }

        readonly List<AnalyzeResult> m_Results = new List<AnalyzeResult>();

        public override string ruleName => "Check Localization Locales";

        static LocaleAnalyzeRule() => AnalyzeSystem.RegisterNewRule<LocaleAnalyzeRule>();

        public override bool CanFix => true;

        public override List<AnalyzeResult> RefreshAnalysis(AddressableAssetSettings settings)
        {
            m_Results.Clear();

            var locales = AssetDatabase.FindAssets("t:Locale");
            foreach (var guid in locales)
            {
                var entry = settings.FindAssetEntry(guid);
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var locale = AssetDatabase.LoadAssetAtPath<Locale>(path);

                if (entry == null)
                {
                    m_Results.Add(new LocaleResult
                    {
                        resultName = $"{locale.LocaleName} - {path}:Not Marked as Addressable",
                        severity = MessageType.Error,
                        FixAction = () => LocalizationEditorSettings.AddLocale(locale)
                    });
                    continue;
                }

                var groupName = AddressableGroupRules.Instance.LocaleResolver.GetExpectedGroupName(new[] {locale.Identifier}, locale, settings);

                if (entry.parentGroup.Name != groupName)
                {
                    m_Results.Add(new LocaleResult
                    {
                        resultName = $"{locale.LocaleName} - {path}:Incorrect Group:Expected `{groupName}` but was `{entry.parentGroup.Name}`",
                        severity = MessageType.Warning,
                        FixAction = () => AddressableGroupRules.Instance.LocaleResolver.AddToGroup(locale, new[] { locale.Identifier }, settings, false)
                    });
                }

                if (!entry.labels.Contains(LocalizationSettings.LocaleLabel))
                {
                    m_Results.Add(new LocaleResult
                    {
                        resultName = $"{locale.LocaleName} - {path}:Missing Locale label",
                        severity = MessageType.Error,
                        FixAction = () =>
                        {
                            entry.SetLabel(LocalizationSettings.LocaleLabel, true, true);
                            LocalizationEditorSettings.EditorEvents.RaiseLocaleAdded(locale);
                        }
                    });
                }
            }

            if (m_Results.Count == 0)
                m_Results.Add(new AnalyzeResult { resultName  = "No issues found" });

            return m_Results;
        }

        public override void ClearAnalysis()
        {
            m_Results.Clear();
        }

        public override void FixIssues(AddressableAssetSettings settings)
        {
            foreach (var analyzeResult in m_Results.Cast<LocaleResult>())
            {
                analyzeResult.FixAction();
            }
        }
    }
}
