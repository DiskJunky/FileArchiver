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
        /// Gets or sets the saved window width.
        /// </summary>
        [UserScopedSetting]
        [DefaultSettingValue("1000")]
        public string WindowWidth
        {
            get => (string)this["WindowWidth"];
            set => this["WindowWidth"] = value;
        }

        /// <summary>
        /// Gets or sets the saved window height.
        /// </summary>
        [UserScopedSetting]
        [DefaultSettingValue("850")]
        public string WindowHeight
        {
            get => (string)this["WindowHeight"];
            set => this["WindowHeight"] = value;
        }

        /// <summary>
        /// Gets or sets the saved window left position.
        /// </summary>
        [UserScopedSetting]
        [DefaultSettingValue("0")]
        public string WindowLeft
        {
            get => (string)this["WindowLeft"];
            set => this["WindowLeft"] = value;
        }

        /// <summary>
        /// Gets or sets the saved window top position.
        /// </summary>
        [UserScopedSetting]
        [DefaultSettingValue("0")]
        public string WindowTop
        {
            get => (string)this["WindowTop"];
            set => this["WindowTop"] = value;
        }

        /// <summary>
        /// Gets or sets the saved height of the main grid's first row (upper content area).
        /// </summary>
        [UserScopedSetting]
        [DefaultSettingValue("400")]
        public string MainGridRow0Height
        {
            get => (string)this["MainGridRow0Height"];
            set => this["MainGridRow0Height"] = value;
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
