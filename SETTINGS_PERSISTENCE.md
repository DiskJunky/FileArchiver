# Settings Persistence Feature

## Overview
The File Archiver now remembers your last used Source folder and search criteria across application sessions. This enhances user experience by reducing repetitive data entry and restoring your previous workflow state.

## Implementation Details

### New File: SettingsManager.cs
A new static utility class that handles all user preference persistence:

```csharp
public static class SettingsManager
```

**Storage Location**: `HKEY_CURRENT_USER\Software\FileArchiver`

**Registry Keys**:
- `FileArchiver_LastSourceFolder` - Stores the last used folder path
- `FileArchiver_LastSearchCriteria` - Stores the last used search text

### Key Methods

#### SaveLastSourceFolder(string folderPath)
- **Purpose**: Persists the folder path when user navigates to a new folder
- **When Called**: 
  - When user selects a folder via Browse button
  - When user initiates a file scan
- **Behavior**: Silently fails if registry access is denied (non-intrusive)

#### LoadLastSourceFolder()
- **Purpose**: Retrieves the last used folder path on application start
- **Returns**: 
  - Last saved folder path if it still exists
  - User's Downloads folder as default fallback
- **Validation**: Verifies the folder still exists before returning

#### SaveLastSearchCriteria(string searchCriteria)
- **Purpose**: Persists the search text for next session
- **When Called**: When user initiates a file scan
- **Behavior**: Only saves non-empty criteria

#### LoadLastSearchCriteria()
- **Purpose**: Retrieves the last used search criteria on application start
- **Returns**: 
  - Last saved search text if available
  - Empty string as default fallback

## Usage Flow

### First Launch
```
Application Starts
  ↓
LoadLastSourceFolder() → Returns Downloads folder (no registry entry)
LoadLastSearchCriteria() → Returns empty string (no registry entry)
User sees empty search box and Downloads folder path
```

### Normal Usage
```
User Browses to C:\Projects
SaveLastSourceFolder() → Registry updated: FileArchiver_LastSourceFolder = "C:\Projects"

User Enters "pdf"
Initiates Scan
SaveLastSearchCriteria() → Registry updated: FileArchiver_LastSearchCriteria = "pdf"
```

### Next Session
```
Application Starts
  ↓
LoadLastSourceFolder() → Returns "C:\Projects"
LoadLastSearchCriteria() → Returns "pdf"
User sees their previous workflow restored
```

## Changes Made

### MainWindowViewModel.cs

**Constructor Changes**:
```csharp
// Before
_folderPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    "Downloads");

// After
_folderPath = SettingsManager.LoadLastSourceFolder();
_searchText = SettingsManager.LoadLastSearchCriteria();
```

**BrowseForFolder() Method**:
```csharp
// Now saves when user selects a new folder
if (dialog.ShowDialog() == true)
{
    FolderPath = dialog.FolderName;
    SettingsManager.SaveLastSourceFolder(dialog.FolderName);
    LogActivity($"Folder changed to: {dialog.FolderName}");
}
```

**ScanFilesCommand() Method**:
```csharp
// Now saves before scan starts
SettingsManager.SaveLastSourceFolder(FolderPath);
SettingsManager.SaveLastSearchCriteria(SearchText);
```

## Benefits

### User Experience
✅ **Faster Workflows** - No need to re-select folders every session  
✅ **Context Preservation** - Users return to their last work location  
✅ **Convenience** - Search criteria automatically restored  
✅ **Seamless Integration** - Works silently in background  

### Reliability
✅ **Graceful Fallback** - Invalid paths automatically revert to Downloads  
✅ **Silent Failures** - Registry errors don't crash the application  
✅ **Path Validation** - Verifies folders still exist before using  

### Design
✅ **Consistent Pattern** - Follows same approach as ThemeManager  
✅ **Separation of Concerns** - Settings logic isolated in SettingsManager  
✅ **Future-Proof** - Easy to add more settings to registry  

## Registry Structure

```
HKEY_CURRENT_USER
└── Software
    └── FileArchiver
        ├── FileArchiver_ThemePreference (existing)
        ├── FileArchiver_LastSourceFolder (new)
        └── FileArchiver_LastSearchCriteria (new)
```

## Future Enhancements

1. **Remember Window State** - Restore window size and position
2. **Recent Folders List** - Maintain history of last 5-10 folders
3. **Export Settings** - Allow users to backup/restore preferences
4. **Settings UI** - Add preferences dialog to configure behavior
5. **Cloud Sync** - Optional sync across devices
6. **Settings Profile** - Different profiles for different workflows

## Testing Scenarios

### Test 1: First Launch
1. Delete registry key `FileArchiver_LastSourceFolder`
2. Launch application
3. ✓ Should default to Downloads folder
4. ✓ Search box should be empty

### Test 2: Remember Folder
1. Click Browse, select `C:\Users\Public\Documents`
2. Close application
3. Relaunch application
4. ✓ Folder path should be `C:\Users\Public\Documents`

### Test 3: Remember Search Criteria
1. Enter search text "report"
2. Click Scan
3. Close application
4. Relaunch application
5. ✓ Search box should contain "report"

### Test 4: Deleted Folder Fallback
1. Set folder to `C:\NonExistentFolder`
2. Manually delete the folder from Windows
3. Relaunch application
4. ✓ Should fall back to Downloads folder
5. ✓ No error should occur

## Code Quality

- ✅ Full XML documentation
- ✅ Error handling with silent failures
- ✅ Null/empty string validation
- ✅ Follows existing code patterns
- ✅ No external dependencies
- ✅ Thread-safe registry operations
