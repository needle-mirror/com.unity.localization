using NUnit.Framework;
using UnityEditor.Localization.UI;

namespace UnityEditor.Localization.Tests.UI
{
    public class AssetTableTypeFieldTests
    {
        [Test]
        public void IncludesDefaultTableTypes()
        {
            var defaultTypes = AddressableAssetTestBase.AllTableTypes();
            var choices = AssetTableTypeField.GetChoices();

            foreach(var typ in defaultTypes)
            {
                Assert.Contains(typ, choices, "Expected the type to be included in the Asset Table Type choices.");
            }
        }
    }
}
