using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Ilumisoft.MergeDice.Survival;

/// <summary>
/// Controls snowy texture overlay for UI Images during specific quests
/// Stores original sprites and applies snowy versions when activated
/// </summary>
public class SnowyImageController : MonoBehaviour
{
    [BoxGroup("Snowy Image Settings")]
    [Header("Snowy Image Configuration")]
    [Tooltip("The UI Images that will have snowy textures applied")]
    public Image[] targetImages;
    
    [BoxGroup("Snowy Image Settings")]
    [Tooltip("The snowy sprite textures corresponding to each target image")]
    public Sprite[] snowySprites;
    
    [BoxGroup("Snowy Image Settings")]
    [Tooltip("Quest system reference to listen for quest events")]
    public QuestSystem questSystem;
    
    [BoxGroup("Snowy Image Settings")]
    [Tooltip("Should snowy effects be cleared when switching between quest states?")]
    public bool clearEffectsBetweenStates = true;
    
    [BoxGroup("Debug")]
    [Header("Debug & Testing")]
    [ReadOnly]
    [ShowInInspector]
    [Tooltip("Currently showing snowy textures?")]
    public bool IsSnowyActive { get; private set; }
    
    [BoxGroup("Debug")]
    [ReadOnly]
    [ShowInInspector]
    [Tooltip("Number of images with stored original sprites")]
    public int StoredOriginalCount => originalSprites != null ? originalSprites.Count : 0;
    
    // Store original sprites to restore later
    private Dictionary<Image, Sprite> originalSprites = new Dictionary<Image, Sprite>();
    private bool isInitialized = false;
    
    [BoxGroup("Debug")]
    [Button("Apply Snowy Textures")]
    [PropertyTooltip("Test applying snowy textures (for debugging)")]
    private void TestApplySnowy()
    {
        ApplySnowyTextures();
    }
    
    [BoxGroup("Debug")]
    [Button("Restore Original Textures")]
    [PropertyTooltip("Test restoring original textures (for debugging)")]
    private void TestRestoreOriginal()
    {
        RestoreOriginalTextures();
    }
    
    private void Awake()
    {
        InitializeOriginalSprites();
    }
    
    private void OnEnable()
    {
        // Subscribe to quest system events
        if (questSystem != null)
        {
            questSystem.OnNewDayStarted += OnQuestStarted;
        }
        
        // Subscribe to environmental effect events from QuestData
        QuestData.OnEnvironmentalEffectRequested += OnEnvironmentalEffectRequested;
        
        // If quest system is not assigned, try to find it
        if (questSystem == null)
        {
            questSystem = FindFirstObjectByType<QuestSystem>();
            if (questSystem != null)
            {
                questSystem.OnNewDayStarted += OnQuestStarted;
                Debug.Log("SnowyImageController: Found and subscribed to QuestSystem");
            }
            else
            {
                Debug.LogWarning("SnowyImageController: No QuestSystem found! Snowy textures won't activate automatically.");
            }
        }
    }
    
    private void OnDisable()
    {
        // Unsubscribe from quest system events
        if (questSystem != null)
        {
            questSystem.OnNewDayStarted -= OnQuestStarted;
        }
        
        // Unsubscribe from environmental effect events
        QuestData.OnEnvironmentalEffectRequested -= OnEnvironmentalEffectRequested;
    }
    
    /// <summary>
    /// Initialize and store the original sprites from target images
    /// </summary>
    private void InitializeOriginalSprites()
    {
        if (isInitialized) return;
        
        originalSprites.Clear();
        
        if (targetImages != null)
        {
            foreach (var image in targetImages)
            {
                if (image != null && image.sprite != null)
                {
                    originalSprites[image] = image.sprite;
                }
            }
        }
        
        isInitialized = true;
        Debug.Log($"SnowyImageController: Initialized with {originalSprites.Count} original sprites stored");
    }
    
    /// <summary>
    /// Called when a new quest/day starts
    /// </summary>
    private void OnQuestStarted()
    {
        // Check if current quest should have snowy textures
        if (questSystem?.CurrentQuestData != null && ShouldApplySnowy(questSystem.CurrentQuestData))
        {
            ApplySnowyTextures();
        }
        else
        {
            RestoreOriginalTextures();
        }
    }
    
    /// <summary>
    /// Called when a quest explicitly requests snowy textures
    /// </summary>
    private void OnSnowyTextureRequested(QuestData questData)
    {
        if (questData != null && questData.enableSnowyTextures)
        {
            ApplySnowyTextures();
            Debug.Log($"SnowyImageController: Applied snowy textures for quest '{questData.title}'");
        }
    }
    
    /// <summary>
    /// Called when a quest requests environmental effects with specific timing
    /// </summary>
    private void OnEnvironmentalEffectRequested(QuestData questData, EnvironmentalEffectTiming timing)
    {
        if (questData != null && questData.enableSnowyTextures)
        {
            // Check if effects should be applied at this timing
            bool shouldApply = questData.effectTiming switch
            {
                EnvironmentalEffectTiming.IntroOnly => timing == EnvironmentalEffectTiming.IntroOnly,
                EnvironmentalEffectTiming.ProgressionOnly => timing == EnvironmentalEffectTiming.ProgressionOnly,
                EnvironmentalEffectTiming.Both => timing == EnvironmentalEffectTiming.IntroOnly || timing == EnvironmentalEffectTiming.ProgressionOnly,
                EnvironmentalEffectTiming.GameplayOnly => timing == EnvironmentalEffectTiming.GameplayOnly,
                _ => false
            };
            
            if (shouldApply)
            {
                ApplySnowyTextures();
                Debug.Log($"SnowyImageController: Applied snowy textures for quest '{questData.title}' at timing: {timing}");
            }
            else if (clearEffectsBetweenStates)
            {
                // Clear effects if they shouldn't be applied at this timing
                RestoreOriginalTextures();
                Debug.Log($"SnowyImageController: Cleared snowy textures for quest '{questData.title}' at timing: {timing} (not configured for this timing)");
            }
        }
        else if (clearEffectsBetweenStates)
        {
            // Restore original textures if quest doesn't need snowy effects
            RestoreOriginalTextures();
        }
    }
    
    /// <summary>
    /// Determine if snowy textures should be applied for this quest
    /// You can customize this logic based on quest properties
    /// </summary>
    private bool ShouldApplySnowy(QuestData questData)
    {
        // First check: explicit enableSnowyTextures flag
        if (questData.enableSnowyTextures)
        {
            return true;
        }
        
        // Second check: Quest title contains winter/snow keywords
        if (questData.title != null)
        {
            string title = questData.title.ToLower();
            return title.Contains("winter") || title.Contains("snow") || title.Contains("cold") || title.Contains("freeze");
        }
        
        return false;
    }
    
    /// <summary>
    /// Apply snowy textures to all target images
    /// </summary>
    public void ApplySnowyTextures()
    {
        if (!isInitialized)
        {
            InitializeOriginalSprites();
        }
        
        if (targetImages == null || snowySprites == null)
        {
            Debug.LogWarning("SnowyImageController: Target images or snowy sprites not assigned!");
            return;
        }
        
        if (targetImages.Length != snowySprites.Length)
        {
            Debug.LogWarning("SnowyImageController: Target images and snowy sprites arrays must have the same length!");
            return;
        }
        
        for (int i = 0; i < targetImages.Length && i < snowySprites.Length; i++)
        {
            if (targetImages[i] != null && snowySprites[i] != null)
            {
                // Store original if not already stored
                if (!originalSprites.ContainsKey(targetImages[i]))
                {
                    originalSprites[targetImages[i]] = targetImages[i].sprite;
                }
                
                // Apply snowy texture
                targetImages[i].sprite = snowySprites[i];
            }
        }
        
        IsSnowyActive = true;
        Debug.Log("SnowyImageController: Applied snowy textures to images");
    }
    
    /// <summary>
    /// Restore original textures to all target images
    /// </summary>
    public void RestoreOriginalTextures()
    {
        if (!isInitialized)
        {
            InitializeOriginalSprites();
        }
        
        if (originalSprites == null || originalSprites.Count == 0)
        {
            Debug.LogWarning("SnowyImageController: No original sprites stored!");
            return;
        }
        
        foreach (var kvp in originalSprites)
        {
            if (kvp.Key != null && kvp.Value != null)
            {
                kvp.Key.sprite = kvp.Value;
            }
        }
        
        IsSnowyActive = false;
        Debug.Log("SnowyImageController: Restored original textures to images");
    }
    
    /// <summary>
    /// Manually set snowy state (useful for custom quest logic)
    /// </summary>
    public void SetSnowyState(bool enableSnowy)
    {
        if (enableSnowy)
        {
            ApplySnowyTextures();
        }
        else
        {
            RestoreOriginalTextures();
        }
    }
    
    /// <summary>
    /// Add a new image-sprite pair at runtime
    /// </summary>
    public void AddSnowyImagePair(Image targetImage, Sprite snowySprite)
    {
        if (targetImage == null || snowySprite == null)
        {
            Debug.LogWarning("SnowyImageController: Cannot add null image or sprite pair!");
            return;
        }
        
        // Store original if not already stored
        if (!originalSprites.ContainsKey(targetImage))
        {
            originalSprites[targetImage] = targetImage.sprite;
        }
        
        // Expand arrays to include new pair
        System.Array.Resize(ref targetImages, targetImages.Length + 1);
        System.Array.Resize(ref snowySprites, snowySprites.Length + 1);
        
        targetImages[targetImages.Length - 1] = targetImage;
        snowySprites[snowySprites.Length - 1] = snowySprite;
        
        Debug.Log($"SnowyImageController: Added new snowy image pair for {targetImage.name}");
    }
    
    #if UNITY_EDITOR
    /// <summary>
    /// Validate arrays in the inspector
    /// </summary>
    private void OnValidate()
    {
        if (targetImages != null && snowySprites != null && targetImages.Length != snowySprites.Length)
        {
            Debug.LogWarning("SnowyImageController: Target images and snowy sprites arrays should have the same length!");
        }
    }
    #endif
}
