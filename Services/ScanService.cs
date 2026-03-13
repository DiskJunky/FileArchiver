using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileArchiver.Services
{
    /// <summary>
    /// Represents a file found during a scan operation.
    /// </summary>
    public class ScannedFile
    {
        /// <summary>
        /// The file name.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The full file path.
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// The file size in bytes.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Whether the file exists in the archive.
        /// </summary>
        public bool ExistsInArchive { get; set; }
    }

    /// <summary>
    /// Represents the result of a scan operation.
    /// </summary>
    public class ScanResult
    {
        /// <summary>
        /// The total number of files scanned.
        /// </summary>
        public int TotalFilesScanned { get; set; }

        /// <summary>
        /// The list of files that matched the search criteria.
        /// </summary>
        public List<ScannedFile> MatchedFiles { get; set; } = new List<ScannedFile>();

        /// <summary>
        /// The number of files that already exist in the archive.
        /// </summary>
        public int ExistingArchiveFileCount { get; set; }
    }

    /// <summary>
    /// Service for file scanning operations.
    /// Abstracts the business logic of scanning and matching files.
    /// </summary>
    public interface IScanService
    {
        /// <summary>
        /// Scans a folder for files matching the search criteria.
        /// </summary>
        /// <param name="folderPath">The folder to scan.</param>
        /// <param name="searchCriteria">The search criteria (space-separated terms).</param>
        /// <param name="progress">Progress callback with (current, total) parameters.</param>
        /// <returns>The scan result containing matched files.</returns>
        Task<ScanResult> ScanAsync(string folderPath, string searchCriteria, Action<int, int> progress = null);
    }

    /// <summary>
    /// Implementation of the scan service.
    /// </summary>
    public class ScanService : IScanService
    {
        private readonly IFileService _fileService;
        private readonly IApplicationLogger _logger;

        /// <summary>
        /// Initializes a new instance of the ScanService.
        /// </summary>
        public ScanService(IFileService fileService, IApplicationLogger logger)
        {
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Scans for matching files.
        /// </summary>
        public async Task<ScanResult> ScanAsync(string folderPath, string searchCriteria, Action<int, int> progress = null)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                throw new ArgumentException("Folder path cannot be null or empty.", nameof(folderPath));

            if (string.IsNullOrWhiteSpace(searchCriteria))
                throw new ArgumentException("Search criteria cannot be null or empty.", nameof(searchCriteria));

            if (!_fileService.DirectoryExists(folderPath))
                throw new DirectoryNotFoundException($"Folder not found: {folderPath}");

            return await Task.Run(() => PerformScan(folderPath, searchCriteria, progress));
        }

        /// <summary>
        /// Performs the actual scan logic.
        /// </summary>
        private ScanResult PerformScan(string folderPath, string searchCriteria, Action<int, int> progress)
        {
            var result = new ScanResult();
            string archiveFolderPath = _fileService.CombinePaths(folderPath, searchCriteria);
            var searchWords = searchCriteria.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                string[] allFiles = _fileService.GetFiles(folderPath, System.IO.SearchOption.TopDirectoryOnly);
                result.TotalFilesScanned = allFiles.Length;

                int processedCount = 0;

                foreach (var filePath in allFiles)
                {
                    string fileName = _fileService.GetFileName(filePath);

                    // Check if all search words match (case-insensitive)
                    if (searchWords.All(word => fileName.Contains(word, StringComparison.OrdinalIgnoreCase)))
                    {
                        var (fileSize, _) = _fileService.GetFileInfo(filePath);

                        var scannedFile = new ScannedFile
                        {
                            FileName = fileName,
                            FullPath = filePath,
                            FileSize = fileSize,
                            ExistsInArchive = false
                        };

                        // Check if file exists in archive
                        if (_fileService.DirectoryExists(archiveFolderPath))
                        {
                            string archiveFilePath = _fileService.CombinePaths(archiveFolderPath, fileName);
                            scannedFile.ExistsInArchive = _fileService.FileExists(archiveFilePath);

                            if (scannedFile.ExistsInArchive)
                            {
                                result.ExistingArchiveFileCount++;
                            }
                        }

                        result.MatchedFiles.Add(scannedFile);
                    }

                    processedCount++;
                    progress?.Invoke(processedCount, result.TotalFilesScanned);
                }

                _logger.LogInformation($"Scan complete: Found {result.MatchedFiles.Count} matching files out of {result.TotalFilesScanned} total");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, $"Error during scan of {folderPath}");
                throw;
            }

            return result;
        }
    }
}
