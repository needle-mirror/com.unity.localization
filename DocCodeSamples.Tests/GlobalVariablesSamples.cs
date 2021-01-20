using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.SmartFormat.Extensions;
using UnityEngine.Localization.SmartFormat.GlobalVariables;
using Random = UnityEngine.Random;

public class GlobalVariablesSamples : MonoBehaviour
{
    #region value-change-example

    void UpdateValue()
    {
        var source = LocalizationSettings.StringDatabase.SmartFormatter.GetSourceExtension<GlobalVariablesSource>();
        var myFloat = source["global"]["my-float"] as FloatGlobalVariable;
        myFloat.Value = 123; // This will trigger an update
    }

    #endregion
}

#region update-scope-example

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
#endregion

#region date-time-example

/// <summary>
/// This is an example of a Global Variable that can return the current time.
/// </summary>
[DisplayName("Current Date Time")]
public class CurrentTime : IGlobalVariable
{
    public object SourceValue => DateTime.Now;
}
#endregion

#region custom-value-changed-example

[Serializable]
public class MyGlobalVariable : IGlobalVariableValueChanged
{
    [SerializeField]
    string m_Value;

    public string Value
    {
        get => m_Value;
        set
        {
            if (m_Value == value)
                return;

            m_Value = value;
            ValueChanged?.Invoke(this);
        }
    }

    public object SourceValue => Value;

    public event Action<IGlobalVariable> ValueChanged;
}
#endregion

#region custom-group-example

struct ReturnValue : IGlobalVariable
{
    public object SourceValue { get; set; }
}

/// <summary>
/// This example shows how a nested group can be used to return custom data without the need for Reflection.
/// </summary>
[DisplayName("Weapon Damage")]
[Serializable]
public class WeaponDamageGroup : IVariableGroup, IGlobalVariable
{
    public object SourceValue => this;

    public bool TryGetValue(string key, out IGlobalVariable value)
    {
        switch (key)
        {
            case "sword":
                value = new ReturnValue { SourceValue = 6 };
                return true;

            case "mace":
                value = new ReturnValue { SourceValue = 5 };
                return true;

            case "axe":
                value = new ReturnValue { SourceValue = 8 };
                return true;

            case "dagger":
                value = new ReturnValue { SourceValue = 2 };
                return true;
        }

        value = null;
        return false;
    }
}
#endregion
