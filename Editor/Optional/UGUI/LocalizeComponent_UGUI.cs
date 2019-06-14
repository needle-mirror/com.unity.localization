#if !UNITY_2019_2_OR_NEWER || PACKAGE_UGUI

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
            const int kMatchThreshold = 5;
            var comp = Undo.AddComponent(target.gameObject, typeof(LocalizeString)) as LocalizeString;
            var setStringMethod = target.GetType().GetProperty("text").GetSetMethod();
            var methodDelegate = System.Delegate.CreateDelegate(typeof(UnityAction<string>), target, setStringMethod) as UnityAction<string>;
            Events.UnityEventTools.AddPersistentListener(comp.UpdateString, methodDelegate);

            // Check if we can find a matching key to the text value
            var tables = LocalizationEditorSettings.GetAssetTablesCollection<StringTableBase>();
            int currentMatchDistance = int.MaxValue;
            KeyDatabase.KeyDatabaseEntry currentEntry = null;
            string tableName = string.Empty;
            foreach (var assetTableCollection in tables)
            {
                var keys = assetTableCollection.Keys;
                var foundKey = keys.FindSimilarKey(target.text, out int distance);
                if (foundKey != null && distance < currentMatchDistance)
                {
                    currentMatchDistance = distance;
                    currentEntry = foundKey;
                    tableName = assetTableCollection.TableName;
                }
            }

            if (currentEntry != null && currentMatchDistance < kMatchThreshold)
            {
                comp.StringReference.KeyId = currentEntry.Id;
                comp.StringReference.TableName = tableName;
                return comp;
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