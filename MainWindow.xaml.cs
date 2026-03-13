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
    public partial class MainWindow : Window
    {
        private ObservableCollection<FileItemModel> _fileItems;
        private bool _isProcessing;

        public MainWindow()
        {
            InitializeComponent();
            _fileItems = new ObservableCollection<FileItemModel>();
            FilesListView.ItemsSource = _fileItems;

            // Pre-fill with Downloads folder
            string downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");
            FolderPathTextBox.Text = downloadsPath;

            // Subscribe to property changes for status updates
            _fileItems.CollectionChanged += (s, e) => UpdateSelectionCount();
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
                MessageBox.Show("Please specify a folder path.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show("The specified folder does not exist.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(searchText))
            {
                MessageBox.Show("Please enter search terms.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
                await Task.Delay(800); // Brief pause to show completion

                if (matchedFiles > 0)
                {
                    MessageBox.Show($"Found {_fileItems.Count} matching file(s).", "Scan Complete",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No files found matching the search criteria.", "Scan Complete",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error scanning folder: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            UpdateSelectionCount();
        }

        private void UpdateArchiveInfo(string archiveFolderPath)
        {
            if (Directory.Exists(archiveFolderPath))
            {
                ArchiveExistsText.Text = "✓ Archive folder exists";
                ArchiveExistsText.Foreground = System.Windows.Media.Brushes.Green;

                int fileCount = Directory.GetFiles(archiveFolderPath).Length;
                ArchiveFileCountText.Text = $"Files in archive: {fileCount}";
            }
            else
            {
                ArchiveExistsText.Text = "✗ Archive folder does not exist (will be created)";
                ArchiveExistsText.Foreground = System.Windows.Media.Brushes.Gray;
                ArchiveFileCountText.Text = "Files in archive: 0";
            }

            ArchiveInfoPanel.Visibility = Visibility.Visible;
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _fileItems)
            {
                if (!item.ExistsInArchive || item.AllowOverwrite)
                {
                    item.IsSelected = true;
                }
            }
        }

        private void DeselectAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in _fileItems)
            {
                item.IsSelected = false;
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
                return;

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
                await Task.Run(() =>
                {
                    if (!Directory.Exists(archiveFolderPath))
                    {
                        Directory.CreateDirectory(archiveFolderPath);
                    }
                });

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
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            errors.Add($"{fileItem.FileName}: {ex.Message}");
                        }
                    }
                });

                ShowProgress($"Archive complete! {successCount} file(s) archived.", 100);
                await Task.Delay(800); // Brief pause to show completion

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
                    ScanButton_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during archiving: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowProgress(string message, int progress)
        {
            ProgressPanel.Visibility = Visibility.Visible;
            ProgressText.Text = message;
            ProgressBar.Value = progress;
        }

        private void HideProgress()
        {
            ProgressPanel.Visibility = Visibility.Collapsed;
            ProgressBar.Value = 0;
            ProgressText.Text = string.Empty;
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
    }
}