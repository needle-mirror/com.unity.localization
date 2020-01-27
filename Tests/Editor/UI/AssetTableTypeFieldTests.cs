using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.Localization.UI;
using UnityEngine.Localization.Tables;
/*
namespace UnityEditor.Localization.Tests.UI
{
    public class AssetTableTypeFieldTests
    {
        public static List<Type> AllTableTypes()
        {
            return new List<Type>()
            {
                typeof(AudioClipAssetTable),
                typeof(SpriteAssetTable),
                typeof(StringTable),
                typeof(Texture2DAssetTable)
            };
        }

        [Test]
        public void IncludesDefaultTableTypes()
        {
            var defaultTypes = AllTableTypes();
            var choices = AssetTableTypeField.GetChoices();

            foreach(var typ in defaultTypes)
            {
                Assert.Contains(typ, choices, "Expected the type to be included in the Asset Table Type choices.");
            }
        }
    }
}*/
