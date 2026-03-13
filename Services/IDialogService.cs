using System.Windows;

namespace FileArchiver.Services
{
    /// <summary>
    /// Service for user interactions and dialogs.
    /// Abstracts UI dialogs to enable testing without WPF dependencies.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Shows an informational message box.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="title">The dialog title.</param>
        void ShowInformation(string message, string title);

        /// <summary>
        /// Shows a warning message box.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="title">The dialog title.</param>
        void ShowWarning(string message, string title);

        /// <summary>
        /// Shows an error message box.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="title">The dialog title.</param>
        void ShowError(string message, string title);

        /// <summary>
        /// Shows a yes/no confirmation dialog.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="title">The dialog title.</param>
        /// <returns>True if user clicked Yes; false otherwise.</returns>
        bool ShowConfirmation(string message, string title);

        /// <summary>
        /// Shows a folder browser dialog.
        /// </summary>
        /// <param name="title">The dialog title.</param>
        /// <param name="initialFolder">The initial folder to show.</param>
        /// <returns>The selected folder path, or null if cancelled.</returns>
        string BrowseForFolder(string title, string initialFolder);
    }
}
