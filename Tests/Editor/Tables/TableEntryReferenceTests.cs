using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization.Tables;
using Object = UnityEngine.Object;

namespace UnityEditor.Localization.Tests
{
    public class TableEntryReferenceTests
    {
        [Test]
        public void AssigningIntId_SetsTypeTo_Id()
        {
            TableEntryReference tableEntryReference = 123;
            Assert.AreEqual(TableEntryReference.Type.Id, tableEntryReference.ReferenceType, "Expected type to be Id when assigning an integer.");
            Assert.AreEqual(123, tableEntryReference.KeyId, "Expected reference id to be set.");
            Assert.IsNull(tableEntryReference.Key, "Key should not be set when using a Key Id.");
        }

        [Test]
        public void AssigningStringName_SetsTypeTo_Name()
        {
            TableEntryReference tableEntryReference = "Name";
            Assert.AreEqual(TableEntryReference.Type.Name, tableEntryReference.ReferenceType, "Expected type to be Name when assigning a string.");
            Assert.AreEqual("Name", tableEntryReference.Key, "Expected reference id to be set.");
            Assert.AreEqual(SharedTableData.EmptyId, tableEntryReference.KeyId, "Key id be EmptyId when using a Name.");
        }

        [Test]
        public void AssigningIntId_ReturnsTheSameIdWithImplicitConversion()
        {
            const long id = 432;
            TableEntryReference tableEntryReference = id;
            long implicitId = tableEntryReference;
            Assert.AreEqual(id, implicitId, "Expected the same id to be returned when assigning a TableEntryReference to a uint.");
        }

        [Test]
        public void AssigningStringName_ReturnsTheSameNameWithImplicitConversion()
        {
            const string name = "key name";
            TableEntryReference tableEntryReference = name;
            string implicitName = tableEntryReference;
            Assert.AreEqual(name, implicitName, "Expected the same Name to be returned when assigning a TableEntryReference to a string.");
        }

        [Test]
        public void Validate_ThrowsException_WhenTypeIsEmpty()
        {
            TableEntryReference tableEntryReference = new TableEntryReference();
            Assert.Throws<ArgumentException>(tableEntryReference.Validate);
        }

        [Test]
        public void Validate_DoesNotThrowException_WhenTypeIsKeyId()
        {
            TableEntryReference tableEntryReference = 123;
            Assert.DoesNotThrow(tableEntryReference.Validate);
        }

        [Test]
        public void Validate_DoesNotThrowException_WhenTypeIsName()
        {
            TableEntryReference tableEntryReference = "Key name";
            Assert.DoesNotThrow(tableEntryReference.Validate);
        }

        [Test]
        public void ResolveKeyName_ReturnsTheSameName_WhenTypeIsName()
        {
            const string keyName = "key name";

            var sharedData = ScriptableObject.CreateInstance<SharedTableData>();

            TableEntryReference tableEntryReference = keyName;
            Assert.AreEqual(keyName, tableEntryReference.ResolveKeyName(sharedData), "Expected key name to be the same when type is Name");

            Object.DestroyImmediate(sharedData);
        }

        [Test]
        public void ResolveKeyName_ReturnsTheNameFromTheSharedTableData_WhenTypeIsKeyId()
        {
            const string keyName = "key name";

            var sharedData = ScriptableObject.CreateInstance<SharedTableData>();
            sharedData.AddKey("some key 1");
            sharedData.AddKey("some key 2");
            sharedData.AddKey("some key 3");

            var keyEntry = sharedData.AddKey(keyName);

            TableEntryReference tableEntryReference = keyEntry.Id;
            Assert.AreEqual(TableEntryReference.Type.Id, tableEntryReference.ReferenceType);
            Assert.AreEqual(keyName, tableEntryReference.ResolveKeyName(sharedData), "Expected key name to be extracted from SharedTableData type is Id");

            Object.DestroyImmediate(sharedData);
        }

        [Test]
        public void ResolveKeyName_ReturnsNull_WhenKeyIdIsNotInSharedTableData()
        {
            const string keyName = "key name";

            var sharedData = ScriptableObject.CreateInstance<SharedTableData>();
            sharedData.AddKey("some key 1");
            sharedData.AddKey("some key 2");
            sharedData.AddKey("some key 3");
            sharedData.AddKey(keyName);

            TableEntryReference tableEntryReference = 123;
            Assert.AreEqual(TableEntryReference.Type.Id, tableEntryReference.ReferenceType);
            Assert.IsNull(tableEntryReference.ResolveKeyName(sharedData), "Expected null to be returned when the key id can not be found.");

            Object.DestroyImmediate(sharedData);
        }

        [Test]
        public void ResolveKeyName_ReturnsNull_ForEmptyReference()
        {
            const string keyName = "key name";

            var sharedData = ScriptableObject.CreateInstance<SharedTableData>();
            sharedData.AddKey("some key 1");
            sharedData.AddKey("some key 2");
            sharedData.AddKey("some key 3");
            sharedData.AddKey(keyName);

            TableEntryReference tableEntryReference = new TableEntryReference();
            Assert.AreEqual(TableEntryReference.Type.Empty, tableEntryReference.ReferenceType);
            Assert.IsNull(tableEntryReference.ResolveKeyName(sharedData), "Expected null to be returned when the reference is Empty.");

            Object.DestroyImmediate(sharedData);
        }

        public static List<(bool expected, TableEntryReference a, TableEntryReference b)> EqualsTestCases()
        {
            var cases = new List<(bool expected, TableEntryReference a, TableEntryReference b)>();
            cases.Add((true, "Key 1", "Key 1"));
            cases.Add((true, 123, 123));
            cases.Add((true, null, null));
            cases.Add((false, "test", 123));
            cases.Add((false, "test", null));
            cases.Add((false, 123, null));
            cases.Add((true, "", ""));
            return cases;
        }

        [TestCaseSource("EqualsTestCases")]
        public void EqualsReturnsExpectedValue((bool expected, TableEntryReference a, TableEntryReference b) args)
        {
            Assert.AreEqual(args.expected, args.a.Equals(args.b));
        }

        [TestCaseSource("EqualsTestCases")]
        public void EqualsWithObjectReturnsExpectedValue((bool expected, TableEntryReference a, TableEntryReference b) args)
        {
            Assert.AreEqual(args.expected, args.a.Equals((object)args.b));
        }
    }
}
