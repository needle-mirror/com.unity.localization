using NUnit.Framework;
using UnityEditor.Localization.UI;

namespace UnityEditor.Localization.Tests.UI
{
    public class StringTableEditorTests : StringTableTestsBase
    {
        [Test]
        public void TableEntrySelected_CreateEntry_CreatesEmptyEntryWhenOneDoesNotExist()
        {
            var entry = Table.SharedData.AddKey("NEW KEY");
            var selected = new TableEntrySelected(Table, entry.Id, null, UnityEngine.Localization.Metadata.MetadataType.All);
            var editor = selected.CreateEditor();
            Assert.NotNull(editor, "Expected an editor to be created but it was null.");
            Assert.IsNotEmpty(Table.TableData, "Expected an entry to be added to the table data but it was not.");
        }
    }
}
