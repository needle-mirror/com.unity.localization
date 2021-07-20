#if PACKAGE_UGUI && ENABLE_PROPERTY_VARIANTS

using System;
using UnityEngine.UI;

namespace UnityEngine.Localization.PropertyVariants.TrackedObjects
{
    /// <summary>
    /// Uses JSON to apply variant data to UGUI <see cref="Graphic"/> instances.
    /// Calls <see cref="Graphic.SetAllDirty"/> on the target object after applying changes.
    /// </summary>
    /// <example>
    /// This shows how to configure a <see cref="GameObjectLocalizer"/> to apply changes to a [Text](https://docs.unity3d.com/Packages/com.unity.ugui@latest/index.html?subfolder=/api/UnityEngine.UI.Text.html)
    /// component for the Font, Font Size and Text properties.
    /// <code source="../../../DocCodeSamples.Tests/GameObjectLocalizerSamples.cs" region="setup-text"/>
    /// </example>
    /// <example>
    /// This shows how to configure a <see cref="GameObjectLocalizer"/> to apply changes to a [TextMeshProUGUI](https://docs.unity3d.com/Packages/com.unity.textmeshpro@latest/index.html?subfolder=/api/TMPro.TextMeshProUGUI.html)
    /// component for the Font, Font Size and Text properties.
    /// <code source="../../../DocCodeSamples.Tests/GameObjectLocalizerSamples.cs" region="setup-tmp-text"/>
    /// </example>
    [Serializable]
    [DisplayName("UI Graphic")]
    [CustomTrackedObject(typeof(Graphic), true)]
    public class TrackedUGuiGraphic : JsonSerializerTrackedObject
    {
        protected override void PostApplyTrackedProperties()
        {
            ((Graphic)Target).SetAllDirty();
            base.PostApplyTrackedProperties();
        }
    }

    /// <summary>
    /// Uses JSON to apply variant data to <see cref="Dropdown"/>.
    /// Calls <see cref="Dropdown.RefreshShownValue"/> on the target object after applying changes.
    /// </summary>
    /// <example>
    /// This shows how to configure a <see cref="GameObjectLocalizer"/> to apply changes to a [Dropdown](https://docs.unity3d.com/Packages/com.unity.ugui@latest/index.html?subfolder=/api/UnityEngine.UI.Dropdown.html)
    /// for the options values.
    /// <code source="../../../DocCodeSamples.Tests/GameObjectLocalizerSamples.cs" region="setup-dropdown"/>
    /// </example>
    [Serializable]
    [DisplayName("UI Dropdown")]
    [CustomTrackedObject(typeof(Dropdown), true)]
    public class TrackedUGuiDropdown : JsonSerializerTrackedObject
    {
        protected override void PostApplyTrackedProperties()
        {
            ((Dropdown)Target).RefreshShownValue();
            base.PostApplyTrackedProperties();
        }
    }
}

#endif
