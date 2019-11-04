#if PACKAGE_TMP

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.Plugins.TMPro
{
    internal static class LocalizeComponent_TMPro
    {
        [MenuItem("CONTEXT/TextMeshProUGUI/Localize")]
        static void LocalizeTMProText(MenuCommand command)
        {
            var target = command.context as TextMeshProUGUI;
            SetupForLocalization(target);
        }

        public static MonoBehaviour SetupForLocalization(TextMeshProUGUI target)
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
                comp.StringReference.TableReference = foundKey.collection.Keys.TableNameGuid;
            }

            return null;
        }
    }
}

#endif