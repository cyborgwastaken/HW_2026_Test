using UnityEngine;
using Doofus.Config;
using StarterAssets;

namespace Doofus.Player
{
    // Generic reset/reposition adapter for a CharacterController-driven player (e.g. a
    // third-party third-person controller this project doesn't own/modify). Exists so
    // GameManager doesn't need to know which movement system the player uses - it just
    // calls ResetState(). Handles the standard Unity gotcha where setting transform.position
    // directly while a CharacterController is enabled fights with its internal move solver.
    //
    // Also applies Doofus's Diary's player speed to the third-person controller's public
    // MoveSpeed field once config loads - a data assignment, not a script edit.
    [RequireComponent(typeof(CharacterController))]
    public class PlayerRig : MonoBehaviour
    {
        private CharacterController _controller;
        private ThirdPersonController _thirdPersonController;

        private CharacterController Controller => _controller != null ? _controller : (_controller = GetComponent<CharacterController>());

        private void Awake()
        {
            _thirdPersonController = GetComponent<ThirdPersonController>();
        }

        private void OnEnable()
        {
            GameConfigLoader.OnConfigReady += HandleConfigReady;
            if (GameConfigLoader.Instance != null && GameConfigLoader.Instance.IsLoaded)
            {
                HandleConfigReady(GameConfigLoader.Instance.Config);
            }
        }

        private void OnDisable()
        {
            GameConfigLoader.OnConfigReady -= HandleConfigReady;
        }

        private void HandleConfigReady(GameConfig config)
        {
            if (_thirdPersonController != null)
            {
                _thirdPersonController.MoveSpeed = config.player_data.speed;
            }
        }

        public void ResetState(Vector3 position)
        {
            CharacterController controller = Controller;
            bool wasEnabled = controller.enabled;
            controller.enabled = false;
            transform.position = position;
            transform.rotation = Quaternion.identity;
            controller.enabled = wasEnabled;
        }
    }
}
