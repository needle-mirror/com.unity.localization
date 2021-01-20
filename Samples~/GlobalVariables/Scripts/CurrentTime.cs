using System;
using UnityEngine.Localization.SmartFormat.GlobalVariables;

namespace UnityEngine.Localization.Samples
{
    /// <summary>
    /// This is an example of a Global Variable that can return the current time.
    /// </summary>
    [DisplayName("Current Date Time")]
    public class CurrentTime : IGlobalVariable
    {
        public object SourceValue => DateTime.Now;
    }
}
