using TMPro;
using UnityEngine;
using Ilumisoft.MergeDice.Survival;
using System.Collections.Generic;
using System.Collections;

public class SurvivalUI : MonoBehaviour
{
    [Header("References")]
    public SurvivalTimer timer;
    public QuestSystem questSystem;
    public SurvivalGameMode gameMode; // Add reference to game mode

    [Header("UI Elements")]
    public TMP_Text timerText;
    public TMP_Text questTitleText;
    public TMP_Text questDescText;
    public TMP_Text questProgressText;

    [Header("Progression UI")]
    public CanvasGroup dayProgressionCanvasGroup; // Assign in inspector instead of GameObject
    public TMP_Text dayProgressionText;
    public UnityEngine.UI.Button nextDayButton;

    [Header("Unlock Animation")]
    public GameObject objectsParent; // Assign in inspector
    public GameObject shadowParent;  // Assign in inspector
    public float flickerDuration = 1.0f;
    public float flickerInterval = 0.15f;

    private Dictionary<int, int> levelToSpriteIndex = new Dictionary<int, int>
    {
        { 0, 5 },
        { 1, 1 },
        { 2, 4 },
        { 3, 0 },
        { 4, 3 },
        { 5, 2 }
    };

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
            int spriteIndex = levelToSpriteIndex.ContainsKey(req.tileLevel) ? levelToSpriteIndex[req.tileLevel] : req.tileLevel;
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
        Transform shadow = shadowParent?.transform.Find(dayName);
        if (obj != null)
        {
            StartCoroutine(UnlockRoutine(obj.gameObject, shadow?.gameObject));
        }
        else
        {
            // fallback: just show next day button
            nextDayButton?.gameObject.SetActive(true);
        }
    }

    private IEnumerator UnlockRoutine(GameObject obj, GameObject shadow)
    {
        if (nextDayButton != null) nextDayButton.gameObject.SetActive(false);
        yield return FlickerUnlockObject(obj, shadow, flickerDuration, flickerInterval);
        if (nextDayButton != null) nextDayButton.gameObject.SetActive(true);
    }
}
