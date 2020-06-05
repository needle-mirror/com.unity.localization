namespace UnityEditor.Localization.Reporting
{
    public interface ITaskReporter
    {
        void Start(string name, string description);
        void ReportProgress(string description, float progress);
        void Completed(string message);
        void Fail(string message);
    }
}
