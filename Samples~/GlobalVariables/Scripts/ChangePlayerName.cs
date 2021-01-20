using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.Extensions;
using UnityEngine.Localization.SmartFormat.GlobalVariables;
using UnityEngine.UI;

namespace UnityEngine.Localization.Samples
{
    public class ChangePlayerName : MonoBehaviour
    {
        public InputField input;

        public string PlayerName
        {
            get => GetVariable().Value;
            set => GetVariable().Value = value;
        }

        StringGlobalVariable GetVariable()
        {
            var source = LocalizationSettings.StringDatabase.SmartFormatter.GetSourceExtension<GlobalVariablesSource>();
            var playerName = source["global-sample"]["player-name"] as StringGlobalVariable;
            return playerName;
        }

        void Start()
        {
            input.text = PlayerName;
            input.onValueChanged.AddListener(OnValueChanges);
        }

        void OnValueChanges(string val)
        {
            PlayerName = val;
        }
    }
}
