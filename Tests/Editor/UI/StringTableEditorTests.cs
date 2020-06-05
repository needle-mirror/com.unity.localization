using NUnit.Framework;
using UnityEditor.Localization.UI;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.Tests.UI
{
    public class StringTableEditorTests
    {
        StringTable m_Table;

        [SetUp]
        public void Setup()
        {
            m_Table = ScriptableObject.CreateInstance<StringTable>();
            m_Table.SharedData = ScriptableObject.CreateInstance<SharedTableData>();
        }

        [TearDown]
        public void Teardown()
        {
            Object.DestroyImmediate(m_Table.SharedData);
            Object.DestroyImmediate(m_Table);
        }

        [Test]
        public void TableEntrySelected_CreateEntry_CreatesEmptyEntryWhenOneDoesNotExist()
        {
            var entry = m_Table.SharedData.AddKey("NEW KEY");
            var selected = new TableEntrySelected(m_Table, entry.Id, null, UnityEngine.Localization.Metadata.MetadataType.All);
            var editor = selected.CreateEditor();
            Assert.NotNull(editor, "Expected an editor to be created but it was null.");
            Assert.IsNotEmpty(m_Table.TableData, "Expected an entry to be added to the table data but it was not.");
        }
    }
}
