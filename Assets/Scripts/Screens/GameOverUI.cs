using UnityEngine;
using UnityEngine.UI;
using Doofus.Core;
using Doofus.Scoring;

namespace Doofus.Screens
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text finalScoreText;
        [SerializeField] private Button retryButton;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private ScoreManager scoreManager;

        private void Awake()
        {
            if (retryButton != null) retryButton.onClick.AddListener(HandleRetryClicked);

            // Subscribing here (not OnEnable) is deliberate: 'panel' is this same
            // GameObject, and deactivating it below would otherwise skip OnEnable
            // entirely on this first pass, silently dropping this subscription.
            GameEvents.OnGameOver += HandleGameOver;

            if (panel != null) panel.SetActive(false);
        }

        private void HandleGameOver()
        {
            if (finalScoreText != null && scoreManager != null)
            {
                finalScoreText.text = $"Final Score: {scoreManager.Score}";
            }
            if (panel != null) panel.SetActive(true);
        }

        private void HandleRetryClicked()
        {
            if (panel != null) panel.SetActive(false);
            if (gameManager != null) gameManager.RestartGame();
        }
    }
}
