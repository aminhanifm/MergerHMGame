using UnityEngine;
using UnityEngine.UI;

namespace Ilumisoft.MergeDice
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField]
        Text scoreText = null;

        [SerializeField]
        Text highscoreText = null;

        [SerializeField]
        Text messageText = null;

        void Start()
        {
            if (HasNewHighscore())
            {
                UpdateHighscore();
                DisplayHighscore(false);
                DisplayNewHighscoreMessage();
            }
            else
            {
                DisplayHighscore(true);
                DisplayNormalMessage();
            }

            DisplayScore();
        }

        bool HasNewHighscore()
        {
            return Score.Value > Highscore.Value;
        }

        void UpdateHighscore()
        {
            Highscore.Value = Score.Value;
        }

        void DisplayNewHighscoreMessage()
        {
            messageText.text = "NEW HIGHSCORE";
        }

        void DisplayNormalMessage()
        {
            messageText.text = "ANOTHER TRY?";
        }

        void DisplayHighscore(bool show)
        {
            if (show)
            {
                highscoreText.text = $"BEST\n{Highscore.Value}";
            }
            else
            {
                highscoreText.text = string.Empty;
            }
        }

        void DisplayScore()
        {
            scoreText.text = Score.Value.ToString();
        }
    }
}
