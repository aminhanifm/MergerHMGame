using System;
using UnityEngine;

namespace Ilumisoft.MergeDice.Survival
{
    [Serializable]
    public class QuestRequirement
    {
        public int tileLevel;
        public int targetAmount;
        public int currentAmount;
    }

    [Serializable]
    public class Quest
    {
        public string title;
        public string description;
        public QuestRequirement[] requirements;
        public bool IsComplete => requirements != null && System.Array.TrueForAll(requirements, r => r.currentAmount >= r.targetAmount);
    }
}
