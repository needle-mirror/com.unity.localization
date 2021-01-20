using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.Extensions;
using UnityEngine.Localization.SmartFormat.GlobalVariables;
using UnityEngine.UI;

namespace UnityEngine.Localization.Samples
{
    public class ChangePlayerStats : MonoBehaviour
    {
        public Slider slider;
        public string stat;

        IntGlobalVariable m_Variable;
        bool m_IsUpdating;

        void Start()
        {
            var source = LocalizationSettings.StringDatabase.SmartFormatter.GetSourceExtension<GlobalVariablesSource>();
            var nestedGroup = source["global-sample"]["player"] as NestedGlobalVariablesGroup;
            m_Variable = nestedGroup.Value[stat] as IntGlobalVariable;

            RefreshSliderValue();
            slider.onValueChanged.AddListener(OnValueChanges);

            // We can be notified when a change occurs
            m_Variable.ValueChanged += VariableValueChanged;
        }

        private void VariableValueChanged(IGlobalVariable obj)
        {
            if (m_IsUpdating)
                return;

            // Is this a batch update? If so we can defer to the end so that we dont get multiple value changed calls
            if (GlobalVariablesSource.IsUpdating)
            {
                m_IsUpdating = true;
                GlobalVariablesSource.EndUpdate += RefreshSliderValue;
            }
            else
            {
                RefreshSliderValue();
            }
        }

        void RefreshSliderValue()
        {
            slider.value = m_Variable.Value;

            if (m_IsUpdating)
            {
                GlobalVariablesSource.EndUpdate -= RefreshSliderValue;
                m_IsUpdating = false;
            }
        }

        void OnValueChanges(float val)
        {
            m_Variable.Value = (int)val;
        }
    }
}
