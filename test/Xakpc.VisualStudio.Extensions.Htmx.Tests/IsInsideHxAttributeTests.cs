using Microsoft.VisualStudio.Text;
using Moq;

namespace Xakpc.VisualStudio.Extensions.HtmxPal.Tests
{
    [TestClass]
    public class IsInsideHxAttributeTests
    {
        private Mock<ITextSnapshot> _mockSnapshot;

        [TestInitialize]
        public void Setup()
        {
            _mockSnapshot = new Mock<ITextSnapshot>();
        }

        [TestMethod]
        public void IsInsideHxAttribute_PointInsideHxAttributeEmptyValue_ReturnsTrue()
        {
            string text = "<div hx-get=\"\">Hello</div>";
            SetupMockSnapshot(text);
            var point = new SnapshotPoint(_mockSnapshot.Object, 12); // Position after '"'

            // Act
            bool result = SnapshotPointExtensions.IsInsideHxAttribute(point, out string attribute);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual("hx-get", attribute);
        }

        [TestMethod]
        public void IsInsideHxAttribute_PointInsideHxAttributeValue_ReturnsTrue()
        {
            string text = "<div hx-get=\"/url\">Hello</div>";
            SetupMockSnapshot(text);
            var point = new SnapshotPoint(_mockSnapshot.Object, 12); // Position after '"'

            // Act
            bool result = SnapshotPointExtensions.IsInsideHxAttribute(point, out string attribute);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual("hx-get", attribute);
        }

        [TestMethod]
        public void IsInsideHxAttribute_PointInsideHxAttribute_ReturnsFalse()
        {
            // Arrange
            string text = "<div hx-get=\"/url\">Hello</div>";
            SetupMockSnapshot(text);
            var point = new SnapshotPoint(_mockSnapshot.Object, 10); // Position of 'g' in hx-get

            // Act
            bool result = SnapshotPointExtensions.IsInsideHxAttribute(point, out string attribute);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(attribute);
        }

        [TestMethod]
        public void IsInsideHxAttribute_PointOutsideHxAttribute_ReturnsFalse()
        {
            // Arrange
            string text = "<div hx-get=\"/url\">Hello</div>";
            SetupMockSnapshot(text);
            var point = new SnapshotPoint(_mockSnapshot.Object, 20); // Position of 'H' in Hello

            // Act
            bool result = SnapshotPointExtensions.IsInsideHxAttribute(point, out string attribute);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(attribute);
        }

        [TestMethod]
        public void IsInsideHxAttribute_PointAtStartOfHxAttribute_ReturnsFalse()
        {
            // Arrange
            string text = "<div hx-get=\"/url\">Hello</div>";
            SetupMockSnapshot(text);
            var point = new SnapshotPoint(_mockSnapshot.Object, 5); // Position of 'h' in hx-get

            // Act
            bool result = SnapshotPointExtensions.IsInsideHxAttribute(point, out string attribute);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(attribute);
        }

        [TestMethod]
        public void IsInsideHxAttribute_PointAtEndOfHxAttribute_ReturnsFalse()
        {
            // Arrange
            string text = "<div hx-get=\"/url\">Hello</div>";
            SetupMockSnapshot(text);
            var point = new SnapshotPoint(_mockSnapshot.Object, 11); // Position of '=' in hx-get=

            // Act
            bool result = SnapshotPointExtensions.IsInsideHxAttribute(point, out string attribute);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(attribute);
        }

        [TestMethod]
        public void IsInsideHxAttribute_PointInNonHxAttribute_ReturnsFalse()
        {
            // Arrange
            string text = "<div class=\"test\">Hello</div>";
            SetupMockSnapshot(text);
            var point = new SnapshotPoint(_mockSnapshot.Object, 10); // Position of 't' in test

            // Act
            bool result = SnapshotPointExtensions.IsInsideHxAttribute(point, out string attribute);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(attribute);
        }

        [TestMethod]
        public void IsInsideHxAttribute_EmptyString_ReturnsFalse()
        {
            // Arrange
            string text = "";
            SetupMockSnapshot(text);
            var point = new SnapshotPoint(_mockSnapshot.Object, 0);

            // Act
            bool result = SnapshotPointExtensions.IsInsideHxAttribute(point, out string attribute);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNull(attribute);
        }

        private void SetupMockSnapshot(string text)
        {
            _mockSnapshot.Setup(s => s.Length).Returns(text.Length);
            _mockSnapshot.Setup(s => s[It.IsAny<int>()]).Returns((int i) => text[i]);

            // create ITextSnapshotLine mock
            var _mockSnapshotLine = new Mock<ITextSnapshotLine>();
            _mockSnapshotLine.Setup(l => l.Start).Returns(new SnapshotPoint(_mockSnapshot.Object, 0));
            _mockSnapshotLine.Setup(l => l.End).Returns(new SnapshotPoint(_mockSnapshot.Object, text.Length));
            _mockSnapshotLine.Setup(l => l.GetText()).Returns(text);

            _mockSnapshot.Setup(s => s.GetLineFromPosition(It.IsAny<int>())).Returns(_mockSnapshotLine.Object);
            _mockSnapshot.Setup(s => s.GetText(It.IsAny<int>(), It.IsAny<int>())).Returns((int start, int length) => text.Substring(start, length));

            _mockSnapshotLine.Setup(l => l.Snapshot).Returns(_mockSnapshot.Object);
        }
    }
}
