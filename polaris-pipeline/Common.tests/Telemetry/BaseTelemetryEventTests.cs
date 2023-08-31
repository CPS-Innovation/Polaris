using Common.Telemetry;
using Xunit;

namespace Common.tests.Telemetry
{

    public class BaseTelemetryEventTests
    {
        [Fact]
        public void GetDurationSeconds_ReturnsExpectedValue()
        {
            // Arrange
            var startTime = new DateTime(2020, 1, 1, 0, 0, 0, 100, DateTimeKind.Utc);
            var endTime = new DateTime(2020, 1, 1, 0, 0, 59, 200, DateTimeKind.Utc);

            // Act
            var duration = BaseTelemetryEvent.GetDurationSeconds(startTime, endTime);

            // Assert
            Assert.Equal(59.1, duration);
        }

        [Fact]
        public void GetDurationSeconds_ReturnsExpectedValue_WhenStartTimeIsDefault()
        {
            // Arrange
            DateTime startTime = default;
            var endTime = new DateTime(2020, 1, 1, 0, 0, 59, 200, DateTimeKind.Utc);

            // Act
            var duration = BaseTelemetryEvent.GetDurationSeconds(startTime, endTime);

            // Assert
            Assert.Null(duration);
        }

        [Fact]
        public void GetDurationSeconds_ReturnsExpectedValue_WhenEndTimeIsDefault()
        {
            // Arrange
            var startTime = new DateTime(2020, 1, 1, 0, 0, 0, 100, DateTimeKind.Utc);
            DateTime endTime = default;

            // Act
            var duration = BaseTelemetryEvent.GetDurationSeconds(startTime, endTime);

            // Assert
            Assert.Null(duration);
        }

        [Theory]
        [InlineData("123456", "123456")]
        [InlineData("CMS-123456", "123456")]
        [InlineData("PCD-123456", "123456")]
        // code copes with missing value without blowing up (this is logging, not business logic)
        [InlineData("", "")]
        [InlineData(null, "")]
        public void EnsureNumericId_ReturnsCleanDocumentId(string input, string expected)
        {
            // Act
            var result = BaseTelemetryEvent.EnsureNumericId(input);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
