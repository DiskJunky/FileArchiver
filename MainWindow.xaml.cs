using System.Windows;

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
        /// Handles the window closing event.
        /// Ensures Serilog is properly flushed and closed.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            var viewModel = this.DataContext as MainWindowViewModel;
            viewModel?.OnWindowClosing();
            base.OnClosed(e);
        }
    }
}