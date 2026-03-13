using System.Windows;

namespace FileArchiver
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Load and apply saved theme preference
            var savedTheme = ThemeManager.LoadThemePreference();
            ThemeManager.ApplyTheme(savedTheme);
        }
    }
}