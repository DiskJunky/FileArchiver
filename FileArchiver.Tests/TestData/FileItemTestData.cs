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
                yield return new object[] { 1024L, "1.00 KB" };
                yield return new object[] { 1536L, "1.50 KB" };
                yield return new object[] { 1048576L, "1.00 MB" };
                yield return new object[] { 1572864L, "1.50 MB" };
                yield return new object[] { 1073741824L, "1.00 GB" };
                yield return new object[] { 1610612736L, "1.50 GB" };
                yield return new object[] { 1099511627776L, "1.00 TB" };
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
                    "Ready", 
                    true  // Can be archived
                };

                // File exists in archive - not selected by default
                yield return new object[] 
                { 
                    FileItemModelBuilder.Create().AsExistingArchiveFile().Build(), 
                    "Already archived", 
                    false // Cannot be archived without overwrite
                };

                // File exists but overwrite is allowed
                yield return new object[] 
                { 
                    FileItemModelBuilder.Create()
                        .WithExistsInArchive(true)
                        .WithAllowOverwrite(true)
                        .WithIsSelected(true)
                        .Build(), 
                    "Already archived", 
                    true // Can be archived with overwrite
                };

                // File not selected
                yield return new object[] 
                { 
                    FileItemModelBuilder.Create()
                        .WithIsSelected(false)
                        .Build(), 
                    "Ready", 
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
                yield return new object[] { FileSizePreset.Small, 1024L, "1.00 KB" };
                yield return new object[] { FileSizePreset.Medium, 1048576L, "1.00 MB" };
                yield return new object[] { FileSizePreset.Large, 10485760L, "10.00 MB" };
                yield return new object[] { FileSizePreset.VeryLarge, 104857600L, "100.00 MB" };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}