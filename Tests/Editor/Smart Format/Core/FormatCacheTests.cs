using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.Localization.SmartFormat.Core.Formatting;
using UnityEngine.Localization.SmartFormat.Core.Parsing;

namespace UnityEngine.Localization.SmartFormat.Tests.Core
{
    class FormatCacheTests
    {
        [Test]
        public void Create_Cache()
        {
            var sf = new SmartFormatter();
            var format = new Format(sf.Settings, "the base string");
            var fc = new FormatCache(format);
            Assert.AreEqual(format, fc.Format);
            Assert.IsAssignableFrom<Dictionary<string, object>>(fc.CachedObjects);
            fc.CachedObjects.Add("key", "value");
            Assert.IsTrue(fc.CachedObjects["key"].ToString() == "value");
        }
    }
}
