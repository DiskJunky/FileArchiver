using System;
using System.Diagnostics;
using System.IO;

namespace FileArchiver.Services
{
    /// <summary>
    /// File system service implementation.
    /// Provides abstraction over actual file system operations.
    /// </summary>
    public class FileService : IFileService
    {
        /// <summary>
        /// Gets all files in a directory.
        /// </summary>
        public string[] GetFiles(string folderPath, System.IO.SearchOption searchOption)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                throw new ArgumentException("Folder path cannot be null or empty.", nameof(folderPath));

            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException($"Directory not found: {folderPath}");

            return Directory.GetFiles(folderPath, "*.*", searchOption);
        }

        /// <summary>
        /// Checks if a file exists.
        /// </summary>
        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        /// <summary>
        /// Checks if a directory exists.
        /// </summary>
        public bool DirectoryExists(string folderPath)
        {
            return Directory.Exists(folderPath);
        }

        /// <summary>
        /// Gets the file name from a path.
        /// </summary>
        public string GetFileName(string filePath)
        {
            return Path.GetFileName(filePath);
        }

        /// <summary>
        /// Combines path segments.
        /// </summary>
        public string CombinePaths(params string[] paths)
        {
            return Path.Combine(paths);
        }

        /// <summary>
        /// Gets file information.
        /// </summary>
        public (long Size, DateTime LastModified) GetFileInfo(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            var fileInfo = new FileInfo(filePath);
            return (fileInfo.Length, fileInfo.LastWriteTime);
        }

        /// <summary>
        /// Creates a directory.
        /// </summary>
        public void CreateDirectory(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        /// <summary>
        /// Moves a file.
        /// </summary>
        public void MoveFile(string sourcePath, string destinationPath, bool overwrite)
        {
            if (!File.Exists(sourcePath))
                throw new FileNotFoundException($"Source file not found: {sourcePath}");

            File.Move(sourcePath, destinationPath, overwrite);
        }

        /// <summary>
        /// Gets the count of files in a directory.
        /// </summary>
        public int GetFileCount(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                return 0;

            return Directory.GetFiles(folderPath).Length;
        }

        /// <summary>
        /// Opens a path with the default application.
        /// </summary>
        public void OpenPath(string path)
        {
            if (!Directory.Exists(path) && !File.Exists(path))
                throw new FileNotFoundException($"Path not found: {path}");

            Process.Start("explorer.exe", path);
        }
    }
}
