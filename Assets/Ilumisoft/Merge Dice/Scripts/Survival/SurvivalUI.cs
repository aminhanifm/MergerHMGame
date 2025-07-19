using TMPro;
using UnityEngine;
using Ilumisoft.MergeDice.Survival;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class SurvivalUI : MonoBehaviour
{
    [BoxGroup("Core References")]
    [Header("References")]
    public SurvivalTimer timer;
    [BoxGroup("Core References")]
    public QuestSystem questSystem;
    [BoxGroup("Core References")]
    public SurvivalGameMode gameMode;

    [BoxGroup("Timer & Quest UI")]
    [Header("UI Elements")]
    public TMP_Text timerText;
    [BoxGroup("Timer & Quest UI")]
    public TMP_Text questTitleText;
    [BoxGroup("Timer & Quest UI")]
    public TMP_Text questDescText;
    [BoxGroup("Timer & Quest UI")]
    public TMP_Text questProgressText;

    [BoxGroup("Survival Resources UI")]
    [Header("Food & Water UI")]
    [Tooltip("UI Slider for food level")]
    public Slider foodSlider;
    [BoxGroup("Survival Resources UI")]
    [Tooltip("Text showing food amount")]
    public TMP_Text foodText;
    [BoxGroup("Survival Resources UI")]
    [Tooltip("UI Slider for water level")]
    public Slider waterSlider;
    [BoxGroup("Survival Resources UI")]
    [Tooltip("Text showing water amount")]
    public TMP_Text waterText;

    [BoxGroup("Progression UI")]
    [Header("Progression UI")]
    public CanvasGroup dayProgressionCanvasGroup; // Assign in inspector instead of GameObject
    [BoxGroup("Progression UI")]
    public TMP_Text dayProgressionText;
    [BoxGroup("Progression UI")]
    public UnityEngine.UI.Button nextDayButton;

    [BoxGroup("Day Intro UI")]
    [Header("Day Intro UI")]
    [Tooltip("Canvas group for the day intro overlay")]
    public CanvasGroup dayIntroCanvasGroup;
    [BoxGroup("Day Intro UI")]
    [Tooltip("Title text for the day intro (e.g., 'Day 2')")]
    public TMP_Text dayIntroTitleText;
    [BoxGroup("Day Intro UI")]
    [Tooltip("Quest title text for the day intro")]
    public TMP_Text dayIntroQuestTitleText;
    [BoxGroup("Day Intro UI")]
    [Tooltip("Quest description text for the day intro")]
    public TMP_Text dayIntroDescriptionText;
    [BoxGroup("Day Intro UI")]
    [Tooltip("Objectives text showing what needs to be done")]
    public TMP_Text dayIntroObjectivesText;
    [BoxGroup("Day Intro UI")]
    [Tooltip("Button to start the day after reading the intro")]
    public UnityEngine.UI.Button startDayButton;

    [BoxGroup("Object Visibility Control")]
    [Header("Global Object Visibility Control")]
    [Tooltip("Objects that are enabled during intro state but disabled during progression state")]
    public GameObject[] enabledOnIntroOnly;
    [BoxGroup("Object Visibility Control")]
    [Tooltip("Objects that are disabled during intro state but enabled during progression state")]
    public GameObject[] enabledOnProgressionOnly;

    // Flag to prevent spam clicking of start day button
    private bool isStartingDay = false;
    // Flag to prevent spam clicking of next day button  
    private bool isProcessingDayTransition = false;

    [BoxGroup("Unlock Animation")]
    [Header("Unlock Animation")]
    public GameObject objectsParent; // Assign in inspector
    [BoxGroup("Unlock Animation")]
    [Range(0.1f, 5f)]
    public float flickerDuration = 1.0f;
    [BoxGroup("Unlock Animation")]
    [Range(0.01f, 1f)]
    public float flickerInterval = 0.15f;

    private void OnEnable()
    {
        if (timer != null)
        {
            timer.OnTimerTick += UpdateTimerText;
            timer.OnTimerEnd += OnTimerEnd;
            // Show TimeLimit if timer is not running and TimeLeft is 0
            float displayTime = (timer.IsRunning || timer.TimeLeft > 0) ? timer.TimeLeft : timer.TimeLimit;
            UpdateTimerText(displayTime);
        }
        if (questSystem != null)
        {
            questSystem.OnQuestChanged += UpdateQuestUI;
            UpdateQuestUI(questSystem.CurrentQuest);
        }

        // Subscribe to survival resources events (will be uncommented when SurvivalResources is ready)
        var survivalResources = FindFirstObjectByType<SurvivalResources>();
        if (survivalResources != null)
        {
            survivalResources.OnResourcesChanged += UpdateResourcesUI;
            survivalResources.OnFoodAdded += OnFoodAdded;
            survivalResources.OnWaterAdded += OnWaterAdded;
            // Initialize UI
            UpdateResourcesUI(survivalResources.CurrentFood, survivalResources.CurrentWater);
        }
    }

    private void OnDisable()
    {
        if (timer != null)
        {
            timer.OnTimerTick -= UpdateTimerText;
            timer.OnTimerEnd -= OnTimerEnd;
        }
        if (questSystem != null)
        {
            questSystem.OnQuestChanged -= UpdateQuestUI;
        }

        // Unsubscribe from survival resources events
        var survivalResources = FindFirstObjectByType<SurvivalResources>();
        if (survivalResources != null)
        {
            survivalResources.OnResourcesChanged -= UpdateResourcesUI;
            survivalResources.OnFoodAdded -= OnFoodAdded;
            survivalResources.OnWaterAdded -= OnWaterAdded;
        }
    }

    private void Start()
    {
        if (questSystem != null)
        {
            questSystem.OnQuestCompleted += () => ShowDayUnlockAnimation(questSystem.Day);
        }
        if (nextDayButton != null)
        {
            nextDayButton.onClick.AddListener(OnNextDayButtonClicked);
        }
        if (startDayButton != null)
        {
            startDayButton.onClick.AddListener(OnStartDayButtonClicked);
        }
        if (dayProgressionCanvasGroup != null)
        {
            dayProgressionCanvasGroup.alpha = 0f;
            dayProgressionCanvasGroup.gameObject.SetActive(false);
        }
        if (dayIntroCanvasGroup != null)
        {
            dayIntroCanvasGroup.alpha = 0f;
            dayIntroCanvasGroup.gameObject.SetActive(false);
        }
        if (nextDayButton != null)
        {
            nextDayButton.gameObject.SetActive(false);
        }
        
        // Show day intro for the initial day (Day 1) after a short delay
        StartCoroutine(ShowInitialDayIntro());
    }

    private IEnumerator ShowInitialDayIntro()
    {
        // Wait for quest system to be ready
        yield return null;
        
        // Show intro for Day 1
        if (questSystem != null && questSystem.CurrentQuest != null)
        {
            ShowDayIntro();
        }
    }

    private void OnDestroy()
    {
        if (questSystem != null)
        {
            questSystem.OnQuestCompleted -= () => ShowDayUnlockAnimation(questSystem.Day);
        }
        if (nextDayButton != null)
        {
            nextDayButton.onClick.RemoveListener(OnNextDayButtonClicked);
        }
        if (startDayButton != null)
        {
            startDayButton.onClick.RemoveListener(OnStartDayButtonClicked);
        }
    }

    void UpdateTimerText(float timeLeft)
    {
        if (timerText != null)
            timerText.text = $"Time: {Mathf.CeilToInt(timeLeft)}s";
    }

    void UpdateQuestUI(Quest quest)
    {
        if (quest == null) return;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (var req in quest.requirements)
        {
            int spriteIndex = req.tileLevel;
            string icon = $"<sprite={spriteIndex}>";
            
            // Check if requirement is met
            bool isCompleted = req.currentAmount >= req.targetAmount;
            string progressText = $"{icon} x {req.currentAmount}/{req.targetAmount}";
            
            // Add stroke effect if requirement is completed
            if (isCompleted)
            {
                // Add rich text markup for stroke/outline effect
                progressText = $"<mark=#00FF0040><b>{progressText}</b></mark>";
            }
            
            sb.AppendLine(progressText);
        }
        if (questTitleText != null)
            questTitleText.text = quest.title;
        if (questDescText != null)
            questDescText.text = quest.description;
        if (questProgressText != null)
            questProgressText.text = sb.ToString();
    }

    void OnTimerEnd()
    {
        if (timerText != null)
            timerText.text = "Time's up!";
    }

    void UpdateResourcesUI(float currentFood, float currentWater)
    {
        // Find the survival resources to get max values (placeholder for now)
        float maxFood = 100f; // This will be retrieved from SurvivalResources when integrated
        float maxWater = 100f;

        // Update food UI
        if (foodSlider != null)
        {
            foodSlider.value = currentFood / maxFood;
        }
        if (foodText != null)
        {
            foodText.text = $"Food\n{currentFood:F0}/{maxFood:F0}";
        }

        // Update water UI
        if (waterSlider != null)
        {
            waterSlider.value = currentWater / maxWater;
        }
        if (waterText != null)
        {
            waterText.text = $"Water\n{currentWater:F0}/{maxWater:F0}";
        }
    }

    void OnFoodAdded(float amount)
    {
        // TODO: Add visual feedback for food gained
    }

    void OnWaterAdded(float amount)
    {
        // TODO: Add visual feedback for water gained  
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float from, float to, float duration)
    {
        float elapsed = 0f;
        canvasGroup.alpha = from;
        if (from == 0f) canvasGroup.gameObject.SetActive(true);
        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = to;
        if (to == 0f) canvasGroup.gameObject.SetActive(false);
    }

    private void ShowDayProgression()
    {
        if (dayProgressionCanvasGroup != null)
        {
            StartCoroutine(FadeCanvasGroup(dayProgressionCanvasGroup, 0f, 1f, 0.4f));
            if (dayProgressionText != null)
            {
                dayProgressionText.text = $"Day <b>{questSystem.Day}</b> Complete!";
                // Show progression text that was hidden during intro
                dayProgressionText.gameObject.SetActive(true);
            }
        }
        
        // Trigger environmental effects for progression timing
        if (questSystem?.CurrentQuestData != null)
        {
            questSystem.CurrentQuestData.TriggerEnvironmentalEffectEvent(EnvironmentalEffectTiming.ProgressionOnly);
        }
        
        // Show objects from current quest that should be visible during progression
        if (questSystem != null)
        {
            questSystem.ShowCurrentQuestShowObjectsWithTiming(EnvironmentalEffectTiming.ProgressionOnly);
        }
        
        // Hide objects that should be hidden during progression for current quest
        if (questSystem != null)
        {
            questSystem.HideCurrentQuestHideObjectsWithTiming(EnvironmentalEffectTiming.ProgressionOnly);
        }
        
        // Handle global progression visibility control
        if (enabledOnIntroOnly != null)
        {
            foreach (var obj in enabledOnIntroOnly)
            {
                if (obj != null) obj.SetActive(false); // Disable during progression
            }
        }
        
        if (enabledOnProgressionOnly != null)
        {
            foreach (var obj in enabledOnProgressionOnly)
            {
                if (obj != null) obj.SetActive(true); // Enable during progression
            }
        }
        
        // Show next day button that was hidden during intro
        if (nextDayButton != null)
        {
            nextDayButton.gameObject.SetActive(true);
        }
    }

    private void OnNextDayButtonClicked()
    {
        // Prevent spam clicking during day transitions
        if (isProcessingDayTransition) return;
        isProcessingDayTransition = true;
        
        // Check if there's a next quest available
        if (questSystem != null && questSystem.HasNextQuest())
        {
            // Use the game mode's method to properly handle day progression
            if (gameMode != null)
            {
                gameMode.OnNextButtonClicked();
            }
            else
            {
                // Fallback to old behavior if game mode reference is missing
                questSystem.NextDay();
            }
            
            // Show day intro immediately after progression (before gameplay starts)
            StartCoroutine(ShowDayIntroAfterDelay());
        }
        else
        {
            // No more quests - Game Over
            HandleGameOver();
            // Reset flag since game is over
            isProcessingDayTransition = false;
        }
    }
    
    private void HandleGameOver()
    {
        Debug.Log("Game Over - All quests completed!");
        // You can add game over UI here or trigger game over event
        // For now, just log the game over state
        
        // Optional: Show a game over screen or return to main menu
        // Example: SceneManager.LoadScene("GameOverScene");
    }
    
    private IEnumerator ShowDayIntroAfterDelay()
    {
        // Small delay to ensure quest system has updated
        yield return new WaitForSeconds(0.5f);
        ShowDayIntro();
    }
    
    private void OnStartDayButtonClicked()
    {
        // Prevent spam clicking during fade transitions
        if (isStartingDay) return;
        isStartingDay = true;
        
        // Trigger environmental effects for gameplay timing
        if (questSystem?.CurrentQuestData != null)
        {
            questSystem.CurrentQuestData.TriggerEnvironmentalEffectEvent(EnvironmentalEffectTiming.GameplayOnly);
        }
        
        // Hide the day intro overlay
        if (dayIntroCanvasGroup != null)
        {
            StartCoroutine(FadeCanvasGroup(dayIntroCanvasGroup, 1f, 0f, 0.3f));
        }
        
        // Fade out the progression UI
        if (dayProgressionCanvasGroup != null)
        {
            StartCoroutine(FadeCanvasGroup(dayProgressionCanvasGroup, 1f, 0f, 0.3f));
        }
        
        // Show gameplay objects and hide intro objects
        SetObjectVisibilityForGameplay();
        
        // Tell the game mode to actually start the day
        if (gameMode != null)
        {
            gameMode.OnDayIntroCompleted();
        }
        
        // Reset flags after a delay to allow next day cycle
        StartCoroutine(ResetStartingDayFlag());
    }
    
    private IEnumerator ResetStartingDayFlag()
    {
        // Wait for fade animations to complete
        yield return new WaitForSeconds(0.5f);
        isStartingDay = false;
        // Note: isProcessingDayTransition is now reset in ShowDayIntro() when the day intro appears
    }
    
    private void SetObjectVisibilityForIntro()
    {
        // Hide all event objects from other quests first
        if (questSystem != null)
        {
            questSystem.HideAllEventObjectsExceptCurrent();
        }
        
        // Show objects from current quest that should be visible during intro
        if (questSystem != null)
        {
            questSystem.ShowCurrentQuestShowObjectsWithTiming(EnvironmentalEffectTiming.IntroOnly);
        }
        
        // Hide objects that should be hidden during intro for current quest
        if (questSystem != null)
        {
            questSystem.HideCurrentQuestHideObjectsWithTiming(EnvironmentalEffectTiming.IntroOnly);
        }
        
        // Show event objects for current quest
        if (questSystem?.CurrentQuestData != null)
        {
            questSystem.CurrentQuestData.ShowEventObjects();
        }
        
        // Handle global intro visibility control
        if (enabledOnIntroOnly != null)
        {
            foreach (var obj in enabledOnIntroOnly)
            {
                if (obj != null) obj.SetActive(true); // Enable during intro
            }
        }
        
        if (enabledOnProgressionOnly != null)
        {
            foreach (var obj in enabledOnProgressionOnly)
            {
                if (obj != null) obj.SetActive(false); // Disable during intro
            }
        }
    }
    
    private void SetObjectVisibilityForGameplay()
    {
        // Hide event objects from current quest (gameplay doesn't show event objects)
        if (questSystem?.CurrentQuestData != null)
        {
            questSystem.CurrentQuestData.HideEventObjects();
        }
        
        // Show objects that were hidden during intro/progression (restore normal state)
        if (questSystem?.CurrentQuestData != null)
        {
            questSystem.CurrentQuestData.ShowHideObjects();
        }
        
        // Note: Global intro/progression objects are not managed here since gameplay is separate
    }
    
    public void ShowDayIntro()
    {
        if (dayIntroCanvasGroup != null && questSystem != null && questSystem.CurrentQuest != null)
        {
            // Reset the day transition flag since we're now showing the intro (transition is complete)
            isProcessingDayTransition = false;
            
            // Set object visibility for intro
            SetObjectVisibilityForIntro();
            
            // Trigger environmental effects for intro timing
            if (questSystem.CurrentQuestData != null)
            {
                questSystem.CurrentQuestData.TriggerEnvironmentalEffectEvent(EnvironmentalEffectTiming.IntroOnly);
            }
            
            // Show progression canvas but hide specific elements during intro
            if (dayProgressionCanvasGroup != null)
            {
                dayProgressionCanvasGroup.gameObject.SetActive(true);
                dayProgressionCanvasGroup.alpha = 1f;
            }
            
            // Hide progression text and next day button during intro
            if (dayProgressionText != null)
                dayProgressionText.gameObject.SetActive(false);
            if (nextDayButton != null)
                nextDayButton.gameObject.SetActive(false);
            
            // Update intro texts
            if (dayIntroTitleText != null)
            {
                dayIntroTitleText.text = $"Day {questSystem.Day}";
            }
            
            if (dayIntroQuestTitleText != null)
            {
                dayIntroQuestTitleText.text = questSystem.CurrentQuest.title;
            }
            
            if (dayIntroDescriptionText != null)
            {
                dayIntroDescriptionText.text = questSystem.CurrentQuest.description;
            }
            
            if (dayIntroObjectivesText != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine("Objectives:");
                foreach (var req in questSystem.CurrentQuest.requirements)
                {
                    int spriteIndex = req.tileLevel;
                    string icon = $"<sprite={spriteIndex}>";
                    sb.AppendLine($"• {icon} Destroy {req.targetAmount} Level {req.tileLevel} tiles");
                }
                dayIntroObjectivesText.text = sb.ToString();
            }
            
            // Show the intro overlay
            StartCoroutine(FadeCanvasGroup(dayIntroCanvasGroup, 0f, 1f, 0.4f));
        }
    }

    private IEnumerator FlickerUnlockObject(GameObject obj, GameObject shadow, float duration, float interval)
    {
        float timer = 0f;
        bool state = false;
        obj.SetActive(false);
        shadow?.SetActive(false);
        while (timer < duration)
        {
            state = !state;
            obj.SetActive(state);
            timer += interval;
            yield return new WaitForSeconds(interval);
        }
        obj.SetActive(true);
    }

    public void ShowDayUnlockAnimation(int day)
    {
        ShowDayProgression();
        
        // Use quest-based unlock animation objects instead of day names
        if (questSystem?.CurrentQuestData != null)
        {
            GameObject[] unlockObjects = questSystem.CurrentQuestData.GetUnlockAnimationObjects();
            if (unlockObjects != null && unlockObjects.Length > 0)
            {
                // Animate all unlock objects for this quest
                foreach (var obj in unlockObjects)
                {
                    if (obj != null)
                    {
                        StartCoroutine(UnlockRoutine(obj, null));
                    }
                }
            }
            else
            {
                // Fallback: show next day button after a delay
                StartCoroutine(DelayedShowNextDayButton());
            }
        }
        else
        {
            // Fallback: show next day button after a delay
            StartCoroutine(DelayedShowNextDayButton());
        }
    }

    private IEnumerator DelayedShowNextDayButton()
    {
        // Wait a short time to ensure the game mode has processed the quest completion
        // and entered the waiting state
        yield return new WaitForSeconds(1f);
        
        if (nextDayButton != null) 
            nextDayButton.gameObject.SetActive(true);
    }

    private IEnumerator UnlockRoutine(GameObject obj, GameObject shadow)
    {
        if (nextDayButton != null) nextDayButton.gameObject.SetActive(false);
        yield return FlickerUnlockObject(obj, shadow, flickerDuration, flickerInterval);
        if (nextDayButton != null) nextDayButton.gameObject.SetActive(true);
    }
}
