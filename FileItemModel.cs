using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FileArchiver
{
    public class FileItemModel : INotifyPropertyChanged
    {
        private bool _isSelected;
        private bool _existsInArchive;
        private bool _allowOverwrite;

        public string FileName { get; set; }
        public string FullPath { get; set; }
        public long FileSize { get; set; }

        public string FileSizeFormatted => FormatFileSize(FileSize);

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

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public bool AllowOverwrite
        {
            get => _allowOverwrite;
            set
            {
                _allowOverwrite = value;
                OnPropertyChanged();
            }
        }

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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}