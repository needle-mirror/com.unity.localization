using NUnit.Framework;

namespace UnityEditor.Localization.Tests
{
    public class EditorIconsTests
    {
        [Test]
        public void LocalizationSettings_IconIsNotNull()
        {
            Assert.NotNull(EditorIcons.LocalizationSettings.image);
        }

        [Test]
        public void AssetTable_IconIsNotNull()
        {
            Assert.NotNull(EditorIcons.AssetTable.image);
        }

        [Test]
        public void StringTable_IconIsNotNull()
        {
            Assert.NotNull(EditorIcons.StringTable.image);
        }

        [Test]
        public void WarningIcon_IconIsNotNull()
        {
            Assert.NotNull(EditorIcons.WarningIcon);
        }
    }
}
