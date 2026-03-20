using FileArchiver.Tests.Builders;
using FileArchiver.Tests.TestInfrastructure;
using FluentAssertions;
using Xunit;

namespace FileArchiver.Tests.Models
{
    /// <summary>
    /// Unit tests for ActivityLogEntry using builder patterns.
    /// </summary>
    public class ActivityLogEntryTests : TestBase
    {
        [Fact]
        public void Builder_AsError_ShouldSetErrorFlagOnly()
        {
            // Arrange & Act
            var entry = ActivityLogEntryBuilder.Create()
                .AsError()
                .WithMessage("Test error")
                .Build();

            // Assert
            entry.IsError.Should().BeTrue();
            entry.IsWarning.Should().BeFalse();
            entry.IsSuccess.Should().BeFalse();
            entry.Message.Should().Be("Test error");
        }

        [Fact]
        public void Builder_AsWarning_ShouldSetWarningFlagOnly()
        {
            // Arrange & Act
            var entry = ActivityLogEntryBuilder.Create()
                .AsWarning()
                .WithMessage("Test warning")
                .Build();

            // Assert
            entry.IsError.Should().BeFalse();
            entry.IsWarning.Should().BeTrue();
            entry.IsSuccess.Should().BeFalse();
            entry.Message.Should().Be("Test warning");
        }

        [Fact]
        public void Builder_AsSuccess_ShouldSetSuccessFlagOnly()
        {
            // Arrange & Act
            var entry = ActivityLogEntryBuilder.Create()
                .AsSuccess()
                .WithMessage("Test success")
                .Build();

            // Assert
            entry.IsError.Should().BeFalse();
            entry.IsWarning.Should().BeFalse();
            entry.IsSuccess.Should().BeTrue();
            entry.Message.Should().Be("Test success");
        }

        [Fact]
        public void Builder_AsInfo_ShouldSetNoSpecialFlags()
        {
            // Arrange & Act
            var entry = ActivityLogEntryBuilder.Create()
                .AsInfo()
                .WithMessage("Test info")
                .Build();

            // Assert
            entry.IsError.Should().BeFalse();
            entry.IsWarning.Should().BeFalse();
            entry.IsSuccess.Should().BeFalse();
            entry.Message.Should().Be("Test info");
        }

        [Theory]
        [InlineData("Error message", true, false, false)]
        [InlineData("Warning message", false, true, false)]
        [InlineData("Success message", false, false, true)]
        [InlineData("Info message", false, false, false)]
        public void ActivityLogEntry_ShouldHaveCorrectFlagsForMessageType(
            string message, 
            bool isError, 
            bool isWarning, 
            bool isSuccess)
        {
            // Arrange & Act
            var entry = new ActivityLogEntry
            {
                Message = message,
                IsError = isError,
                IsWarning = isWarning,
                IsSuccess = isSuccess,
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            // Assert
            entry.IsError.Should().Be(isError);
            entry.IsWarning.Should().Be(isWarning);
            entry.IsSuccess.Should().Be(isSuccess);
        }

        [Fact]
        public void Builder_WithCustomTimestamp_ShouldSetCorrectly()
        {
            // Arrange
            var customTimestamp = "2026-03-19 14:30:00";

            // Act
            var entry = ActivityLogEntryBuilder.Create()
                .WithTimestamp(customTimestamp)
                .WithMessage("Timestamped message")
                .Build();

            // Assert
            entry.Timestamp.Should().Be(customTimestamp);
        }
    }
}