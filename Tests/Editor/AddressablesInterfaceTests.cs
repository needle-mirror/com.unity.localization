using NUnit.Framework;
using UnityEngine.Localization;

namespace UnityEditor.Localization.Tests
{
    public class AddressablesInterfaceTests
    {
        [Test]
        public void EditorAddressablesInterface_IsUsedInEditor()
        {
            AddressablesInterface.Instance = null;
            Assert.That(AddressablesInterface.Instance, Is.TypeOf<EditorAddressablesInterface>());
        }
    }
}
