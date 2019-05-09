﻿#if !UNITY_2019_2_OR_NEWER || PACKAGE_UGUI

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace UnityEditor.Localization
{
    /// <summary>
    /// Attempts to setup a component for localizing.
    /// </summary>
    public static partial class LocalizeComponent
    {
        public static LocalizationBehaviour SetupForLocalization(Text target)
        {
            var comp = Undo.AddComponent(target.gameObject, typeof(LocalizeString)) as LocalizeString;
            var setStringMethod = target.GetType().GetProperty("text").GetSetMethod();
            var methodDelegate = System.Delegate.CreateDelegate(typeof(UnityAction<string>), target, setStringMethod) as UnityAction<string>;
            Events.UnityEventTools.AddPersistentListener(comp.UpdateString, methodDelegate);

            // Check if we can find a matching key to the text value
            var tables = LocalizationEditorSettings.GetAssetTablesCollection<StringTableBase>();
            foreach (var assetTableCollection in tables)
            {
                var keys = assetTableCollection.Keys;
                if (keys != null && keys.Contains(target.text))
                {
                    comp.StringReference.KeyId = keys.GetId(target.text);
                    comp.StringReference.TableName = assetTableCollection.TableName;
                    return comp;
                }
            }
            return comp;
        }

        public static LocalizationBehaviour SetupForLocalization(RawImage target)
        {
            var comp = Undo.AddComponent(target.gameObject, typeof(LocalizeTexture2D)) as LocalizeTexture2D;
            var setTextureMethod = target.GetType().GetProperty("texture").GetSetMethod();
            var methodDelegate = System.Delegate.CreateDelegate(typeof(UnityAction<Texture2D>), target, setTextureMethod) as UnityAction<Texture2D>;
            Events.UnityEventTools.AddPersistentListener(comp.UpdateAsset, methodDelegate);
            return comp;
        }
    }
}

#endif