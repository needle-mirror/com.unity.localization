using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.Extensions;
using UnityEngine.Localization.SmartFormat.GlobalVariables;

namespace UnityEngine.Localization.Samples
{
    public class RandomPlayerStats : MonoBehaviour
    {
        public string[] stats = new[] { "vitality", "endurance", "strength", "dexterity", "intelligence" };

        public void RandomStats()
        {
            var source = LocalizationSettings.StringDatabase.SmartFormatter.GetSourceExtension<GlobalVariablesSource>();
            var nestedGroup = source["global-sample"]["player"] as NestedGlobalVariablesGroup;

            // An UpdateScope or using BeginUpdating and EndUpdating can be used to combine multiple changes into a single Update.
            // This prevents unnecessary string refreshes when updating multiple Global Variables.
            using (GlobalVariablesSource.UpdateScope())
            {
                foreach (var name in stats)
                {
                    var variable = nestedGroup.Value[name] as IntGlobalVariable;
                    variable.Value = Random.Range(0, 10);
                }
            }
        }
    }
}
