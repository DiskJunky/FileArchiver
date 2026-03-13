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
    /// Code-behind is minimal and only handles window lifecycle and UI events,
    /// delegating all business logic to the ViewModel.
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// Sets up the ViewModel and UI bindings.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainWindowViewModel();
            this.DataContext = _viewModel;
        }

        /// <summary>
        /// Handles the window loaded event.
        /// Delegates to ViewModel to restore window state (size, position, and splitter location).
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Get restored window state from ViewModel
            var (width, height, left, top, row0Height) = _viewModel.RestoreWindowState();

            // Apply window dimensions
            this.Width = width;
            this.Height = height;

            // Apply window position if valid (within screen bounds)
            // Validate that position is not too far off-screen to prevent inaccessible windows
            if (left >= -100 && top >= -100 && left < SystemParameters.VirtualScreenWidth + 100 && 
                top < SystemParameters.VirtualScreenHeight + 100)
            {
                this.Left = left;
                this.Top = top;
                this.WindowStartupLocation = WindowStartupLocation.Manual;
            }

            // Apply splitter position
            ApplySplitterPosition(row0Height);
        }

        /// <summary>
        /// Handles the window closing event.
        /// Delegates to ViewModel to save window state.
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Get current window state and save to ViewModel
            double splitterRow0Height = GetMainGridRow0Height();
            _viewModel.SaveWindowState(this.Width, this.Height, this.Left, this.Top, splitterRow0Height);
        }

        /// <summary>
        /// Handles the window closed event.
        /// Ensures ViewModel cleanup and Serilog shutdown occur.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            _viewModel?.OnWindowClosing();
            base.OnClosed(e);
        }

        /// <summary>
        /// Handles the KeyDown event for the Search Criteria TextBox.
        /// Triggers the Scan command when the user presses the RETURN key.
        /// </summary>
        private void SearchTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Return)
            {
                // Get ScanCommand from ViewModel
                if (_viewModel?.ScanCommand != null && _viewModel.ScanCommand.CanExecute(null))
                {
                    _viewModel.ScanCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Applies the saved splitter position to the main content grid.
        /// </summary>
        /// <param name="row0Height">The height of the first row (upper content area).</param>
        private void ApplySplitterPosition(double row0Height)
        {
            var mainGrid = this.FindName("MainContentGrid") as Grid;
            if (mainGrid != null && mainGrid.RowDefinitions.Count > 0)
            {
                mainGrid.RowDefinitions[0].Height = new GridLength(row0Height);
            }
        }

        /// <summary>
        /// Gets the height of the main grid's first row (upper content area).
        /// </summary>
        /// <returns>The actual height of the first row, or a default value if the grid cannot be accessed.</returns>
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