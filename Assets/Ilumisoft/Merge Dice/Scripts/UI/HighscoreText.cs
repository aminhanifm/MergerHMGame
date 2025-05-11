using UnityEngine;
using UnityEngine.UI;

namespace Ilumisoft.MergeDice
{
    public class HighscoreText : MonoBehaviour
    {
        [SerializeField]
        string prefix = string.Empty;

        Text textComponent;

        private void Awake()
        {
            textComponent = GetComponent<Text>();
        }

        void Start()
        {
            UpdateText();
        }

        void UpdateText()
        {
            textComponent.text = prefix + Highscore.Value.ToString();
        }
    }
}