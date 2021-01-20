using System.Collections.Generic;
using NUnit.Framework;

namespace UnityEngine.Localization.SmartFormat.Tests.Core
{
    class FormatCacheTests
    {
        [Test]
        public void Create_Cache()
        {
            var sf = new SmartFormatter();
            var format = FormatItemPool.GetFormat(sf.Settings, "the base string");
            var fc = FormatCachePool.Get(format);
            Assert.AreEqual(format, fc.Format);
            Assert.IsAssignableFrom<Dictionary<string, object>>(fc.CachedObjects);
            fc.CachedObjects.Add("key", "value");
            Assert.IsTrue(fc.CachedObjects["key"].ToString() == "value");
            FormatCachePool.Release(fc);
        }
    }
}
