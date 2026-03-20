using FileArchiver.Tests.Builders;
using FileArchiver.Tests.TestData;
using FileArchiver.Tests.TestInfrastructure;
using FluentAssertions;
using Xunit;

namespace FileArchiver.Tests.Models
{
    /// <summary>
    /// Unit tests for FileItemModel using data-driven tests and builder patterns.
    /// </summary>
    public class FileItemModelTests : TestBase
    {
        [Fact]
        public void Constructor_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var fileItem = new FileItemModel();

            // Assert
            fileItem.FileName.Should().BeNull();
            fileItem.FullPath.Should().BeNull();
            fileItem.FileSize.Should().Be(0);
            fileItem.IsSelected.Should().BeFalse();
            fileItem.ExistsInArchive.Should().BeFalse();
            fileItem.AllowOverwrite.Should().BeFalse();
        }

        [Theory]
        [ClassData(typeof(FileItemTestData.FileSizeFormattingData))]
        public void FileSizeFormatted_ShouldReturnCorrectFormat(long fileSize, string expectedFormat)
        {
            // Arrange
            var fileItem = FileItemModelBuilder.Create()
                .WithFileSize(fileSize)
                .Build();

            // Act
            var result = fileItem.FileSizeFormatted;

            // Assert
            result.Should().Be(expectedFormat);
        }

        [Theory]
        [ClassData(typeof(FileItemTestData.FileSizePresetData))]
        public void Builder_WithSizePreset_ShouldSetCorrectSize(
            FileSizePreset preset, 
            long expectedSize, 
            string expectedFormat)
        {
            // Arrange & Act
            var fileItem = FileItemModelBuilder.Create()
                .WithSizePreset(preset)
                .Build();

            // Assert
            fileItem.FileSize.Should().Be(expectedSize);
            fileItem.FileSizeFormatted.Should().Be(expectedFormat);
        }

        [Theory]
        [ClassData(typeof(FileItemTestData.ArchiveStatusData))]
        public void Status_ShouldReflectCorrectState(
            FileItemModel fileItem, 
            string expectedStatus, 
            bool canBeArchived)
        {
            // Act
            var status = fileItem.Status;
            var shouldBeArchived = fileItem.IsSelected && 
                                  (!fileItem.ExistsInArchive || fileItem.AllowOverwrite);

            // Assert
            status.Should().Be(expectedStatus);
            shouldBeArchived.Should().Be(canBeArchived);
        }

        [Fact]
        public void ExistsInArchive_WhenSetToTrue_ShouldSetIsSelectedAndAllowOverwriteToFalse()
        {
            // Arrange
            var fileItem = FileItemModelBuilder.Create()
                .WithIsSelected(true)
                .WithAllowOverwrite(false)
                .Build();

            // Act
            fileItem.ExistsInArchive = true;

            // Assert
            fileItem.AllowOverwrite.Should().BeFalse();
            fileItem.IsSelected.Should().BeFalse("ExistsInArchive sets IsSelected to false");
        }

        [Fact]
        public void IsSelected_WhenChanged_ShouldRaisePropertyChanged()
        {
            // Arrange
            var fileItem = FileItemModelBuilder.Create().Build();
            var propertyChangedRaised = false;
            fileItem.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(FileItemModel.IsSelected))
                    propertyChangedRaised = true;
            };

            // Act
            fileItem.IsSelected = !fileItem.IsSelected;

            // Assert
            propertyChangedRaised.Should().BeTrue();
        }

        [Fact]
        public void AllowOverwrite_WhenChanged_ShouldRaisePropertyChanged()
        {
            // Arrange
            var fileItem = FileItemModelBuilder.Create()
                .WithExistsInArchive(true)
                .Build();
            
            var propertyChangedRaised = false;
            fileItem.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(FileItemModel.AllowOverwrite))
                    propertyChangedRaised = true;
            };

            // Act
            fileItem.AllowOverwrite = true;

            // Assert
            propertyChangedRaised.Should().BeTrue();
        }

        [Fact]
        public void Builder_AsNewFile_ShouldConfigureCorrectly()
        {
            // Arrange & Act
            var fileItem = FileItemModelBuilder.Create()
                .AsNewFile()
                .Build();

            // Assert
            fileItem.ExistsInArchive.Should().BeFalse();
            fileItem.IsSelected.Should().BeTrue();
            fileItem.AllowOverwrite.Should().BeFalse();
            fileItem.Status.Should().Be("Selected");
        }

        [Fact]
        public void Builder_AsExistingArchiveFile_ShouldConfigureCorrectly()
        {
            // Arrange & Act
            var fileItem = FileItemModelBuilder.Create()
                .AsExistingArchiveFile()
                .Build();

            // Assert
            fileItem.ExistsInArchive.Should().BeTrue();
            fileItem.IsSelected.Should().BeFalse();
            fileItem.AllowOverwrite.Should().BeFalse();
            fileItem.Status.Should().Be("Exists (Skip)");
        }

        [Theory]
        [InlineData("document.pdf", @"C:\files\document.pdf")]
        [InlineData("image.jpg", @"C:\photos\vacation\image.jpg")]
        [InlineData("data.xlsx", @"\\server\share\reports\data.xlsx")]
        public void Builder_WithFileNameAndPath_ShouldSetCorrectly(string fileName, string fullPath)
        {
            // Arrange & Act
            var fileItem = FileItemModelBuilder.Create()
                .WithFileName(fileName)
                .WithFullPath(fullPath)
                .Build();

            // Assert
            fileItem.FileName.Should().Be(fileName);
            fileItem.FullPath.Should().Be(fullPath);
        }

        [Fact]
        public void PropertyChanged_ShouldBeRaisedForAllSettableProperties()
        {
            // Arrange
            var fileItem = FileItemModelBuilder.Create().Build();
            var changedProperties = new List<string>();
            fileItem.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName!);

            // Act
            fileItem.IsSelected = !fileItem.IsSelected;
            fileItem.ExistsInArchive = !fileItem.ExistsInArchive;
            fileItem.AllowOverwrite = true;

            // Assert
            changedProperties.Should().Contain(nameof(FileItemModel.IsSelected));
            changedProperties.Should().Contain(nameof(FileItemModel.ExistsInArchive));
            changedProperties.Should().Contain(nameof(FileItemModel.AllowOverwrite));
        }

        [Fact]
        public void Status_WhenExistsAndOverwriteAllowed_ShouldReturnWillOverwrite()
        {
            // Arrange
            var fileItem = FileItemModelBuilder.Create()
                .WithExistsInArchive(true)
                .Build();

            // Act
            fileItem.AllowOverwrite = true;

            // Assert
            fileItem.Status.Should().Be("Will Overwrite");
        }

        [Fact]
        public void Status_WhenNotSelected_ShouldReturnNotSelected()
        {
            // Arrange & Act
            var fileItem = FileItemModelBuilder.Create()
                .WithIsSelected(false)
                .Build();

            // Assert
            fileItem.Status.Should().Be("Not Selected");
        }
    }
}