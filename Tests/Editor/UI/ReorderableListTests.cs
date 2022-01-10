using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.Localization.UI.Toolkit;

namespace UnityEditor.Localization.Tests.UI
{
    public class ReorderableListTests
    {
        [Test]
        public void InstantiatingDoesNotProduceError()
        {
            // The uxml <Style> tag creates errors on 2019.4
            var ro = new ReorderableList(new List<int>());
            Assert.NotNull(ro);
        }
    }
}
