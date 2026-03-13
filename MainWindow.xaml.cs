using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace FileArchiver
{
    // Activity Log Entry Model
    public class ActivityLogEntry
    {
        public string Timestamp { get; set; }
        public string Message { get; set; }
        public bool IsError { get; set; }
        public bool IsWarning { get; set; }
        public bool IsSuccess { get; set; }
    }

    public partial class MainWindow : Window
    {
        private ObservableCollection<FileItemModel> _fileItems;
        private ObservableCollection<ActivityLogEntry> _activityLog;
        private bool _isProcessing;
        private AppTheme _currentTheme;

        public MainWindow()
        {
            InitializeComponent();
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

            // Initialize theme
            _currentTheme = ThemeManager.LoadThemePreference();
            UpdateThemeMenuChecks();
            LogActivity($"Theme set to: {_currentTheme}");
        }

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
            
            // Optional: Limit log size to prevent memory issues (keep last 500 entries)
            if (_activityLog.Count > 500)
            {
                _activityLog.RemoveAt(_activityLog.Count - 1);
            }
        }

        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            _activityLog.Clear();
            LogActivity("Activity log cleared");
        }

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

        private async Task ScanFilesAsync(string folderPath, string searchText)
        {
            // Perform scan
            _fileItems.Clear();
            var searchWords = searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string archiveFolderPath = Path.Combine(folderPath, searchText);

            try
            {
                ShowProgress("Initializing scan...", 0);

                // Get all files first
                var allFiles = await Task.Run(() =>
                    Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly));

                LogActivity($"Found {allFiles.Length} total files in folder");
                ShowProgress($"Found {allFiles.Length} files. Analyzing matches...", 10);

                int totalFiles = allFiles.Length;
                int processedFiles = 0;
                int matchedFiles = 0;

                // Process files with progress updates
                await Task.Run(() =>
                {
                    foreach (var file in allFiles)
                    {
                        string fileName = Path.GetFileName(file);

                        // Check if all search words are in the filename (case-insensitive)
                        if (searchWords.All(word => fileName.Contains(word, StringComparison.OrdinalIgnoreCase)))
                        {
                            var fileInfo = new FileInfo(file);
                            var fileItem = new FileItemModel
                            {
                                FileName = fileName,
                                FullPath = file,
                                FileSize = fileInfo.Length,
                                IsSelected = true // Default to selected
                            };

                            // Check if file exists in archive folder
                            if (Directory.Exists(archiveFolderPath))
                            {
                                string archiveFilePath = Path.Combine(archiveFolderPath, fileName);
                                fileItem.ExistsInArchive = File.Exists(archiveFilePath);
                            }

                            // Add to collection on UI thread
                            Dispatcher.Invoke(() =>
                            {
                                // Subscribe to property changes for this item
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

                        // Update progress every 10 files or on last file
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

                // Update archive info
                ShowProgress("Checking archive folder...", 95);
                UpdateArchiveInfo(archiveFolderPath);

                ShowProgress($"Scan complete! Found {matchedFiles} matching file(s).", 100);
                LogActivity($"Scan complete: {matchedFiles} file(s) matched out of {totalFiles} total", isSuccess: true);
                
                await Task.Delay(800); // Brief pause to show completion

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

        private void ShowProgress(string message, int progress)
        {
            ProgressStatusItem.Visibility = Visibility.Visible;
            ReadyStatusText.Visibility = Visibility.Collapsed;
            ProgressText.Text = message;
            ProgressBar.Value = progress;
        }

        private void HideProgress()
        {
            ProgressStatusItem.Visibility = Visibility.Collapsed;
            ReadyStatusText.Visibility = Visibility.Visible;
            ReadyStatusText.Text = "Ready";
            ProgressBar.Value = 0;
            ProgressText.Text = string.Empty;
        }

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

        private void UpdateSelectionCount()
        {
            int selectedCount = _fileItems.Count(f => f.IsSelected && (!f.ExistsInArchive || f.AllowOverwrite));
            SelectionCountText.Text = $"Selected for archive: {selectedCount} file(s)";
            ArchiveButton.IsEnabled = selectedCount > 0 && !_isProcessing;
        }

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

            // Disable UI during archive
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

        private async Task ArchiveFilesAsync(string archiveFolderPath, List<FileItemModel> filesToArchive)
        {
            try
            {
                ShowProgress("Preparing archive...", 0);

                // Create archive folder if it doesn't exist
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

                            // Update progress before moving
                            int progress = 5 + (int)((i / (double)totalFiles) * 90);
                            Dispatcher.Invoke(() =>
                            {
                                ShowProgress($"Archiving: {i + 1}/{totalFiles} - {fileItem.FileName}", progress);
                            });

                            // Move the file (overwrite if allowed)
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
                await Task.Delay(800); // Brief pause to show completion

                // Log summary
                LogActivity($"Archive complete: {successCount} succeeded, {errorCount} failed", 
                    isError: errorCount > 0, isSuccess: errorCount == 0);

                // Show results
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

                // Refresh the scan
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
                UpdateSelectionCount(); // This will set the proper state for ArchiveButton
            }
        }

        private void LightTheme_Click(object sender, RoutedEventArgs e)
        {
            _currentTheme = AppTheme.Light;
            ThemeManager.ApplyTheme(_currentTheme);
            ThemeManager.SaveThemePreference(_currentTheme);
            UpdateThemeMenuChecks();
            LogActivity("Theme changed to Light");
        }

        private void DarkTheme_Click(object sender, RoutedEventArgs e)
        {
            _currentTheme = AppTheme.Dark;
            ThemeManager.ApplyTheme(_currentTheme);
            ThemeManager.SaveThemePreference(_currentTheme);
            UpdateThemeMenuChecks();
            LogActivity("Theme changed to Dark");
        }

        private void SystemTheme_Click(object sender, RoutedEventArgs e)
        {
            _currentTheme = AppTheme.System;
            ThemeManager.ApplyTheme(_currentTheme);
            ThemeManager.SaveThemePreference(_currentTheme);
            UpdateThemeMenuChecks();
            var systemTheme = ThemeManager.GetSystemTheme();
            LogActivity($"Theme changed to System (currently {systemTheme})");
        }

        private void UpdateThemeMenuChecks()
        {
            LightThemeMenuItem.IsChecked = (_currentTheme == AppTheme.Light);
            DarkThemeMenuItem.IsChecked = (_currentTheme == AppTheme.Dark);
            SystemThemeMenuItem.IsChecked = (_currentTheme == AppTheme.System);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            LogActivity("Application closing");
            Application.Current.Shutdown();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "File Archiver v1.0\n\n" +
                "A utility for organizing and archiving files.\n\n" +
                "© 2026 File Archiver",
                "About File Archiver",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}