using UnityEngine;
using UnityEngine.UI;

namespace Ilumisoft.MergeDice
{
    public class GameUI : MonoBehaviour
    {
        [SerializeField]
        Text scoreText = null;

        ScoreTween scoreTween;

        int lastScore = 0;

        private void Awake()
        {
            scoreTween = new ScoreTween(scoreText);
        }

        private void OnEnable()
        {
            Score.OnScoreChanged += OnScoreChanged;
        }

        private void OnDisable()
        {
            Score.OnScoreChanged -= OnScoreChanged;
        }

        private void OnScoreChanged(int currentScore)
        {
            scoreTween.Fade(lastScore, currentScore, 1.0f);
            lastScore = currentScore;
        }
    }
}