using System;
using System.IO;
using FileArchiver.Properties;
using Serilog;

namespace FileArchiver
{
    /// <summary>
    /// Manages user preferences and settings using .NET Configuration API.
    /// Handles persistence of application state across sessions using user.config file.
    /// All operations include comprehensive error logging via Serilog.
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
                Log.Information("Successfully saved last source folder: {FolderPath}", folderPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save last source folder: {FolderPath}", folderPath);
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
                    Log.Information("Loaded last source folder: {FolderPath}", folderPath);
                    return folderPath;
                }

                if (!string.IsNullOrWhiteSpace(folderPath) && !Directory.Exists(folderPath))
                {
                    Log.Warning("Saved source folder no longer exists: {FolderPath}. Using Downloads folder instead.", folderPath);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load last source folder. Using Downloads folder instead.");
            }

            // Default to Downloads folder
            string downloadsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads");
            Log.Information("Using default Downloads folder: {DownloadsPath}", downloadsPath);
            return downloadsPath;
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
                Log.Information("Successfully saved last search criteria: {SearchCriteria}", searchCriteria);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save last search criteria: {SearchCriteria}", searchCriteria);
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
                string criteria = Settings.Default.LastSearchCriteria ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(criteria))
                {
                    Log.Information("Loaded last search criteria: {SearchCriteria}", criteria);
                }
                return criteria;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load last search criteria. Using empty string instead.");
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
                Log.Information("Successfully saved theme preference: {Theme}", theme);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save theme preference: {Theme}", theme);
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
                    Log.Information("Loaded theme preference: {Theme}", theme);
                    return theme;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load theme preference. Using System theme instead.");
            }

            Log.Information("Using default theme preference: System");
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
                Log.Information("Successfully saved window state. Width: {Width}, Height: {Height}, Left: {Left}, Top: {Top}, Row0Height: {Row0Height}",
                    width, height, left, top, mainGridRow0Height);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save window state. Width: {Width}, Height: {Height}, Left: {Left}, Top: {Top}, Row0Height: {Row0Height}",
                    width, height, left, top, mainGridRow0Height);
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
                {
                    width = w;
                    Log.Debug("Loaded WindowWidth from settings: {WindowWidth}", width);
                }
                else
                {
                    Log.Warning("Failed to parse WindowWidth from settings. Using default: {DefaultWidth}", width);
                }

                if (double.TryParse(Settings.Default.WindowHeight, out double h))
                {
                    height = h;
                    Log.Debug("Loaded WindowHeight from settings: {WindowHeight}", height);
                }
                else
                {
                    Log.Warning("Failed to parse WindowHeight from settings. Using default: {DefaultHeight}", height);
                }

                if (double.TryParse(Settings.Default.WindowLeft, out double l))
                {
                    left = l;
                    Log.Debug("Loaded WindowLeft from settings: {WindowLeft}", left);
                }
                else
                {
                    Log.Warning("Failed to parse WindowLeft from settings. Using default: {DefaultLeft}", left);
                }

                if (double.TryParse(Settings.Default.WindowTop, out double t))
                {
                    top = t;
                    Log.Debug("Loaded WindowTop from settings: {WindowTop}", top);
                }
                else
                {
                    Log.Warning("Failed to parse WindowTop from settings. Using default: {DefaultTop}", top);
                }

                if (double.TryParse(Settings.Default.MainGridRow0Height, out double r))
                {
                    mainGridRow0Height = r;
                    Log.Debug("Loaded MainGridRow0Height from settings: {MainGridRow0Height}", mainGridRow0Height);
                }
                else
                {
                    Log.Warning("Failed to parse MainGridRow0Height from settings. Using default: {DefaultRow0Height}", mainGridRow0Height);
                }

                Log.Information("Successfully loaded window state. Width: {Width}, Height: {Height}, Left: {Left}, Top: {Top}, Row0Height: {Row0Height}",
                    width, height, left, top, mainGridRow0Height);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load window state. Using default values. Width: {DefaultWidth}, Height: {DefaultHeight}, Left: {DefaultLeft}, Top: {DefaultTop}, Row0Height: {DefaultRow0Height}",
                    width, height, left, top, mainGridRow0Height);
            }
        }
    }
}