using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace UnityEditor.Localization
{
    /// <summary>
    /// Adds context menus to Localize components.
    /// </summary>
    public static class LocalizeContextMenuItem
    {
        [MenuItem("CONTEXT/Text/Localize")]
        static void LocalizeUIText(MenuCommand command)
        {
            var target = command.context as Text;
            var comp = Undo.AddComponent(target.gameObject, typeof(LocalizeString)) as LocalizeString;
            var setStringMethod = target.GetType().GetProperty("text").GetSetMethod();
            var methodDelegate = System.Delegate.CreateDelegate(typeof(UnityAction<string>), target, setStringMethod) as UnityAction<string>;
            Events.UnityEventTools.AddPersistentListener(comp.UpdateString, methodDelegate);

            // Check if we can find a matching key to the text value
            var tables = LocalizationPlayerSettings.GetAssetTables<StringTableBase>();
            foreach (var assetTableCollection in tables)
            {
                var keys = assetTableCollection.GetKeys();
                if (keys.Contains(target.text))
                {
                    comp.StringReference.Key = target.text;
                    comp.StringReference.TableName = assetTableCollection.TableName;
                    return;
                }
            }
        }

        [MenuItem("CONTEXT/RawImage/Localize")]
        static void LocalizeUIRawImage(MenuCommand command)
        {
            var target = command.context as RawImage;
            var comp = Undo.AddComponent(target.gameObject, typeof(LocalizeTexture2D)) as LocalizeTexture2D;
            var setTextureMethod = target.GetType().GetProperty("texture").GetSetMethod();
            var methodDelegate = System.Delegate.CreateDelegate(typeof(UnityAction<Texture2D>), target, setTextureMethod) as UnityAction<Texture2D>;
            Events.UnityEventTools.AddPersistentListener(comp.UpdateAsset, methodDelegate);
        }

        [MenuItem("CONTEXT/AudioSource/Localize")]
        static void LocalizeAudioSource(MenuCommand command)
        {
            var target = command.context as AudioSource;
            var comp = Undo.AddComponent(target.gameObject, typeof(LocalizeAudioClip)) as LocalizeAudioClip;
            var setTextureMethod = target.GetType().GetProperty("clip").GetSetMethod();
            var methodDelegate = System.Delegate.CreateDelegate(typeof(UnityAction<AudioClip>), target, setTextureMethod) as UnityAction<AudioClip>;
            Events.UnityEventTools.AddPersistentListener(comp.UpdateAsset, methodDelegate);
            Events.UnityEventTools.AddVoidPersistentListener(comp.UpdateAsset, target.Play);
        }
    }
}