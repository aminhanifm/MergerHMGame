using UnityEngine;

namespace Ilumisoft.MergeDice
{
    public interface IGameTileFactory
    {
        GameTile Spawn(GameTile prefab, Vector3 position);
        GameTile Spawn(Vector3 position);
    }
}