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
    /// </summary>
    public static class ThemeManager
    {
        /// <summary>
        /// Registry key name for storing the user's theme preference.
        /// </summary>
        private const string ThemePreferenceKey = "FileArchiver_ThemePreference";

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
        /// Saves the user's theme preference to the Windows registry.
        /// </summary>
        /// <param name="theme">The theme preference to save.</param>
        /// <remarks>
        /// Saves to HKEY_CURRENT_USER\Software\FileArchiver.
        /// Fails silently if registry access is denied.
        /// </remarks>
        public static void SaveThemePreference(AppTheme theme)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(
                    @"Software\FileArchiver"))
                {
                    key?.SetValue(ThemePreferenceKey, theme.ToString());
                }
            }
            catch
            {
                // Silently fail if we can't write to registry
            }
        }

        /// <summary>
        /// Loads the user's saved theme preference from the Windows registry.
        /// </summary>
        /// <returns>
        /// The saved theme preference, or <see cref="AppTheme.System"/> if no preference is found.
        /// </returns>
        /// <remarks>
        /// Reads from HKEY_CURRENT_USER\Software\FileArchiver.
        /// Defaults to <see cref="AppTheme.System"/> if the registry key doesn't exist or cannot be read.
        /// </remarks>
        public static AppTheme LoadThemePreference()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\FileArchiver"))
                {
                    var value = key?.GetValue(ThemePreferenceKey);
                    if (value != null && Enum.TryParse<AppTheme>(value.ToString(), out var theme))
                    {
                        return theme;
                    }
                }
            }
            catch
            {
                // Silently fail if we can't read from registry
            }

            return AppTheme.System; // Default to system theme
        }
    }
}