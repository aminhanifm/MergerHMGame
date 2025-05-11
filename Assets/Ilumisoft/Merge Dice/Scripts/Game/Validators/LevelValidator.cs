namespace Ilumisoft.MergeDice
{
    public struct LevelValidator : IConnectionValidator
    {
        public bool IsValid(GameTile first, GameTile second)
        {
            if (first is ICanLevelUp firstLevel && second is ICanLevelUp secondLevel)
            {
                return firstLevel.CurrentLevel == secondLevel.CurrentLevel;
            }

            return false;
        }
    }
}