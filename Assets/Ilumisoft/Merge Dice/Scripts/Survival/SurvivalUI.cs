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
        if (dayProgressionCanvasGroup != null)
        {
            dayProgressionCanvasGroup.alpha = 0f;
            dayProgressionCanvasGroup.gameObject.SetActive(false);
        }
        if (nextDayButton != null)
        {
            nextDayButton.gameObject.SetActive(false);
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
            sb.AppendLine($"{icon} x {req.currentAmount}/{req.targetAmount}");
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
            }
        }
    }

    private void OnNextDayButtonClicked()
    {
        if (dayProgressionCanvasGroup != null)
        {
            StartCoroutine(FadeCanvasGroup(dayProgressionCanvasGroup, 1f, 0f, 0.3f));
        }
        
        // Use the game mode's method to properly handle time bonuses
        if (gameMode != null)
        {
            gameMode.OnNextButtonClicked();
        }
        else
        {
            // Fallback to old behavior if game mode reference is missing
            if (timer != null)
            {
                timer.StartTimer();
            }
            if (questSystem != null)
            {
                questSystem.NextDay();
            }
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
        string dayName = $"Day {day}";
        Transform obj = objectsParent?.transform.Find(dayName);
        if (obj != null)
        {
            StartCoroutine(UnlockRoutine(obj.gameObject, null)); // Remove shadow dependency
        }
        else
        {
            // fallback: show next day button after a delay to ensure game mode is in waiting state
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
