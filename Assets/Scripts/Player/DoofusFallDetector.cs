using UnityEngine;
using Doofus.Core;

namespace Doofus.Player
{
    // Watches Doofus's height and ends the game the moment he falls off/through a pulpit.
    public class DoofusFallDetector : MonoBehaviour
    {
        [SerializeField] private float fallThresholdY = -15f;

        private bool _armed;
        private bool _hasFallen;

        private void OnEnable()
        {
            GameEvents.OnGameStart += Arm;
            GameEvents.OnGameReset += Disarm;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStart -= Arm;
            GameEvents.OnGameReset -= Disarm;
        }

        private void Arm()
        {
            _armed = true;
            _hasFallen = false;
        }

        private void Disarm()
        {
            _armed = false;
            _hasFallen = false;
        }

        private void Update()
        {
            if (!_armed || _hasFallen) return;
            if (transform.position.y >= fallThresholdY) return;

            _hasFallen = true;
            _armed = false;
            GameEvents.RaisePlayerFell();
            GameEvents.RaiseGameOver();
        }
    }
}
