using System;
using System.Collections.Generic;
using System.IO;

namespace FileArchiver.Services
{
    /// <summary>
    /// Represents a file to be archived.
    /// </summary>
    public class FileToArchive
    {
        /// <summary>
        /// The file name.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The full source file path.
        /// </summary>
        public string SourcePath { get; set; }

        /// <summary>
        /// Whether to allow overwriting if file exists in archive.
        /// </summary>
        public bool AllowOverwrite { get; set; }

        /// <summary>
        /// Whether the file already exists in the archive.
        /// </summary>
        public bool ExistsInArchive { get; set; }
    }

    /// <summary>
    /// Represents the result of an archive operation.
    /// </summary>
    public class ArchiveResult
    {
        /// <summary>
        /// The number of files successfully archived.
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// The number of files that failed to archive.
        /// </summary>
        public int ErrorCount { get; set; }

        /// <summary>
        /// List of error messages for files that failed.
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();
    }

    /// <summary>
    /// Service for archive operations.
    /// Abstracts the business logic of archiving files.
    /// </summary>
    public interface IArchiveService
    {
        /// <summary>
        /// Archives files to a destination folder.
        /// </summary>
        /// <param name="archiveFolderPath">The destination archive folder.</param>
        /// <param name="filesToArchive">The files to archive.</param>
        /// <param name="progress">Progress callback with (current, total) parameters.</param>
        /// <returns>The archive result with success/error counts.</returns>
        Task<ArchiveResult> ArchiveAsync(string archiveFolderPath, List<FileToArchive> filesToArchive, Action<int, int> progress = null);

        /// <summary>
        /// Gets the count of files in an archive folder.
        /// </summary>
        /// <param name="archiveFolderPath">The archive folder path.</param>
        /// <returns>The number of files in the archive.</returns>
        int GetArchiveFileCount(string archiveFolderPath);
    }

    /// <summary>
    /// Implementation of the archive service.
    /// </summary>
    public class ArchiveService : IArchiveService
    {
        private readonly IFileService _fileService;
        private readonly IApplicationLogger _logger;

        /// <summary>
        /// Initializes a new instance of the ArchiveService.
        /// </summary>
        public ArchiveService(IFileService fileService, IApplicationLogger logger)
        {
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Archives files asynchronously.
        /// </summary>
        public async Task<ArchiveResult> ArchiveAsync(string archiveFolderPath, List<FileToArchive> filesToArchive, Action<int, int> progress = null)
        {
            if (string.IsNullOrWhiteSpace(archiveFolderPath))
                throw new ArgumentException("Archive folder path cannot be null or empty.", nameof(archiveFolderPath));

            if (filesToArchive == null || filesToArchive.Count == 0)
                throw new ArgumentException("No files to archive.", nameof(filesToArchive));

            return await Task.Run(() => PerformArchive(archiveFolderPath, filesToArchive, progress));
        }

        /// <summary>
        /// Performs the actual archive logic.
        /// </summary>
        private ArchiveResult PerformArchive(string archiveFolderPath, List<FileToArchive> filesToArchive, Action<int, int> progress)
        {
            var result = new ArchiveResult();

            try
            {
                // Create archive folder if it doesn't exist
                _fileService.CreateDirectory(archiveFolderPath);
                _logger.LogInformation($"Archive folder ready: {archiveFolderPath}");

                int processedCount = 0;

                foreach (var fileToArchive in filesToArchive)
                {
                    try
                    {
                        string destinationPath = _fileService.CombinePaths(archiveFolderPath, fileToArchive.FileName);

                        // Move file to archive
                        _fileService.MoveFile(fileToArchive.SourcePath, destinationPath, fileToArchive.AllowOverwrite);

                        result.SuccessCount++;

                        string action = fileToArchive.AllowOverwrite && fileToArchive.ExistsInArchive ? "Overwrote" : "Archived";
                        _logger.LogInformation($"{action}: {fileToArchive.FileName}");
                    }
                    catch (Exception ex)
                    {
                        result.ErrorCount++;
                        result.Errors.Add($"{fileToArchive.FileName}: {ex.Message}");
                        _logger.LogError($"Error archiving {fileToArchive.FileName}: {ex.Message}");
                    }

                    processedCount++;
                    progress?.Invoke(processedCount, filesToArchive.Count);
                }

                _logger.LogInformation($"Archive operation complete: {result.SuccessCount} succeeded, {result.ErrorCount} failed");
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, "Critical error during archive operation");
                throw;
            }

            return result;
        }

        /// <summary>
        /// Gets the count of files in an archive folder.
        /// </summary>
        public int GetArchiveFileCount(string archiveFolderPath)
        {
            return _fileService.GetFileCount(archiveFolderPath);
        }
    }
}
