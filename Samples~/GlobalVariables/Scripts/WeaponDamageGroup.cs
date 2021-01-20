using System;
using UnityEngine.Localization.SmartFormat.GlobalVariables;

namespace UnityEngine.Localization.Samples
{
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
}
