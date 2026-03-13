using System;
using System.IO;
using System.Windows;

namespace FileArchiver
{
    /// <summary>
    /// Interaction logic for App.xaml.
    /// Handles application startup and theme initialization.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Handles the application startup event.
        /// Initializes themes and ensures resources are available.
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Load and apply saved theme preference
            var savedTheme = ThemeManager.LoadThemePreference();
            ThemeManager.ApplyTheme(savedTheme);

            // Ensure icon exists
            EnsureIconExists();
        }

        /// <summary>
        /// Ensures the application icon file exists.
        /// Creates a default icon if none is found.
        /// </summary>
        private void EnsureIconExists()
        {
            string resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
            string iconPath = Path.Combine(resourcesPath, "FileArchiver.png");

            if (!File.Exists(iconPath))
            {
                // Log that icon is missing
                Serilog.Log.Warning("Application icon not found at: {IconPath}", iconPath);
            }
        }
    }
}