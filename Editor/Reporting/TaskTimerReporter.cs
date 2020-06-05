using System.Diagnostics;
using System.Text;

namespace UnityEditor.Localization.Reporting
{
    public abstract class TaskTimerReporter : ITaskReporter
    {
        StringBuilder m_Summary = new StringBuilder();
        Stopwatch m_Timer;
        string m_Title;
        string m_CurrentDescription;
        double m_TotalTimeInSeconds;

        public virtual void Start(string title, string description)
        {
            m_Title = title;
            m_CurrentDescription = description;
            m_Summary.Clear();
            m_Timer = Stopwatch.StartNew();
            m_TotalTimeInSeconds = 0;
        }

        public void ReportProgress(string description, float progress)
        {
            LogStep();
            PrintStatus(m_Title, description, progress);
            m_CurrentDescription = description;
            m_Timer.Restart();
        }

        public void Completed(string message)
        {
            LogStep();
            m_Summary.AppendFormat("Total time: {0:0.##}s\n{1}", m_TotalTimeInSeconds, message);
            m_Summary.Insert(0, $"{m_Title}\n");
            PrintSummary(m_Summary.ToString(), false);
        }

        public void Fail(string message)
        {
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
