using UnityEngine.Events;

namespace Ilumisoft.MergeDice
{
    public interface ICanLevelUp
    {
        int CurrentLevel { get; set; }

        int MaxLevel { get; }

        UnityAction OnLevelChanged { get; set; }

        void LevelUp();

        bool HasMaxLevel { get; }
    }
}