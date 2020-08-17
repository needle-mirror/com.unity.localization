using NUnit.Framework;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.Tests
{
    public class SequentialIDGeneratorTests
    {
        [Test]
        public void DefaultInstanceStartsAt1()
        {
            var generator = new SequentialIDGenerator();
            Assert.AreEqual(1, generator.NextAvailableId, "Expected NextAvailableId to be 1.");
            Assert.AreEqual(1, generator.GetNextKey(), "Expected GetNextKey to return NextAvailableId.");
        }

        [Test]
        public void GetNextKeyIncrementsNextAvailableIdBy1()
        {
            var generator = new SequentialIDGenerator();
            var nextId = generator.NextAvailableId;
            generator.GetNextKey();
            Assert.AreEqual(nextId + 1, generator.NextAvailableId, "Expected NextAvailableId to have been increased by 1");
        }

        [Test]
        public void StartIdIsUsed()
        {
            const long StartID = 1234;
            var generator = new SequentialIDGenerator(StartID);

            Assert.AreEqual(StartID, generator.NextAvailableId, "Expected NextAvailableId to be the starting Id.");
            Assert.AreEqual(StartID, generator.GetNextKey(), "Expected GetNextKey to return the starting Id.");
            Assert.AreEqual(StartID + 1, generator.NextAvailableId, "Expected NextAvailableId to be starting Id + 1 after calling GetNextKey.");
        }
    }
}
