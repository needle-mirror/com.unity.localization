using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.AnalyzeRules;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.Addressables
{
    [InitializeOnLoad]
    class StringTableAnalyzeRule : TableAnalyzeRule<StringTable>
    {
        public override string ruleName => "Check Localization String Tables";

        static StringTableAnalyzeRule() => AnalyzeSystem.RegisterNewRule<StringTableAnalyzeRule>();
    }

    class TableAnalyzeRule<TTable> : AnalyzeRule where TTable : LocalizationTable
    {
        protected class TableResult : AnalyzeResult
        {
            public Action FixAction { get; set; }
        }

        protected readonly List<AnalyzeResult> m_Results = new List<AnalyzeResult>();

        public override bool CanFix => true;

        public override List<AnalyzeResult> RefreshAnalysis(AddressableAssetSettings settings)
        {
            m_Results.Clear();
            Analyze(settings, AddressableGroupRules.Instance.StringTablesResolver);

            if (m_Results.Count == 0)
                m_Results.Add(new AnalyzeResult { resultName = "No issues found" });

            return m_Results;
        }

        protected virtual void Analyze(AddressableAssetSettings settings, GroupResolver resolver)
        {
            try
            {
                EditorUtility.DisplayProgressBar(ruleName, "Finding Tables", 0);
                var tables = AssetDatabase.FindAssets($"t:{typeof(TTable).Name}");

                for (var i = 0; i < tables.Length; ++i)
                {
                    var progress = i / (float)tables.Length;

                    var guid = tables[i];
                    var entry = settings.FindAssetEntry(guid);
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var table = AssetDatabase.LoadAssetAtPath<TTable>(path);
                    var label = $"{table} - {path}";

                    EditorUtility.DisplayProgressBar(ruleName, $"Checking Table {path}", progress);

                    var collection = LocalizationEditorSettings.GetCollectionForSharedTableData(table.SharedData);
                    if (collection == null)
                    {
                        m_Results.Add(new TableResult
                        {
                            resultName = $"{table} - {path}:Loose Table.",
                            severity = MessageType.Info,
                            // TODO: Create collection for it?
                        });
                        continue;
                    }

                    CheckContents(table, label, settings, collection);

                    if (entry == null)
                    {
                        m_Results.Add(new TableResult
                        {
                            resultName = $"{label}:Not Marked as Addressable",
                            severity = MessageType.Error,
                            FixAction = () =>
                            {
                                collection.AddTable(table);
                                collection.AddSharedTableDataToAddressables();
                            }
                        });
                        continue;
                    }

                    // Group Name
                    var groupName = resolver.GetExpectedGroupName(new[] { table.LocaleIdentifier }, table, settings);
                    if (entry.parentGroup.Name != groupName)
                    {
                        m_Results.Add(new TableResult
                        {
                            resultName = $"{label}:Incorrect Group:Expected `{groupName}` but was `{entry.parentGroup.Name}`",
                            severity = MessageType.Warning,
                            FixAction = () => resolver.AddToGroup(table, new[] { table.LocaleIdentifier }, settings, false)
                        });
                    }

                    // Label
                    var expectedLabel = AddressHelper.FormatAssetLabel(table.LocaleIdentifier);
                    if (!entry.labels.Contains(expectedLabel))
                    {
                        m_Results.Add(new TableResult
                        {
                            resultName = $"{label}:Missing Locale label.",
                            severity = MessageType.Warning,
                            FixAction = () => entry.SetLabel(expectedLabel, true, true)
                        });
                    }

                    // Address
                    var expectedAddress = AddressHelper.GetTableAddress(table.TableCollectionName, table.LocaleIdentifier);
                    if (!entry.labels.Contains(expectedLabel))
                    {
                        m_Results.Add(new TableResult
                        {
                            resultName = $"{label}:Incorrect Address:Expected `{expectedAddress}` but was `{entry.address}`",
                            severity = MessageType.Error,
                            FixAction = () => entry.address = expectedAddress
                        });
                    }

                    // Shared Table Data
                    var sharedGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(table.SharedData));
                    var g = new Guid(sharedGuid);
                    if (table.SharedData.TableCollectionNameGuid != g)
                    {
                        m_Results.Add(new TableResult
                        {
                            resultName = $"{label}:Incorrect Shared Table Guid:Expected {g} but was {table.SharedData.TableCollectionNameGuid}",
                            severity = MessageType.Error,
                            FixAction = () =>
                            {
                                table.SharedData.TableCollectionNameGuid = g;
                                EditorUtility.SetDirty(table.SharedData);
                            }
                        });
                    }

                    var sharedEntry = settings.FindAssetEntry(sharedGuid);
                    if (sharedEntry == null)
                    {
                        m_Results.Add(new TableResult
                        {
                            resultName = $"{label}:Shared Table Not Marked as Addressable",
                            severity = MessageType.Warning,
                            FixAction = () => resolver.AddToGroup(table.SharedData, null, settings, false)
                        });
                        continue;
                    }

                    // Shared Group Name
                    var sharedGroupName = resolver.GetExpectedGroupName(null, table.SharedData, settings);
                    if (sharedEntry.parentGroup.Name != sharedGroupName)
                    {
                        m_Results.Add(new TableResult
                        {
                            resultName = $"{label}:Incorrect Shared Table Data Group:Expected `{sharedGroupName}` but was `{sharedEntry.parentGroup.Name}`",
                            severity = MessageType.Warning,
                            FixAction = () => resolver.AddToGroup(table.SharedData, null, settings, false)
                        });
                    }

                    var expectedSharedGroupName = resolver.GetExpectedGroupName(null, table.SharedData, settings);
                    if (sharedEntry.parentGroup.Name != expectedSharedGroupName)
                    {
                        m_Results.Add(new TableResult
                        {
                            resultName = $"{label}:Incorrect Group:Expected `{expectedSharedGroupName}` but was `{sharedEntry.parentGroup.Name}`",
                            severity = MessageType.Warning,
                            FixAction = () => resolver.AddToGroup(table.SharedData, null, settings, false)
                        });
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        protected virtual void CheckContents(TTable table, string label, AddressableAssetSettings settings, LocalizationTableCollection collection) {}

        public override void ClearAnalysis()
        {
            m_Results.Clear();
        }

        public override void FixIssues(AddressableAssetSettings settings)
        {
            foreach (var analyzeResult in m_Results.Cast<TableResult>())
            {
                analyzeResult.FixAction?.Invoke();
            }
        }
    }
}
