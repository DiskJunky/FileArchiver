# Auto-Save Feature - Implementation Summary

## Overview
The File Archiver now automatically saves any changes to the Source folder path and Search criteria as the user types. A debounce mechanism ensures efficient registry usage by waiting 500ms after the user stops typing before saving.

## What Changed

### 1. **Updated Imports**
Added `System.Threading` for Timer support:
```csharp
using System.Threading;
```

### 2. **New Private Fields**
```csharp
private Timer _folderPathSaveTimer;
private Timer _searchTextSaveTimer;
private const int DebounceDelayMs = 500;
```

### 3. **Enhanced FolderPath Property**
**Before**: Simple property with no auto-save
```csharp
public string FolderPath
{
    get => _folderPath;
    set => SetField(ref _folderPath, value);
}
```

**After**: Auto-saves with debounce
```csharp
public string FolderPath
{
    get => _folderPath;
    set
    {
        if (SetField(ref _folderPath, value))
        {
            _folderPathSaveTimer?.Dispose();
            _folderPathSaveTimer = new Timer(
                _ => SettingsManager.SaveLastSourceFolder(value),
                null,
                DebounceDelayMs,
                Timeout.Infinite);
        }
    }
}
```

### 4. **Enhanced SearchText Property**
**Before**: Simple property with no auto-save
```csharp
public string SearchText
{
    get => _searchText;
    set => SetField(ref _searchText, value);
}
```

**After**: Auto-saves with debounce
```csharp
public string SearchText
{
    get => _searchText;
    set
    {
        if (SetField(ref _searchText, value))
        {
            _searchTextSaveTimer?.Dispose();
            _searchTextSaveTimer = new Timer(
                _ => SettingsManager.SaveLastSearchCriteria(value),
                null,
                DebounceDelayMs,
                Timeout.Infinite);
        }
    }
}
```

### 5. **Improved OnWindowClosing Method**
**Before**: Only flushed Serilog
```csharp
public void OnWindowClosing()
{
    Log.Information("Application shutdown");
    Log.CloseAndFlush();
}
```

**After**: Cleans up timers and ensures final save
```csharp
public void OnWindowClosing()
{
    _folderPathSaveTimer?.Dispose();
    _searchTextSaveTimer?.Dispose();
    
    SettingsManager.SaveLastSourceFolder(FolderPath);
    SettingsManager.SaveLastSearchCriteria(SearchText);
    
    Log.Information("Application shutdown");
    Log.CloseAndFlush();
}
```

### 6. **Simplified Command Methods**
- `BrowseForFolder()`: Removed explicit save (now automatic)
- `ScanFilesCommand()`: Removed explicit save (now automatic)

## How It Works

### Debounce Timer Workflow
```
┌─────────────────────────────────────┐
│ User types in FolderPath TextBox    │
└──────────────┬──────────────────────┘
               │
               ▼
     ┌─────────────────────┐
     │ FolderPath setter   │
     │ is called           │
     └──────────┬──────────┘
                │
                ▼
     ┌──────────────────────────────┐
     │ Dispose old timer (if any)   │
     │ Create new Timer             │
     │ Set 500ms callback           │
     └──────────┬───────────────────┘
                │
         ┌──────┴──────┐
         │             │
    (wait 500ms) (User types again)
         │             │
         │             ▼
         │     ┌──────────────┐
         │     │ Timer reset  │
         │     └──────┬───────┘
         │            │
         │      (wait 500ms)
         │            │
         ▼            ▼
     ┌─────────────────────────┐
     │ Timer fires after 500ms │
     │ SettingsManager.Save()  │
     │ Registry updated ✓      │
     └─────────────────────────┘
```

## User Experience

### Typing Behavior
```
User: C [100ms]
      C:\ [200ms]
      C:\Users [300ms]
      C:\Users\Documents [400ms]
      [stops typing]
      [after 500ms] → Saved to registry ✓
```

### Multiple Changes
```
Browse to Folder1 (timer: 500ms)
Browse to Folder2 (timer reset: 500ms)
Browse to Folder3 (timer reset: 500ms)
[500ms passes] → Folder3 saved ✓
```

### Close During Edit
```
User types: "C:\Project"
User closes app
OnWindowClosing() called
Registry saved immediately ✓
No data loss
```

## Benefits

### ✅ Performance
- Reduces registry writes by debouncing
- No excessive I/O during typing
- Minimal CPU/memory overhead

### ✅ Reliability
- No explicit save needed
- Final save on app close
- Silent failure handling

### ✅ User Experience
- Seamless, automatic persistence
- No UI disruption
- Settings always current

### ✅ Code Quality
- Clean, maintainable implementation
- Proper resource disposal
- Follows MVVM pattern

## Technical Details

### Timer Parameters
```csharp
new Timer(
    callback,           // Method to call when timer fires
    state,             // State object (null)
    dueTime,           // Initial delay (500ms)
    period             // Recurring period (Timeout.Infinite = one-time)
)
```

### Debounce Pattern
1. **On property change**: Create/reset timer
2. **Timer callback**: Execute save
3. **On app close**: Force save and cleanup

### Thread Safety
- `Timer` is thread-safe
- Registry operations are atomic
- SettingsManager handles errors

## Testing Checklist

- [ ] Type folder path slowly → Verify single save after typing stops
- [ ] Type, delete, retype → Verify only last version saved
- [ ] Type and immediately close app → Verify data saved
- [ ] Type search criteria → Verify both folder and search saved
- [ ] Close app while typing → Verify immediate save in OnWindowClosing
- [ ] Verify registry keys created/updated
- [ ] Check performance with rapid edits
- [ ] Verify no errors in Activity Log

## Registry Verification

After testing, check registry:
```
Registry Path: HKEY_CURRENT_USER\Software\FileArchiver

Expected Keys:
- FileArchiver_ThemePreference
- FileArchiver_LastSourceFolder (updated with latest value)
- FileArchiver_LastSearchCriteria (updated with latest value)
```

## Files Modified
- `MainWindowViewModel.cs` - Added debounce timers and auto-save logic

## Files Not Modified
- `SettingsManager.cs` - Still handles all registry I/O (no changes needed)
- `MainWindow.xaml` - Bindings already in place
- `MainWindow.xaml.cs` - No changes needed
- Theme files - No changes needed

## Backward Compatibility
✅ Fully backward compatible
- Existing registry values preserved
- Default fallback still works
- No breaking changes

## Future Improvements
1. **Configurable Debounce Delay** - Allow user to adjust timing
2. **Save Notification** - Optional indicator when saving
3. **History Tracking** - Track last 10 folders used
4. **Selective Auto-Save** - Toggle auto-save per field
