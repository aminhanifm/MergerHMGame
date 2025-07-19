using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Ilumisoft.MergeDice;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// When environmental effects should be applied during quest states
/// </summary>
public enum EnvironmentalEffectTiming
{
    IntroOnly,      // Apply effects only during quest intro
    ProgressionOnly, // Apply effects only during progression display
    Both,           // Apply effects during both intro and progression
    GameplayOnly    // Apply effects only during actual gameplay
}

[System.Serializable]
public class TileRequirement
{
    [BoxGroup("Tile Requirement")]
    [HorizontalGroup("Tile Requirement/Row")]
    [LabelWidth(80)]
    [Range(0, 10)]
    [Tooltip("The level of tile that needs to be destroyed")]
    public int tileLevel;
    
    [HorizontalGroup("Tile Requirement/Row")]
    [LabelWidth(80)]
    [MinValue(1)]
    [Tooltip("How many tiles of this level need to be destroyed")]
    public int targetAmount;
    
    [BoxGroup("Tile Requirement")]
    [ReadOnly]
    [ShowInInspector]
    [PreviewField(55, ObjectFieldAlignment.Center)]
    [PropertyTooltip("Visual preview of this dice level")]
    [ShowIf("@GetLevelSprite() != null")]
    public Sprite LevelPreview => GetLevelSprite();
    
    [BoxGroup("Tile Requirement")]
    [ReadOnly]
    [ShowInInspector]
    [GUIColor("@GetLevelColor()")]
    [PropertyTooltip("Level info with dice visual properties")]
    public string LevelInfo => $"Level {tileLevel} - {targetAmount} required";
    
    // Helper methods for Odin Inspector visual enhancements
    private Sprite GetLevelSprite()
    {
        if (DiceLevelManager.Instance != null && DiceLevelManager.Instance.IsValidLevel(tileLevel))
        {
            return DiceLevelManager.Instance.GetLevelOverlay(tileLevel);
        }
        return null;
    }
    
    private Color GetLevelColor()
    {
        if (DiceLevelManager.Instance != null && DiceLevelManager.Instance.IsValidLevel(tileLevel))
        {
            return DiceLevelManager.Instance.GetLevelColor(tileLevel);
        }
        return Color.white;
    }
}

[CreateAssetMenu(fileName = "QuestData", menuName = "Survival/QuestData", order = 1)]
public class QuestData : ScriptableObject
{
    [TitleGroup("Quest Information")]
    [LabelWidth(80)]
    [Tooltip("The display name of the quest")]
    public string title;
    
    [TitleGroup("Quest Information")]
    [LabelWidth(80)]
    [TextArea(2, 4)]
    [Tooltip("Detailed description of what the player needs to do")]
    public string description;
    
    [TitleGroup("Requirements")]
    [TableList(ShowIndexLabels = true, DrawScrollView = true, MaxScrollViewHeight = 300)]
    [Tooltip("List of tile requirements that must be completed")]
    public TileRequirement[] requirements;
    
    [FoldoutGroup("Quest Settings")]
    [LabelWidth(100)]
    [Tooltip("Time limit for this quest in seconds (0 = no limit)")]
    [MinValue(0)]
    public float timeLimit = 0f;
    
    [FoldoutGroup("Quest Settings")]
    [LabelWidth(100)]
    [Tooltip("Priority of this quest (higher numbers appear first)")]
    public int priority = 0;
    
    [FoldoutGroup("Quest Settings")]
    [LabelWidth(100)]
    [Tooltip("Can this quest be skipped by the player?")]
    public bool canBeSkipped = false;
    
    [FoldoutGroup("Rewards")]
    [LabelWidth(100)]
    [Tooltip("Score bonus awarded for completing this quest")]
    [MinValue(0)]
    public int scoreReward = 100;
    
    [FoldoutGroup("Rewards")]
    [LabelWidth(100)]
    [Tooltip("Time bonus awarded for completing this quest (in seconds)")]
    [MinValue(0)]
    public float timeBonus = 0f;
    
    [FoldoutGroup("Visual Settings")]
    [InfoBox("GameObject references can become 'None' if objects are destroyed, renamed, or moved. Use the 'Clean Null References' button to fix this, or ensure objects are always present in the scene.", InfoMessageType.Warning)]
    [InfoBox("IMPORTANT: Only reference GameObjects that are:\n• In the same scene as your quest system\n• Prefabs in the project (not scene instances)\n• Persistent objects that won't be destroyed\n\nAfter adding references, use 'Force Save Asset' to ensure they persist.", InfoMessageType.Warning)]
    [InfoBox("NEW: Scene Object Names - Enter the exact names of scene GameObjects. Objects will be found by name at runtime.", InfoMessageType.Info)]
    [LabelWidth(120)]
    [Tooltip("Names of objects to show during this quest's intro (objects will be found by name in the scene)")]
    [ListDrawerSettings(ShowFoldout = true, DraggableItems = true, ShowIndexLabels = true)]
    public string[] eventObjectNames;
    
    [FoldoutGroup("Visual Settings")]
    [LabelWidth(120)]
    [Tooltip("Names of objects to animate/flicker when this quest is completed")]
    [ListDrawerSettings(ShowFoldout = true, DraggableItems = true, ShowIndexLabels = true)]
    public string[] unlockAnimationObjectNames;
    
    [FoldoutGroup("Visual Settings")]
    [LabelWidth(120)]
    [Tooltip("Names of objects to show during intro and progression states")]
    [ListDrawerSettings(ShowFoldout = true, DraggableItems = true, ShowIndexLabels = true)]
    public string[] showObjectNames;
    
    [FoldoutGroup("Visual Settings")]
    [LabelWidth(120)]
    [Tooltip("When should the show objects be displayed?")]
    public EnvironmentalEffectTiming showObjectsTiming = EnvironmentalEffectTiming.Both;
    
    [FoldoutGroup("Visual Settings")]
    [LabelWidth(120)]
    [Tooltip("Names of objects to hide during intro and progression states")]
    [ListDrawerSettings(ShowFoldout = true, DraggableItems = true, ShowIndexLabels = true)]
    public string[] hideObjectNames;
    
    [FoldoutGroup("Visual Settings")]
    [LabelWidth(120)]
    [Tooltip("When should the hide objects be hidden?")]
    public EnvironmentalEffectTiming hideObjectsTiming = EnvironmentalEffectTiming.Both;
    
    [FoldoutGroup("Visual Settings")]
    [Header("Legacy GameObject References (Deprecated)")]
    [InfoBox("These GameObject arrays are deprecated. Use the string name arrays above instead.", InfoMessageType.Warning)]
    [HideInInspector]
    [Tooltip("Objects to show during this quest's intro (these will be the only objects visible during this quest's intro)")]
    [SerializeField]
    [ListDrawerSettings(ShowFoldout = true, DraggableItems = true, ShowIndexLabels = true)]
    public GameObject[] eventObjects;
    
    [HideInInspector]
    [LabelWidth(120)]
    [Tooltip("Objects to animate/flicker when this quest is completed (replaces day-based unlock animation)")]
    [SerializeField]
    [ListDrawerSettings(ShowFoldout = true, DraggableItems = true, ShowIndexLabels = true)]
    public GameObject[] unlockAnimationObjects;
    
    [HideInInspector]
    [LabelWidth(120)]
    [Tooltip("Objects to show during intro and progression states (e.g., night time effects, UI decorations)")]
    [SerializeField]
    [ListDrawerSettings(ShowFoldout = true, DraggableItems = true, ShowIndexLabels = true)]
    public GameObject[] showObjects;
    
    [HideInInspector]
    [LabelWidth(120)]
    [Tooltip("Objects to hide during intro and progression states (will be hidden when this quest is active)")]
    [SerializeField]
    [ListDrawerSettings(ShowFoldout = true, DraggableItems = true, ShowIndexLabels = true)]
    public GameObject[] hideObjects;
    
    [FoldoutGroup("Environmental Effects")]
    [Header("Environmental Effects")]
    [LabelWidth(120)]
    [Tooltip("Enable snowy texture overlay on UI images when this quest starts")]
    public bool enableSnowyTextures = false;
    
    [FoldoutGroup("Environmental Effects")]
    [LabelWidth(120)]
    [Tooltip("When should environmental effects be applied during the quest?")]
    [ShowIf("enableSnowyTextures")]
    public EnvironmentalEffectTiming effectTiming = EnvironmentalEffectTiming.Both;
    
    [TitleGroup("Quest Status", "Configuration Only - Progress tracked at runtime")]
    [ReadOnly]
    [ShowInInspector]
    [PropertyTooltip("Total tiles that need to be destroyed")]
    public int TotalTilesRequired
    {
        get
        {
            int total = 0;
            if (requirements != null)
            {
                foreach (var req in requirements)
                {
                    total += req.targetAmount;
                }
            }
            return total;
        }
    }
    
    [TitleGroup("Quest Status")]
    [ReadOnly]
    [ShowInInspector]
    [PropertyTooltip("Number of different tile levels required")]
    public int RequirementCount => requirements != null ? requirements.Length : 0;
    
    [TitleGroup("Quest Status")]
    [ReadOnly]
    [ShowInInspector]
    [PropertyTooltip("Does this quest have event objects to show during intro?")]
    public bool HasEventObjects => (eventObjectNames != null && eventObjectNames.Length > 0) || (eventObjects != null && eventObjects.Length > 0);
    
    [TitleGroup("Quest Status")]
    [ReadOnly]
    [ShowInInspector]
    [PropertyTooltip("Does this quest have objects to animate when unlocked?")]
    public bool HasUnlockAnimationObjects => (unlockAnimationObjectNames != null && unlockAnimationObjectNames.Length > 0) || (unlockAnimationObjects != null && unlockAnimationObjects.Length > 0);
    
    [TitleGroup("Quest Status")]
    [ReadOnly]
    [ShowInInspector]
    [PropertyTooltip("Does this quest have objects to show during intro/progression?")]
    public bool HasShowObjects => (showObjectNames != null && showObjectNames.Length > 0) || (showObjects != null && showObjects.Length > 0);
    
    [TitleGroup("Quest Status")]
    [ReadOnly]
    [ShowInInspector]
    [PropertyTooltip("Does this quest have objects to hide during intro/progression?")]
    public bool HasHideObjects => (hideObjectNames != null && hideObjectNames.Length > 0) || (hideObjects != null && hideObjects.Length > 0);
    
    
    [Button(ButtonSizes.Large)]
    [GUIColor(0.8f, 1f, 0.8f)]
    [PropertyTooltip("Add a new tile requirement to this quest")]
    private void AddRequirement()
    {
        if (requirements == null)
        {
            requirements = new TileRequirement[1];
        }
        else
        {
            System.Array.Resize(ref requirements, requirements.Length + 1);
        }
        requirements[requirements.Length - 1] = new TileRequirement();
    }
    
    [InfoBox("This quest has no requirements! Add at least one requirement.", InfoMessageType.Warning)]
    [ShowIf("@requirements == null || requirements.Length == 0")]
    [Button("Quick Setup - Easy Quest")]
    private void SetupEasyQuest()
    {
        title = "Destroy Basic Tiles";
        description = "Destroy tiles to clear the board.";
        requirements = new TileRequirement[]
        {
            new TileRequirement { tileLevel = 0, targetAmount = 5 },
            new TileRequirement { tileLevel = 1, targetAmount = 3 }
        };
        scoreReward = 150;
        timeLimit = 60f;
    }
    
    [Button("Quick Setup - Medium Quest")]
    [ShowIf("@requirements == null || requirements.Length == 0")]
    private void SetupMediumQuest()
    {
        title = "Mixed Tile Challenge";
        description = "Destroy a variety of different tile levels.";
        requirements = new TileRequirement[]
        {
            new TileRequirement { tileLevel = 1, targetAmount = 8 },
            new TileRequirement { tileLevel = 2, targetAmount = 5 },
            new TileRequirement { tileLevel = 3, targetAmount = 2 }
        };
        scoreReward = 300;
        timeLimit = 90f;
    }
    
    [Button("Quick Setup - Hard Quest")]
    [ShowIf("@requirements == null || requirements.Length == 0")]
    private void SetupHardQuest()
    {
        title = "High Level Challenge";
        description = "Focus on destroying higher level tiles.";
        requirements = new TileRequirement[]
        {
            new TileRequirement { tileLevel = 3, targetAmount = 10 },
            new TileRequirement { tileLevel = 4, targetAmount = 5 },
            new TileRequirement { tileLevel = 5, targetAmount = 2 }
        };
        scoreReward = 500;
        timeLimit = 120f;
        priority = 1;
    }
    
    [FoldoutGroup("Visual Settings")]
    [Button("Convert GameObjects to Names")]
    [GUIColor(0.7f, 1f, 0.7f)]
    [PropertyTooltip("Convert existing GameObject references to string names (recommended for scene objects)")]
    private void ConvertGameObjectsToNames()
    {
        bool converted = false;
        
        // Convert event objects
        if (eventObjects != null && eventObjects.Length > 0)
        {
            var names = new System.Collections.Generic.List<string>();
            if (eventObjectNames != null) names.AddRange(eventObjectNames);
            
            foreach (var obj in eventObjects)
            {
                if (obj != null && !names.Contains(obj.name))
                {
                    names.Add(obj.name);
                    converted = true;
                }
            }
            eventObjectNames = names.ToArray();
        }
        
        // Convert unlock animation objects
        if (unlockAnimationObjects != null && unlockAnimationObjects.Length > 0)
        {
            var names = new System.Collections.Generic.List<string>();
            if (unlockAnimationObjectNames != null) names.AddRange(unlockAnimationObjectNames);
            
            foreach (var obj in unlockAnimationObjects)
            {
                if (obj != null && !names.Contains(obj.name))
                {
                    names.Add(obj.name);
                    converted = true;
                }
            }
            unlockAnimationObjectNames = names.ToArray();
        }
        
        // Convert show objects
        if (showObjects != null && showObjects.Length > 0)
        {
            var names = new System.Collections.Generic.List<string>();
            if (showObjectNames != null) names.AddRange(showObjectNames);
            
            foreach (var obj in showObjects)
            {
                if (obj != null && !names.Contains(obj.name))
                {
                    names.Add(obj.name);
                    converted = true;
                }
            }
            showObjectNames = names.ToArray();
        }
        
        // Convert hide objects
        if (hideObjects != null && hideObjects.Length > 0)
        {
            var names = new System.Collections.Generic.List<string>();
            if (hideObjectNames != null) names.AddRange(hideObjectNames);
            
            foreach (var obj in hideObjects)
            {
                if (obj != null && !names.Contains(obj.name))
                {
                    names.Add(obj.name);
                    converted = true;
                }
            }
            hideObjectNames = names.ToArray();
        }
        
        #if UNITY_EDITOR
        if (converted)
        {
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"Quest '{title}': Converted GameObject references to string names");
        }
        else
        {
            Debug.Log($"Quest '{title}': No GameObjects to convert");
        }
        #endif
    }
    
    [FoldoutGroup("Visual Settings")]
    [Button("Validate Object Names")]
    [GUIColor(0.8f, 0.8f, 1f)]
    [PropertyTooltip("Check if all named objects exist in the current scene")]
    private void ValidateObjectNames()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Object name validation only works in Play Mode when scenes are loaded");
            return;
        }
        
        int missingCount = 0;
        
        // Check event object names
        if (eventObjectNames != null)
        {
            foreach (var objName in eventObjectNames)
            {
                if (!string.IsNullOrEmpty(objName))
                {
                    var obj = FindGameObjectByName(objName);
                    if (obj == null)
                    {
                        Debug.LogWarning($"Quest '{title}': Event object '{objName}' not found in scene");
                        missingCount++;
                    }
                }
            }
        }
        
        // Check unlock animation object names
        if (unlockAnimationObjectNames != null)
        {
            foreach (var objName in unlockAnimationObjectNames)
            {
                if (!string.IsNullOrEmpty(objName))
                {
                    var obj = FindGameObjectByName(objName);
                    if (obj == null)
                    {
                        Debug.LogWarning($"Quest '{title}': Unlock animation object '{objName}' not found in scene");
                        missingCount++;
                    }
                }
            }
        }
        
        // Check show object names
        if (showObjectNames != null)
        {
            foreach (var objName in showObjectNames)
            {
                if (!string.IsNullOrEmpty(objName))
                {
                    var obj = FindGameObjectByName(objName);
                    if (obj == null)
                    {
                        Debug.LogWarning($"Quest '{title}': Show object '{objName}' not found in scene");
                        missingCount++;
                    }
                }
            }
        }
        
        // Check hide object names
        if (hideObjectNames != null)
        {
            foreach (var objName in hideObjectNames)
            {
                if (!string.IsNullOrEmpty(objName))
                {
                    var obj = FindGameObjectByName(objName);
                    if (obj == null)
                    {
                        Debug.LogWarning($"Quest '{title}': Hide object '{objName}' not found in scene");
                        missingCount++;
                    }
                }
            }
        }
        
        if (missingCount == 0)
        {
            Debug.Log($"Quest '{title}': All named objects found in scene!");
        }
        else
        {
            Debug.LogWarning($"Quest '{title}': {missingCount} named objects not found in scene");
        }
    }
    
    [FoldoutGroup("Visual Settings")]
    [Button("Clean Null References")]
    [GUIColor(1f, 0.7f, 0.7f)]
    [PropertyTooltip("Remove all null/missing GameObject references from arrays")]
    private void CleanNullReferences()
    {
        CleanGameObjectArray(ref eventObjects, "Event Objects");
        CleanGameObjectArray(ref unlockAnimationObjects, "Unlock Animation Objects");
        CleanGameObjectArray(ref showObjects, "Show Objects");
        CleanGameObjectArray(ref hideObjects, "Hide Objects");
        
        // Also clean string arrays
        CleanStringArray(ref eventObjectNames, "Event Object Names");
        CleanStringArray(ref unlockAnimationObjectNames, "Unlock Animation Object Names");
        CleanStringArray(ref showObjectNames, "Show Object Names");
        CleanStringArray(ref hideObjectNames, "Hide Object Names");
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
        
        Debug.Log($"Quest '{title}': Cleaned null references from all arrays");
    }
    
    [FoldoutGroup("Visual Settings")]
    [Button("Validate All References")]
    [GUIColor(0.8f, 0.8f, 1f)]
    [PropertyTooltip("Check all GameObject arrays for null references")]
    private void ValidateAllReferences()
    {
        int totalNulls = 0;
        totalNulls += CountNullReferences(eventObjects, "Event Objects");
        totalNulls += CountNullReferences(unlockAnimationObjects, "Unlock Animation Objects");
        totalNulls += CountNullReferences(showObjects, "Show Objects");
        totalNulls += CountNullReferences(hideObjects, "Hide Objects");
        
        if (totalNulls == 0)
        {
            Debug.Log($"Quest '{title}': All GameObject references are valid!");
        }
        else
        {
            Debug.LogWarning($"Quest '{title}': Found {totalNulls} null references. Use 'Clean Null References' to fix them.");
        }
    }
    
    [FoldoutGroup("Visual Settings")]
    [Button("Force Save Asset")]
    [GUIColor(0.7f, 1f, 0.7f)]
    [PropertyTooltip("Force Unity to save this ScriptableObject and mark it as dirty")]
    private void ForceSaveAsset()
    {
        #if UNITY_EDITOR
        CleanNullReferencesInternal();
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
        UnityEditor.AssetDatabase.Refresh();
        Debug.Log($"Quest '{title}': Asset has been force saved and marked as dirty.");
        #endif
    }
    
    [FoldoutGroup("Visual Settings")]
    [Button("Preview Event Objects")]
    [GUIColor(0.8f, 1f, 0.8f)]
    [PropertyTooltip("Show event objects for this quest (for testing)")]
    private void PreviewEventObjects()
    {
        if (Application.isPlaying)
        {
            ShowEventObjects();
            Debug.Log($"Quest '{title}': Showing event objects");
        }
    }
    
    [FoldoutGroup("Visual Settings")]
    [Button("Hide Event Objects")]
    [GUIColor(1f, 0.8f, 0.8f)]
    [PropertyTooltip("Hide event objects for this quest (for testing)")]
    private void PreviewHideEventObjects()
    {
        if (Application.isPlaying)
        {
            HideEventObjects();
            Debug.Log($"Quest '{title}': Hiding event objects");
        }
    }
    
    [FoldoutGroup("Visual Settings")]
    [Button("Preview Show Objects")]
    [GUIColor(0.8f, 0.8f, 1f)]
    [PropertyTooltip("Show objects during intro/progression for this quest (for testing)")]
    private void PreviewShowObjects()
    {
        if (Application.isPlaying)
        {
            ShowObjects();
            Debug.Log($"Quest '{title}': Showing objects during intro/progression");
        }
    }
    
    [FoldoutGroup("Visual Settings")]
    [Button("Preview Show Objects (Intro Only)")]
    [GUIColor(0.7f, 0.7f, 1f)]
    [PropertyTooltip("Test show objects with intro timing only")]
    private void PreviewShowObjectsIntro()
    {
        if (Application.isPlaying)
        {
            ShowObjectsWithTiming(EnvironmentalEffectTiming.IntroOnly);
            Debug.Log($"Quest '{title}': Testing show objects with intro timing");
        }
    }
    
    [FoldoutGroup("Visual Settings")]
    [Button("Preview Show Objects (Progression Only)")]
    [GUIColor(0.6f, 0.6f, 1f)]
    [PropertyTooltip("Test show objects with progression timing only")]
    private void PreviewShowObjectsProgression()
    {
        if (Application.isPlaying)
        {
            ShowObjectsWithTiming(EnvironmentalEffectTiming.ProgressionOnly);
            Debug.Log($"Quest '{title}': Testing show objects with progression timing");
        }
    }
    
    [FoldoutGroup("Visual Settings")]
    [Button("Preview Hide Objects")]
    [GUIColor(1f, 0.8f, 1f)]
    [PropertyTooltip("Hide specified objects for this quest (for testing)")]
    private void PreviewHideObjects()
    {
        if (Application.isPlaying)
        {
            HideObjects();
            Debug.Log($"Quest '{title}': Hiding specified objects");
        }
    }
    
    [FoldoutGroup("Visual Settings")]
    [Button("Preview Hide Objects (Intro Only)")]
    [GUIColor(1f, 0.7f, 1f)]
    [PropertyTooltip("Test hide objects with intro timing only")]
    private void PreviewHideObjectsIntro()
    {
        if (Application.isPlaying)
        {
            HideObjectsWithTiming(EnvironmentalEffectTiming.IntroOnly);
            Debug.Log($"Quest '{title}': Testing hide objects with intro timing");
        }
    }
    
    [FoldoutGroup("Visual Settings")]
    [Button("Preview Hide Objects (Progression Only)")]
    [GUIColor(1f, 0.6f, 1f)]
    [PropertyTooltip("Test hide objects with progression timing only")]
    private void PreviewHideObjectsProgression()
    {
        if (Application.isPlaying)
        {
            HideObjectsWithTiming(EnvironmentalEffectTiming.ProgressionOnly);
            Debug.Log($"Quest '{title}': Testing hide objects with progression timing");
        }
    }
    
    [FoldoutGroup("Environmental Effects")]
    [Button("Test Intro Environmental Effect")]
    [GUIColor(0.8f, 1f, 1f)]
    [PropertyTooltip("Test environmental effects for intro timing")]
    [ShowIf("enableSnowyTextures")]
    private void TestIntroEnvironmentalEffect()
    {
        if (Application.isPlaying)
        {
            TriggerEnvironmentalEffectEvent(EnvironmentalEffectTiming.IntroOnly);
        }
    }
    
    [FoldoutGroup("Environmental Effects")]
    [Button("Test Progression Environmental Effect")]
    [GUIColor(1f, 0.8f, 1f)]
    [PropertyTooltip("Test environmental effects for progression timing")]
    [ShowIf("enableSnowyTextures")]
    private void TestProgressionEnvironmentalEffect()
    {
        if (Application.isPlaying)
        {
            TriggerEnvironmentalEffectEvent(EnvironmentalEffectTiming.ProgressionOnly);
        }
    }
    
    [FoldoutGroup("Environmental Effects")]
    [Button("Test Gameplay Environmental Effect")]
    [GUIColor(1f, 1f, 0.8f)]
    [PropertyTooltip("Test environmental effects for gameplay timing")]
    [ShowIf("enableSnowyTextures")]
    private void TestGameplayEnvironmentalEffect()
    {
        if (Application.isPlaying)
        {
            TriggerEnvironmentalEffectEvent(EnvironmentalEffectTiming.GameplayOnly);
        }
    }
    
    /// <summary>
    /// Helper method to show event objects for this quest
    /// </summary>
    public void ShowEventObjects()
    {
        // Use string names first (new system)
        if (eventObjectNames != null)
        {
            foreach (var objName in eventObjectNames)
            {
                if (!string.IsNullOrEmpty(objName))
                {
                    var obj = FindGameObjectByName(objName);
                    if (obj != null) obj.SetActive(true);
                    else Debug.LogWarning($"Quest '{title}': Could not find event object with name '{objName}'");
                }
            }
        }
        
        // Fallback to legacy GameObject references
        if (eventObjects != null)
        {
            foreach (var obj in eventObjects)
            {
                if (obj != null) obj.SetActive(true);
            }
        }
    }
    
    /// <summary>
    /// Helper method to hide event objects for this quest
    /// </summary>
    public void HideEventObjects()
    {
        // Use string names first (new system)
        if (eventObjectNames != null)
        {
            foreach (var objName in eventObjectNames)
            {
                if (!string.IsNullOrEmpty(objName))
                {
                    var obj = FindGameObjectByName(objName);
                    if (obj != null) obj.SetActive(false);
                    else Debug.LogWarning($"Quest '{title}': Could not find event object with name '{objName}'");
                }
            }
        }
        
        // Fallback to legacy GameObject references
        if (eventObjects != null)
        {
            foreach (var obj in eventObjects)
            {
                if (obj != null) obj.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Helper method to show objects during intro/progression states
    /// </summary>
    public void ShowObjects()
    {
        ShowObjectsWithTiming(EnvironmentalEffectTiming.Both); // Default behavior for backward compatibility
    }
    
    /// <summary>
    /// Helper method to show objects during intro/progression states with specific timing
    /// </summary>
    public void ShowObjectsWithTiming(EnvironmentalEffectTiming currentTiming)
    {
        // Only show if the timing matches the configured timing
        if (ShouldApplyObjectTimingAt(showObjectsTiming, currentTiming))
        {
            // Use string names first (new system)
            if (showObjectNames != null)
            {
                foreach (var objName in showObjectNames)
                {
                    if (!string.IsNullOrEmpty(objName))
                    {
                        var obj = FindGameObjectByName(objName);
                        if (obj != null) obj.SetActive(true);
                        else Debug.LogWarning($"Quest '{title}': Could not find show object with name '{objName}'");
                    }
                }
            }
            
            // Fallback to legacy GameObject references
            if (showObjects != null)
            {
                foreach (var obj in showObjects)
                {
                    if (obj != null) obj.SetActive(true);
                }
            }
        }
    }
    
    /// <summary>
    /// Helper method to hide objects during intro/progression states
    /// </summary>
    public void HideObjects()
    {
        HideObjectsWithTiming(EnvironmentalEffectTiming.Both); // Default behavior for backward compatibility
    }
    
    /// <summary>
    /// Helper method to hide objects during intro/progression states with specific timing
    /// </summary>
    public void HideObjectsWithTiming(EnvironmentalEffectTiming currentTiming)
    {
        // Only hide if the timing matches the configured timing
        if (ShouldApplyObjectTimingAt(hideObjectsTiming, currentTiming))
        {
            // Use string names first (new system)
            if (hideObjectNames != null)
            {
                foreach (var objName in hideObjectNames)
                {
                    if (!string.IsNullOrEmpty(objName))
                    {
                        var obj = FindGameObjectByName(objName);
                        if (obj != null) obj.SetActive(false);
                        else Debug.LogWarning($"Quest '{title}': Could not find hide object with name '{objName}'");
                    }
                }
            }
            
            // Fallback to legacy GameObject references
            if (hideObjects != null)
            {
                foreach (var obj in hideObjects)
                {
                    if (obj != null) obj.SetActive(false);
                }
            }
        }
    }
    
    /// <summary>
    /// Helper method to show objects that should be hidden (opposite of HideObjects)
    /// </summary>
    public void ShowHideObjects()
    {
        ShowHideObjectsWithTiming(EnvironmentalEffectTiming.Both); // Default behavior for backward compatibility
    }
    
    /// <summary>
    /// Helper method to show objects that should be hidden with specific timing
    /// </summary>
    public void ShowHideObjectsWithTiming(EnvironmentalEffectTiming currentTiming)
    {
        // Only show if the timing matches the configured timing
        if (ShouldApplyObjectTimingAt(hideObjectsTiming, currentTiming))
        {
            // Use string names first (new system)
            if (hideObjectNames != null)
            {
                foreach (var objName in hideObjectNames)
                {
                    if (!string.IsNullOrEmpty(objName))
                    {
                        var obj = FindGameObjectByName(objName);
                        if (obj != null) obj.SetActive(true);
                        else Debug.LogWarning($"Quest '{title}': Could not find hide object with name '{objName}'");
                    }
                }
            }
            
            // Fallback to legacy GameObject references
            if (hideObjects != null)
            {
                foreach (var obj in hideObjects)
                {
                    if (obj != null) obj.SetActive(true);
                }
            }
        }
    }
    
    /// <summary>
    /// Get unlock animation objects for this quest (returns both named objects and legacy references)
    /// </summary>
    public GameObject[] GetUnlockAnimationObjects()
    {
        var objects = new System.Collections.Generic.List<GameObject>();
        
        // Add objects from string names (new system)
        if (unlockAnimationObjectNames != null)
        {
            foreach (var objName in unlockAnimationObjectNames)
            {
                if (!string.IsNullOrEmpty(objName))
                {
                    var obj = FindGameObjectByName(objName);
                    if (obj != null) objects.Add(obj);
                    else Debug.LogWarning($"Quest '{title}': Could not find unlock animation object with name '{objName}'");
                }
            }
        }
        
        // Add legacy GameObject references
        if (unlockAnimationObjects != null)
        {
            foreach (var obj in unlockAnimationObjects)
            {
                if (obj != null) objects.Add(obj);
            }
        }
        
        return objects.ToArray();
    }
    
    /// <summary>
    /// Find a GameObject by name in the scene (searches all loaded scenes)
    /// </summary>
    private GameObject FindGameObjectByName(string objectName)
    {
        if (string.IsNullOrEmpty(objectName)) return null;
        
        // First try to find in the active scene
        var obj = GameObject.Find(objectName);
        if (obj != null) return obj;
        
        // If not found, search through all root objects in all loaded scenes
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
            if (scene.isLoaded)
            {
                var rootObjects = scene.GetRootGameObjects();
                foreach (var rootObj in rootObjects)
                {
                    // Check if this root object has the name we're looking for
                    if (rootObj.name == objectName) return rootObj;
                    
                    // Search children recursively
                    var foundObj = FindInChildren(rootObj.transform, objectName);
                    if (foundObj != null) return foundObj;
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Recursively search for an object by name in children
    /// </summary>
    private GameObject FindInChildren(Transform parent, string objectName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == objectName) return child.gameObject;
            
            var foundObj = FindInChildren(child, objectName);
            if (foundObj != null) return foundObj;
        }
        return null;
    }
    
    /// <summary>
    /// Event triggered when this quest starts and environmental effects should be applied
    /// </summary>
    public static event System.Action<QuestData, EnvironmentalEffectTiming> OnEnvironmentalEffectRequested;
    
    /// <summary>
    /// Trigger environmental effect event for this quest (called at different quest states)
    /// </summary>
    public void TriggerEnvironmentalEffectEvent(EnvironmentalEffectTiming currentTiming)
    {
        if (enableSnowyTextures && ShouldApplyEffectAtTiming(currentTiming))
        {
            OnEnvironmentalEffectRequested?.Invoke(this, currentTiming);
            Debug.Log($"Quest '{title}': Triggered environmental effect event for {currentTiming}");
        }
    }
    
    /// <summary>
    /// Legacy method for backward compatibility (triggers snowy texture event)
    /// </summary>
    public void TriggerSnowyTextureEvent()
    {
        TriggerEnvironmentalEffectEvent(EnvironmentalEffectTiming.GameplayOnly);
    }
    
    /// <summary>
    /// Check if effects should be applied at the current timing
    /// </summary>
    private bool ShouldApplyEffectAtTiming(EnvironmentalEffectTiming currentTiming)
    {
        return effectTiming switch
        {
            EnvironmentalEffectTiming.IntroOnly => currentTiming == EnvironmentalEffectTiming.IntroOnly,
            EnvironmentalEffectTiming.ProgressionOnly => currentTiming == EnvironmentalEffectTiming.ProgressionOnly,
            EnvironmentalEffectTiming.Both => currentTiming == EnvironmentalEffectTiming.IntroOnly || currentTiming == EnvironmentalEffectTiming.ProgressionOnly,
            EnvironmentalEffectTiming.GameplayOnly => currentTiming == EnvironmentalEffectTiming.GameplayOnly,
            _ => false
        };
    }
    
    /// <summary>
    /// Check if object timing should be applied at the current timing
    /// </summary>
    private bool ShouldApplyObjectTimingAt(EnvironmentalEffectTiming objectTiming, EnvironmentalEffectTiming currentTiming)
    {
        return objectTiming switch
        {
            EnvironmentalEffectTiming.IntroOnly => currentTiming == EnvironmentalEffectTiming.IntroOnly,
            EnvironmentalEffectTiming.ProgressionOnly => currentTiming == EnvironmentalEffectTiming.ProgressionOnly,
            EnvironmentalEffectTiming.Both => currentTiming == EnvironmentalEffectTiming.IntroOnly || currentTiming == EnvironmentalEffectTiming.ProgressionOnly,
            EnvironmentalEffectTiming.GameplayOnly => currentTiming == EnvironmentalEffectTiming.GameplayOnly,
            _ => true // Default to always apply if timing is undefined
        };
    }
    
    /// <summary>
    /// Validate GameObject array for null references (used by Odin Inspector)
    /// </summary>
    private bool ValidateGameObjectArray(GameObject[] array)
    {
        if (array == null) return true; // Null arrays are valid
        
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == null) return false; // Found null reference
        }
        return true; // All references are valid
    }
    
    /// <summary>
    /// Clean null references from a GameObject array
    /// </summary>
    private void CleanGameObjectArray(ref GameObject[] array, string arrayName)
    {
        if (array == null) return;
        
        var validObjects = new System.Collections.Generic.List<GameObject>();
        int removedCount = 0;
        
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] != null)
            {
                validObjects.Add(array[i]);
            }
            else
            {
                removedCount++;
            }
        }
        
        array = validObjects.ToArray();
        
        if (removedCount > 0)
        {
            Debug.Log($"Quest '{title}': Removed {removedCount} null references from {arrayName}");
        }
    }
    
    /// <summary>
    /// Clean null or empty strings from a string array
    /// </summary>
    private void CleanStringArray(ref string[] array, string arrayName)
    {
        if (array == null) return;
        
        var validStrings = new System.Collections.Generic.List<string>();
        int removedCount = 0;
        
        for (int i = 0; i < array.Length; i++)
        {
            if (!string.IsNullOrEmpty(array[i]))
            {
                validStrings.Add(array[i]);
            }
            else
            {
                removedCount++;
            }
        }
        
        array = validStrings.ToArray();
        
        if (removedCount > 0)
        {
            Debug.Log($"Quest '{title}': Removed {removedCount} empty entries from {arrayName}");
        }
    }
    
    /// <summary>
    /// Count null references in a GameObject array
    /// </summary>
    private int CountNullReferences(GameObject[] array, string arrayName)
    {
        if (array == null) return 0;
        
        int nullCount = 0;
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == null) nullCount++;
        }
        
        if (nullCount > 0)
        {
            Debug.LogWarning($"Quest '{title}': {nullCount} null references in {arrayName}");
        }
        
        return nullCount;
    }
    
    #if UNITY_EDITOR
    /// <summary>
    /// Called when the ScriptableObject is loaded or values are changed in the Inspector
    /// Disabled to prevent interference with dragging operations and array modifications
    /// </summary>
    private void OnValidate()
    {
        // Only mark as dirty, don't auto-clean as it interferes with Inspector editing
        UnityEditor.EditorUtility.SetDirty(this);
    }
    
    /// <summary>
    /// Internal cleanup method that doesn't log messages (to avoid spam during OnValidate)
    /// </summary>
    private void CleanNullReferencesInternal()
    {
        CleanGameObjectArraySilent(ref eventObjects);
        CleanGameObjectArraySilent(ref unlockAnimationObjects);
        CleanGameObjectArraySilent(ref showObjects);
        CleanGameObjectArraySilent(ref hideObjects);
        
        // Also clean string arrays silently
        CleanStringArraySilent(ref eventObjectNames);
        CleanStringArraySilent(ref unlockAnimationObjectNames);
        CleanStringArraySilent(ref showObjectNames);
        CleanStringArraySilent(ref hideObjectNames);
    }
    
    /// <summary>
    /// Silent version of CleanGameObjectArray for OnValidate
    /// </summary>
    private void CleanGameObjectArraySilent(ref GameObject[] array)
    {
        if (array == null) return;
        
        var validObjects = new System.Collections.Generic.List<GameObject>();
        
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] != null)
            {
                validObjects.Add(array[i]);
            }
        }
        
        if (validObjects.Count != array.Length)
        {
            array = validObjects.ToArray();
        }
    }
    
    /// <summary>
    /// Silent version of CleanStringArray for OnValidate
    /// </summary>
    private void CleanStringArraySilent(ref string[] array)
    {
        if (array == null) return;
        
        var validStrings = new System.Collections.Generic.List<string>();
        
        for (int i = 0; i < array.Length; i++)
        {
            if (!string.IsNullOrEmpty(array[i]))
            {
                validStrings.Add(array[i]);
            }
        }
        
        if (validStrings.Count != array.Length)
        {
            array = validStrings.ToArray();
        }
    }
    #endif
}
