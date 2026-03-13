using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Serilog;

namespace FileArchiver
{
    /// <summary>
    /// ViewModel for the File Archiver main window.
    /// Manages file scanning, archiving operations, activity logging, and theme management.
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        private ObservableCollection<FileItemModel> _fileItems;
        private ObservableCollection<ActivityLogEntry> _activityLog;
        private string _folderPath;
        private string _searchText;
        private string _selectionCountText;
        private string _progressText;
        private int _progressValue;
        private Visibility _progressStatusVisibility;
        private Visibility _readyStatusVisibility;
        private Visibility _archiveInfoStatusVisibility;
        private Visibility _statusSeparatorVisibility;
        private string _archiveExistsText;
        private object _archiveExistsColor;
        private string _archiveFileCountText;
        private bool _isProcessing;
        private bool _isUIEnabled;
        private AppTheme _currentTheme;
        private bool _isLightThemeChecked;
        private bool _isDarkThemeChecked;
        private bool _isSystemThemeChecked;
        private bool _isArchiveButtonEnabled;
        private bool _isScanButtonEnabled;

        private readonly string _logFolderPath;
        private ICommand _scanCommand;
        private ICommand _archiveCommand;
        private ICommand _selectAllCommand;
        private ICommand _deselectAllCommand;
        private ICommand _browseCommand;
        private ICommand _clearLogCommand;
        private ICommand _openLogFolderCommand;
        private ICommand _lightThemeCommand;
        private ICommand _darkThemeCommand;
        private ICommand _systemThemeCommand;
        private ICommand _exitCommand;
        private ICommand _aboutCommand;

        // Debounce timers for auto-save functionality
        private Timer _folderPathSaveTimer;
        private Timer _searchTextSaveTimer;
        private const int DebounceDelayMs = 500; // Wait 500ms after user stops typing

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// Sets up collections, initializes Serilog, and loads persisted preferences.
        /// </summary>
        public MainWindowViewModel()
        {
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

            // Subscribe to file items collection changes
            _fileItems.CollectionChanged += (s, e) => UpdateSelectionCount();

            // Load last used folder path from registry, default to Downloads
            _folderPath = SettingsManager.LoadLastSourceFolder();
            _searchText = SettingsManager.LoadLastSearchCriteria();

            // Initialize UI state
            _isUIEnabled = true;
            _progressStatusVisibility = Visibility.Collapsed;
            _readyStatusVisibility = Visibility.Visible;
            _archiveInfoStatusVisibility = Visibility.Collapsed;
            _statusSeparatorVisibility = Visibility.Collapsed;
            _isArchiveButtonEnabled = false;
            _isScanButtonEnabled = true;

            // Log application start
            LogActivity("Application started");
            LogActivity($"Source folder loaded: {_folderPath}");
            LogActivity($"Log folder: {_logFolderPath}");

            // Initialize and load theme
            _currentTheme = ThemeManager.LoadThemePreference();
            UpdateThemeMenuChecks();
            LogActivity($"Theme set to: {_currentTheme}");
        }

        #region Properties

        /// <summary>
        /// Gets or sets the collection of file items found by the scan.
        /// </summary>
        public ObservableCollection<FileItemModel> FileItems
        {
            get => _fileItems;
            set => SetField(ref _fileItems, value);
        }

        /// <summary>
        /// Gets or sets the collection of activity log entries.
        /// </summary>
        public ObservableCollection<ActivityLogEntry> ActivityLog
        {
            get => _activityLog;
            set => SetField(ref _activityLog, value);
        }

        /// <summary>
        /// Gets or sets the folder path to scan.
        /// Changes are automatically saved to registry with debouncing (500ms delay).
        /// </summary>
        public string FolderPath
        {
            get => _folderPath;
            set
            {
                if (SetField(ref _folderPath, value))
                {
                    // Reset and restart the debounce timer when folder path changes
                    _folderPathSaveTimer?.Dispose();
                    _folderPathSaveTimer = new Timer(
                        _ => SettingsManager.SaveLastSourceFolder(value),
                        null,
                        DebounceDelayMs,
                        Timeout.Infinite);
                }
            }
        }

        /// <summary>
        /// Gets or sets the search criteria text.
        /// Changes are automatically saved to registry with debouncing (500ms delay).
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetField(ref _searchText, value))
                {
                    // Reset and restart the debounce timer when search text changes
                    _searchTextSaveTimer?.Dispose();
                    _searchTextSaveTimer = new Timer(
                        _ => SettingsManager.SaveLastSearchCriteria(value),
                        null,
                        DebounceDelayMs,
                        Timeout.Infinite);
                }
            }
        }

        /// <summary>
        /// Gets or sets the selection count display text.
        /// </summary>
        public string SelectionCountText
        {
            get => _selectionCountText;
            set => SetField(ref _selectionCountText, value);
        }

        /// <summary>
        /// Gets or sets the progress message text.
        /// </summary>
        public string ProgressText
        {
            get => _progressText;
            set => SetField(ref _progressText, value);
        }

        /// <summary>
        /// Gets or sets the progress bar value (0-100).
        /// </summary>
        public int ProgressValue
        {
            get => _progressValue;
            set => SetField(ref _progressValue, value);
        }

        /// <summary>
        /// Gets or sets the visibility of the progress status item.
        /// </summary>
        public Visibility ProgressStatusVisibility
        {
            get => _progressStatusVisibility;
            set => SetField(ref _progressStatusVisibility, value);
        }

        /// <summary>
        /// Gets or sets the visibility of the ready status text.
        /// </summary>
        public Visibility ReadyStatusVisibility
        {
            get => _readyStatusVisibility;
            set => SetField(ref _readyStatusVisibility, value);
        }

        /// <summary>
        /// Gets or sets the visibility of the archive info status item.
        /// </summary>
        public Visibility ArchiveInfoStatusVisibility
        {
            get => _archiveInfoStatusVisibility;
            set => SetField(ref _archiveInfoStatusVisibility, value);
        }

        /// <summary>
        /// Gets or sets the visibility of the status bar separator.
        /// </summary>
        public Visibility StatusSeparatorVisibility
        {
            get => _statusSeparatorVisibility;
            set => SetField(ref _statusSeparatorVisibility, value);
        }

        /// <summary>
        /// Gets or sets the archive status text.
        /// </summary>
        public string ArchiveExistsText
        {
            get => _archiveExistsText;
            set => SetField(ref _archiveExistsText, value);
        }

        /// <summary>
        /// Gets or sets the archive exists indicator color.
        /// </summary>
        public object ArchiveExistsColor
        {
            get => _archiveExistsColor;
            set => SetField(ref _archiveExistsColor, value);
        }

        /// <summary>
        /// Gets or sets the archive file count text.
        /// </summary>
        public string ArchiveFileCountText
        {
            get => _archiveFileCountText;
            set => SetField(ref _archiveFileCountText, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether a processing operation is in progress.
        /// </summary>
        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetField(ref _isProcessing, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the UI controls are enabled.
        /// </summary>
        public bool IsUIEnabled
        {
            get => _isUIEnabled;
            set => SetField(ref _isUIEnabled, value);
        }

        /// <summary>
        /// Gets or sets the current application theme.
        /// </summary>
        public AppTheme CurrentTheme
        {
            get => _currentTheme;
            set => SetField(ref _currentTheme, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Light theme menu item is checked.
        /// </summary>
        public bool IsLightThemeChecked
        {
            get => _isLightThemeChecked;
            set => SetField(ref _isLightThemeChecked, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Dark theme menu item is checked.
        /// </summary>
        public bool IsDarkThemeChecked
        {
            get => _isDarkThemeChecked;
            set => SetField(ref _isDarkThemeChecked, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the System theme menu item is checked.
        /// </summary>
        public bool IsSystemThemeChecked
        {
            get => _isSystemThemeChecked;
            set => SetField(ref _isSystemThemeChecked, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the archive button is enabled.
        /// </summary>
        public bool IsArchiveButtonEnabled
        {
            get => _isArchiveButtonEnabled;
            set => SetField(ref _isArchiveButtonEnabled, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the scan button is enabled.
        /// </summary>
        public bool IsScanButtonEnabled
        {
            get => _isScanButtonEnabled;
            set => SetField(ref _isScanButtonEnabled, value);
        }

        #endregion

        #region Commands

        /// <summary>
        /// Gets the Scan command.
        /// </summary>
        public ICommand ScanCommand
        {
            get { return _scanCommand ?? (_scanCommand = new RelayCommand(param => ScanFilesCommand())); }
        }

        /// <summary>
        /// Gets the Archive command.
        /// </summary>
        public ICommand ArchiveCommand
        {
            get { return _archiveCommand ?? (_archiveCommand = new RelayCommand(param => ArchiveFilesCommand())); }
        }

        /// <summary>
        /// Gets the Select All command.
        /// </summary>
        public ICommand SelectAllCommand
        {
            get { return _selectAllCommand ?? (_selectAllCommand = new RelayCommand(param => SelectAllFiles())); }
        }

        /// <summary>
        /// Gets the Deselect All command.
        /// </summary>
        public ICommand DeselectAllCommand
        {
            get { return _deselectAllCommand ?? (_deselectAllCommand = new RelayCommand(param => DeselectAllFiles())); }
        }

        /// <summary>
        /// Gets the Browse Folder command.
        /// </summary>
        public ICommand BrowseCommand
        {
            get { return _browseCommand ?? (_browseCommand = new RelayCommand(param => BrowseForFolder())); }
        }

        /// <summary>
        /// Gets the Clear Log command.
        /// </summary>
        public ICommand ClearLogCommand
        {
            get { return _clearLogCommand ?? (_clearLogCommand = new RelayCommand(param => ClearActivityLog())); }
        }

        /// <summary>
        /// Gets the Open Log Folder command.
        /// </summary>
        public ICommand OpenLogFolderCommand
        {
            get { return _openLogFolderCommand ?? (_openLogFolderCommand = new RelayCommand(param => OpenLogFolder())); }
        }

        /// <summary>
        /// Gets the Light Theme command.
        /// </summary>
        public ICommand LightThemeCommand
        {
            get { return _lightThemeCommand ?? (_lightThemeCommand = new RelayCommand(param => ChangeTheme(AppTheme.Light))); }
        }

        /// <summary>
        /// Gets the Dark Theme command.
        /// </summary>
        public ICommand DarkThemeCommand
        {
            get { return _darkThemeCommand ?? (_darkThemeCommand = new RelayCommand(param => ChangeTheme(AppTheme.Dark))); }
        }

        /// <summary>
        /// Gets the System Theme command.
        /// </summary>
        public ICommand SystemThemeCommand
        {
            get { return _systemThemeCommand ?? (_systemThemeCommand = new RelayCommand(param => ChangeTheme(AppTheme.System))); }
        }

        /// <summary>
        /// Gets the Exit command.
        /// </summary>
        public ICommand ExitCommand
        {
            get { return _exitCommand ?? (_exitCommand = new RelayCommand(param => RequestExit())); }
        }

        /// <summary>
        /// Gets the About command.
        /// </summary>
        public ICommand AboutCommand
        {
            get { return _aboutCommand ?? (_aboutCommand = new RelayCommand(param => ShowAbout())); }
        }

        #endregion

        #region Command Methods

        /// <summary>
        /// Initiates the file scan operation.
        /// </summary>
        private async void ScanFilesCommand()
        {
            if (IsProcessing) return;

            // Validation
            if (string.IsNullOrWhiteSpace(FolderPath))
            {
                LogActivity("No folder path specified", isError: true);
                MessageBox.Show("Please specify a folder path.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Directory.Exists(FolderPath))
            {
                LogActivity($"Folder does not exist: {FolderPath}", isError: true);
                MessageBox.Show("The specified folder does not exist.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LogActivity("No search criteria specified", isError: true);
                MessageBox.Show("Please enter search terms.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            LogActivity($"Starting scan for: '{SearchText}' in {FolderPath}");

            SetUIEnabled(false);
            IsProcessing = true;

            try
            {
                await ScanFilesAsync(FolderPath, SearchText);
            }
            finally
            {
                SetUIEnabled(true);
                IsProcessing = false;
                HideProgress();
                // Update selection count after IsProcessing is set to false
                // This ensures the Archive button is properly enabled/disabled
                UpdateSelectionCount();
            }
        }

        /// <summary>
        /// Initiates the archive operation.
        /// </summary>
        private async void ArchiveFilesCommand()
        {
            if (IsProcessing) return;

            string searchText = SearchText?.Trim();
            string archiveFolderPath = Path.Combine(FolderPath, searchText);

            var filesToArchive = FileItems
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
            IsProcessing = true;

            try
            {
                await ArchiveFilesAsync(archiveFolderPath, filesToArchive);
            }
            finally
            {
                SetUIEnabled(true);
                IsProcessing = false;
                HideProgress();
                // Update selection count after IsProcessing is set to false
                // This ensures the Archive button is properly enabled/disabled
                UpdateSelectionCount();
            }
        }

        /// <summary>
        /// Selects all files that can be archived.
        /// </summary>
        private void SelectAllFiles()
        {
            int count = 0;
            foreach (var item in FileItems)
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
        /// Deselects all files.
        /// </summary>
        private void DeselectAllFiles()
        {
            int count = FileItems.Count(f => f.IsSelected);

            foreach (var item in FileItems)
            {
                item.IsSelected = false;
            }

            if (count > 0)
            {
                LogActivity($"Deselected all files ({count} file(s))");
            }
        }

        /// <summary>
        /// Opens a folder browser dialog to select a folder.
        /// The selected folder is automatically saved to registry via property change.
        /// </summary>
        private void BrowseForFolder()
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select Folder to Scan",
                InitialDirectory = FolderPath
            };

            if (dialog.ShowDialog() == true)
            {
                FolderPath = dialog.FolderName;
                LogActivity($"Folder changed to: {dialog.FolderName}");
            }
        }

        /// <summary>
        /// Clears the activity log.
        /// </summary>
        private void ClearActivityLog()
        {
            ActivityLog.Clear();
            LogActivity("Activity log cleared");
        }

        /// <summary>
        /// Opens the log folder in Windows Explorer.
        /// </summary>
        private void OpenLogFolder()
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
        /// Changes the application theme.
        /// </summary>
        /// <param name="theme">The theme to apply.</param>
        private void ChangeTheme(AppTheme theme)
        {
            CurrentTheme = theme;
            ThemeManager.ApplyTheme(CurrentTheme);
            ThemeManager.SaveThemePreference(CurrentTheme);
            UpdateThemeMenuChecks();
            var displayTheme = theme == AppTheme.System ? ThemeManager.GetSystemTheme() : theme;
            LogActivity($"Theme changed to {theme} (currently {displayTheme})");
        }

        /// <summary>
        /// Requests application exit and flushes Serilog.
        /// </summary>
        private void RequestExit()
        {
            LogActivity("Application closing");
            Log.CloseAndFlush();
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Shows the About dialog.
        /// </summary>
        private void ShowAbout()
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

        #endregion

        #region Helper Methods

        /// <summary>
        /// Initializes Serilog with file sink configuration.
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
        /// Logs an activity message to both the UI log and Serilog file.
        /// </summary>
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

            ActivityLog.Insert(0, logEntry);

            if (ActivityLog.Count > 500)
            {
                ActivityLog.RemoveAt(ActivityLog.Count - 1);
            }

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
        /// Asynchronously scans for files matching the search criteria.
        /// </summary>
        private async Task ScanFilesAsync(string folderPath, string searchText)
        {
            FileItems.Clear();
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

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                fileItem.PropertyChanged += (s, args) =>
                                {
                                    if (args.PropertyName == nameof(FileItemModel.IsSelected) ||
                                        args.PropertyName == nameof(FileItemModel.AllowOverwrite))
                                    {
                                        UpdateSelectionCount();
                                    }
                                };

                                FileItems.Add(fileItem);
                            });

                            matchedFiles++;
                        }

                        processedFiles++;

                        if (processedFiles % 10 == 0 || processedFiles == totalFiles)
                        {
                            int progress = 10 + (int)((processedFiles / (double)totalFiles) * 85);
                            Application.Current.Dispatcher.Invoke(() =>
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
                    int existingCount = FileItems.Count(f => f.ExistsInArchive);
                    if (existingCount > 0)
                    {
                        LogActivity($"{existingCount} file(s) already exist in archive", isWarning: true);
                    }

                    MessageBox.Show($"Found {FileItems.Count} matching file(s).", "Scan Complete",
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
        /// Asynchronously archives selected files to the destination folder.
        /// </summary>
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
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                ShowProgress($"Archiving: {i + 1}/{totalFiles} - {fileItem.FileName}", progress);
                            });

                            File.Move(fileItem.FullPath, destinationPath, fileItem.AllowOverwrite);
                            successCount++;

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                string action = fileItem.AllowOverwrite && fileItem.ExistsInArchive ? "Overwrote" : "Archived";
                                LogActivity($"{action}: {fileItem.FileName}", isSuccess: true);
                            });
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            errors.Add($"{fileItem.FileName}: {ex.Message}");

                            Application.Current.Dispatcher.Invoke(() =>
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
                    ScanFilesCommand();
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
        /// Shows progress information.
        /// </summary>
        private void ShowProgress(string message, int progress)
        {
            ProgressStatusVisibility = Visibility.Visible;
            ReadyStatusVisibility = Visibility.Collapsed;
            ProgressText = message;
            ProgressValue = progress;
        }

        /// <summary>
        /// Hides the progress indicator.
        /// </summary>
        private void HideProgress()
        {
            ProgressStatusVisibility = Visibility.Collapsed;
            ReadyStatusVisibility = Visibility.Visible;
            ProgressValue = 0;
            ProgressText = string.Empty;
        }

        /// <summary>
        /// Updates the archive information display.
        /// </summary>
        private void UpdateArchiveInfo(string archiveFolderPath)
        {
            if (Directory.Exists(archiveFolderPath))
            {
                ArchiveExistsText = "✓ Archive exists";
                ArchiveExistsColor = System.Windows.Media.Brushes.Green;

                int fileCount = Directory.GetFiles(archiveFolderPath).Length;
                ArchiveFileCountText = $"{fileCount} file(s)";

                LogActivity($"Archive folder exists with {fileCount} file(s)");
            }
            else
            {
                ArchiveExistsText = "✗ Archive will be created";
                ArchiveExistsColor = System.Windows.Media.Brushes.Gray;
                ArchiveFileCountText = "0 file(s)";

                LogActivity("Archive folder does not exist (will be created)", isWarning: true);
            }

            ArchiveInfoStatusVisibility = Visibility.Visible;
            StatusSeparatorVisibility = Visibility.Visible;
        }

        /// <summary>
        /// Updates the selection count and archive button enabled state.
        /// </summary>
        private void UpdateSelectionCount()
        {
            int selectedCount = FileItems.Count(f => f.IsSelected && (!f.ExistsInArchive || f.AllowOverwrite));
            SelectionCountText = $"Selected for archive: {selectedCount} file(s)";
            IsArchiveButtonEnabled = selectedCount > 0 && !IsProcessing;
        }

        /// <summary>
        /// Enables or disables the UI controls.
        /// </summary>
        private void SetUIEnabled(bool enabled)
        {
            IsUIEnabled = enabled;
            IsScanButtonEnabled = enabled;

            if (!enabled)
            {
                IsArchiveButtonEnabled = false;
            }
            else
            {
                UpdateSelectionCount();
            }
        }

        /// <summary>
        /// Updates the theme menu item checked states.
        /// </summary>
        private void UpdateThemeMenuChecks()
        {
            IsLightThemeChecked = (CurrentTheme == AppTheme.Light);
            IsDarkThemeChecked = (CurrentTheme == AppTheme.Dark);
            IsSystemThemeChecked = (CurrentTheme == AppTheme.System);
        }

        /// <summary>
        /// Flushes and closes Serilog on application shutdown.
        /// Also disposes of any pending debounce timers.
        /// </summary>
        public void OnWindowClosing()
        {
            // Dispose of debounce timers
            _folderPathSaveTimer?.Dispose();
            _searchTextSaveTimer?.Dispose();

            // Ensure final settings are saved
            SettingsManager.SaveLastSourceFolder(FolderPath);
            SettingsManager.SaveLastSearchCriteria(SearchText);

            Log.Information("Application shutdown");
            Log.CloseAndFlush();
        }

        #endregion
    }
}
