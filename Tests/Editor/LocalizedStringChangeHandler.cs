using System;
using NUnit.Framework;
using UnityEngine.Localization;

namespace UnityEditor.Localization.Tests
{
    public class LocalizedStringChangeHandler
    {
        [Test]
        public void StringChanged_Add_ThrowsException_WhenNullIsPassed()
        {
            LocalizedString locString = new LocalizedString();
            Assert.Throws<ArgumentNullException>(() => locString.StringChanged += null);
        }

        [Test]
        public void StringChanged_Remove_ThrowsException_WhenNullIsPassed()
        {
            LocalizedString locString = new LocalizedString();
            Assert.Throws<ArgumentNullException>(() => locString.StringChanged -= null);
        }
    }
}
