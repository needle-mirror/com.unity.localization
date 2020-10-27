using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.Tests
{
    public abstract class StringTableTestsBase
    {
        protected StringTable Table { get; set; }

        [SetUp]
        public virtual void Setup()
        {
            var sharedTableData = ScriptableObject.CreateInstance<SharedTableData>();
            Table = ScriptableObject.CreateInstance<StringTable>();
            Table.SharedData = sharedTableData;

            // Start with assets that are not dirty
            EditorUtility.ClearDirty(Table);
            EditorUtility.ClearDirty(sharedTableData);
        }

        [TearDown]
        public virtual void Teardown()
        {
            Object.DestroyImmediate(Table.SharedData);
            Object.DestroyImmediate(Table);
        }
    }
}
