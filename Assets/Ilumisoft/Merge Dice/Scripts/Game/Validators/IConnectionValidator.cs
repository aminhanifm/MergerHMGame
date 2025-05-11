namespace Ilumisoft.MergeDice
{
    public interface IConnectionValidator
    {
        bool IsValid(GameTile first, GameTile second);
    }
}