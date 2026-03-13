using System;
using System.IO;
using FileArchiver.Properties;

namespace FileArchiver
{
    /// <summary>
    /// Manages user preferences and settings using .NET Configuration API.
    /// Handles persistence of application state across sessions using user.config file.
    /// </summary>
    public static class SettingsManager
    {
        /// <summary>
        /// Saves the last used source folder to application settings.
        /// </summary>
        /// <param name="folderPath">The folder path to save.</param>
        /// <remarks>
        /// Settings are persisted to user.config file in AppData\Local\FileArchiver\
        /// Fails silently if settings cannot be written.
        /// </remarks>
        public static void SaveLastSourceFolder(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
                return;

            try
            {
                Settings.Default.LastSourceFolder = folderPath;
                Settings.Default.Save();
            }
            catch
            {
                // Silently fail if we can't write settings
            }
        }

        /// <summary>
        /// Loads the last used source folder from application settings.
        /// </summary>
        /// <returns>
        /// The saved folder path if it exists, or the user's Downloads folder if no preference is found.
        /// </returns>
        /// <remarks>
        /// Reads from user.config file in AppData\Local\FileArchiver\
        /// Defaults to the Downloads folder if the setting is empty or the path doesn't exist.
        /// </remarks>
        public static string LoadLastSourceFolder()
        {
            try
            {
                string folderPath = Settings.Default.LastSourceFolder;

                // Verify the folder still exists and the setting is not empty
                if (!string.IsNullOrWhiteSpace(folderPath) && Directory.Exists(folderPath))
                {
                    return folderPath;
                }
            }
            catch
            {
                // Silently fail if we can't read from settings
            }

            // Default to Downloads folder
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");
        }

        /// <summary>
        /// Saves the last used search criteria to application settings.
        /// </summary>
        /// <param name="searchCriteria">The search criteria to save.</param>
        /// <remarks>
        /// Settings are persisted to user.config file in AppData\Local\FileArchiver\
        /// Fails silently if settings cannot be written.
        /// </remarks>
        public static void SaveLastSearchCriteria(string searchCriteria)
        {
            if (string.IsNullOrWhiteSpace(searchCriteria))
                return;

            try
            {
                Settings.Default.LastSearchCriteria = searchCriteria;
                Settings.Default.Save();
            }
            catch
            {
                // Silently fail if we can't write settings
            }
        }

        /// <summary>
        /// Loads the last used search criteria from application settings.
        /// </summary>
        /// <returns>
        /// The saved search criteria, or an empty string if no preference is found.
        /// </returns>
        /// <remarks>
        /// Reads from user.config file in AppData\Local\FileArchiver\
        /// Defaults to empty string if the setting is empty or cannot be read.
        /// </remarks>
        public static string LoadLastSearchCriteria()
        {
            try
            {
                return Settings.Default.LastSearchCriteria ?? string.Empty;
            }
            catch
            {
                // Silently fail if we can't read from settings
            }

            return string.Empty;
        }

        /// <summary>
        /// Saves the application theme preference to settings.
        /// </summary>
        /// <param name="theme">The theme to save.</param>
        /// <remarks>
        /// Settings are persisted to user.config file in AppData\Local\FileArchiver\
        /// Fails silently if settings cannot be written.
        /// </remarks>
        public static void SaveThemePreference(string theme)
        {
            if (string.IsNullOrWhiteSpace(theme))
                return;

            try
            {
                Settings.Default.ThemePreference = theme;
                Settings.Default.Save();
            }
            catch
            {
                // Silently fail if we can't write settings
            }
        }

        /// <summary>
        /// Loads the application theme preference from settings.
        /// </summary>
        /// <returns>
        /// The saved theme preference, or "System" if no preference is found.
        /// </returns>
        /// <remarks>
        /// Reads from user.config file in AppData\Local\FileArchiver\
        /// Defaults to "System" theme if the setting is empty or cannot be read.
        /// </remarks>
        public static string LoadThemePreference()
        {
            try
            {
                string theme = Settings.Default.ThemePreference;
                if (!string.IsNullOrWhiteSpace(theme))
                {
                    return theme;
                }
            }
            catch
            {
                // Silently fail if we can't read from settings
            }

            return "System";
        }
    }
}
