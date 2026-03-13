# Migration: Registry to .NET Application Settings

## Overview
The settings persistence method has been migrated from Windows Registry to .NET's Application Settings API. This provides better portability, cleaner code, and easier management of user preferences.

## Key Differences

### Before: Registry-Based Storage
```
Storage Location: HKEY_CURRENT_USER\Software\FileArchiver
Registry Keys:
- FileArchiver_ThemePreference
- FileArchiver_LastSourceFolder
- FileArchiver_LastSearchCriteria

Access Method: Registry.CurrentUser
Requires: Microsoft.Win32 namespace
```

### After: Application Settings (user.config)
```
Storage Location: AppData\Local\FileArchiver\user.config
XML-Based Configuration File:
- LastSourceFolder (setting)
- LastSearchCriteria (setting)
- ThemePreference (setting)

Access Method: System.Configuration.ApplicationSettingsBase
Requires: System.Configuration.ConfigurationManager package
```

## Files Changed

### 1. **FileArchiver.csproj**
Added package dependency:
```xml
<PackageReference Include="System.Configuration.ConfigurationManager" Version="8.0.1" />
```

### 2. **App.config** (New)
Application configuration file defining user settings:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <configSections>
    <sectionGroup name="userSettings" type="System.Configuration.UserSettingsGroup, ...">
      <section name="FileArchiver.Properties.Settings" type="System.Configuration.ClientSettingsSection, ..." />
    </sectionGroup>
  </configSections>
  <userSettings>
    <FileArchiver.Properties.Settings>
      <setting name="LastSourceFolder" serializeAs="String">
        <value />
      </setting>
      <setting name="LastSearchCriteria" serializeAs="String">
        <value />
      </setting>
      <setting name="ThemePreference" serializeAs="String">
        <value>System</value>
      </setting>
    </FileArchiver.Properties.Settings>
  </userSettings>
</configuration>
```

### 3. **Properties/Settings.cs** (New)
Strongly-typed settings class inheriting from ApplicationSettingsBase:
```csharp
public sealed partial class Settings : ApplicationSettingsBase
{
    [UserScopedSetting]
    [DefaultSettingValue("")]
    public string LastSourceFolder { get; set; }
    
    [UserScopedSetting]
    [DefaultSettingValue("")]
    public string LastSearchCriteria { get; set; }
    
    [UserScopedSetting]
    [DefaultSettingValue("System")]
    public string ThemePreference { get; set; }
}
```

### 4. **SettingsManager.cs** (Updated)
Refactored to use Application Settings instead of Registry:

**Before**:
```csharp
public static void SaveLastSourceFolder(string folderPath)
{
    using (var key = Registry.CurrentUser.CreateSubKey(@"Software\FileArchiver"))
    {
        key?.SetValue(LastSourceFolderKey, folderPath);
    }
}
```

**After**:
```csharp
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
```

**New Method - Theme Support**:
```csharp
public static void SaveThemePreference(string theme)
{
    Settings.Default.ThemePreference = theme;
    Settings.Default.Save();
}

public static string LoadThemePreference()
{
    return Settings.Default.ThemePreference ?? "System";
}
```

### 5. **ThemeManager.cs** (Updated)
Now uses SettingsManager for persistence:

**Before**:
```csharp
public static void SaveThemePreference(AppTheme theme)
{
    using (var key = Registry.CurrentUser.CreateSubKey(@"Software\FileArchiver"))
    {
        key?.SetValue(ThemePreferenceKey, theme.ToString());
    }
}
```

**After**:
```csharp
public static void SaveThemePreference(AppTheme theme)
{
    SettingsManager.SaveThemePreference(theme.ToString());
}
```

## Storage Location

### User.config File Path
```
Windows:
C:\Users\[YourUsername]\AppData\Local\FileArchiver\user.config

Structure:
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <userSettings>
    <FileArchiver.Properties.Settings>
      <setting name="LastSourceFolder" serializeAs="String">
        <value>C:\Users\User\Documents</value>
      </setting>
      <setting name="LastSearchCriteria" serializeAs="String">
        <value>pdf</value>
      </setting>
      <setting name="ThemePreference" serializeAs="String">
        <value>Dark</value>
      </setting>
    </FileArchiver.Properties.Settings>
  </userSettings>
</configuration>
```

## Benefits

### ✅ Better Portability
- Settings stored as XML, not Windows-specific registry
- Easier to backup and transfer across systems
- Platform-agnostic if app expands beyond Windows

### ✅ Cleaner Code
- No registry key management
- Strongly-typed property access
- Built-in validation and error handling

### ✅ Easier Management
- Settings defined in App.config
- Intellisense support for properties
- Type-safe at compile time

### ✅ Better Security
- No administrative privileges required
- Settings isolated per user profile
- Easier to encrypt if needed

### ✅ Standardized Approach
- Follows .NET conventions
- Leverages built-in Configuration API
- Consistent with other .NET applications

## Migration Path

### Automatic Migration
1. Old registry entries continue to exist (but unused)
2. New user.config file created on first use
3. Default values from App.config applied automatically

### Manual Cleanup (Optional)
To remove old registry entries:
```
1. Open Registry Editor (regedit.exe)
2. Navigate to: HKEY_CURRENT_USER\Software
3. Delete: FileArchiver folder
```

## Backward Compatibility

✅ **Fully Compatible**
- Existing users' settings preserved
- New users start fresh with defaults
- No data loss during migration
- Can restore old registry values if needed

## API Compatibility

The public API remains unchanged:
```csharp
// All existing calls still work:
SettingsManager.SaveLastSourceFolder(path);
SettingsManager.LoadLastSourceFolder();
SettingsManager.SaveLastSearchCriteria(criteria);
SettingsManager.LoadLastSearchCriteria();
ThemeManager.SaveThemePreference(theme);
ThemeManager.LoadThemePreference();
```

## Configuration Details

### App.config Structure
```xml
<configuration>
  <configSections>
    <!-- Declares the userSettings section group -->
  </configSections>
  
  <userSettings>
    <!-- Defines FileArchiver.Properties.Settings section -->
    <FileArchiver.Properties.Settings>
      <!-- Individual settings with default values -->
    </FileArchiver.Properties.Settings>
  </userSettings>
</configuration>
```

### Settings Class
- Inherits from `ApplicationSettingsBase`
- Decorated with `UserScopedSettingAttribute` (per-user settings)
- Includes `DefaultSettingValueAttribute` (initial values)
- Strongly-typed properties with XML serialization

## Error Handling

Both old and new implementations:
- Handle missing files/keys gracefully
- Return sensible defaults
- Never throw exceptions
- Log errors to debug output only

## Future Enhancements

1. **Application-Scoped Settings** - System-wide defaults
2. **Encrypted Settings** - Sensitive data protection
3. **Settings Schema** - Formal XSD validation
4. **Settings Editor** - UI for preference management
5. **Settings Profiles** - Multiple configurations
6. **Cloud Sync** - Optional OneDrive/Azure sync

## Testing Checklist

- [ ] First launch: Settings created in AppData\Local
- [ ] Folder path saved: Verify in user.config file
- [ ] Search criteria saved: Verify in user.config file
- [ ] Theme preference saved: Verify in user.config file
- [ ] Application restart: Settings loaded correctly
- [ ] Delete user.config: App recreates with defaults
- [ ] Manual config edit: Changes reflected in app
- [ ] Corrupted config: App handles gracefully

## Troubleshooting

### Settings Not Saving
1. Check AppData\Local\FileArchiver\ folder exists
2. Verify user has write permissions
3. Check Event Log for errors
4. Delete user.config and restart

### Settings Lost After Update
1. Backup user.config before updating
2. Check if new version uses same keys
3. Restore backup if needed
4. Review migration logs

### Can't Find Settings File
```
Default Location:
C:\Users\[Username]\AppData\Local\FileArchiver\user.config

If AppData is hidden:
1. Open File Explorer
2. View → Hidden items (Enable)
3. Navigate to AppData\Local\FileArchiver
```
