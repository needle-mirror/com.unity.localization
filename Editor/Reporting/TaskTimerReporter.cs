using System.Diagnostics;
using System.Text;

namespace UnityEditor.Localization.Reporting
{
    static class TaskReporter
    {
        public static ITaskReporter CreateDefaultReporter()
        {
            #if UNITY_2020_1_OR_NEWER
            return new ProgressReporter();
            #else
            return new ProgressBarReporter();
            #endif
        }
    }

    public abstract class TaskTimerReporter : ITaskReporter
    {
        public float CurrentProgress { get; private set; }

        public bool Active { get; private set; }

        public bool Started { get { return m_Started; } }

        StringBuilder m_Summary = new StringBuilder();
        Stopwatch m_Timer;
        string m_Title;
        string m_CurrentDescription;
        double m_TotalTimeInSeconds;
        bool m_Started;

        public virtual void Start(string title, string description)
        {
            if (m_Started)
                return;

            m_Started = true;
            m_Title = title;
            m_CurrentDescription = description;
            m_Summary.Clear();
            m_Timer = Stopwatch.StartNew();
            m_TotalTimeInSeconds = 0;
        }

        public void ReportProgress(string description, float progress)
        {
            if (!m_Started)
            {
                Start(description, description);
            }

            CurrentProgress = progress;
            LogStep();
            PrintStatus(m_Title, description, progress);
            m_CurrentDescription = description;
            m_Timer.Restart();
        }

        public void Completed(string message)
        {
            if (!m_Started)
                return;

            m_Started = false;
            CurrentProgress = 1;
            LogStep();
            m_Summary.AppendFormat("Total time: {0:0.##}s\n{1}", m_TotalTimeInSeconds, message);
            m_Summary.Insert(0, $"{m_Title}\n");
            PrintSummary(m_Summary.ToString(), false);
        }

        public void Fail(string message)
        {
            if (!m_Started)
                return;

            m_Started = false;
            LogStep();
            m_Summary.AppendFormat("Total time: {0:0.##}s", m_TotalTimeInSeconds);
            m_Summary.Insert(0, $"{m_Title}: Failed\n{message}\n");
            PrintSummary(m_Summary.ToString(), true);
        }

        void LogStep()
        {
            double secs = m_Timer.Elapsed.TotalSeconds;
            m_TotalTimeInSeconds += secs;
            m_Summary.AppendFormat("{0}: {1:0.##}s\n", m_CurrentDescription, secs);
        }

        protected abstract void PrintStatus(string title, string description, float progress);

        protected abstract void PrintSummary(string summary, bool fail);
    }
}
