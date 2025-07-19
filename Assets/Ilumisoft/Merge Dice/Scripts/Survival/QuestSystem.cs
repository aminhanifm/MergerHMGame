using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ilumisoft.MergeDice.Survival
{
    [System.Serializable]
    public class QuestRequirementProgress
    {
        public int tileLevel;
        public int targetAmount;
        public int currentAmount;
        
        public QuestRequirementProgress(int tileLevel, int targetAmount)
        {
            this.tileLevel = tileLevel;
            this.targetAmount = targetAmount;
            this.currentAmount = 0;
        }
        
        public bool IsComplete => currentAmount >= targetAmount;
        public float ProgressPercentage => targetAmount > 0 ? (float)currentAmount / targetAmount : 0f;
    }

    [System.Serializable]
    public class QuestProgress
    {
        public string title;
        public string description;
        public QuestRequirementProgress[] requirements;
        
        // Quest properties from QuestData
        public float timeLimit;
        public int priority;
        public bool canBeSkipped;
        public int scoreReward;
        public float timeBonus;
        
        public QuestProgress(QuestData questData)
        {
            title = questData.title;
            description = questData.description;
            timeLimit = questData.timeLimit;
            priority = questData.priority;
            canBeSkipped = questData.canBeSkipped;
            scoreReward = questData.scoreReward;
            timeBonus = questData.timeBonus;
            
            requirements = new QuestRequirementProgress[questData.requirements.Length];
            
            for (int i = 0; i < questData.requirements.Length; i++)
            {
                requirements[i] = new QuestRequirementProgress(
                    questData.requirements[i].tileLevel,
                    questData.requirements[i].targetAmount
                );
            }
        }
        
        public bool IsComplete
        {
            get
            {
                if (requirements == null || requirements.Length == 0) return false;
                foreach (var req in requirements)
                {
                    if (!req.IsComplete) return false;
                }
                return true;
            }
        }
        
        public float OverallProgress
        {
            get
            {
                if (requirements == null || requirements.Length == 0) return 1f;
                float totalProgress = 0f;
                foreach (var req in requirements)
                {
                    totalProgress += req.ProgressPercentage;
                }
                return totalProgress / requirements.Length;
            }
        }
    }

    public struct QuestGenerationResult
    {
        public QuestData questData;
        public Quest quest;
        
        public QuestGenerationResult(QuestData questData, Quest quest)
        {
            this.questData = questData;
            this.quest = quest;
        }
    }

    public class QuestSystem : MonoBehaviour
    {
        public Quest CurrentQuest { get; private set; }
        public QuestData CurrentQuestData { get; private set; }
        public QuestProgress CurrentQuestProgress { get; private set; }
        public int Day { get; private set; } = 1;

        public event Action<Quest> OnQuestChanged;
        public event Action OnQuestCompleted;
        public event Action OnAllQuestsCompleted; // New event for game over
        public event Action<int> OnScoreReward; // New event for score rewards
        public event Action<float> OnTimeBonus; // New event for time bonuses
        public event Action OnNewDayStarted; // New event for day/quest transitions

        public bool AllQuestsCompleted {get; private set;} = false;

        public QuestDatabase questDatabase; // Assign in inspector

        void Awake()
        {
            OnAllQuestsCompleted += () => 
            {
                AllQuestsCompleted = true;
                Debug.Log("All quests completed! Game over.");
            };
        }

        public void StartNewDay()
        {
            var questResult = GenerateQuestForDay(Day);
            CurrentQuestData = questResult.questData;
            CurrentQuest = questResult.quest;
            
            // Create runtime progress tracking
            if (CurrentQuestData != null)
            {
                CurrentQuestProgress = new QuestProgress(CurrentQuestData);
            }
            
            if (CurrentQuest == null)
            {
                OnAllQuestsCompleted?.Invoke();
                return;
            }
            
            OnNewDayStarted?.Invoke();
            OnQuestChanged?.Invoke(CurrentQuest);
        }

        public void ProgressQuest(int tileLevel, int amount)
        {
            if (CurrentQuest == null || CurrentQuestProgress == null || CurrentQuestProgress.IsComplete)
                return;

            bool changed = false;
            
            // Update both runtime quest and progress tracking
            for (int i = 0; i < CurrentQuest.requirements.Length; i++)
            {
                var req = CurrentQuest.requirements[i];
                var progressReq = CurrentQuestProgress.requirements[i];
                
                if (req.tileLevel == tileLevel && req.currentAmount < req.targetAmount)
                {
                    // Only add up to the targetAmount
                    int addAmount = Mathf.Min(amount, req.targetAmount - req.currentAmount);
                    
                    // Update runtime quest
                    req.currentAmount += addAmount;
                    
                    // Update progress tracking
                    progressReq.currentAmount += addAmount;
                    
                    changed = addAmount > 0 || changed;
                }
            }
            
            if (changed)
            {
                OnQuestChanged?.Invoke(CurrentQuest);
                if (CurrentQuestProgress.IsComplete)
                {
                    CompleteCurrentQuest();
                }
            }
        }

        private void CompleteCurrentQuest()
        {
            if (CurrentQuestProgress == null) return;

            Debug.Log($"Quest completed: {CurrentQuestProgress.title}");

            // Apply quest rewards
            if (CurrentQuestProgress.scoreReward > 0)
            {
                OnScoreReward?.Invoke(CurrentQuestProgress.scoreReward);
                Debug.Log($"Score reward granted: {CurrentQuestProgress.scoreReward}");
            }

            if (CurrentQuestProgress.timeBonus > 0)
            {
                OnTimeBonus?.Invoke(CurrentQuestProgress.timeBonus);
                Debug.Log($"Time bonus granted: {CurrentQuestProgress.timeBonus} seconds");
            }

            OnQuestCompleted?.Invoke();
        }

        public bool CanSkipCurrentQuest()
        {
            return CurrentQuestProgress != null && CurrentQuestProgress.canBeSkipped;
        }

        public void SkipCurrentQuest()
        {
            if (!CanSkipCurrentQuest())
            {
                Debug.LogWarning("Current quest cannot be skipped!");
                return;
            }

            Debug.Log($"Quest skipped: {CurrentQuestProgress.title}");
            NextDay(); // Move to next day/quest
        }

        public float GetCurrentQuestTimeLimit()
        {
            return CurrentQuestProgress?.timeLimit ?? 0f;
        }

        public int GetCurrentQuestPriority()
        {
            return CurrentQuestProgress?.priority ?? 0;
        }

        public void NextDay()
        {
            Day++;
            StartNewDay();
        }

        public void ResetCurrentQuestProgress()
        {
            if (CurrentQuest == null || CurrentQuestProgress == null)
                return;
                
            // Reset both runtime quest and progress tracking
            for (int i = 0; i < CurrentQuest.requirements.Length; i++)
            {
                CurrentQuest.requirements[i].currentAmount = 0;
                CurrentQuestProgress.requirements[i].currentAmount = 0;
            }
            
            OnQuestChanged?.Invoke(CurrentQuest);
        }

        public void CompleteCurrentQuestDebug()
        {
            if (CurrentQuest == null || CurrentQuestProgress == null)
                return;
                
            Debug.Log("Debug: Completing current quest");
                
            // Complete all requirements
            for (int i = 0; i < CurrentQuest.requirements.Length; i++)
            {
                CurrentQuest.requirements[i].currentAmount = CurrentQuest.requirements[i].targetAmount;
                CurrentQuestProgress.requirements[i].currentAmount = CurrentQuestProgress.requirements[i].targetAmount;
            }
            
            OnQuestChanged?.Invoke(CurrentQuest);
            
            // Trigger quest completion manually
            if (CurrentQuestProgress.IsComplete)
            {
                // Award score reward if applicable
                if (CurrentQuestProgress.scoreReward > 0)
                {
                    OnScoreReward?.Invoke(CurrentQuestProgress.scoreReward);
                    Debug.Log($"Debug: Score reward granted: {CurrentQuestProgress.scoreReward}");
                }

                // Award time bonus if applicable  
                if (CurrentQuestProgress.timeBonus > 0)
                {
                    OnTimeBonus?.Invoke(CurrentQuestProgress.timeBonus);
                    Debug.Log($"Debug: Time bonus granted: {CurrentQuestProgress.timeBonus} seconds");
                }

                OnQuestCompleted?.Invoke();
                Debug.Log("Debug: Quest completion event triggered");
            }
        }

        public void ResetDay()
        {
            Day = 1;
        }

        // You can edit this method to customize quests per day
        public QuestGenerationResult GenerateQuestForDay(int day)
        {
            if (questDatabase != null && questDatabase.quests != null && day - 1 < questDatabase.quests.Length)
            {
                var data = questDatabase.quests[day - 1];
                if (data != null && data.requirements != null)
                {
                    var quest = new Quest
                    {
                        title = data.title,
                        description = data.description,
                        requirements = new QuestRequirement[data.requirements.Length]
                    };
                    for (int i = 0; i < data.requirements.Length; i++)
                    {
                        quest.requirements[i] = new QuestRequirement
                        {
                            tileLevel = data.requirements[i].tileLevel,
                            targetAmount = data.requirements[i].targetAmount,
                            currentAmount = 0 // Always start fresh
                        };
                    }
                    return new QuestGenerationResult(data, quest);
                }
            }
            // No quest found for this day
            return new QuestGenerationResult(null, null);
        }
    }
}
