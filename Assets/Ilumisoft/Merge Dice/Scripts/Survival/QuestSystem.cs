using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ilumisoft.MergeDice.Survival
{
    public class QuestSystem : MonoBehaviour
    {
        public Quest CurrentQuest { get; private set; }
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
            CurrentQuest = GenerateQuestForDay(Day);
            if (CurrentQuest == null)
            {
                OnAllQuestsCompleted?.Invoke();
                return;
            }
            OnQuestChanged?.Invoke(CurrentQuest);
        }

        public void ProgressQuest(int tileLevel, int amount)
        {
            if (CurrentQuest == null || CurrentQuest.IsComplete)
                return;

            bool changed = false;
            foreach (var req in CurrentQuest.requirements)
            {
                if (req.tileLevel == tileLevel && req.currentAmount < req.targetAmount)
                {
                    // Only add up to the targetAmount
                    int addAmount = Mathf.Min(amount, req.targetAmount - req.currentAmount);
                    req.currentAmount += addAmount;
                    changed = addAmount > 0 || changed;
                }
            }
            if (changed)
            {
                OnQuestChanged?.Invoke(CurrentQuest);
                if (CurrentQuest.IsComplete)
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

        public void ResetDay()
        {
            Day = 1;
        }

        // You can edit this method to customize quests per day
        public Quest GenerateQuestForDay(int day)
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
                            currentAmount = 0
                        };
                    }
                    return quest;
                }
            }
            // No quest found for this day
            return null;
        }
    }
}
