#if PACKAGE_UGUI

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Tables;
using UnityEngine.UI;

namespace UnityEditor.Localization.Plugins.UGUI
{
    /// <summary>
    /// Attempts to setup a component for localizing.
    /// </summary>
    internal static class LocalizeComponent_UGUI
    {
        [MenuItem("CONTEXT/Text/Localize")]
        static void LocalizeUIText(MenuCommand command)
        {
            var target = command.context as Text;
            SetupForLocalization(target);
        }

        [MenuItem("CONTEXT/RawImage/Localize")]
        static void LocalizeUIRawImage(MenuCommand command)
        {
            var target = command.context as RawImage;
            SetupForLocalization(target);
        }

        public static MonoBehaviour SetupForLocalization(Text target)
        {
            var comp = Undo.AddComponent(target.gameObject, typeof(LocalizeStringBehaviour)) as LocalizeStringBehaviour;
            var setStringMethod = target.GetType().GetProperty("text").GetSetMethod();
            var methodDelegate = System.Delegate.CreateDelegate(typeof(UnityAction<string>), target, setStringMethod) as UnityAction<string>;
            Events.UnityEventTools.AddPersistentListener(comp.OnUpdateString, methodDelegate);

            const int kMatchThreshold = 5;
            var foundKey = LocalizationEditorSettings.FindSimilarKey<StringTable>(target.text);

            if (foundKey.entry != null && foundKey.matchDistance < kMatchThreshold)
            {
                comp.StringReference.TableEntryReference = foundKey.entry.Id;
                comp.StringReference.TableReference = foundKey.collection.SharedData.TableNameGuid;
            }

            return comp;
        }

        public static MonoBehaviour SetupForLocalization(RawImage target)
        {
            var comp = Undo.AddComponent(target.gameObject, typeof(LocalizeTextureBehaviour)) as LocalizeTextureBehaviour;
            var setTextureMethod = target.GetType().GetProperty("texture").GetSetMethod();
            var methodDelegate = System.Delegate.CreateDelegate(typeof(UnityAction<Texture>), target, setTextureMethod) as UnityAction<Texture>;
            Events.UnityEventTools.AddPersistentListener(comp.OnUpdateAsset, methodDelegate);
            return comp;
        }
    }
}

#endif
