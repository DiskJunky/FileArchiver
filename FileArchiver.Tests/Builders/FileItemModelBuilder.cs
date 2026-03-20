using FileArchiver;

namespace FileArchiver.Tests.Builders
{
    /// <summary>
    /// Builder pattern for creating FileItemModel instances in tests.
    /// Provides fluent API for constructing test data with default values.
    /// </summary>
    public class FileItemModelBuilder
    {
        private string _fileName = "test-file.txt";
        private string _fullPath = @"C:\temp\test-file.txt";
        private long _fileSize = 1024;
        private bool _isSelected = true;
        private bool _existsInArchive = false;
        private bool _allowOverwrite = false;

        /// <summary>
        /// Sets the file name.
        /// </summary>
        public FileItemModelBuilder WithFileName(string fileName)
        {
            _fileName = fileName;
            return this;
        }

        /// <summary>
        /// Sets the full file path.
        /// </summary>
        public FileItemModelBuilder WithFullPath(string fullPath)
        {
            _fullPath = fullPath;
            return this;
        }

        /// <summary>
        /// Sets the file size in bytes.
        /// </summary>
        public FileItemModelBuilder WithFileSize(long fileSize)
        {
            _fileSize = fileSize;
            return this;
        }

        /// <summary>
        /// Sets the IsSelected property.
        /// </summary>
        public FileItemModelBuilder WithIsSelected(bool isSelected)
        {
            _isSelected = isSelected;
            return this;
        }

        /// <summary>
        /// Sets the ExistsInArchive property.
        /// </summary>
        public FileItemModelBuilder WithExistsInArchive(bool existsInArchive)
        {
            _existsInArchive = existsInArchive;
            return this;
        }

        /// <summary>
        /// Sets the AllowOverwrite property.
        /// </summary>
        public FileItemModelBuilder WithAllowOverwrite(bool allowOverwrite)
        {
            _allowOverwrite = allowOverwrite;
            return this;
        }

        /// <summary>
        /// Configures the file as an existing archive file (exists in archive).
        /// </summary>
        public FileItemModelBuilder AsExistingArchiveFile()
        {
            _existsInArchive = true;
            _isSelected = false;
            _allowOverwrite = false;
            return this;
        }

        /// <summary>
        /// Configures the file as a new file ready for archiving.
        /// </summary>
        public FileItemModelBuilder AsNewFile()
        {
            _existsInArchive = false;
            _isSelected = true;
            _allowOverwrite = false;
            return this;
        }

        /// <summary>
        /// Configures the file with a specific size preset.
        /// </summary>
        public FileItemModelBuilder WithSizePreset(FileSizePreset preset)
        {
            _fileSize = preset switch
            {
                FileSizePreset.Empty => 0,
                FileSizePreset.Small => 1024, // 1 KB
                FileSizePreset.Medium => 1024 * 1024, // 1 MB
                FileSizePreset.Large => 10 * 1024 * 1024, // 10 MB
                FileSizePreset.VeryLarge => 100 * 1024 * 1024, // 100 MB
                _ => 1024
            };
            return this;
        }

        /// <summary>
        /// Builds the FileItemModel instance with configured properties.
        /// </summary>
        public FileItemModel Build()
        {
            var model = new FileItemModel
            {
                FileName = _fileName,
                FullPath = _fullPath,
                FileSize = _fileSize,
                ExistsInArchive = _existsInArchive
            };

            model.IsSelected = _isSelected;
            model.AllowOverwrite = _allowOverwrite;

            return model;
        }

        /// <summary>
        /// Creates a new builder instance.
        /// </summary>
        public static FileItemModelBuilder Create() => new FileItemModelBuilder();
    }

    /// <summary>
    /// Predefined file size presets for testing.
    /// </summary>
    public enum FileSizePreset
    {
        Empty,
        Small,
        Medium,
        Large,
        VeryLarge
    }
}