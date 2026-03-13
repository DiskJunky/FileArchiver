using System.Windows;
using Microsoft.Win32;

namespace FileArchiver.Services
{
    /// <summary>
    /// Dialog service implementation using WPF.
    /// Provides user interaction abstractions for testing.
    /// </summary>
    public class DialogService : IDialogService
    {
        /// <summary>
        /// Shows an informational message.
        /// </summary>
        public void ShowInformation(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Shows a warning message.
        /// </summary>
        public void ShowWarning(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>
        /// Shows an error message.
        /// </summary>
        public void ShowError(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Shows a confirmation dialog.
        /// </summary>
        public bool ShowConfirmation(string message, string title)
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

        /// <summary>
        /// Shows a folder browser dialog.
        /// </summary>
        public string BrowseForFolder(string title, string initialFolder)
        {
            var dialog = new OpenFolderDialog
            {
                Title = title,
                InitialDirectory = initialFolder
            };

            if (dialog.ShowDialog() == true)
            {
                return dialog.FolderName;
            }

            return null;
        }
    }
}
