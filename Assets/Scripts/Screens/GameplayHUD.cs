using UnityEngine;
using UnityEngine.UI;
using Doofus.Core;

namespace Doofus.Screens
{
    public class GameplayHUD : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text scoreText;

        private void Awake()
        {
            // Subscribing here (not OnEnable) is deliberate: 'panel' is this same
            // GameObject, and deactivating it below would otherwise skip OnEnable
            // entirely on this first pass, silently dropping these subscriptions.
            GameEvents.OnScoreChanged += HandleScoreChanged;
            GameEvents.OnGameStart += HandleGameStart;
            GameEvents.OnGameOver += HandleGameOver;

            if (panel != null) panel.SetActive(false);
        }

        private void HandleScoreChanged(int score)
        {
            if (scoreText != null) scoreText.text = $"Pulpits: {score}";
        }

        private void HandleGameStart()
        {
            if (panel != null) panel.SetActive(true);
            HandleScoreChanged(0);
        }

        private void HandleGameOver()
        {
            if (panel != null) panel.SetActive(false);
        }
    }
}
