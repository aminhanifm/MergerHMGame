using UnityEngine;

namespace Ilumisoft.MergeDice
{
    public struct GameTileRaycast
    {
        public Vector2 Position { get; set; }
        public Vector2 Direction { get; set; }
        public float MaxDistance { get; set; }

        public GameTileRaycast(Vector2 position, Vector2 direction, float maxDistance)
        {
            this.Position = position;
            this.Direction = direction;
            this.MaxDistance = maxDistance;
        }

        public bool Perform(out GameTile gameTile)
        {
            var hit = Physics2D.Raycast(Position, Direction, MaxDistance);

            if (hit.collider != null)
            {
                gameTile = hit.collider.GetComponentInParent<GameTile>();

                if (gameTile != null)
                {
                    return true;
                }
            }

            gameTile = null;

            return false;
        }
    }
}