using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Components;

namespace UnityEditor.Localization
{
    /// <summary>
    /// Attempts to setup a component for localizing.
    /// </summary>
    public static partial class LocalizeComponent
    {
        public static LocalizationBehaviour SetupForLocalization(AudioSource target)
        {
            var comp = Undo.AddComponent(target.gameObject, typeof(LocalizeAudioClip)) as LocalizeAudioClip;
            var setTextureMethod = target.GetType().GetProperty("clip").GetSetMethod();
            var methodDelegate = System.Delegate.CreateDelegate(typeof(UnityAction<AudioClip>), target, setTextureMethod) as UnityAction<AudioClip>;
            Events.UnityEventTools.AddPersistentListener(comp.UpdateAsset, methodDelegate);
            Events.UnityEventTools.AddVoidPersistentListener(comp.UpdateAsset, target.Play);
            return comp;
        }
    }
}