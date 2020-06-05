using UnityEngine;

namespace UnityEditor.Localization.Reporting
{
    public class ProgressBarReporter : TaskTimerReporter
    {
        public bool ReportTaskSummaryInConsole { get; set; } = true;

        protected override void PrintStatus(string title, string description, float progress)
        {
            EditorUtility.DisplayProgressBar(title, description, progress);
        }

        protected override void PrintSummary(string summary, bool fail)
        {
            EditorUtility.ClearProgressBar();

            if (!ReportTaskSummaryInConsole)
                return;

            if (fail)
                Debug.LogError(summary);
            else
                Debug.Log(summary);
        }
    }
}
