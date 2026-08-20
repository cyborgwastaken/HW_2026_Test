using UnityEngine;
using Doofus.Core;

namespace Doofus.Scoring
{
    // Tracks Doofus's score: +1 for every distinct new pulpit successfully walked onto.
    public class ScoreManager : MonoBehaviour
    {
        public int Score { get; private set; }

        private void OnEnable()
        {
            GameEvents.OnPulpitLanded += HandlePulpitLanded;
            GameEvents.OnGameReset += HandleReset;
        }

        private void OnDisable()
        {
            GameEvents.OnPulpitLanded -= HandlePulpitLanded;
            GameEvents.OnGameReset -= HandleReset;
        }

        private void HandlePulpitLanded()
        {
            Score++;
            GameEvents.RaiseScoreChanged(Score);
        }

        private void HandleReset()
        {
            Score = 0;
            GameEvents.RaiseScoreChanged(Score);
        }
    }
}
