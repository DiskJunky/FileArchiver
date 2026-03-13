using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Serilog;

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
    /// Provides file search, preview, and archiving functionality.
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<FileItemModel> _fileItems;
        private ObservableCollection<ActivityLogEntry> _activityLog;
        private bool _isProcessing;
        private AppTheme _currentTheme;
        private readonly string _logFolderPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// Sets up UI bindings, initializes Serilog, and configures the default folder path.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize log folder path
            _logFolderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "FileArchiver",
                "Logs");

            // Ensure log directory exists
            Directory.CreateDirectory(_logFolderPath);

            // Initialize Serilog
            InitializeSerilog();

            // Initialize collections
            _fileItems = new ObservableCollection<FileItemModel>();
            _activityLog = new ObservableCollection<ActivityLogEntry>();
            
            FilesListView.ItemsSource = _fileItems;
            ActivityLogListView.ItemsSource = _activityLog;

            // Pre-fill with Downloads folder
            string downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");
            FolderPathTextBox.Text = downloadsPath;

            // Subscribe to property changes for status updates
            _fileItems.CollectionChanged += (s, e) => UpdateSelectionCount();

            // Log application start
            LogActivity("Application started");
            LogActivity($"Default folder set to: {downloadsPath}");
            LogActivity($"Log folder: {_logFolderPath}");

            // Initialize theme
            _currentTheme = ThemeManager.LoadThemePreference();
            UpdateThemeMenuChecks();
            LogActivity($"Theme set to: {_currentTheme}");
        }

        /// <summary>
        /// Initializes Serilog with file sink configured to write to the application's AppData folder.
        /// Creates rolling log files with size limits and retention policy.
        /// </summary>
        private void InitializeSerilog()
        {
            string logFilePath = Path.Combine(_logFolderPath, "FileArchiver-.log");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    logFilePath,
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: 10_485_760, // 10 MB
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Log.Information("Serilog initialized successfully");
        }

        /// <summary>
        /// Logs an activity message to both the UI activity log and the Serilog file log.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="isError">Indicates if this is an error message.</param>
        /// <param name="isWarning">Indicates if this is a warning message.</param>
        /// <param name="isSuccess">Indicates if this is a success message.</param>
        private void LogActivity(string message, bool isError = false, bool isWarning = false, bool isSuccess = false)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            var logEntry = new ActivityLogEntry
            {
                Timestamp = timestamp,
                Message = message,
                IsError = isError,
                IsWarning = isWarning,
                IsSuccess = isSuccess
            };
            
            // Insert at the beginning to show most recent first
            _activityLog.Insert(0, logEntry);
            
            // Limit log size to prevent memory issues (keep last 500 entries)
            if (_activityLog.Count > 500)
            {
                _activityLog.RemoveAt(_activityLog.Count - 1);
            }

            // Log to Serilog file
            if (isError)
            {
                Log.Error(message);
            }
            else if (isWarning)
            {
                Log.Warning(message);
            }
            else if (isSuccess)
            {
                Log.Information("[SUCCESS] {Message}", message);
            }
            else
            {
                Log.Information(message);
            }
        }

        /// <summary>
        /// Handles the Clear Log button click event.
        /// Clears the in-memory activity log display.
        /// </summary>
        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            _activityLog.Clear();
            LogActivity("Activity log cleared");
        }

        /// <summary>
        /// Handles the Browse button click event to select a source folder.
        /// </summary>
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Select Folder to Scan",
                InitialDirectory = FolderPathTextBox.Text
            };

            if (dialog.ShowDialog() == true)
            {
                FolderPathTextBox.Text = dialog.FolderName;
                LogActivity($"Folder changed to: {dialog.FolderName}");
            }
        }

        /// <summary>
        /// Handles the Open Log Folder status bar item click event.
        /// Opens the log folder in Windows Explorer.
        /// </summary>
        private void OpenLogFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Directory.Exists(_logFolderPath))
                {
                    Process.Start("explorer.exe", _logFolderPath);
                    LogActivity($"Opened log folder: {_logFolderPath}");
                }
                else
                {
                    MessageBox.Show($"Log folder does not exist:\n{_logFolderPath}", 
                        "Folder Not Found", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);
                    LogActivity($"Failed to open log folder - does not exist: {_logFolderPath}", isError: true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening log folder:\n{ex.Message}", 
                    "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                LogActivity($"Error opening log folder: {ex.Message}", isError: true);
            }
        }

        /// <summary>
        /// Handles the Scan Files button click event.
        /// Validates input and initiates an asynchronous file scan operation.
        /// </summary>
        private async void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isProcessing) return;

            string folderPath = FolderPathTextBox.Text;
            string searchText = SearchTextBox.Text?.Trim();

            // Validation
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                LogActivity("No folder path specified", isError: true);
                MessageBox.Show("Please specify a folder path.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Directory.Exists(folderPath))
            {
                LogActivity($"Folder does not exist: {folderPath}", isError: true);
                MessageBox.Show("The specified folder does not exist.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(searchText))
            {
                LogActivity("No search criteria specified", isError: true);
                MessageBox.Show("Please enter search terms.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            LogActivity($"Starting scan for: '{searchText}' in {folderPath}");

            // Disable UI during scan
            SetUIEnabled(false);
            _isProcessing = true;

            try
            {
                await ScanFilesAsync(folderPath, searchText);
            }
            finally
            {
                SetUIEnabled(true);
                _isProcessing = false;
                HideProgress();
            }
        }

        /// <summary>
        /// Asynchronously scans the specified folder for files matching the search criteria.
        /// Updates progress and displays results in the preview list.
        /// </summary>
        /// <param name="folderPath">The folder path to scan.</param>
        /// <param name="searchText">The search text containing words that must all match in filenames.</param>
        private async Task ScanFilesAsync(string folderPath, string searchText)
        {
            _fileItems.Clear();
            var searchWords = searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string archiveFolderPath = Path.Combine(folderPath, searchText);

            try
            {
                ShowProgress("Initializing scan...", 0);

                var allFiles = await Task.Run(() =>
                    Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly));

                LogActivity($"Found {allFiles.Length} total files in folder");
                ShowProgress($"Found {allFiles.Length} files. Analyzing matches...", 10);

                int totalFiles = allFiles.Length;
                int processedFiles = 0;
                int matchedFiles = 0;

                await Task.Run(() =>
                {
                    foreach (var file in allFiles)
                    {
                        string fileName = Path.GetFileName(file);

                        if (searchWords.All(word => fileName.Contains(word, StringComparison.OrdinalIgnoreCase)))
                        {
                            var fileInfo = new FileInfo(file);
                            var fileItem = new FileItemModel
                            {
                                FileName = fileName,
                                FullPath = file,
                                FileSize = fileInfo.Length,
                                IsSelected = true
                            };

                            if (Directory.Exists(archiveFolderPath))
                            {
                                string archiveFilePath = Path.Combine(archiveFolderPath, fileName);
                                fileItem.ExistsInArchive = File.Exists(archiveFilePath);
                            }

                            Dispatcher.Invoke(() =>
                            {
                                fileItem.PropertyChanged += (s, args) =>
                                {
                                    if (args.PropertyName == nameof(FileItemModel.IsSelected) ||
                                        args.PropertyName == nameof(FileItemModel.AllowOverwrite))
                                    {
                                        UpdateSelectionCount();
                                    }
                                };

                                _fileItems.Add(fileItem);
                            });

                            matchedFiles++;
                        }

                        processedFiles++;

                        if (processedFiles % 10 == 0 || processedFiles == totalFiles)
                        {
                            int progress = 10 + (int)((processedFiles / (double)totalFiles) * 85);
                            Dispatcher.Invoke(() =>
                            {
                                ShowProgress($"Scanning: {processedFiles}/{totalFiles} files • {matchedFiles} matches found", progress);
                            });
                        }
                    }
                });

                ShowProgress("Checking archive folder...", 95);
                UpdateArchiveInfo(archiveFolderPath);

                ShowProgress($"Scan complete! Found {matchedFiles} matching file(s).", 100);
                LogActivity($"Scan complete: {matchedFiles} file(s) matched out of {totalFiles} total", isSuccess: true);
                
                await Task.Delay(800);

                if (matchedFiles > 0)
                {
                    int existingCount = _fileItems.Count(f => f.ExistsInArchive);
                    if (existingCount > 0)
                    {
                        LogActivity($"{existingCount} file(s) already exist in archive", isWarning: true);
                    }
                    
                    MessageBox.Show($"Found {_fileItems.Count} matching file(s).", "Scan Complete",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    LogActivity("No matching files found", isWarning: true);
                    MessageBox.Show("No files found matching the search criteria.", "Scan Complete",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                LogActivity($"Error during scan: {ex.Message}", isError: true);
                MessageBox.Show($"Error scanning folder: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            UpdateSelectionCount();
        }

        /// <summary>
        /// Shows progress information in the status bar.
        /// </summary>
        /// <param name="message">The progress message to display.</param>
        /// <param name="progress">The progress percentage (0-100).</param>
        private void ShowProgress(string message, int progress)
        {
            ProgressStatusItem.Visibility = Visibility.Visible;
            ReadyStatusText.Visibility = Visibility.Collapsed;
            ProgressText.Text = message;
            ProgressBar.Value = progress;
        }

        /// <summary>
        /// Hides the progress indicator and shows the ready status.
        /// </summary>
        private void HideProgress()
        {
            ProgressStatusItem.Visibility = Visibility.Collapsed;
            ReadyStatusText.Visibility = Visibility.Visible;
            ReadyStatusText.Text = "Ready";
            ProgressBar.Value = 0;
            ProgressText.Text = string.Empty;
        }

        /// <summary>
        /// Updates the archive information displayed in the status bar.
        /// </summary>
        /// <param name="archiveFolderPath">The path to the archive folder.</param>
        private void UpdateArchiveInfo(string archiveFolderPath)
        {
            if (Directory.Exists(archiveFolderPath))
            {
                ArchiveExistsText.Text = "✓ Archive exists";
                ArchiveExistsText.Foreground = System.Windows.Media.Brushes.Green;

                int fileCount = Directory.GetFiles(archiveFolderPath).Length;
                ArchiveFileCountText.Text = $"{fileCount} file(s)";
                
                LogActivity($"Archive folder exists with {fileCount} file(s)");
            }
            else
            {
                ArchiveExistsText.Text = "✗ Archive will be created";
                ArchiveExistsText.Foreground = System.Windows.Media.Brushes.Gray;
                ArchiveFileCountText.Text = "0 file(s)";
                
                LogActivity("Archive folder does not exist (will be created)", isWarning: true);
            }

            ArchiveInfoStatusItem.Visibility = Visibility.Visible;
            StatusSeparator.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Handles the Select All button click event.
        /// Selects all files that can be archived.
        /// </summary>
        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            int count = 0;
            foreach (var item in _fileItems)
            {
                if (!item.ExistsInArchive || item.AllowOverwrite)
                {
                    if (!item.IsSelected)
                    {
                        item.IsSelected = true;
                        count++;
                    }
                }
            }
            
            if (count > 0)
            {
                LogActivity($"Selected all files ({count} file(s))");
            }
        }

        /// <summary>
        /// Handles the Deselect All button click event.
        /// Deselects all files in the preview list.
        /// </summary>
        private void DeselectAllButton_Click(object sender, RoutedEventArgs e)
        {
            int count = _fileItems.Count(f => f.IsSelected);
            
            foreach (var item in _fileItems)
            {
                item.IsSelected = false;
            }
            
            if (count > 0)
            {
                LogActivity($"Deselected all files ({count} file(s))");
            }
        }

        /// <summary>
        /// Updates the selection count display and enables/disables the Archive button.
        /// </summary>
        private void UpdateSelectionCount()
        {
            int selectedCount = _fileItems.Count(f => f.IsSelected && (!f.ExistsInArchive || f.AllowOverwrite));
            SelectionCountText.Text = $"Selected for archive: {selectedCount} file(s)";
            ArchiveButton.IsEnabled = selectedCount > 0 && !_isProcessing;
        }

        /// <summary>
        /// Handles the Archive button click event.
        /// Confirms user intent and initiates the archive operation.
        /// </summary>
        private async void ArchiveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isProcessing) return;

            string folderPath = FolderPathTextBox.Text;
            string searchText = SearchTextBox.Text?.Trim();
            string archiveFolderPath = Path.Combine(folderPath, searchText);

            var filesToArchive = _fileItems
                .Where(f => f.IsSelected && (!f.ExistsInArchive || f.AllowOverwrite))
                .ToList();

            if (filesToArchive.Count == 0)
            {
                LogActivity("No files selected for archiving", isWarning: true);
                MessageBox.Show("No files selected for archiving.", "Information",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Archive {filesToArchive.Count} file(s) to:\n{archiveFolderPath}\n\nContinue?",
                "Confirm Archive",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                LogActivity("Archive operation cancelled by user", isWarning: true);
                return;
            }

            LogActivity($"Starting archive operation: {filesToArchive.Count} file(s) to {archiveFolderPath}");

            SetUIEnabled(false);
            _isProcessing = true;

            try
            {
                await ArchiveFilesAsync(archiveFolderPath, filesToArchive);
            }
            finally
            {
                SetUIEnabled(true);
                _isProcessing = false;
                HideProgress();
            }
        }

        /// <summary>
        /// Asynchronously archives the selected files to the specified folder.
        /// </summary>
        /// <param name="archiveFolderPath">The destination archive folder path.</param>
        /// <param name="filesToArchive">The list of files to archive.</param>
        private async Task ArchiveFilesAsync(string archiveFolderPath, List<FileItemModel> filesToArchive)
        {
            try
            {
                ShowProgress("Preparing archive...", 0);

                bool folderCreated = false;
                await Task.Run(() =>
                {
                    if (!Directory.Exists(archiveFolderPath))
                    {
                        Directory.CreateDirectory(archiveFolderPath);
                        folderCreated = true;
                    }
                });

                if (folderCreated)
                {
                    LogActivity($"Created archive folder: {archiveFolderPath}", isSuccess: true);
                }

                int successCount = 0;
                int errorCount = 0;
                var errors = new List<string>();
                int totalFiles = filesToArchive.Count;

                ShowProgress("Archiving files...", 5);

                await Task.Run(() =>
                {
                    for (int i = 0; i < filesToArchive.Count; i++)
                    {
                        var fileItem = filesToArchive[i];
                        try
                        {
                            string destinationPath = Path.Combine(archiveFolderPath, fileItem.FileName);

                            int progress = 5 + (int)((i / (double)totalFiles) * 90);
                            Dispatcher.Invoke(() =>
                            {
                                ShowProgress($"Archiving: {i + 1}/{totalFiles} - {fileItem.FileName}", progress);
                            });

                            File.Move(fileItem.FullPath, destinationPath, fileItem.AllowOverwrite);
                            successCount++;
                            
                            Dispatcher.Invoke(() =>
                            {
                                string action = fileItem.AllowOverwrite && fileItem.ExistsInArchive ? "Overwrote" : "Archived";
                                LogActivity($"{action}: {fileItem.FileName}", isSuccess: true);
                            });
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            errors.Add($"{fileItem.FileName}: {ex.Message}");
                            
                            Dispatcher.Invoke(() =>
                            {
                                LogActivity($"Error archiving {fileItem.FileName}: {ex.Message}", isError: true);
                            });
                        }
                    }
                });

                ShowProgress($"Archive complete! {successCount} file(s) archived.", 100);
                await Task.Delay(800);

                LogActivity($"Archive complete: {successCount} succeeded, {errorCount} failed", 
                    isError: errorCount > 0, isSuccess: errorCount == 0);

                string message = $"Archive complete!\n\nSuccessfully archived: {successCount} file(s)";
                if (errorCount > 0)
                {
                    message += $"\nErrors: {errorCount} file(s)\n\n" + string.Join("\n", errors.Take(5));
                    if (errors.Count > 5)
                        message += $"\n... and {errors.Count - 5} more errors";
                }

                MessageBox.Show(message, "Archive Complete",
                    MessageBoxButton.OK,
                    errorCount > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);

                if (successCount > 0)
                {
                    LogActivity("Refreshing file list...");
                    ScanButton_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                LogActivity($"Critical error during archive: {ex.Message}", isError: true);
                MessageBox.Show($"Error during archiving: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Enables or disables UI controls during processing operations.
        /// </summary>
        /// <param name="enabled">True to enable controls, false to disable.</param>
        private void SetUIEnabled(bool enabled)
        {
            FolderPathTextBox.IsEnabled = enabled;
            BrowseButton.IsEnabled = enabled;
            SearchTextBox.IsEnabled = enabled;
            ScanButton.IsEnabled = enabled;
            SelectAllButton.IsEnabled = enabled;
            DeselectAllButton.IsEnabled = enabled;
            FilesListView.IsEnabled = enabled;
            
            if (!enabled)
            {
                ArchiveButton.IsEnabled = false;
            }
            else
            {
                UpdateSelectionCount();
            }
        }

        /// <summary>
        /// Handles the Light theme menu item click event.
        /// </summary>
        private void LightTheme_Click(object sender, RoutedEventArgs e)
        {
            _currentTheme = AppTheme.Light;
            ThemeManager.ApplyTheme(_currentTheme);
            ThemeManager.SaveThemePreference(_currentTheme);
            UpdateThemeMenuChecks();
            LogActivity("Theme changed to Light");
        }

        /// <summary>
        /// Handles the Dark theme menu item click event.
        /// </summary>
        private void DarkTheme_Click(object sender, RoutedEventArgs e)
        {
            _currentTheme = AppTheme.Dark;
            ThemeManager.ApplyTheme(_currentTheme);
            ThemeManager.SaveThemePreference(_currentTheme);
            UpdateThemeMenuChecks();
            LogActivity("Theme changed to Dark");
        }

        /// <summary>
        /// Handles the System Default theme menu item click event.
        /// </summary>
        private void SystemTheme_Click(object sender, RoutedEventArgs e)
        {
            _currentTheme = AppTheme.System;
            ThemeManager.ApplyTheme(_currentTheme);
            ThemeManager.SaveThemePreference(_currentTheme);
            UpdateThemeMenuChecks();
            var systemTheme = ThemeManager.GetSystemTheme();
            LogActivity($"Theme changed to System (currently {systemTheme})");
        }

        /// <summary>
        /// Updates the theme menu item checkmarks based on the current theme selection.
        /// </summary>
        private void UpdateThemeMenuChecks()
        {
            LightThemeMenuItem.IsChecked = (_currentTheme == AppTheme.Light);
            DarkThemeMenuItem.IsChecked = (_currentTheme == AppTheme.Dark);
            SystemThemeMenuItem.IsChecked = (_currentTheme == AppTheme.System);
        }

        /// <summary>
        /// Handles the Exit menu item click event.
        /// Closes the application and flushes Serilog.
        /// </summary>
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            LogActivity("Application closing");
            Log.CloseAndFlush();
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Handles the About menu item click event.
        /// Displays application information.
        /// </summary>
        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "File Archiver v1.0\n\n" +
                "A utility for organizing and archiving files.\n\n" +
                $"Log Location: {_logFolderPath}\n\n" +
                "© 2026 File Archiver",
                "About File Archiver",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        /// <summary>
        /// Handles the window closing event.
        /// Ensures Serilog is properly flushed and closed.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            Log.Information("Application shutdown");
            Log.CloseAndFlush();
            base.OnClosed(e);
        }
    }
}