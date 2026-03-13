using System;
using System.Collections.Generic;

namespace FileArchiver.Services
{
    /// <summary>
    /// Service for file system operations.
    /// Abstracts file I/O to enable testing without actual file system dependencies.
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// Gets all files in a directory matching the search option.
        /// </summary>
        /// <param name="folderPath">The folder path to search.</param>
        /// <param name="searchOption">Search option (top directory only or all subdirectories).</param>
        /// <returns>An array of file paths.</returns>
        string[] GetFiles(string folderPath, System.IO.SearchOption searchOption);

        /// <summary>
        /// Checks if a file exists.
        /// </summary>
        /// <param name="filePath">The file path to check.</param>
        /// <returns>True if the file exists; otherwise, false.</returns>
        bool FileExists(string filePath);

        /// <summary>
        /// Checks if a directory exists.
        /// </summary>
        /// <param name="folderPath">The directory path to check.</param>
        /// <returns>True if the directory exists; otherwise, false.</returns>
        bool DirectoryExists(string folderPath);

        /// <summary>
        /// Gets the file name from a full file path.
        /// </summary>
        /// <param name="filePath">The full file path.</param>
        /// <returns>The file name.</returns>
        string GetFileName(string filePath);

        /// <summary>
        /// Combines path segments into a single path.
        /// </summary>
        /// <param name="paths">The path segments to combine.</param>
        /// <returns>The combined path.</returns>
        string CombinePaths(params string[] paths);

        /// <summary>
        /// Gets file information (size, etc).
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <returns>A tuple containing file size and other info.</returns>
        (long Size, DateTime LastModified) GetFileInfo(string filePath);

        /// <summary>
        /// Creates a directory if it doesn't exist.
        /// </summary>
        /// <param name="folderPath">The directory path to create.</param>
        void CreateDirectory(string folderPath);

        /// <summary>
        /// Moves a file to a new location.
        /// </summary>
        /// <param name="sourcePath">The source file path.</param>
        /// <param name="destinationPath">The destination file path.</param>
        /// <param name="overwrite">Whether to overwrite if the destination file exists.</param>
        void MoveFile(string sourcePath, string destinationPath, bool overwrite);

        /// <summary>
        /// Gets the count of files in a directory.
        /// </summary>
        /// <param name="folderPath">The directory path.</param>
        /// <returns>The number of files in the directory.</returns>
        int GetFileCount(string folderPath);

        /// <summary>
        /// Opens a file or folder with the default application.
        /// </summary>
        /// <param name="path">The file or folder path to open.</param>
        void OpenPath(string path);
    }
}
