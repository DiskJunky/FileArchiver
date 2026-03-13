using System.Configuration;

namespace FileArchiver.Properties
{
    /// <summary>
    /// Application settings provider for user preferences.
    /// Manages persistence of application state using .NET Configuration API.
    /// </summary>
    internal sealed partial class Settings : ApplicationSettingsBase
    {
        private static readonly Lazy<Settings> _instance = new Lazy<Settings>(() => new Settings());

        /// <summary>
        /// Gets the singleton instance of application settings.
        /// </summary>
        public static Settings Default
        {
            get => _instance.Value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        public Settings()
        {
            // Settings are automatically initialized from App.config
        }

        /// <summary>
        /// Gets or sets the last used source folder path.
        /// </summary>
        [UserScopedSetting]
        [DefaultSettingValue("")]
        public string LastSourceFolder
        {
            get => (string)this["LastSourceFolder"];
            set => this["LastSourceFolder"] = value;
        }

        /// <summary>
        /// Gets or sets the last used search criteria.
        /// </summary>
        [UserScopedSetting]
        [DefaultSettingValue("")]
        public string LastSearchCriteria
        {
            get => (string)this["LastSearchCriteria"];
            set => this["LastSearchCriteria"] = value;
        }

        /// <summary>
        /// Gets or sets the last used application theme.
        /// </summary>
        [UserScopedSetting]
        [DefaultSettingValue("System")]
        public string ThemePreference
        {
            get => (string)this["ThemePreference"];
            set => this["ThemePreference"] = value;
        }

        /// <summary>
        /// Saves all settings to user.config file.
        /// </summary>
        public override void Save()
        {
            try
            {
                base.Save();
            }
            catch (Exception ex)
            {
                // Silently fail if settings cannot be saved
                System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }
    }
}
