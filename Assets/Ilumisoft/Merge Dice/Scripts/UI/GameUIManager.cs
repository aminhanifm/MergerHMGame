using Ilumisoft.MergeDice.Events;
using System.Collections;
using UnityEngine;

namespace Ilumisoft.MergeDice
{
    public class GameUIManager : MonoBehaviour
    {
        [SerializeField]
        GameObject gameUI = null;

        [SerializeField]
        GameObject gameOverUI = null;

        [SerializeField]
        OverlayCanvas overlayCanvas = null;

        private void OnEnable()
        {
            GameEvents<UIEventType>.OnTrigger += OnTriggerUI;
        }

        private void OnDisable()
        {
            GameEvents<UIEventType>.OnTrigger -= OnTriggerUI;
        }

        private void OnTriggerUI(UIEventType type)
        {
            switch (type)
            {
                case UIEventType.GameOver:
                    ShowGameOverUI();
                    break;
            }
        }

        void ShowGameOverUI()
        {
            StartCoroutine(ShowGameOverUICoroutine());
        }

        IEnumerator ShowGameOverUICoroutine()
        {
            yield return overlayCanvas.FadeIn();
            gameUI.gameObject.SetActive(false);
            gameOverUI.gameObject.SetActive(true);
            yield return overlayCanvas.FadeOut();
        }
    }
}