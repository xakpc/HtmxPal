using Microsoft.VisualStudio.Text;
using Moq;

namespace Xakpc.VisualStudio.Extensions.HtmxPal.Tests
{
    [TestClass]
    public class IsInsideHtmlTagTests
    {
        private Mock<ITextSnapshot> _mockSnapshot;

        [TestInitialize]
        public void Setup()
        {
            _mockSnapshot = new Mock<ITextSnapshot>();
        }

        [TestMethod]
        public void IsInsideHtmlTag_PointInsideTag_ReturnsTrue()
        {
            // Arrange
            string text = "<div>Hello</div>";
            SetupMockSnapshot(text);
            var point = new SnapshotPoint(_mockSnapshot.Object, 2); // Position of 'i' in <div>

            // Act
            bool result = SnapshotPointExtensions.IsInsideHtmlTag(point);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsInsideHtmlTag_PointOutsideTag_ReturnsFalse()
        {
            // Arrange
            string text = "<div>Hello</div>";
            SetupMockSnapshot(text);
            var point = new SnapshotPoint(_mockSnapshot.Object, 6); // Position of 'e' in Hello

            // Act
            bool result = SnapshotPointExtensions.IsInsideHtmlTag(point);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsInsideHtmlTag_PointAtStartOfTag_ReturnsFalse()
        {
            // Arrange
            string text = "<div>Hello</div>";
            SetupMockSnapshot(text);
            var point = new SnapshotPoint(_mockSnapshot.Object, 0); // Position of '<' (before it)

            // Act
            bool result = SnapshotPointExtensions.IsInsideHtmlTag(point);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsInsideHtmlTag_PointAtEndOfTag_ReturnsTrue()
        {
            // Arrange
            string text = "<div>Hello</div>";
            SetupMockSnapshot(text);
            var point = new SnapshotPoint(_mockSnapshot.Object, 4); // Position of '>' in opening tag

            // Act
            bool result = SnapshotPointExtensions.IsInsideHtmlTag(point);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsInsideHtmlTag_PointInNestedTags_ReturnsTrue()
        {
            // Arrange
            string text = "<div><span>Hello</span></div>";
            SetupMockSnapshot(text);
            var point = new SnapshotPoint(_mockSnapshot.Object, 7); // Position of 'p' in <span>

            // Act
            bool result = SnapshotPointExtensions.IsInsideHtmlTag(point);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsInsideHtmlTag_PointInTagWithAttributes_ReturnsTrue()
        {
            // Arrange
            string text = "<div class=\"test\">Hello</div>";
            SetupMockSnapshot(text);
            var point = new SnapshotPoint(_mockSnapshot.Object, 10); // Position of 't' in test

            // Act
            bool result = SnapshotPointExtensions.IsInsideHtmlTag(point);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsInsideHtmlTag_EmptyString_ReturnsFalse()
        {
            // Arrange
            string text = "";
            SetupMockSnapshot(text);
            var point = new SnapshotPoint(_mockSnapshot.Object, 0);

            // Act
            bool result = SnapshotPointExtensions.IsInsideHtmlTag(point);

            // Assert
            Assert.IsFalse(result);
        }

        private void SetupMockSnapshot(string text)
        {
            _mockSnapshot.Setup(s => s.Length).Returns(text.Length);
            _mockSnapshot.Setup(s => s[It.IsAny<int>()]).Returns((int i) => text[i]);
        }
    }
}
