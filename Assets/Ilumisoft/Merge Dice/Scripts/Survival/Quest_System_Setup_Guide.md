# Quest System Setup Guide

## 🚀 **Quick Setup Steps**

### **Step 1: Create DiceLevelManager**
1. In Unity, go to `Assets > Create > Merge Dice > Dice Level Manager`
2. Name it `DiceLevelManager` and place it in `Assets/Resources/` folder
3. Configure your dice levels:
   - Click "Create Default Levels" for a 6-level setup
   - Or manually add levels using "Add New Level" button
   - Set colors and overlay sprites for each level

### **Step 2: Update Existing Dice Prefabs**
Your existing dice prefabs will continue working, but you can now:
1. Remove individual `diceLevels` arrays from `DiceLevelBehaviour` components (they're no longer needed)
2. The system will automatically use `DiceLevelManager` data

### **Step 3: Enhance Your QuestData Assets**
1. Open existing `QuestData` ScriptableObjects
2. Enjoy the new Odin Inspector interface:
   - Horizontal layouts for cleaner appearance
   - Level information display
   - Quick setup buttons for common quests

### **Step 4: Runtime Monitoring**
1. During play mode, select the `QuestSystem` GameObject
2. Watch live progress in the Inspector
3. Use debug buttons to test quest flow

## 🔧 **Advanced Configuration**

### **DiceLevelManager Settings**
```csharp
// Access dice properties anywhere in code:
Color diceColor = DiceLevelManager.Instance.GetLevelColor(2);
Sprite diceSprite = DiceLevelManager.Instance.GetLevelOverlay(2);
int maxLevel = DiceLevelManager.Instance.MaxLevel;
```

### **Quest Progress Access**
```csharp
// In your custom scripts, access runtime progress:
QuestSystem questSystem = FindFirstObjectByType<QuestSystem>();
if (questSystem.CurrentQuestProgress != null)
{
    float overallProgress = questSystem.CurrentQuestProgress.OverallProgress;
    bool isComplete = questSystem.CurrentQuestProgress.IsComplete;
}
```

## 🎯 **Key Benefits You'll See**

### **Immediate Improvements**:
- ✅ Cleaner Quest ScriptableObjects (no runtime data pollution)
- ✅ Beautiful Odin Inspector interfaces
- ✅ Centralized dice level management
- ✅ Live progress monitoring during play mode

### **Development Benefits**:
- ✅ Easier quest creation with templates
- ✅ Visual dice level previews (coming when DiceLevelManager is integrated)
- ✅ Debug tools built into inspector
- ✅ Consistent dice properties across all prefabs

## ⚠️ **Important Notes**

1. **DiceLevelManager Location**: Must be in `Resources` folder for global access
2. **Backward Compatibility**: All existing quests continue working
3. **Progress Tracking**: Now handled automatically - don't manually modify ScriptableObjects
4. **Runtime Data**: Quest progress is temporary and resets properly between sessions

## 🔄 **Future Enhancements Ready**

The new architecture supports easy additions:
- Visual dice previews in quest requirements
- Color-coded quest difficulty indicators  
- Enhanced analytics and progress tracking
- Custom quest templates and generators

## 🐛 **Troubleshooting**

**"DiceLevelManager not found" error**:
- Ensure `DiceLevelManager.asset` is in `Assets/Resources/` folder
- Check that the asset name is exactly "DiceLevelManager"

**Quest progress not updating**:
- Verify `QuestSystem.ProgressQuest()` is being called when tiles are destroyed
- Check that quest requirements match actual dice levels

**Inspector not showing progress**:
- Ensure you're in play mode
- Select the `QuestSystem` GameObject in hierarchy
- Make sure a quest is active (CurrentQuestProgress is not null)

Your quest system is now refactored for better maintainability, visual design, and developer experience! 🎉
