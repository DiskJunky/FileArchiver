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

        /// <summary>
        /// Saves the window state (size and position) to application settings.
        /// </summary>
        /// <param name="width">The window width in device-independent pixels.</param>
        /// <param name="height">The window height in device-independent pixels.</param>
        /// <param name="left">The window left position on screen.</param>
        /// <param name="top">The window top position on screen.</param>
        /// <param name="mainGridRow0Height">The height of the main grid's first row (upper content area).</param>
        /// <remarks>
        /// Settings are persisted to user.config file in AppData\Local\FileArchiver\
        /// Fails silently if settings cannot be written.
        /// </remarks>
        public static void SaveWindowState(double width, double height, double left, double top, double mainGridRow0Height)
        {
            try
            {
                Settings.Default.WindowWidth = width.ToString();
                Settings.Default.WindowHeight = height.ToString();
                Settings.Default.WindowLeft = left.ToString();
                Settings.Default.WindowTop = top.ToString();
                Settings.Default.MainGridRow0Height = mainGridRow0Height.ToString();
                Settings.Default.Save();
            }
            catch
            {
                // Silently fail if we can't write settings
            }
        }

        /// <summary>
        /// Loads the window state (size and position) from application settings.
        /// </summary>
        /// <param name="width">Output parameter for the window width.</param>
        /// <param name="height">Output parameter for the window height.</param>
        /// <param name="left">Output parameter for the window left position.</param>
        /// <param name="top">Output parameter for the window top position.</param>
        /// <param name="mainGridRow0Height">Output parameter for the main grid's first row height.</param>
        /// <remarks>
        /// Reads from user.config file in AppData\Local\FileArchiver\
        /// Uses default values if settings cannot be read or contain invalid data.
        /// Defaults: Width=1000, Height=850, Left=0, Top=0, Row0Height=400
        /// </remarks>
        public static void LoadWindowState(out double width, out double height, out double left, out double top, out double mainGridRow0Height)
        {
            width = 1000;
            height = 850;
            left = 0;
            top = 0;
            mainGridRow0Height = 400;

            try
            {
                if (double.TryParse(Settings.Default.WindowWidth, out double w))
                    width = w;
                if (double.TryParse(Settings.Default.WindowHeight, out double h))
                    height = h;
                if (double.TryParse(Settings.Default.WindowLeft, out double l))
                    left = l;
                if (double.TryParse(Settings.Default.WindowTop, out double t))
                    top = t;
                if (double.TryParse(Settings.Default.MainGridRow0Height, out double r))
                    mainGridRow0Height = r;
            }
            catch
            {
                // Silently fail, keeping default values
            }
        }
    }
}