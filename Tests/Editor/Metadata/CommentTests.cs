using System;
using NUnit.Framework;
using UnityEngine.Localization.Metadata;

namespace UnityEditor.Localization.Tests.Metadata
{
    public class CommentTests
    {
        [Test]
        public void NewComment_SetsTimeStampToNow()
        {
            var comment = new Comment();
            var difference = DateTime.Now - comment.TimeStamp;
            Assert.Less(difference.TotalSeconds, 1.0,  "Expected Time Stamp to be current time.");
        }
    }
}
