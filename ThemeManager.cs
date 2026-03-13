using System;
using System.Windows;
using Microsoft.Win32;

namespace FileArchiver
{
    /// <summary>
    /// Defines the available application theme options.
    /// </summary>
    public enum AppTheme
    {
        /// <summary>
        /// Light theme with bright backgrounds and dark text.
        /// </summary>
        Light,

        /// <summary>
        /// Dark theme with dark backgrounds and light text.
        /// </summary>
        Dark,

        /// <summary>
        /// Automatically uses the current Windows system theme setting.
        /// </summary>
        System
    }

    /// <summary>
    /// Manages application theme selection, persistence, and system theme detection.
    /// Theme preferences are persisted using .NET application settings.
    /// </summary>
    public static class ThemeManager
    {
        /// <summary>
        /// Applies the specified theme to the application.
        /// If System theme is selected, automatically detects the current Windows theme.
        /// </summary>
        /// <param name="theme">The theme to apply.</param>
        public static void ApplyTheme(AppTheme theme)
        {
            var actualTheme = theme;

            // If system theme, detect current Windows theme
            if (theme == AppTheme.System)
            {
                actualTheme = GetSystemTheme();
            }

            var themeDictionary = new ResourceDictionary();
            themeDictionary.Source = new Uri(
                actualTheme == AppTheme.Dark
                    ? "Themes/DarkTheme.xaml"
                    : "Themes/LightTheme.xaml",
                UriKind.Relative);

            // Clear existing theme resources and apply new theme
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(themeDictionary);
        }

        /// <summary>
        /// Detects the current Windows system theme setting from the registry.
        /// </summary>
        /// <returns>
        /// <see cref="AppTheme.Dark"/> if Windows is using dark mode,
        /// <see cref="AppTheme.Light"/> if Windows is using light mode or if the registry value cannot be read.
        /// </returns>
        public static AppTheme GetSystemTheme()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    var value = key?.GetValue("AppsUseLightTheme");
                    if (value is int intValue)
                    {
                        return intValue == 0 ? AppTheme.Dark : AppTheme.Light;
                    }
                }
            }
            catch
            {
                // If we can't read registry, default to Light
            }

            return AppTheme.Light;
        }

        /// <summary>
        /// Saves the user's theme preference to application settings.
        /// </summary>
        /// <param name="theme">The theme preference to save.</param>
        /// <remarks>
        /// Settings are persisted to user.config file via SettingsManager.
        /// Fails silently if settings cannot be written.
        /// </remarks>
        public static void SaveThemePreference(AppTheme theme)
        {
            SettingsManager.SaveThemePreference(theme.ToString());
        }

        /// <summary>
        /// Loads the user's saved theme preference from application settings.
        /// </summary>
        /// <returns>
        /// The saved theme preference, or <see cref="AppTheme.System"/> if no preference is found.
        /// </returns>
        /// <remarks>
        /// Reads from user.config file via SettingsManager.
        /// Defaults to <see cref="AppTheme.System"/> if the setting is empty or cannot be read.
        /// </remarks>
        public static AppTheme LoadThemePreference()
        {
            try
            {
                string themeString = SettingsManager.LoadThemePreference();
                if (Enum.TryParse<AppTheme>(themeString, out var theme))
                {
                    return theme;
                }
            }
            catch
            {
                // Silently fail if we can't read from settings
            }

            return AppTheme.System; // Default to system theme
        }
    }
}