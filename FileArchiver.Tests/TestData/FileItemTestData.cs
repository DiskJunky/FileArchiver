using FileArchiver.Tests.Builders;
using System.Collections;

namespace FileArchiver.Tests.TestData
{
    /// <summary>
    /// Provides test data for FileItemModel tests using data-driven testing.
    /// </summary>
    public class FileItemTestData
    {
        /// <summary>
        /// Test data for file size formatting tests.
        /// </summary>
        public class FileSizeFormattingData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { 0L, "0 B" };
                yield return new object[] { 512L, "512 B" };
                yield return new object[] { 1024L, "1 KB" };
                yield return new object[] { 1536L, "1.5 KB" };
                yield return new object[] { 1048576L, "1 MB" };
                yield return new object[] { 1572864L, "1.5 MB" };
                yield return new object[] { 1073741824L, "1 GB" };
                yield return new object[] { 1610612736L, "1.5 GB" };
                yield return new object[] { 1099511627776L, "1 TB" };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        /// <summary>
        /// Test data for archive status scenarios.
        /// </summary>
        public class ArchiveStatusData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                // New file - ready to archive
                yield return new object[] 
                { 
                    FileItemModelBuilder.Create().AsNewFile().Build(), 
                    "Selected", 
                    true  // Can be archived
                };

                // File exists in archive - not selected by default
                yield return new object[] 
                { 
                    FileItemModelBuilder.Create().AsExistingArchiveFile().Build(), 
                    "Exists (Skip)", 
                    false // Cannot be archived without overwrite
                };

                // File exists but overwrite is allowed
                var fileWithOverwrite = FileItemModelBuilder.Create()
                    .WithExistsInArchive(false)  // Set to false first
                    .WithIsSelected(true)
                    .Build();
                fileWithOverwrite.ExistsInArchive = true;  // This sets IsSelected to false
                fileWithOverwrite.IsSelected = true;  // Re-enable selection
                fileWithOverwrite.AllowOverwrite = true;
                
                yield return new object[] 
                { 
                    fileWithOverwrite,
                    "Will Overwrite", 
                    true // Can be archived with overwrite
                };

                // File not selected
                yield return new object[] 
                { 
                    FileItemModelBuilder.Create()
                        .WithIsSelected(false)
                        .Build(), 
                    "Not Selected", 
                    false // Cannot be archived - not selected
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        /// <summary>
        /// Test data for file name validation scenarios.
        /// </summary>
        public class FileNameValidationData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { "simple.txt", true };
                yield return new object[] { "file with spaces.doc", true };
                yield return new object[] { "file-with-dashes.pdf", true };
                yield return new object[] { "file_with_underscores.jpg", true };
                yield return new object[] { "file.multiple.dots.txt", true };
                yield return new object[] { "", false };
                yield return new object[] { " ", false };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        /// <summary>
        /// Test data for various file size presets.
        /// </summary>
        public class FileSizePresetData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { FileSizePreset.Empty, 0L, "0 B" };
                yield return new object[] { FileSizePreset.Small, 1024L, "1 KB" };
                yield return new object[] { FileSizePreset.Medium, 1048576L, "1 MB" };
                yield return new object[] { FileSizePreset.Large, 10485760L, "10 MB" };
                yield return new object[] { FileSizePreset.VeryLarge, 104857600L, "100 MB" };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}