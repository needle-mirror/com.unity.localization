using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.Extensions;
using UnityEngine.Localization.SmartFormat.GlobalVariables;

namespace UnityEngine.Localization.Samples
{
    [ExecuteAlways]
    public class SetupScene : MonoBehaviour
    {
        public GlobalVariablesGroup group;

        void Awake()
        {
            // You would normally set this up through the Localization Settings Editor, however
            // we do it here for the sample so that it can work without any changes to the project.

            // Do we have a GlobalVariablesSource in our settings?
            var source = LocalizationSettings.StringDatabase.SmartFormatter.GetSourceExtension<GlobalVariablesSource>();
            if (source == null)
            {
                source = new GlobalVariablesSource(LocalizationSettings.StringDatabase.SmartFormatter);
                LocalizationSettings.StringDatabase.SmartFormatter.AddExtensions(source);
            }

            // Do we have a group called global-sample?
            if (!source.ContainsKey("global-sample"))
                source.Add("global-sample", group);
        }
    }
}
