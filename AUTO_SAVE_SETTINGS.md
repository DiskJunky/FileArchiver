# Auto-Save Settings on User Input

## Overview
The File Archiver now automatically saves the Source folder path and Search criteria to the registry as the user types or modifies them. This provides a seamless experience where all changes are persisted without requiring manual action.

## Implementation Details

### Debounce Mechanism
To avoid excessive registry writes while the user is actively typing, a debounce mechanism is implemented:

- **Debounce Delay**: 500 milliseconds
- **Behavior**: Waits 500ms after the user stops typing before saving to registry
- **Restart on Change**: If the user types again, the timer restarts

### How It Works

```
User Types 'C:\' 
  ↓
FolderPath property setter triggered
  ↓
Debounce timer starts (500ms)
  ↓ (User types '\Documents')
Timer cancelled and restarted
  ↓
User stops typing (waits 500ms)
  ↓
Timer expires → SaveLastSourceFolder() called
  ↓
Registry updated
```

### Changes to MainWindowViewModel

#### 1. **New Fields**
```csharp
private Timer _folderPathSaveTimer;
private Timer _searchTextSaveTimer;
private const int DebounceDelayMs = 500; // Wait 500ms after user stops typing
```

#### 2. **Updated FolderPath Property**
```csharp
public string FolderPath
{
    get => _folderPath;
    set
    {
        if (SetField(ref _folderPath, value))
        {
            // Reset and restart the debounce timer
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

#### 3. **Updated SearchText Property**
```csharp
public string SearchText
{
    get => _searchText;
    set
    {
        if (SetField(ref _searchText, value))
        {
            // Reset and restart the debounce timer
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

#### 4. **Updated OnWindowClosing Method**
```csharp
public void OnWindowClosing()
{
    // Dispose of debounce timers
    _folderPathSaveTimer?.Dispose();
    _searchTextSaveTimer?.Dispose();

    // Ensure final settings are saved
    SettingsManager.SaveLastSourceFolder(FolderPath);
    SettingsManager.SaveLastSearchCriteria(SearchText);

    Log.Information("Application shutdown");
    Log.CloseAndFlush();
}
```

## Key Features

### ✅ Real-Time Persistence
- Settings are saved automatically without user action
- No explicit "Save" button needed
- Changes persisted immediately after user stops typing

### ✅ Smart Debouncing
- Prevents excessive registry writes during typing
- 500ms delay allows natural typing rhythm
- Timer restarts with each keystroke

### ✅ Graceful Cleanup
- Timers properly disposed on application close
- Final settings saved before exit
- No resource leaks

### ✅ Transparent to User
- No UI indicators needed
- Works silently in background
- Automatic recovery on next launch

## Behavior Examples

### Example 1: User Types Folder Path
```
Time    Event                           Registry Updated
0ms     User types 'C'                  [timer started]
100ms   User types 'C:\'                [timer reset]
200ms   User types 'C:\U'               [timer reset]
300ms   User types 'C:\Users'           [timer reset]
400ms   User types 'C:\Users\...'       [timer reset]
800ms   [500ms timeout expired]         ✓ Registry saved
```

### Example 2: User Changes Folder Multiple Times
```
0ms     User browses to C:\Folder1      [save timer: 500ms]
100ms   User browses to C:\Folder2      [timer cancelled, new: 500ms]
200ms   User browses to C:\Folder3      [timer cancelled, new: 500ms]
600ms   [500ms timeout expired]         ✓ Registry saved (Folder3)
```

### Example 3: User Closes App While Typing
```
0ms     User types 'D:\Test'            [timer started]
50ms    User closes application         [timer disposed]
        OnWindowClosing() called        ✓ Immediate save to registry
```

## Performance Impact

### Registry Writes Reduced
- **Before**: Multiple writes per action (browse + scan)
- **After**: Single write after 500ms delay

### Example: Scanning a Folder
```
Before (Multiple Writes):
1. Browse folder → Save to registry
2. Scan initiated → Save to registry
Total: 2 registry writes

After (Single Write Per Action):
1. Browse folder → Timer started (500ms delay)
2. Timer expires → 1 registry write
Total: 1 registry write per action
```

### Resource Usage
- ✅ Minimal CPU impact (timers only during changes)
- ✅ Negligible memory (two Timer objects)
- ✅ No disk I/O until timer expires

## User Experience Flow

### Scenario: User Works with Multiple Projects

**Session 1**:
```
User opens app
↓
Loads previous folder: C:\Project1
↓
User browses to C:\Project2
↓
After 500ms delay → C:\Project2 saved
↓
User types search criteria "report"
↓
After 500ms delay → "report" saved
↓
User closes app
```

**Session 2**:
```
User opens app
↓
Automatically loads:
- Folder: C:\Project2
- Search: report
↓
User immediately ready to work
```

## Edge Cases Handled

### 1. **Rapid Folder Changes**
- Each change restarts the timer
- Only final folder is saved (after 500ms idle)
- No duplicate or partial saves

### 2. **Application Closed During Typing**
- OnWindowClosing() is called
- Timers are disposed
- Final values saved immediately

### 3. **Empty Values**
- SettingsManager validates before saving
- Empty strings not persisted
- Previous values retained

### 4. **Registry Access Denied**
- Timers continue normally
- Registry save fails silently
- User session continues unaffected

## Testing Scenarios

### Test 1: Type Folder Path
1. Clear folder path
2. Type slowly: `C:\Users\...\Downloads` (character by character)
3. Close app immediately
4. ✓ Reopen app → Latest path appears

### Test 2: Multiple Rapid Changes
1. Paste a folder path
2. Immediately delete and paste different path
3. Repeat 5-10 times
4. Wait 500ms
5. ✓ Only last value saved to registry

### Test 3: Type and Scan
1. Type folder path
2. Immediately scan (before 500ms)
3. Close app
4. ✓ Reopen → Both values present

### Test 4: Registry Validation
1. Delete registry keys manually
2. Type in folder/search
3. Wait for save
4. ✓ Registry keys recreated with new values

## Future Enhancements

1. **Adjustable Debounce Delay** - Settings dialog to configure delay
2. **Save History** - Keep history of last 10 folders
3. **Real-Time Validation** - Show folder validity indicator
4. **Auto-Complete** - Suggest recent folders while typing
5. **Performance Monitoring** - Log save timings for optimization
