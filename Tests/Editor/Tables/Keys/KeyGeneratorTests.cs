using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.Localization.Tables;

namespace UnityEditor.Localization.Tests
{
    [TestFixture(typeof(SequentialIDGenerator))]
    [TestFixture(typeof(DistributedUIDGenerator))]
    public class KeyGeneratorTests<T> where T : IKeyGenerator, new()
    {
        const int kGeneratedIdCount = 5000;

        [Test]
        public void GeneratedIdsAreUnique()
        {
            var generator = new T();
            var generatedIds = new HashSet<long>();
            for (int i = 0; i < kGeneratedIdCount; ++i)
            {
                var id = generator.GetNextKey();
                Assert.False(generatedIds.Contains(id), $"Duplicate id. The id {id} has already been generated.");
                generatedIds.Add(id);
            }
        }

        [Test]
        public void GeneratedIdsAreIncreasingInValue()
        {
            var generator = new T();
            long lastValue = 0;
            for (int i = 0; i < kGeneratedIdCount; ++i)
            {
                var id = generator.GetNextKey();
                Assert.Greater(id, lastValue, "Expected next key Id to be greater than previous.");
                lastValue = id;
            }
        }
    }
}
