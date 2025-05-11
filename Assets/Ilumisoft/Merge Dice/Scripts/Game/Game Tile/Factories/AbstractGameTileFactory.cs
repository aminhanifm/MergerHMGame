using UnityEngine;

namespace Ilumisoft.MergeDice
{
    public abstract class AbstractGameTileFactory : MonoBehaviour, IGameTileFactory
    {
        public abstract GameTile Spawn(GameTile prefab, Vector3 position);
        public abstract GameTile Spawn(Vector3 position);
    }
}