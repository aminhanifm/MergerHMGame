using System.Collections;
using UnityEngine;

namespace Ilumisoft.MergeDice
{
    public abstract class GameMode : MonoBehaviour
    {
        public abstract IEnumerator StartGame();

        public abstract IEnumerator RunGame();

        public abstract IEnumerator EndGame();
    }
}