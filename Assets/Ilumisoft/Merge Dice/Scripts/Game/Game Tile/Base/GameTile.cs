using UnityEngine;
using UnityEngine.Events;

namespace Ilumisoft.MergeDice
{
    [RequireComponent(typeof(DiceMoveToBehaviour))]
    public class GameTile : MonoBehaviour
    {
        [SerializeField]
        protected Collider2D inputCollider = null;

        public UnityAction<GameTile> OnTileDestroyed { get; set; }

        public virtual bool IsDestroyed { get; private set; }

        public virtual void EnableCollider()
        {
            this.inputCollider.enabled = true;
        }

        public virtual void DisableCollider()
        {
            this.inputCollider.enabled = false;
        }

        public virtual void Pop()
        {
            IsDestroyed = true;

            DisableCollider();

            OnTileDestroyed?.Invoke(this);

            Destroy(this.gameObject, 1.0f);
        }
    }
}