using System.Collections;
using UnityEngine;

namespace Ilumisoft.MergeDice
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        GameMode gameMode = null;

        IEnumerator Start()
        {
            // Start the game
            yield return gameMode.StartGame();

            // Run the game and wait until it is over
            yield return gameMode.RunGame();

            // Finish the game
            yield return gameMode.EndGame();
        }
    }
}