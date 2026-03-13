using System.Windows;
using System.Windows.Controls;

namespace FileArchiver
{
    /// <summary>
    /// Represents an entry in the activity log with categorized message types.
    /// </summary>
    public class ActivityLogEntry
    {
        /// <summary>
        /// Gets or sets the timestamp when the log entry was created.
        /// </summary>
        public string Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the log message content.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is an error message.
        /// </summary>
        public bool IsError { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a warning message.
        /// </summary>
        public bool IsWarning { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a success message.
        /// </summary>
        public bool IsSuccess { get; set; }
    }

    /// <summary>
    /// Main window for the File Archiver application.
    /// Provides the UI for file search, preview, and archiving functionality.
    /// Uses MVVM pattern with MainWindowViewModel for business logic.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// Sets up the ViewModel and UI bindings.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new MainWindowViewModel();
        }

        /// <summary>
        /// Handles the window loaded event.
        /// Restores the window state (size, position, and splitter location) from saved settings.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SettingsManager.LoadWindowState(out double width, out double height, out double left, out double top, out double row0Height);

            // Restore window dimensions
            this.Width = width;
            this.Height = height;

            // Restore window position - validate that it's within reasonable screen bounds
            // Only restore position if it appears to be valid (not negative and not too far off-screen)
            if (left >= -100 && top >= -100 && left < SystemParameters.VirtualScreenWidth + 100 && 
                top < SystemParameters.VirtualScreenHeight + 100)
            {
                this.Left = left;
                this.Top = top;
                this.WindowStartupLocation = WindowStartupLocation.Manual;
            }

            // Restore main content area splitter position
            var mainGrid = this.FindName("MainContentGrid") as Grid;
            if (mainGrid != null && mainGrid.RowDefinitions.Count > 0)
            {
                // Set the first row height to the saved value
                mainGrid.RowDefinitions[0].Height = new GridLength(row0Height);
            }
        }

        /// <summary>
        /// Handles the window closing event.
        /// Saves the current window state and ensures proper cleanup.
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Save window state before closing
            SettingsManager.SaveWindowState(this.Width, this.Height, this.Left, this.Top, 
                GetMainGridRow0Height());
        }

        /// <summary>
        /// Handles the window closed event.
        /// Ensures Serilog is properly flushed and closed.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            var viewModel = this.DataContext as MainWindowViewModel;
            viewModel?.OnWindowClosing();
            base.OnClosed(e);
        }

        /// <summary>
        /// Gets the height of the main grid's first row (upper content area).
        /// </summary>
        /// <returns>The height of the first row, or a default value if the grid cannot be accessed.</returns>
        private double GetMainGridRow0Height()
        {
            var mainGrid = this.FindName("MainContentGrid") as Grid;
            if (mainGrid != null && mainGrid.RowDefinitions.Count > 0)
            {
                return mainGrid.RowDefinitions[0].ActualHeight;
            }
            return 400; // Default fallback
        }
    }
}