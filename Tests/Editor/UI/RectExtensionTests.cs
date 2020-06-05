using NUnit.Framework;
using UnityEngine;

namespace UnityEditor.Localization.Tests.UI
{
    public class RectExtensionTests
    {
        const float kRectX = 100, kRectY = 50, kRectWidth = 221, kRectHeight = 100;

        [Test]
        public void MoveToNextLine_IncrementsRectYValueAndPreservesSize()
        {
            var rect = new Rect(kRectX, kRectY, kRectWidth, kRectHeight);
            float expectedY = kRectY + EditorGUIUtility.standardVerticalSpacing + kRectHeight;

            rect.MoveToNextLine();

            Assert.AreEqual(expectedY, rect.y, "Expected rect to be incremented by its height and the standardVerticalSpacing");
            Assert.AreEqual(kRectWidth, rect.width, "Expected width to be preserved.");
            Assert.AreEqual(kRectHeight, rect.height, "Expected height to be preserved.");
            Assert.AreEqual(kRectX, rect.x, "Expected x to be preserved.");
        }

        [TestCase(100, 0, 121, 100)]
        [TestCase(100, 2, 119, 100)]
        [TestCase(50, 1, 170, 50)]
        public void SplitHorizontalFixedWidthRight_SplitsRectWithCorrectValues(float rightWidth, float padding, float expectedLeftWidth, float expectedRightWidth)
        {
            var rect = new Rect(kRectX, kRectY, kRectWidth, kRectHeight);

            var rects = rect.SplitHorizontalFixedWidthRight(rightWidth, padding);

            Assert.AreEqual(expectedLeftWidth, rects.left.width, "Expected left rect width to match.");
            Assert.AreEqual(expectedRightWidth, rects.right.width, "Expected left rect width to match.");
            Assert.AreEqual(kRectY, rects.left.y, "Expected y value to not change for left rect.");
            Assert.AreEqual(kRectY, rects.right.y, "Expected y value to not change for right rect.");

            Assert.AreEqual(kRectX, rects.left.x, "Expected left rect x value to match.");
            Assert.AreEqual(kRectX + expectedLeftWidth + padding, rects.right.x, "Expected right rect x value to match.");
            Assert.AreEqual(kRectHeight, rects.left.height, "Expected height to not change for left rect.");
            Assert.AreEqual(kRectHeight, rects.right.height, "Expected height to not change for right rect.");
        }

        [TestCase(0.5f, 0, 110.5f, 110.5f)]
        [TestCase(0.5f, 1, 110, 110)]
        [TestCase(0.65f, 5, 140.399994f, 75.6000061f)]
        public void SplitHorizontal_SplitsRectsEvenlyWithCorrectValues(float leftAmount, float padding, float expectedLeftWidth, float expectedRightWidth)
        {
            var rect = new Rect(kRectX, kRectY, kRectWidth, kRectHeight);

            var rects = rect.SplitHorizontal(leftAmount, padding);

            Assert.AreEqual(expectedLeftWidth, rects.left.width, "Expected left rect width to match.");
            Assert.AreEqual(expectedRightWidth, rects.right.width, "Expected left rect width to match.");
            Assert.AreEqual(kRectY, rects.left.y, "Expected y value to not change for left rect.");
            Assert.AreEqual(kRectY, rects.right.y, "Expected y value to not change for right rect.");

            Assert.AreEqual(kRectX, rects.left.x, "Expected left rect x value to match.");
            Assert.AreEqual(kRectX + expectedLeftWidth + padding, rects.right.x, "Expected right rect x value to match.");
            Assert.AreEqual(kRectHeight, rects.left.height, "Expected height to not change for left rect.");
            Assert.AreEqual(kRectHeight, rects.right.height, "Expected height to not change for right rect.");
        }
    }
}
