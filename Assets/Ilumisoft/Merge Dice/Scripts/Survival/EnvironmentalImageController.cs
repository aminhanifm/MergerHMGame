using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Ilumisoft.MergeDice.Survival;

/// <summary>
/// Advanced environmental effect controller that can apply different texture overlays
/// based on quest requirements (snow, rain, night, etc.)
/// </summary>
public class EnvironmentalImageController : MonoBehaviour
{
    [System.Serializable]
    public class ImageEffectPair
    {
        [HorizontalGroup("Image Effect")]
        [LabelWidth(80)]
        [Tooltip("The UI Image to apply effects to")]
        public Image targetImage;
        
        [HorizontalGroup("Image Effect")]
        [LabelWidth(80)]
        [Tooltip("The snowy/winter version of this image")]
        public Sprite snowySprite;
        
        [HorizontalGroup("Image Effect")]
        [LabelWidth(80)]
        [Tooltip("The rainy version of this image (optional)")]
        public Sprite rainySprite;
        
        [HorizontalGroup("Image Effect")]
        [LabelWidth(80)]
        [Tooltip("The night version of this image (optional)")]
        public Sprite nightSprite;
        
        [ReadOnly]
        [ShowInInspector]
        [PropertyTooltip("Original sprite (automatically stored)")]
        public Sprite OriginalSprite { get; set; }
    }
    
    public enum EnvironmentalEffect
    {
        None,
        Snowy,
        Rainy,
        Night
    }
    
    [BoxGroup("Environmental Settings")]
    [Header("Environmental Effect Configuration")]
    [TableList(ShowIndexLabels = true, DrawScrollView = true, MaxScrollViewHeight = 400)]
    [Tooltip("Image-effect pairs for environmental changes")]
    public List<ImageEffectPair> imageEffectPairs = new List<ImageEffectPair>();
    
    [BoxGroup("Environmental Settings")]
    [Tooltip("Quest system reference to listen for quest events")]
    public QuestSystem questSystem;
    
    [BoxGroup("Environmental Settings")]
    [Tooltip("Should environmental effects be cleared when switching between quest states?")]
    public bool clearEffectsBetweenStates = true;
    
    [BoxGroup("Debug")]
    [Header("Debug & Testing")]
    [ReadOnly]
    [ShowInInspector]
    [Tooltip("Currently active environmental effect")]
    public EnvironmentalEffect CurrentEffect { get; private set; } = EnvironmentalEffect.None;
    
    [BoxGroup("Debug")]
    [ReadOnly]
    [ShowInInspector]
    [Tooltip("Number of images with stored originals")]
    public int StoredOriginalCount => GetStoredOriginalCount();
    
    private bool isInitialized = false;
    
    [BoxGroup("Debug")]
    [Button("Apply Snowy Effect")]
    [PropertyTooltip("Test applying snowy textures")]
    private void TestApplySnowy() => ApplyEnvironmentalEffect(EnvironmentalEffect.Snowy);
    
    [BoxGroup("Debug")]
    [Button("Apply Rainy Effect")]
    [PropertyTooltip("Test applying rainy textures")]
    private void TestApplyRainy() => ApplyEnvironmentalEffect(EnvironmentalEffect.Rainy);
    
    [BoxGroup("Debug")]
    [Button("Apply Night Effect")]
    [PropertyTooltip("Test applying night textures")]
    private void TestApplyNight() => ApplyEnvironmentalEffect(EnvironmentalEffect.Night);
    
    [BoxGroup("Debug")]
    [Button("Restore Original")]
    [PropertyTooltip("Test restoring original textures")]
    private void TestRestoreOriginal() => ApplyEnvironmentalEffect(EnvironmentalEffect.None);
    
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
        
        // Auto-find quest system if not assigned
        if (questSystem == null)
        {
            questSystem = FindFirstObjectByType<QuestSystem>();
            if (questSystem != null)
            {
                questSystem.OnNewDayStarted += OnQuestStarted;
                Debug.Log("EnvironmentalImageController: Found and subscribed to QuestSystem");
            }
        }
    }
    
    private void OnDisable()
    {
        if (questSystem != null)
        {
            questSystem.OnNewDayStarted -= OnQuestStarted;
        }
        
        QuestData.OnEnvironmentalEffectRequested -= OnEnvironmentalEffectRequested;
    }
    
    private void InitializeOriginalSprites()
    {
        if (isInitialized) return;
        
        foreach (var pair in imageEffectPairs)
        {
            if (pair.targetImage != null && pair.targetImage.sprite != null)
            {
                pair.OriginalSprite = pair.targetImage.sprite;
            }
        }
        
        isInitialized = true;
        Debug.Log($"EnvironmentalImageController: Initialized {imageEffectPairs.Count} image effect pairs");
    }
    
    private void OnQuestStarted()
    {
        var effect = DetermineEnvironmentalEffect(questSystem?.CurrentQuestData);
        ApplyEnvironmentalEffect(effect);
    }
    
    private void OnSnowyTextureRequested(QuestData questData)
    {
        if (questData != null && questData.enableSnowyTextures)
        {
            ApplyEnvironmentalEffect(EnvironmentalEffect.Snowy);
            Debug.Log($"EnvironmentalImageController: Applied snowy effect for quest '{questData.title}'");
        }
    }
    
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
                var effect = DetermineEnvironmentalEffect(questData);
                ApplyEnvironmentalEffect(effect);
                Debug.Log($"EnvironmentalImageController: Applied {effect} effect for quest '{questData.title}' at timing: {timing}");
            }
            else if (clearEffectsBetweenStates)
            {
                // Clear effects if they shouldn't be applied at this timing
                ApplyEnvironmentalEffect(EnvironmentalEffect.None);
                Debug.Log($"EnvironmentalImageController: Cleared effects for quest '{questData.title}' at timing: {timing} (not configured for this timing)");
            }
        }
        else if (clearEffectsBetweenStates)
        {
            // Restore original if no quest data or effects disabled
            ApplyEnvironmentalEffect(EnvironmentalEffect.None);
        }
    }
    
    private EnvironmentalEffect DetermineEnvironmentalEffect(QuestData questData)
    {
        if (questData == null) return EnvironmentalEffect.None;
        
        // Check explicit snowy texture flag
        if (questData.enableSnowyTextures)
        {
            return EnvironmentalEffect.Snowy;
        }
        
        // Check quest title for keywords
        if (questData.title != null)
        {
            string title = questData.title.ToLower();
            
            if (title.Contains("winter") || title.Contains("snow") || title.Contains("cold") || title.Contains("freeze"))
                return EnvironmentalEffect.Snowy;
            
            if (title.Contains("rain") || title.Contains("storm") || title.Contains("wet"))
                return EnvironmentalEffect.Rainy;
            
            if (title.Contains("night") || title.Contains("dark") || title.Contains("midnight"))
                return EnvironmentalEffect.Night;
        }
        
        return EnvironmentalEffect.None;
    }
    
    /// <summary>
    /// Apply the specified environmental effect to all images
    /// </summary>
    public void ApplyEnvironmentalEffect(EnvironmentalEffect effect)
    {
        if (!isInitialized)
        {
            InitializeOriginalSprites();
        }
        
        foreach (var pair in imageEffectPairs)
        {
            if (pair.targetImage == null) continue;
            
            Sprite spriteToApply = GetSpriteForEffect(pair, effect);
            if (spriteToApply != null)
            {
                pair.targetImage.sprite = spriteToApply;
            }
        }
        
        CurrentEffect = effect;
        
        string effectName = effect == EnvironmentalEffect.None ? "original" : effect.ToString().ToLower();
        Debug.Log($"EnvironmentalImageController: Applied {effectName} effect to {imageEffectPairs.Count} images");
    }
    
    private Sprite GetSpriteForEffect(ImageEffectPair pair, EnvironmentalEffect effect)
    {
        return effect switch
        {
            EnvironmentalEffect.Snowy => pair.snowySprite ?? pair.OriginalSprite,
            EnvironmentalEffect.Rainy => pair.rainySprite ?? pair.OriginalSprite,
            EnvironmentalEffect.Night => pair.nightSprite ?? pair.OriginalSprite,
            EnvironmentalEffect.None => pair.OriginalSprite,
            _ => pair.OriginalSprite
        };
    }
    
    /// <summary>
    /// Add a new image effect pair at runtime
    /// </summary>
    public void AddImageEffectPair(Image targetImage, Sprite snowySprite = null, Sprite rainySprite = null, Sprite nightSprite = null)
    {
        if (targetImage == null)
        {
            Debug.LogWarning("EnvironmentalImageController: Cannot add null image!");
            return;
        }
        
        var newPair = new ImageEffectPair
        {
            targetImage = targetImage,
            snowySprite = snowySprite,
            rainySprite = rainySprite,
            nightSprite = nightSprite,
            OriginalSprite = targetImage.sprite
        };
        
        imageEffectPairs.Add(newPair);
        Debug.Log($"EnvironmentalImageController: Added new effect pair for {targetImage.name}");
    }
    
    /// <summary>
    /// Remove an image effect pair
    /// </summary>
    public void RemoveImageEffectPair(Image targetImage)
    {
        for (int i = imageEffectPairs.Count - 1; i >= 0; i--)
        {
            if (imageEffectPairs[i].targetImage == targetImage)
            {
                // Restore original before removing
                if (targetImage != null && imageEffectPairs[i].OriginalSprite != null)
                {
                    targetImage.sprite = imageEffectPairs[i].OriginalSprite;
                }
                
                imageEffectPairs.RemoveAt(i);
                Debug.Log($"EnvironmentalImageController: Removed effect pair for {targetImage.name}");
                break;
            }
        }
    }
    
    private int GetStoredOriginalCount()
    {
        int count = 0;
        foreach (var pair in imageEffectPairs)
        {
            if (pair.OriginalSprite != null) count++;
        }
        return count;
    }
    
    /// <summary>
    /// Manually trigger specific environmental effects
    /// </summary>
    public void TriggerSnowyEffect() => ApplyEnvironmentalEffect(EnvironmentalEffect.Snowy);
    public void TriggerRainyEffect() => ApplyEnvironmentalEffect(EnvironmentalEffect.Rainy);
    public void TriggerNightEffect() => ApplyEnvironmentalEffect(EnvironmentalEffect.Night);
    public void RestoreOriginal() => ApplyEnvironmentalEffect(EnvironmentalEffect.None);
    
    #if UNITY_EDITOR
    private void OnValidate()
    {
        // Validate that each pair has at least one effect sprite
        foreach (var pair in imageEffectPairs)
        {
            if (pair.targetImage != null && 
                pair.snowySprite == null && 
                pair.rainySprite == null && 
                pair.nightSprite == null)
            {
                Debug.LogWarning($"EnvironmentalImageController: Image '{pair.targetImage.name}' has no effect sprites assigned!");
            }
        }
    }
    #endif
}
