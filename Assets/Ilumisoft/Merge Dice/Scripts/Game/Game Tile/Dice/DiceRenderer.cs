using UnityEngine;

namespace Ilumisoft.MergeDice
{
    public class DiceRenderer : MonoBehaviour
    {
        [SerializeField]
        SpriteRenderer spriteRenderer = null;

        [SerializeField]
        SpriteRenderer overlay = null;

        DiceGameTile diceGameTile;

        DiceLevelBehaviour diceLevelBehaviour;

        private void Awake()
        {
            diceGameTile = GetComponent<DiceGameTile>();
            diceGameTile.OnTileDestroyed += OnTileDestroyed;
            diceGameTile.OnLevelChanged += OnLevelChanged;

            diceLevelBehaviour = diceGameTile.GetComponent<DiceLevelBehaviour>();
        }

        private void OnLevelChanged()
        {
            spriteRenderer.color = diceLevelBehaviour.Color;
            overlay.sprite = diceLevelBehaviour.Overlay;
        }

        private void OnTileDestroyed(GameTile tile)
        {
            spriteRenderer.gameObject.SetActive(false);
            overlay.gameObject.SetActive(false);
        }
    }
}