using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FileArchiver
{
    /// <summary>
    /// Represents a file item in the archive operation with selection and status tracking.
    /// Implements INotifyPropertyChanged to support UI binding and property change notifications.
    /// </summary>
    public class FileItemModel : INotifyPropertyChanged
    {
        private bool _isSelected;
        private bool _existsInArchive;
        private bool _allowOverwrite;

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the full path of the file.
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// Gets or sets the size of the file in bytes.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Gets the file size formatted as a human-readable string (e.g., "1.5 MB").
        /// </summary>
        public string FileSizeFormatted => FormatFileSize(FileSize);

        /// <summary>
        /// Gets or sets a value indicating whether this file exists in the archive.
        /// When set to true, automatically sets IsSelected to false and AllowOverwrite to false.
        /// </summary>
        public bool ExistsInArchive
        {
            get => _existsInArchive;
            set
            {
                _existsInArchive = value;
                OnPropertyChanged();
                // If file exists, default is NOT to overwrite
                if (value)
                {
                    AllowOverwrite = false;
                    IsSelected = false;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this file is selected for archiving.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to overwrite the file if it already exists in the archive.
        /// </summary>
        public bool AllowOverwrite
        {
            get => _allowOverwrite;
            set
            {
                _allowOverwrite = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the current status of the file as a human-readable string.
        /// </summary>
        /// <remarks>
        /// Returns one of: "Will Overwrite", "Exists (Skip)", "Selected", or "Not Selected".
        /// </remarks>
        public string Status
        {
            get
            {
                if (ExistsInArchive)
                {
                    return AllowOverwrite ? "Will Overwrite" : "Exists (Skip)";
                }
                return IsSelected ? "Selected" : "Not Selected";
            }
        }

        /// <summary>
        /// Formats a file size in bytes as a human-readable string with appropriate units.
        /// </summary>
        /// <param name="bytes">The file size in bytes.</param>
        /// <returns>A formatted file size string (e.g., "1.5 MB", "2.3 GB").</returns>
        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}