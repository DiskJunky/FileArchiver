using System;
using System.Windows;
using Microsoft.Win32;

namespace FileArchiver
{
    public enum AppTheme
    {
        Light,
        Dark,
        System
    }

    public static class ThemeManager
    {
        private const string ThemePreferenceKey = "FileArchiver_ThemePreference";

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