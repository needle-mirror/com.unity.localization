using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.Localization.UI;
using UnityEngine;
using UnityEngine.Localization.Tables;
using Object = UnityEngine.Object;

namespace UnityEditor.Localization.Tests
{
    public class TableReferenceTests
    {
        [Test]
        public void AssigningGuid_SetsTypeTo_Guid()
        {
            var guid = Guid.NewGuid();
            TableReference tableReference = guid;
            Assert.AreEqual(TableReference.Type.Guid, tableReference.ReferenceType, "Expected type to be Guid when assigning a Guid.");
            Assert.AreEqual(guid, tableReference.TableCollectionNameGuid);
            Assert.IsNull(tableReference.TableCollectionName);
        }

        [Test]
        public void AssigningStringName_SetsTypeTo_Name()
        {
            const string name = "table collection name";
            TableReference tableReference = name;
            Assert.AreEqual(TableReference.Type.Name, tableReference.ReferenceType, "Expected type to be Name when assigning a string name.");
            Assert.AreEqual(Guid.Empty, tableReference.TableCollectionNameGuid);
            Assert.AreEqual(name, tableReference.TableCollectionName);
        }

        [Test]
        public void AssigningGuid_ReturnsTheSameGuidWithImplicitConversion()
        {
            var guid = Guid.NewGuid();
            TableReference tableReference = guid;
            Guid implicitGuid = tableReference;
            Assert.AreEqual(guid, implicitGuid, "Expected the same Guid to be returned when assigning a TableReference to a Guid");
        }

        [Test]
        public void AssigningStringName_ReturnsTheSameNameWithImplicitConversion()
        {
            const string name = "table collection name";
            TableReference tableReference = name;
            string implicitName = tableReference;
            Assert.AreEqual(name, implicitName, "Expected the same Name to be returned when assigning a TableReference to a string.");
        }

        [Test]
        public void Validate_ThrowsException_WhenTypeIsEmpty()
        {
            TableReference tableReference = new TableReference();
            Assert.Throws<ArgumentException>(tableReference.Validate);
        }

        [Test]
        public void Validate_ThrowsException_WhenTypeIsGuid_AndGuidIsEmpty()
        {
            TableReference tableReference = Guid.Empty;
            Assert.Throws<ArgumentException>(tableReference.Validate);
        }

        [Test]
        public void Validate_DoesNotThrowException_WhenTypeIsGuid()
        {
            TableReference tableReference = Guid.NewGuid();
            Assert.DoesNotThrow(tableReference.Validate);
        }

        [Test]
        public void Validate_DoesNotThrowException_WhenTypeIsName()
        {
            TableReference tableReference = "table collection name";
            Assert.DoesNotThrow(tableReference.Validate);
        }

        [Test]
        public void GuidFromString_ParseValidGuidString()
        {
            const string guidString = "GUID:6ba6c90a0a3c4e688f102531a843db17";
            var expectedGuid = Guid.Parse("6ba6c90a0a3c4e688f102531a843db17");
            Assert.AreNotEqual(Guid.Empty, expectedGuid);

            Assert.AreEqual(expectedGuid, TableReference.GuidFromString(guidString));
        }

        [Test]
        public void StringToGuid_GeneratesValidString()
        {
            const string guidString = "6ba6c90a0a3c4e688f102531a843db17";
            var guid = Guid.Parse(guidString);
            Assert.AreNotEqual(Guid.Empty, guid);
            Assert.AreEqual(guidString, TableReference.StringFromGuid(guid));
        }

        [Test]
        public void GetSerializedString_ReturnsValidString_WhenUsingGuid()
        {
            const string guidString = "6ba6c90a0a3c4e688f102531a843db17";
            const string expectedSerializedString = "GUID:6ba6c90a0a3c4e688f102531a843db17";
            var guid = Guid.Parse(guidString);
            Assert.AreNotEqual(Guid.Empty, guid);

            TableReference tableReference = guid;
            Assert.AreEqual(expectedSerializedString, tableReference.GetSerializedString());
        }

        [Test]
        public void GetSerializedString_ReturnsValidString_WhenUsingName()
        {
            const string name = "table collection name";
            TableReference tableReference = name;
            Assert.AreEqual(name, tableReference.GetSerializedString());
        }

        [Test]
        public void IsGuid_ReturnsTrue_WhenStringIsAGuidString()
        {
            const string guidString = "GUID:6ba6c90a0a3c4e688f102531a843db17";
            Assert.IsTrue(TableReference.IsGuid(guidString));
        }

        [Test]
        public void IsGuid_ReturnsFalse_WhenStringIsNotAGuidString()
        {
            const string guidString = "not a guid string";
            Assert.IsFalse(TableReference.IsGuid(guidString));
        }

        [Test]
        public void IsGuid_ReturnsFalse_WhenStringIsEmpty()
        {
            const string guidString = "";
            Assert.IsFalse(TableReference.IsGuid(guidString));
        }

        [Test]
        public void IsGuid_ReturnsFalse_WhenStringIsNull()
        {
            const string guidString = null;
            Assert.IsFalse(TableReference.IsGuid(guidString));
        }

        public static List<(bool expected, TableReference a, TableReference b)> EqualsTestCases()
        {
            var guid = Guid.Parse("6ba6c90a0a3c4e688f102531a843db17");

            var cases = new List<(bool expected, TableReference a, TableReference b)>();
            cases.Add((true, "Key 1", "Key 1"));
            cases.Add((true, guid, guid));
            cases.Add((true, null, null));
            cases.Add((false, "test", guid));
            cases.Add((false, "test", null));
            cases.Add((false, guid, null));
            return cases;
        }

        [TestCaseSource("EqualsTestCases")]
        public void EqualsReturnsExpectedValue((bool expected, TableReference a, TableReference b) args)
        {
            Assert.AreEqual(args.expected, args.a.Equals(args.b));
        }

        [TestCaseSource("EqualsTestCases")]
        public void EqualsWithObjectReturnsExpectedValue((bool expected, TableReference a, TableReference b) args)
        {
            Assert.AreEqual(args.expected, args.a.Equals((object)args.b));
        }
    }
}
