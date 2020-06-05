using System;
using NUnit.Framework;
using UnityEngine.Localization;

namespace UnityEditor.Localization.Tests
{
    public class LocalizedStringChangeHandler
    {
        [Test]
        public void RegisterChangeHandler_ThrowsException_WhenNullIsPassed()
        {
            LocalizedString locString = new LocalizedString();
            Assert.Throws<ArgumentNullException>(() => locString.RegisterChangeHandler(null));
        }
    }
}
