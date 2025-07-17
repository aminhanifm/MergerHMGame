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
        
        public QuestProgress(QuestData questData)
        {
            title = questData.title;
            description = questData.description;
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
                    OnQuestCompleted?.Invoke();
                }
            }
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
