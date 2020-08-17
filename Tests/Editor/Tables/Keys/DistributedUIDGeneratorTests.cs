using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.Tests
{
    public class DistributedUIDGeneratorTests
    {
        const int kGeneratedIdCount = 5000;

        int m_MachineId;

        [SetUp]
        public void Setup()
        {
            // Backup the machine id so we don't break any projects on the same machine
            if (EditorPrefs.HasKey(DistributedUIDGenerator.MachineIdPrefKey))
                m_MachineId = EditorPrefs.GetInt(DistributedUIDGenerator.MachineIdPrefKey, 0);
        }

        [TearDown]
        public void Teardown()
        {
            if (EditorPrefs.HasKey(DistributedUIDGenerator.MachineIdPrefKey))
                EditorPrefs.SetInt(DistributedUIDGenerator.MachineIdPrefKey, m_MachineId);
        }

        [Test]
        public void MachineIdValuesAreClampedTo10bitRange()
        {
            const int kMachineIdBits = 10;
            int MaxNodeId = (int)(Mathf.Pow(2, kMachineIdBits) - 1);

            var generator = new DistributedUIDGenerator();

            // Too large
            generator.MachineId = MaxNodeId + 1;
            Assert.AreEqual(MaxNodeId, generator.MachineId, $"Expected value to be clamped when it is greater than MaxNodeId({MaxNodeId})");

            // Too small
            generator.MachineId = -123;
            Assert.AreEqual(1, generator.MachineId, "Expected value to be clamped when it is less than 1.");

            // Just right
            const int CorrectId = 321;
            generator.MachineId = CorrectId;
            Assert.AreEqual(CorrectId, generator.MachineId);
        }

        [Test]
        public void DefaultMachineIdIsTheSame()
        {
            var machineId = new DistributedUIDGenerator().MachineId;
            Assert.AreNotEqual(0, machineId, "Expected a valid machine id");

            for (int i = 0; i < 10; ++i)
            {
                Assert.AreEqual(machineId, new DistributedUIDGenerator().MachineId, "Expected machine id to match but it did not");
            }
        }

        [Test]
        public void NewInstanceUsesCurrentTimeAsCustomEpoch()
        {
            var epoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var generator = new DistributedUIDGenerator();

            // Allow for a 1 millisecond diff in case we ticked over during the test.
            var diff = generator.CustomEpoch - epoch;
            Assert.LessOrEqual(diff, 1, "Expected the custom epoch to be equal to the current Unix time now but it was not.");
        }

        [Test]
        public void IdsAreUniqueForEachMachine()
        {
            HashSet<long> generatedIds = new HashSet<long>();

            // Same time for all.
            var epoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 1;

            var machine1 = new DistributedUIDGenerator(epoch){ MachineId = 11 };
            var machine2 = new DistributedUIDGenerator(epoch){ MachineId = 2312 };
            var machine3 = new DistributedUIDGenerator(epoch){ MachineId = 33 };

            for (int i = 0; i < kGeneratedIdCount; ++i)
            {
                var id1 = machine1.GetNextKey();
                var id2 = machine2.GetNextKey();
                var id3 = machine3.GetNextKey();

                Assert.False(generatedIds.Contains(id1), $"Duplicate id. The id {id1} has already been generated.");
                generatedIds.Add(id1);

                Assert.False(generatedIds.Contains(id2), $"Duplicate id. The id {id2} has already been generated.");
                generatedIds.Add(id2);

                Assert.False(generatedIds.Contains(id3), $"Duplicate id. The id {id3} has already been generated.");
                generatedIds.Add(id3);
            }
        }
    }
}
